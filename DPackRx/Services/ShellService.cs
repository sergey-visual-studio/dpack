using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Input;

using DPackRx.CodeModel;
using DPackRx.Extensions;
using DPackRx.Language;
using DPackRx.Options;
using DPackRx.Package;

using EnvDTE;
using EnvDTE80;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.VCProjectEngine;
using VSLangProj;
using VsWebSite;
using VSPackage = Microsoft.VisualStudio.Shell.Package;

namespace DPackRx.Services
{
	/// <summary>
	/// Combined service that accesses Visual Studio and DTE APIs and thus is untestable.
	/// It's broken into smaller logically grouped sub-services.
	/// </summary>
	public class ShellService : IShellHelperService, IShellStatusBarService, IShellReferenceService,
		IShellSelectionService, IShellProjectService, IShellCodeModelService
	{
		#region Fields

		private readonly IServiceProvider _serviceProvider;
		private readonly ILog _log;
		private readonly ILanguageService _languageService;
		private DTE _dte;
		private IFileTypeResolver _fileTypeResolver;
		private readonly bool _webInteropAvailable;
		private readonly bool _cppInteropAvailable;

		private const string LOG_CATEGORY = "Shell";
		internal const int DEFAULT_WAIT_MSECS = 1000;
		internal const string PROJECT_ITEM_KIND_SETUP = "{54435603-DBB4-11D2-8724-00A0C9A8B90C}";
		internal const string TEXT_DOCUMENT = "TextDocument";

		private const string ELEMENT_TYPE = "vsCMElement";
		private const string KIND_CLASS = "Class";
		private const string KIND_MODULE = "Module";
		private const string KIND_STRUCT = "Structure";
		private const string KIND_ENUM = "Enumeration";
		private const string KIND_INTERFACE = "Interface";
		private const string KIND_METHOD = "Method";
		private const string KIND_PROPERTY = "Property";
		private const string KIND_VARIABLE = "Variable";
		private const string KIND_EVENT = "Event";
		private const string KIND_DELEGATE = "Delegate";

		#endregion

		public ShellService(IServiceProvider serviceProvider, ILog log, ILanguageService languageService)
		{
			_serviceProvider = serviceProvider;
			_log = log;
			_languageService = languageService;

			try
			{
				TestWebInterop();
				_webInteropAvailable = true;
			}
			catch
			{
				_webInteropAvailable = false;

				_log.LogMessage("Web Interop is not available", LOG_CATEGORY);
			}

			try
			{
				TestCPPInterop();
				_cppInteropAvailable = true;
			}
			catch
			{
				_cppInteropAvailable = false;

				_log.LogMessage("C++ Interop is not available", LOG_CATEGORY);
			}
		}

		#region IShellHelperService Members

		/// <summary>
		/// Returns untyped DTE instance.
		/// </summary>
		public object GetDTE()
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			return GetDTEInternal();
		}

		/// <summary>
		/// Returns selected Solution Explorer item path.
		/// </summary>
		public string GetSelectedItemPath()
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var dte = GetDTEInternal();

			string path = null;
			var selectedItems = dte.SelectedItems;
			if ((selectedItems != null) && (selectedItems.Count >= 1) &&
					(selectedItems.SelectionContainer != null) && (selectedItems.SelectionContainer.Count >= 1))
			{
				var item = selectedItems.Item(1).ProjectItem;
				var project = selectedItems.Item(1).Project;
				var obj = selectedItems.SelectionContainer.Item(1);
				if (item != null) // file or folder
				{
					path = Path.GetDirectoryName(item.get_FileNames(1));
					if (string.IsNullOrEmpty(path) && (item.ContainingProject != null))
					{
						GetProjectPath(item.ContainingProject, out path, out _);
						if (!string.IsNullOrEmpty(path) &&
								item.Kind.Equals(VSConstants.ItemTypeGuid.PhysicalFolder_string, StringComparison.OrdinalIgnoreCase))
							path = Path.Combine(path, item.Name);
					}
				}
				else if (project != null) // project
				{
					GetProjectPath(project, out path, out _);
				}
				else if (obj is Reference) // reference
				{
					var reference = (Reference)obj;
					try
					{
						var referencePath = reference.Path;
						if (!string.IsNullOrEmpty(referencePath) && Path.IsPathRooted(referencePath))
							path = Path.GetDirectoryName(referencePath);
					}
					catch
					{
						// Do nothing
					}
				}
				else // solution
				{
					var solutionPath = dte.Solution.FullName;
					if (!string.IsNullOrEmpty(solutionPath) && Path.IsPathRooted(solutionPath))
						path = Path.GetDirectoryName(solutionPath);
				}
			}

			return path;
		}

		/// <summary>
		/// Returns selected Solution Explorer project path.
		/// </summary>
		public string GetCurrentProjectPath()
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			if (!GetSelectionProject(out Project project, out _, out _, out _) ||
					(project == null))
				return null;

			try
			{
				return project.FullName;
			}
			catch (Exception ex)
			{
				_log.LogMessage(ex, LOG_CATEGORY);
				return null;
			}
		}

		/// <summary>
		/// Selects currently open document in Solution Explorer.
		/// </summary>
		public bool SelectSolutionExplorerDocument(out string documentName)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var dteDocument = GetActiveDocument() as Document;
			if (dteDocument == null)
			{
				documentName = string.Empty;
				return false;
			}

			documentName = GetActiveFileName();

			var projectItem = GetDocumentProjectItem(dteDocument);
			if ((projectItem == null) || !SelectSolutionExplorerItem(projectItem))
				return false;

			return true;
		}

		/// <summary>
		/// Collapses all projects in Solution Explorer.
		/// </summary>
		public void CollapseAllProjects()
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			_log.LogMessage("Collapse all projects - enter", LOG_CATEGORY);

			System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
			try
			{
				var dte = GetDTEInternal();
				var solution = dte.Solution;
				var solutionName = GetSolutionName(solution);

				_log.LogMessage($"Collapse all projects - solution {solutionName}", LOG_CATEGORY);

				var solutionWindow = dte.Windows.Item(EnvDTE.Constants.vsWindowKindSolutionExplorer);
				if (solutionWindow == null)
				{
					_log.LogMessage("Collapse all projects - failed to retrieve Solution Explorer window", LOG_CATEGORY);
					return;
				}

				solutionWindow.Activate();

				var hierarchy = dte.ActiveWindow.Object as UIHierarchy;
				if (hierarchy != null)
				{
					var solutionItem = hierarchy.GetItem(solutionName);
					UIHierarchyItems solutionItems;
					if (solutionItem != null)
						solutionItems = solutionItem.UIHierarchyItems;
					else
						solutionItems = null;

					if ((solutionItems != null) && (solutionItems.Count > 0))
					{
						foreach (UIHierarchyItem item in solutionItems)
						{
							CollapseAllRecursively(hierarchy, item);
						}

						// Select top item
						solutionItems.Item(1).Select(vsUISelectionType.vsUISelectionTypeSelect);
					}
					else
					{
						_log.LogMessage("Collapse all projects - no solution items detected", LOG_CATEGORY);
					}
				}
			}
			finally
			{
				System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
			}

			_log.LogMessage("Collapse all projects - exit", LOG_CATEGORY);
		}

		/// <summary>
		/// Opens a given file name editor navigating to line and column.
		/// </summary>
		public bool OpenFileAt(string fileName, int line, int column)
		{
			if (string.IsNullOrEmpty(fileName))
				throw new ArgumentNullException(nameof(fileName));

			if ((line <= 0) || (column <= 0))
				return false;

			ThreadHelper.ThrowIfNotOnUIThread();

			var textView = GetTextView(fileName);
			if (textView == null)
				return false;

			if (textView.SetCaretPos(line - 1, column - 1) != VSConstants.S_OK) // text view coordinates are 0-based
				return false;

			textView.CenterLines(line - 1, 1);
			return true;
		}

		/// <summary>
		/// Opens given file editors.
		/// </summary>
		public void OpenFiles(IEnumerable<IExtensibilityItem> files)
		{
			if ((files == null) || (files.Count() == 0))
				return;

			ThreadHelper.ThrowIfNotOnUIThread();

			if (files.Count() > 1)
			{
				files.ForEach(f => ActivateItem(f, false, false));
				ActivateWindow(files.First(), false);
			}
			else
			{
				ActivateItem(files.First(), true, false);
			}
		}

		/// <summary>
		/// Opens given file designers.
		/// </summary>
		public void OpenDesignerFiles(IEnumerable<IExtensibilityItem> files)
		{
			if ((files == null) || (files.Count() == 0))
				return;

			ThreadHelper.ThrowIfNotOnUIThread();

			if (files.Count() > 1)
			{
				files.ForEach(f => ActivateItem(f, false, true));
				ActivateWindow(files.First(), true);
			}
			else
			{
				ActivateItem(files.First(), true, true);
			}
		}

		/// <summary>
		/// Assigns package shortcuts.
		/// </summary>
		public bool AssignShortcuts()
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var oldCursor = Mouse.OverrideCursor;
			Mouse.SetCursor(Cursors.Wait);
			try
			{
				var dte = GetDTEInternal();

				var scope = GetGlobalScopeName(dte);
				if (string.IsNullOrEmpty(scope))
				{
					_log.LogMessage("Failed to retrieve global scope name", LOG_CATEGORY);
					return false;
				}

				var fields = typeof(CommandIDs).GetFields(BindingFlags.Public | BindingFlags.Static).ToList();
				var attribs = new List<CommandNameAttribute>(fields.Count);
				fields.ForEach(f => attribs.Add((CommandNameAttribute)f.GetCustomAttribute(typeof(CommandNameAttribute))));

				foreach (var attrib in attribs)
				{
					if (!string.IsNullOrEmpty(attrib.Binding))
					{
						try
						{
							var command = dte.Commands.Item(attrib.Name, 1);
							if (command != null)
								command.Bindings = $"{scope}::{attrib.Binding}";
							else
								_log.LogMessage($"Command {attrib.Name} is not available", LOG_CATEGORY);
						}
						catch (Exception ex)
						{
							_log.LogMessage($"Error assigning shortcut for {attrib.Name} command and {attrib.Binding} binding", ex, LOG_CATEGORY);
						}
					}
				}

				// Process all VS commands
				var shell = _serviceProvider.GetService<IVsUIShell5, SVsUIShell>();
				var scopes = new Dictionary<string, string>();

				foreach (var attrib in CommandBindings.Commands)
				{
					try
					{
						var command = dte.Commands.Item(attrib.Name, 1);
						if (command != null)
						{
							var binding = attrib.Binding;

							// Resolve optional scope binding
							if (string.IsNullOrEmpty(attrib.Scope))
							{
								binding = $"{scope}::{binding}";
							}
							else
							{
								var resolvedScope = string.Empty;

								if (scopes.ContainsKey(attrib.Scope))
									resolvedScope = scopes[attrib.Scope];
								else
								{
									var bindingGuide = new Guid(attrib.Scope);
									resolvedScope = shell.GetKeyBindingScope(ref bindingGuide);
									if (!string.IsNullOrEmpty(resolvedScope))
									{
										scopes.Add(attrib.Scope, resolvedScope);
										_log.LogMessage($"VS command {attrib.Name} - resolved {attrib.Scope} scope to {resolvedScope}", LOG_CATEGORY);
									}
								}

								if (string.IsNullOrEmpty(resolvedScope))
								{
									_log.LogMessage($"VS command {attrib.Name} - could not resolve {attrib.Scope} scope", LOG_CATEGORY);
									continue;
								}

								binding = $"{resolvedScope}::{binding}";
							}

							command.Bindings = binding;
						}
						else
						{
							_log.LogMessage($"VS command {attrib.Name} is not available", LOG_CATEGORY);
						}
					}
					catch (Exception ex)
					{
						_log.LogMessage($"Error assigning shortcut for VS {attrib.Name} command and {attrib.Binding} binding with {attrib.Scope} scope", ex, LOG_CATEGORY);
					}
				}

				_log.LogMessage("Assigned shortcuts", LOG_CATEGORY);
				return true;
			}
			catch (Exception ex)
			{
				_log.LogMessage($"Error assigning shortcuts: {ex.Message}", ex, LOG_CATEGORY);
				return false;
			}
			finally
			{
				Mouse.SetCursor(oldCursor);
			}
		}

		/// <summary>
		/// Shows options setting page.
		/// </summary>
		public void ShowOptions<T>() where T : OptionsBase
		{
			var package = _serviceProvider.GetService<VSPackage>();
			package.ShowOptionPage(typeof(T));
		}

		/// <summary>
		/// Executes built-in command.
		/// </summary>
		/// <param name="command">Internal command name.</param>
		/// <param name="arguments">Optional command arguments.</param>
		public void ExecuteCommand(string command, string arguments = null)
		{
			if (string.IsNullOrEmpty(command))
				throw new ArgumentNullException(nameof(command));

			ThreadHelper.ThrowIfNotOnUIThread();

			var dte = GetDTEInternal();
			try
			{
				dte.ExecuteCommand(command);
			}
			catch (Exception ex)
			{
				_log.LogMessage($"Error executing {command} command: {ex.Message}", ex, LOG_CATEGORY);
			}
		}

		#endregion

		#region IShellStatusBarService Members

		/// <summary>
		/// Sets status bar text.
		/// </summary>
		public void SetStatusBarText(string text)
		{
			if (string.IsNullOrEmpty(text))
				return;

			ThreadHelper.ThrowIfNotOnUIThread();

			GetDTEInternal().StatusBar.Text = text;
		}

		#endregion

		#region IShellReferenceService Members

		/// <summary>
		/// Returns current project references.
		/// </summary>
		public ICollection<ProjectReference> GetProjectReferences(bool selectedOnly)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			if (!GetSelectionProject(out _, out VSProject vsProject, out _, out SelectedItems selectedItems))
				return null;

			List<string> selectedNames = null;
			if (selectedOnly)
			{
				selectedNames = new List<string>(selectedItems.Count);
				for (var index = 1; index <= selectedItems.Count; index++)
				{
					selectedNames.Add(selectedItems.Item(index).Name);
				}
			}

			var references = new List<ProjectReference>();

			for (var index = 1; index <= vsProject.References.Count; index++)
			{
				var reference = vsProject.References.Item(index);

				if (reference.Type == prjReferenceType.prjReferenceTypeAssembly)
				{
					if ((reference.Name == null) || reference.Name.Equals("mscorlib", StringComparison.OrdinalIgnoreCase))
						continue;

					if ((selectedNames == null) || (selectedNames.Contains(reference.Name)))
					{
						if (!string.IsNullOrEmpty(reference.Path))
						{
							var projectReference = new ProjectReference { Name = reference.Name, Path = reference.Path };
							if (reference.SourceProject != null)
								projectReference.ReferencingProjectName = reference.SourceProject.UniqueName;
							references.Add(projectReference);
						}
					}
				}
			}

			return references;
		}

		/// <summary>
		/// Adds project reference to current project.
		/// </summary>
		public bool AddProjectReference(string projectName)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			if (!GetSelectionProject(out Project project, out _, out IVsSolution solution, out _))
				return false;

			var result = solution.GetProjectOfUniqueName(projectName, out IVsHierarchy referenceHierarchy);
			if ((result == VSConstants.S_OK) && (referenceHierarchy != null))
			{
				var referenceProject = GetProjectFromHierarchy(referenceHierarchy);
				if ((referenceProject != null) && (referenceProject != project) && !IsAlreadyReferenced(project, referenceProject.Name))
				{
					try
					{
						var vsProj = project?.Object as VSProject;
						return (vsProj != null) && (vsProj.References.AddProject(referenceProject) != null);
					}
					catch (Exception ex)
					{
						_log.LogMessage($"Error referencing project: {projectName}", ex, LOG_CATEGORY);

						return false;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Adds assembly references to current project.
		/// </summary>
		public bool AddAssemblyReference(string fileName)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			if (!GetSelectionProject(out Project project, out _, out _, out _))
				return false;

			if (IsAlreadyReferenced(project, fileName))
				return true;

			try
			{
				var vsProj = project?.Object as VSProject;
				return (vsProj != null) && (vsProj.References.Add(fileName) != null);
			}
			catch (Exception ex)
			{
				_log.LogMessage($"Error referencing project assembly: {fileName}", ex, LOG_CATEGORY);
				throw;
			}
		}

		#endregion

		#region IShellSelectionService Members

		/// <summary>
		/// Checks whether selection context is active.
		/// </summary>
		public bool IsContextActive(ContextType context)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			switch (context)
			{
				case ContextType.SolutionExistsAndFullyLoaded:
					return IsContextActiveInternal(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_guid);
				case ContextType.SolutionHasProjects:
					return IsContextActiveInternal(VSConstants.UICONTEXT_SolutionExists) && (
						IsContextActiveInternal(VSConstants.UICONTEXT_SolutionHasSingleProject) ||
						IsContextActiveInternal(VSConstants.UICONTEXT_SolutionHasMultipleProjects));
				case ContextType.SolutionExists:
					return IsContextActiveInternal(VSConstants.UICONTEXT_SolutionExists);
				case ContextType.CodeWindow:
					return IsContextActiveInternal(VSConstants.UICONTEXT_CodeWindow);
				case ContextType.TextEditor:
					return IsContextActiveInternal(new Guid(GUIDs.vsContextGuidTextEditor));
				case ContextType.XMLTextEditor:
					return IsContextActiveInternal(new Guid(GUIDs.vsContextGuidXMLTextEditor));
				case ContextType.CSSTextEditor:
					return IsContextActiveInternal(new Guid(GUIDs.vsContextGuidCSSTextEditor));
				case ContextType.HTMLCodeView:
					return IsContextActiveInternal(new Guid(GUIDs.vsContextGuidHTMLCodeView));
				case ContextType.HTMLSourceView:
					return IsContextActiveInternal(new Guid(GUIDs.vsContextGuidHTMLSourceView));
				case ContextType.HTMLSourceEditor:
					return IsContextActiveInternal(new Guid(GUIDs.vsContextGuidHTMLSourceEditor));
				case ContextType.XamlEditor:
					return IsContextActiveInternal(new Guid(GUIDs.GUID_XAML_LANGUAGE_SERVICE));
				case ContextType.NewXamlEditor:
					return IsContextActiveInternal(new Guid(GUIDs.GUID_NEW_XAML_LANGUAGE_SERVICE));
				case ContextType.XamlDesigner:
					return IsContextActiveInternal(new Guid(GUIDs.GUID_XAML_DESIGNER));
				default:
					_log.LogMessage($"Invalid selection context: {context}", LOG_CATEGORY);
					return false;
			}
		}

		/// <summary>
		/// Returns active document's untyped Project instance.
		/// </summary>
		public object GetActiveProject()
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			Document document = GetActiveDocument() as Document;
			if (document == null)
				return null;

			return document.ProjectItem?.ContainingProject;
		}

		/// <summary>
		/// Returns active document's untyped ProjectItem instance.
		/// </summary>
		public object GetActiveItem()
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			Document document = GetActiveDocument() as Document;
			if (document == null)
				return null;

			return document.ProjectItem;
		}

		/// <summary>
		/// Returns active document's untyped Document instance.
		/// </summary>
		public object GetActiveDocument()
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			try
			{
				var document = GetDTEInternal().ActiveDocument;
				return document;
			}
			catch
			{
				// Exception's raised when project properties document is active
				return null;
			}
		}

		/// <summary>
		/// Returns active document's untyped Window instance.
		/// </summary>
		public object GetActiveWindow()
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var dteDocument = GetActiveDocument() as Document;
			if (dteDocument == null)
				return null;

			try
			{
				var window = dteDocument.ActiveWindow;
				return window;
			}
			catch
			{
				// Exception's raised when project properties document is active
				return null;
			}
		}

		/// <summary>
		/// Returns active document's file name.
		/// </summary>
		public string GetActiveFileName()
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var document = GetActiveDocument() as Document;
			if (document != null)
				return document.FullName;
			else
				return null;
		}

		/// <summary>
		/// Returns active document's cursor position.
		/// </summary>
		public Position GetActiveFilePosition()
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var textManager = _serviceProvider.GetService<IVsTextManager, SVsTextManager>();
			if (textManager.GetActiveView(Convert.ToInt32(true), null, out IVsTextView textView) != VSConstants.S_OK)
				return Position.Empty;

			if (textView.GetCaretPos(out int line, out int column) != VSConstants.S_OK)
				return Position.Empty;

			return new Position(line + 1, column + 1); // text view coordinates are 0-based
		}

		/// <summary>
		/// Sets active document's cursor position.
		/// </summary>
		public bool SetActiveFilePosition(int line, int column)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			if ((line <= 0) || (column <= 0))
				return false;

			var textManager = _serviceProvider.GetService<IVsTextManager, SVsTextManager>();
			if (textManager.GetActiveView(Convert.ToInt32(true), null, out IVsTextView textView) != VSConstants.S_OK)
				return false;

			if (textView.SetCaretPos(line - 1, column - 1) != VSConstants.S_OK) // text view coordinates are 0-based
				return false;

			textView.CenterLines(line - 1, 1);
			return true;
		}

		#endregion

		#region IShellProjectService Members

		/// <summary>
		/// Checks whether untyped Project instance is a qualified project.
		/// </summary>
		public bool IsProject(object project)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var dteProject = GetProjectInternal(project, false);

			return (dteProject != null) &&
				(dteProject.Kind != EnvDTE.Constants.vsProjectKindSolutionItems) &&
				(dteProject.Kind != EnvDTE.Constants.vsProjectKindUnmodeled) &&
				(dteProject.Kind != EnvDTE.Constants.vsProjectKindMisc);
		}

		/// <summary>
		/// Checks whether untyped Project instance is a qualified classic ASP.NET web project.
		/// </summary>
		public bool IsWebProject(object project)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			return _webInteropAvailable && IsWebProjectInternal(GetProjectInternal(project));
		}

		/// <summary>
		/// Checks whether untyped ProjectItem instance is a qualified classic ASP.NET web project item.
		/// </summary>
		public bool IsWebProjectItem(object projectItem)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			return _webInteropAvailable && IsWebProjectItemInternal(GetProjectItemInternal(projectItem));
		}

		/// <summary>
		/// Checks whether untyped Project instance project load's deferred, ie project hasn't been fully loaded yet.
		/// </summary>
		public bool IsProjectLoadDeferred(object project, out bool loaded)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			loaded = false;

			if (project == null)
				return false;

			var dteProject = GetProjectInternal(project);

			// Deffered load is not applicable to solution projects
			if (dteProject.Kind.Equals(EnvDTE.Constants.vsProjectKindSolutionItems, StringComparison.OrdinalIgnoreCase))
				return false;

			var serviceProvider = new ServiceProvider(GetDTEInternal() as Microsoft.VisualStudio.OLE.Interop.IServiceProvider);
			if (serviceProvider == null)
				return false;

			var solution = serviceProvider.GetService<IVsSolution, SVsSolution>(false);
			if ((solution == null) || !(solution is IVsSolution4))
				return false;

			if ((solution.GetProperty((int)__VSPROPID7.VSPROPID_DeferredProjectCount, out object deferredCount) != VSConstants.S_OK) ||
					!(deferredCount is int) || ((int)deferredCount == 0))
				return false;

			IVsHierarchy hierarchy = GetVsHierarchy(dteProject);
			if (hierarchy == null)
				return false;

			if ((hierarchy.GetProperty((uint)VSConstants.VSITEMID.Root, (int)__VSHPROPID9.VSHPROPID_IsDeferred, out object deferred) != VSConstants.S_OK) ||
					!(deferred is bool) || !(bool)deferred)
				return false;

			_log.LogMessage($"Project '{dteProject?.Name}' load is deferred", LOG_CATEGORY);

			if (hierarchy.GetGuidProperty((uint)VSConstants.VSITEMID.Root, (int)__VSHPROPID.VSHPROPID_ProjectIDGuid, out Guid projectGuid) != VSConstants.S_OK)
				return false;

			int result = ((IVsSolution4)solution).EnsureProjectIsLoaded(projectGuid, (uint)__VSBSLFLAGS.VSBSLFLAGS_None);
			if (result == VSConstants.S_OK)
			{
				loaded = true;
				_log.LogMessage($"Deferred project '{dteProject?.Name}' is loaded", LOG_CATEGORY);
			}
			else
			{
				_log.LogMessage($"Failed to load deferred project '{dteProject?.Name}' - error {result}", LOG_CATEGORY);
				return true;
			}

			return false;
		}

		/// <summary>
		/// Checks whether untyped ProjectItem instance is a qualified C++ project item.
		/// </summary>
		public bool IsCppFile(object projectItem)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			if (_cppInteropAvailable)
				return IsCppFileInternal(projectItem);
			else
				return false;
		}

		/// <summary>
		/// Returns untyped Project instance project name.
		/// </summary>
		public string GetFriendlyProjectName(object project)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var dteProject = GetProjectInternal(project);
			string result;
			try
			{
				result = dteProject.Name;
			}
			catch
			{
				return string.Empty;
			}

			if (!string.IsNullOrEmpty(result) && IsWebProject(dteProject))
			{
				webType webSiteType;
				try
				{
					Properties props = dteProject.Properties;
					if (props == null)
						return result;

					Property prop = props.Item("WebSiteType");
					if (prop == null)
						return result;
					else
						webSiteType = (webType)prop.Value;
				}
				catch
				{
					return result;
				}

				switch (webSiteType)
				{
					case webType.webTypeLocalhost:
					case webType.webTypeFrontPage:
					case webType.webTypeFTP:
						Uri uri;
						if (Uri.TryCreate(result, UriKind.RelativeOrAbsolute, out uri))
						{
							if ((uri != null) && uri.IsAbsoluteUri)
							{
								result = uri.AbsolutePath;
								if ((result.Length >= 2) &&
										result.StartsWith("/", StringComparison.OrdinalIgnoreCase) &&
										result.EndsWith("/", StringComparison.OrdinalIgnoreCase))
									result = result.Substring(1, result.Length - 2); // strip leading and trailing '/'
								else
								if (result.StartsWith("/", StringComparison.OrdinalIgnoreCase))
									result = result.Substring(1, result.Length - 1); // strip leading '/'
								else
								if (result.EndsWith("/", StringComparison.OrdinalIgnoreCase))
									result = result.Substring(0, result.Length - 1); // strip trailing '/'
							}
						}
						break;
					case webType.webTypeFileSystem:
						if (result.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.OrdinalIgnoreCase))
							result = result.Substring(0, result.Length - 1);
						result = Path.GetFileName(result);
						break;
				}
			}

			return result;
		}

		/// <summary>
		/// Returns untyped Project instance project path and full name.
		/// </summary>
		public void GetProjectPath(object project, out string projectPath, out string projectFullName)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var dteProject = GetProjectInternal(project);
			var projectName = GetFriendlyProjectName(project);

			if (string.IsNullOrEmpty(projectName))
			{
				projectPath = string.Empty;
				projectFullName = string.Empty;
			}
			else
			{
				// Don't even attempt getting full name on projects that don't support extensibility
				// code model. Those include DB projects, Script debug time projects, etc.
				if (dteProject.Kind != EnvDTE.Constants.vsProjectKindUnmodeled)
				{
					try
					{
						projectFullName = dteProject.FullName;
					}
					catch
					{
						projectFullName = string.Empty;
					}
				}
				else
					projectFullName = string.Empty;

				// Safety check for projects like setups, etc...
				if (!string.IsNullOrEmpty(projectFullName))
				{
					var fileName1 = Path.GetFileName(projectFullName);
					string fileName2;
					try
					{
						fileName2 = Path.GetFileName(dteProject.UniqueName);
					}
					catch
					{
						fileName2 = fileName1;
					}
					if (string.Compare(fileName1, fileName2, StringComparison.OrdinalIgnoreCase) != 0)
						projectFullName = Path.Combine(Path.GetDirectoryName(projectFullName), fileName2);
				}

				try
				{
					if (dteProject.Kind.Equals(EnvDTE.Constants.vsProjectKindSolutionItems, StringComparison.OrdinalIgnoreCase))
					{
						projectFullName = dteProject.Name;
						projectPath = string.Empty;
					}
					else if (string.IsNullOrEmpty(projectFullName))
						projectPath = string.Empty;
					else
						projectPath = Path.GetDirectoryName(projectFullName);
				}
				catch
				{
					projectPath = string.Empty;
				}
			}
		}

		/// <summary>
		/// Checks whether untyped Project instance has valid code model.
		/// </summary>
		public bool IsCodeModelProject(object project)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var dteProject = GetProjectInternal(project);

			if (IsWebProject(dteProject))
				return true;
			else if (IsKnownNoCodeModelProject(dteProject))
				return false;
			else
				return !string.IsNullOrEmpty(GetProjectLanguage(dteProject, DEFAULT_WAIT_MSECS));
		}

		/// <summary>
		/// Returns untyped Project instance project code model language.
		/// </summary>
		public string GetProjectLanguage(object project)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var dteProject = GetProjectInternal(project);

			// C++ project check, which may not have its code model fully loaded
			if (IsCppProject(dteProject))
			{
				var languageSet = _languageService.GetLanguage(LanguageType.CPP);
				if (languageSet?.Type != LanguageType.Unknown)
					return languageSet.Language;
				else
					return string.Empty;
			}

			EnvDTE.CodeModel codeModel;
			try
			{
				if (IsKnownNoCodeModelProject(dteProject))
					return null;

				codeModel = dteProject.CodeModel;
			}
			catch (Exception ex)
			{
				if (ex is NotImplementedException) // This is an acceptable condition
				{
					_log.LogMessage($"Project '{dteProject.Name}' doesn't implement code model", LOG_CATEGORY);
					return string.Empty;
				}

				_log.LogMessage($"Project '{dteProject.Name}' code model is not yet available", LOG_CATEGORY);

				// Project could still be loading
				System.Threading.Thread.Sleep(DEFAULT_WAIT_MSECS);

				codeModel = null;
			}

			if ((codeModel == null) && IsWebProject(dteProject))
			{
				_log.LogMessage($"Loading web project: {dteProject.Name}", LOG_CATEGORY);

				try
				{
					WebProjectLoad(dteProject);

					codeModel = dteProject.CodeModel;
				}
				catch (Exception ex)
				{
					_log.LogMessage($"Project '{dteProject.Name}' code model is not available", ex, LOG_CATEGORY);
					return string.Empty;
				}

				if (codeModel == null)
					_log.LogMessage($"Project '{dteProject.Name}' code model is not available", LOG_CATEGORY);
			}

			if ((codeModel != null) && (codeModel.Language != null))
				return codeModel.Language;
			else
				return string.Empty;
		}

		/// <summary>
		/// Returns untyped Project instance project language Guid for projects with no code model.
		/// </summary>
		public string GetNoCodeModelProjectLanguage(object project)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var dteProject = GetProjectInternal(project);

			// Solution items project has no code model
			if (dteProject.Kind.Equals(EnvDTE.Constants.vsProjectKindSolutionItems, StringComparison.OrdinalIgnoreCase))
				return _languageService.GetLanguage(LanguageType.SolutionItems)?.Language;

			if (IsKnownNoCodeModelProject(dteProject))
				return string.Empty;

			string webLanguage;
			try
			{
				var props = dteProject.Properties;
				if (props == null)
					return string.Empty;

				var languageProp = props.Item("CurrentWebsiteLanguage");
				if (languageProp == null)
					webLanguage = string.Empty;
				else
					webLanguage = (string)languageProp.Value;
			}
			catch
			{
				return string.Empty;
			}

			LanguageSettings languageSet;
			if (!string.IsNullOrEmpty(webLanguage))
				languageSet = _languageService.GetWebNameLanguage(webLanguage);
			else
				languageSet = LanguageSettings.UnknownLanguage;

			// Fallback scenario - use Kind guid as language
			if (languageSet.Type == LanguageType.Unknown)
			{
				try
				{
					webLanguage = dteProject.Kind;
				}
				catch
				{
					webLanguage = null;
				}

				if (!string.IsNullOrEmpty(webLanguage))
					languageSet = _languageService.GetLanguage(webLanguage);
			}

			if (languageSet.Type != LanguageType.Unknown)
				return languageSet.Language;
			else
				return string.Empty;
		}

		/// <summary>
		/// Returns untyped Project instance from untyped IVsHierarchy instance.
		/// </summary>
		public object GetHierarchyProject(object hierarchy)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			return GetProjectFromHierarchy(hierarchy as IVsHierarchy);
		}

		/// <summary>
		/// Returns untyped FileCodeModel instance for untyped ProjectItem instance.
		/// </summary>
		public object GetFileCodeModel(object projectItem)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var dteItem = projectItem as ProjectItem;
			if (dteItem == null)
				return null;

			EnvDTE.FileCodeModel result;
			try
			{
				_log.LogMessage($"Collecting item code model: {dteItem.Name}", LOG_CATEGORY);

				result = dteItem.FileCodeModel;
			}
			catch (Exception ex)
			{
				// File/directory missing means that file is not available
				if ((ex is DirectoryNotFoundException) || (ex is FileNotFoundException))
				{
					_log.LogMessage($"File is not on disk: {dteItem.Name}", LOG_CATEGORY);
					result = null;
				}
				else
				{
					_log.LogMessage($"File code model access error: {dteItem.Name}", LOG_CATEGORY);
					throw;
				}
			}

			if ((result == null) && IsWebProjectItem(projectItem))
			{
				_log.LogMessage($"Loading web item: {dteItem.Name}", LOG_CATEGORY);

				try
				{
					var webItemLoaded = WebItemLoad(dteItem);

					result = dteItem.FileCodeModel;

					if (webItemLoaded)
						WebItemUnload(dteItem);
				}
				catch (Exception ex)
				{
					_log.LogMessage($"File '{dteItem.Name}' code model is not available", ex, LOG_CATEGORY);
					result = null;
				}
			}

			_log.LogMessage($"Collected item code model: {dteItem.Name}", LOG_CATEGORY);
			return result;
		}

		/// <summary>
		/// Returns untyped Project or ProjectItem instance BuildAction property value.
		/// </summary>
		public bool CompileBuildAction(object projectItemOrProject)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			try
			{
				Properties props;
				if (projectItemOrProject is ProjectItem)
					props = ((ProjectItem)projectItemOrProject).Properties;
				else if (projectItemOrProject is Project)
					props = ((Project)projectItemOrProject).Properties;
				else
					return false;

				var buildActionProp = props.Item("BuildAction");
				if (buildActionProp == null)
					return true;

				var value = buildActionProp.Value;
				return (value is int) && ((int)value == (int)prjBuildAction.prjBuildActionCompile);
			}
			catch (Exception ex)
			{
				_log.LogMessage("Error retrieving build action", ex, LOG_CATEGORY);
				return false;
			}
		}

		/// <summary>
		/// Returns untyped Project instance for untyped Document instance.
		/// </summary>
		public object GetDocumentProject(object document)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var dteDocument = document as Document;
			if (document != null)
			{
				try
				{
					return dteDocument.ProjectItem?.ContainingProject;
				}
				catch
				{
					// Do nothing
				}
			}

			return null;
		}

		/// <summary>
		/// Returns untyped ProjectItem instance for untyped Document instance.
		/// </summary>
		public object GetDocumentProjectItem(object document)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var dteDocument = document as Document;
			if (document != null)
			{
				try
				{
					return dteDocument.ProjectItem;
				}
				catch
				{
					// Do nothing
				}
			}

			return null;
		}

		/// <summary>
		/// Returns untyped Project file name.
		/// </summary>
		public string GetItemFileName(object projectItem)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var dteItem = GetProjectItemInternal(projectItem, false);
			if (dteItem == null)
				return null;

			if (dteItem.FileCount == 0)
				return null;

			return dteItem.FileNames[1];
		}

		/// <summary>
		/// Returns item's SubType.
		/// </summary>
		/// <param name="item">Project item.</param>
		/// <returns>SubType or null.</returns>
		public string GetProjectItemSubType(object projectItem)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var dteItem = GetProjectItemInternal(projectItem, false);
			if (dteItem == null)
				return null;

			var project = dteItem.ContainingProject;
			if (project == null)
				return null;

			var fileName = GetItemFileName(dteItem);
			if (string.IsNullOrEmpty(fileName))
				return null;

			var solution = _serviceProvider.GetService<IVsSolution, IVsSolution>();
			if (solution.GetProjectOfUniqueName(project.UniqueName, out IVsHierarchy projectHierarchy) != VSConstants.S_OK)
			{
				_log.LogMessage($"Failed to retrieve '{project.UniqueName}' project hierarchy", LOG_CATEGORY);
				return null;
			}

			if (projectHierarchy.ParseCanonicalName(fileName, out uint itemId) != VSConstants.S_OK)
			{
				_log.LogMessage($"Failed to parse '{project.UniqueName}' project name", LOG_CATEGORY);
				return null;
			}

			if (projectHierarchy.GetProperty(itemId, (int)__VSHPROPID.VSHPROPID_ItemSubType, out object value) != VSConstants.S_OK)
			{
				_log.LogMessage($"'{project.UniqueName}' project {itemId} item sub-type is not available", LOG_CATEGORY);
				return null;
			}

			if (!(value is string))
			{
				_log.LogMessage($"'{project.UniqueName}' project {itemId} item sub-type is invalid: {value}", LOG_CATEGORY);
				return null;
			}

			return (string)value;
		}

		#endregion

		#region IShellCodeModelService Members

		/// <summary>
		/// Returns whether untyped CodeElement instance is constant member.
		/// </summary>
		public bool IsConstant(object element)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var dteElement = element as CodeElement;
			if (dteElement == null)
				throw new ArgumentNullException(nameof(dteElement));

			switch (dteElement.Kind)
			{
				case vsCMElement.vsCMElementVariable:
					return ((CodeVariable)dteElement).IsConstant;
				default:
					return false;
			}
		}

		/// <summary>
		/// Returns whether untyped CodeElement instance is static member.
		/// </summary>
		public bool IsStatic(object element)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var dteElement = element as CodeElement;
			if (dteElement == null)
				throw new ArgumentNullException(nameof(dteElement));

			switch (dteElement.Kind)
			{
				case vsCMElement.vsCMElementVariable:
					return ((CodeVariable)dteElement).IsShared;
				case vsCMElement.vsCMElementProperty:
					var result = false;
					if (dteElement is CodeProperty2)
					{
						result = ((CodeProperty2)dteElement).IsShared;
					}
					else
					{
						var prop = (CodeProperty)dteElement;
						var accessor = prop.Getter;
						if (accessor == null)
							accessor = prop.Setter;
						if (accessor != null)
							result = accessor.IsShared;
					}
					return result;
				case vsCMElement.vsCMElementFunction:
					return ((CodeFunction)dteElement).IsShared;
				case vsCMElement.vsCMElementEvent:
					return ((CodeEvent)dteElement).IsShared;
				case vsCMElement.vsCMElementClass:
				case vsCMElement.vsCMElementModule:
					if (dteElement is CodeClass2)
						return ((CodeClass2)dteElement).IsShared;
					else
						return false;
				default:
					return false;
			}
		}

		/// <summary>
		/// Returns untyped CodeElement generic member suffix.
		/// </summary>
		public string GetGenericsSuffix(object element)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var dteElement = element as CodeElement;
			if (dteElement == null)
				throw new ArgumentNullException(nameof(element));

			var genericElement = false;
			switch (dteElement.Kind)
			{
				case vsCMElement.vsCMElementClass:
					if (dteElement is CodeClass2)
					{
						CodeClass2 cls = (CodeClass2)dteElement;
						if (cls.IsGeneric)
							genericElement = true;
					}
					break;
				case vsCMElement.vsCMElementInterface:
					if (dteElement is CodeInterface2)
					{
						CodeInterface2 intf = (CodeInterface2)dteElement;
						if (intf.IsGeneric)
							genericElement = true;
					}
					break;
				case vsCMElement.vsCMElementStruct:
					if (dteElement is CodeStruct2)
					{
						CodeStruct2 str = (CodeStruct2)dteElement;
						if (str.IsGeneric)
							genericElement = true;
					}
					break;
				case vsCMElement.vsCMElementDelegate:
					if (dteElement is CodeDelegate2)
					{
						CodeDelegate2 del = (CodeDelegate2)dteElement;
						if (del.IsGeneric)
							genericElement = true;
					}
					break;
			}

			var suffix = string.Empty;
			if (genericElement)
			{
				var name = dteElement.Name;
				var fullName = dteElement.FullName;
				if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(fullName) && !fullName.EndsWith(name))
				{
					var index = fullName.LastIndexOf(name);
					if (index >= 0)
						suffix = fullName.Substring(index + name.Length);
				}
			}

			return suffix;
		}

		/// <summary>
		/// Returns untyped CodeElement instance member modifier.
		/// </summary>
		public Modifier GetElementModifier(object element)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var dteElement = element as CodeElement;
			if (dteElement == null)
				throw new ArgumentNullException(nameof(element));

			var access = vsCMAccess.vsCMAccessDefault;

			switch (dteElement.Kind)
			{
				case vsCMElement.vsCMElementVariable:
					if (dteElement is CodeVariable)
						access = ((CodeVariable)dteElement).Access;
					break;
				case vsCMElement.vsCMElementProperty:
					if (dteElement is CodeProperty)
						access = ((CodeProperty)dteElement).Access;
					break;
				case vsCMElement.vsCMElementFunction:
					if (dteElement is CodeFunction)
						access = ((CodeFunction)dteElement).Access;
					break;
				case vsCMElement.vsCMElementEvent:
					if (dteElement is CodeEvent)
						access = ((CodeEvent)dteElement).Access;
					break;
				case vsCMElement.vsCMElementDelegate:
					if (dteElement is CodeDelegate)
						access = ((CodeDelegate)dteElement).Access;
					break;
				case vsCMElement.vsCMElementEnum:
					if (dteElement is CodeEnum)
						access = ((CodeEnum)dteElement).Access;
					break;
				case vsCMElement.vsCMElementClass:
				case vsCMElement.vsCMElementModule:
					if (dteElement is CodeClass)
						access = ((CodeClass)dteElement).Access;
					break;
				case vsCMElement.vsCMElementInterface:
					if (dteElement is CodeInterface)
						access = ((CodeInterface)dteElement).Access;
					break;
				case vsCMElement.vsCMElementStruct:
					if (dteElement is CodeStruct)
						access = ((CodeStruct)dteElement).Access;
					break;
				default:
					access = vsCMAccess.vsCMAccessPublic;
					break;
			}

			switch (access)
			{
				case vsCMAccess.vsCMAccessPublic:
					return Modifier.Public;
				case vsCMAccess.vsCMAccessProject:
					return Modifier.Internal;
				case vsCMAccess.vsCMAccessProtected:
					return Modifier.Protected;
				case vsCMAccess.vsCMAccessProjectOrProtected:
				case vsCMAccess.vsCMAccessAssemblyOrFamily:
					return Modifier.ProtectedInternal;
				case vsCMAccess.vsCMAccessPrivate:
					return Modifier.Private;
				default:
					return Modifier.Any;
			}
		}

		/// <summary>
		/// Returns untyped CodeElement instance member kind.
		/// </summary>
		public Kind GetElementKind(object element)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var dteElement = element as CodeElement;
			if (dteElement == null)
				throw new ArgumentNullException(nameof(element));

			switch (dteElement.Kind)
			{
				case vsCMElement.vsCMElementClass:
					return Kind.Class;
				case vsCMElement.vsCMElementModule:
					return Kind.Module;
				case vsCMElement.vsCMElementStruct:
					return Kind.Struct;
				case vsCMElement.vsCMElementEnum:
					return Kind.Enum;
				case vsCMElement.vsCMElementInterface:
					return Kind.Interface;
				case vsCMElement.vsCMElementFunction:
					return Kind.Method;
				case vsCMElement.vsCMElementProperty:
					return Kind.Property;
				case vsCMElement.vsCMElementVariable:
					return Kind.Variable;
				case vsCMElement.vsCMElementEvent:
					return Kind.Event;
				case vsCMElement.vsCMElementDelegate:
					return Kind.Delegate;
				default:
					return Kind.Unknown;
			}
		}

		/// <summary>
		/// Returns untyped CodeElement instance member name.
		/// </summary>
		public string GetElementKindName(object element)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			switch (GetElementKind(element))
			{
				case Kind.Class:
					return KIND_CLASS;
				case Kind.Module:
					return KIND_MODULE;
				case Kind.Struct:
					return KIND_STRUCT;
				case Kind.Enum:
					return KIND_ENUM;
				case Kind.Interface:
					return KIND_INTERFACE;
				case Kind.Method:
					return KIND_METHOD;
				case Kind.Property:
					return KIND_PROPERTY;
				case Kind.Variable:
					return KIND_VARIABLE;
				case Kind.Event:
					return KIND_EVENT;
				case Kind.Delegate:
					return KIND_DELEGATE;
				default:
					var dteElement = element as CodeElement;
					var kind = dteElement.Kind.ToString();
					var length = kind.IndexOf(ELEMENT_TYPE) >= 0 ? ELEMENT_TYPE.Length : 0;
					return kind.Substring(length++, kind.Length - (length - 1));
			}
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Returns DTE instance.
		/// </summary>
		private DTE GetDTEInternal()
		{
			if (_dte != null)
				return _dte;

			ThreadHelper.ThrowIfNotOnUIThread();

			_dte = _serviceProvider.GetService<DTE>(false);
			return _dte;
		}

		private IFileTypeResolver GetFileTypeResolver()
		{
			if (_fileTypeResolver != null)
				return _fileTypeResolver;

			// Cannot use DI constructor because of circular service references
			_fileTypeResolver = _serviceProvider.GetService<IFileTypeResolver>();
			return _fileTypeResolver;
		}

		/// <summary>
		/// Web Interop JITer test - when not available on it'd throw an error stepping into this method.
		/// </summary>
		private void TestWebInterop()
		{
			object dummy = new object();
			if (dummy is VSWebSite)
				return;
		}

		/// <summary>
		/// C++ Interop JITer test - when not available on it'd throw an error stepping into this method.
		/// </summary>
		private void TestCPPInterop()
		{
			object dummy = new object();
			if (dummy is VCProject)
				return;
		}

		/// <summary>
		/// Indicates whether given project is web based, i.e. web forms or web service.
		/// </summary>
		private bool IsWebProjectInternal(Project project)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			return (project?.Object != null) && (project.Object is VSWebSite);
		}

		/// <summary>
		/// Indicates whether given project item is web based.
		/// </summary>
		private bool IsWebProjectItemInternal(ProjectItem item)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			return (item?.Object != null) && (item.Object is VSWebProjectItem);
		}

		private bool WebItemLoad(ProjectItem item)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			if (item?.Object is VSWebProjectItem)
			{
				VSWebProjectItem webItem = (VSWebProjectItem)item.Object;
				webItem.Load();
				webItem.WaitUntilReady();
				return true;
			}

			return false;
		}

		/// <summary>
		/// Unloads web project item.
		/// </summary>
		private bool WebItemUnload(ProjectItem item)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			if (item?.Object is VSWebProjectItem)
			{
				VSWebProjectItem webItem = (VSWebProjectItem)item.Object;
				webItem.Unload();
				return true;
			}

			return false;
		}

		/// <summary>
		/// Returns solution name.
		/// </summary>
		private string GetSolutionName(Solution solution)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var solutionName = solution.Properties.Item("Name").Value as string;
			return solutionName;
		}

		/// <summary>
		/// Converts instance to Project.
		/// </summary>
		private Project GetProjectInternal(object project, bool throwOnError = true)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var dteProject = project as Project;
			if (dteProject == null)
			{
				if (throwOnError)
					throw new ArgumentNullException(nameof(project));
				else
					return null;
			}

			return dteProject;
		}

		/// <summary>
		/// Converts instance to ProjectItem.
		/// </summary>
		private ProjectItem GetProjectItemInternal(object item, bool throwOnError = true)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var dteItem = item as ProjectItem;
			if (dteItem == null)
			{
				if (throwOnError)
					throw new ArgumentNullException(nameof(item));
				else
					return null;
			}

			return dteItem;
		}

		/// <summary>
		/// Retrieves project reference (and other references) from the currently selected project items.
		/// </summary>
		private bool GetSelectionProject(out Project project, out VSProject vsProject,
			out IVsSolution solution, out SelectedItems selectedItems)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			project = null;
			vsProject = null;
			solution = _serviceProvider.GetService<IVsSolution>();
			selectedItems = GetDTEInternal().SelectedItems;

			if (solution == null)
			{
				_log.LogMessage("Failed to retrieve solution service", LOG_CATEGORY);
				return false;
			}

			if ((selectedItems == null) || (selectedItems.Count == 0))
			{
				_log.LogMessage("Selection is empty", LOG_CATEGORY);
				return false;
			}

			var hierarchy = GetSelectionHierarchy();
			if (hierarchy == null)
			{
				_log.LogMessage("Failed to retrieve selection", LOG_CATEGORY);
				return false;
			}

			project = GetProjectFromHierarchy(hierarchy);
			if (project == null)
			{
				_log.LogMessage("Failed to retrieve selected project", LOG_CATEGORY);
				return false;
			}

			vsProject = project.Object as VSProject;
			return ((project != null) && (vsProject != null) && (solution != null));
		}

		/// <summary>
		/// Returns selection's hierarchy object.
		/// </summary>
		private IVsHierarchy GetSelectionHierarchy()
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var monitorSelection = _serviceProvider.GetService<IVsMonitorSelection>();

			var result = monitorSelection.GetCurrentSelection(out IntPtr pHeir, out _, out _, out _);
			if ((result != VSConstants.S_OK) || (pHeir == IntPtr.Zero))
				return null;

			var hierarchy = Marshal.GetObjectForIUnknown(pHeir) as IVsHierarchy;
			if (pHeir != IntPtr.Zero)
				Marshal.Release(pHeir);

			return hierarchy;
		}

		/// <summary>
		/// Returns project instance for a given hierarchy or null if it's not a qualified hierarchy.
		/// </summary>
		private Project GetProjectFromHierarchy(IVsHierarchy hierarchy)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			if (hierarchy != null)
			{
				var result = hierarchy.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out object project);
				if ((result == VSConstants.S_OK) && (project != null) && (project is Project))
					return (Project)project;
			}

			return null;
		}

		/// <summary>
		/// Returns project hierarchy for a given project reference.
		/// </summary>
		private IVsHierarchy GetVsHierarchy(Project project)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			if (project != null)
			{
				var serviceProvider = new ServiceProvider(project.DTE as Microsoft.VisualStudio.OLE.Interop.IServiceProvider);
				if (serviceProvider != null)
				{
					var solution = serviceProvider.GetService<IVsSolution>(false);
					if (solution != null)
					{
						solution.GetProjectOfUniqueName(project.UniqueName, out IVsHierarchy hierarchy);
						return hierarchy;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Returns project instance for a given project reference.
		/// </summary>
		private IVsProject GetVsProject(Project project)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			return GetVsHierarchy(project) as IVsProject;
		}

		/// <summary>
		/// Checks if given assembly name is already referenced by the project.
		/// </summary>
		private bool IsAlreadyReferenced(Project project, string assemblyName)
		{
			if (_webInteropAvailable && IsAlreadyReferencedWeb(project, assemblyName))
				return true;

			var vsProject = project?.Object as VSProject;
			if (vsProject != null)
			{
				for (int index = 1; index <= vsProject.References.Count; index++)
				{
					var referenceName = vsProject.References.Item(index).Name;
					if (string.Compare(referenceName, assemblyName, StringComparison.OrdinalIgnoreCase) == 0)
						return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Checks if given assembly name is already referenced by the web project.
		/// </summary>
		private bool IsAlreadyReferencedWeb(Project project, string assemblyName)
		{
			var vsWeb = project?.Object as VSWebSite;
			if (vsWeb != null)
			{
				for (int index = 1; index <= vsWeb.References.Count; index++)
				{
					string referenceName = vsWeb.References.Item(index).Name;
					if (string.Compare(referenceName, assemblyName, StringComparison.OrdinalIgnoreCase) == 0)
						return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Returns project item from a given document.
		/// </summary>
		private ProjectItem GetDocumentProjectItem(Document document)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			if (document != null)
			{
				try
				{
					return document.ProjectItem;
				}
				catch
				{
					// Do nothing
				}
			}

			return null;
		}

		/// <summary>
		/// Selects a project item in Solution Explorer.
		/// </summary>
		private bool SelectSolutionExplorerItem(ProjectItem item)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			if (item == null)
				return false;

			var uiShell = _serviceProvider.GetService<IVsUIShell, SVsUIShell>();

			var solutionExplorerGuid = new Guid(EnvDTE.Constants.vsWindowKindSolutionExplorer);
			var result = uiShell.FindToolWindow(0, ref solutionExplorerGuid, out IVsWindowFrame solutionExplorerFrame);
			if ((result != VSConstants.S_OK) || (solutionExplorerFrame == null))
			{
				_log.LogMessage("Failed to retrieve Solution Explorer toolwindow", LOG_CATEGORY);
				return false;
			}

			result = solutionExplorerFrame.Show();
			if (result != VSConstants.S_OK)
				return false;

			result = solutionExplorerFrame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out object obj);
			if (result != VSConstants.S_OK)
			{
				_log.LogMessage("Failed to retrieve Solution Explorer hierarchy window", LOG_CATEGORY);
				return false;
			}

			var solutionExplorerWindow = obj as IVsUIHierarchyWindow;
			if (solutionExplorerWindow == null)
				return false;

			var project = GetVsProject(item.ContainingProject);
			if (project == null)
				return false;

			var pdwPriority = new VSDOCUMENTPRIORITY[] { VSDOCUMENTPRIORITY.DP_Standard };
			result = project.IsDocumentInProject(item.get_FileNames(1), out int found, pdwPriority, out uint itemId);
			if ((result == VSConstants.S_OK) && (found == Convert.ToUInt32(true)))
			{
				var uiHierarchy = project as IVsUIHierarchy;
				if (uiHierarchy != null)
				{
					result = solutionExplorerWindow.ExpandItem(uiHierarchy, itemId, EXPANDFLAGS.EXPF_SelectItem);

					_log.LogMessage($"Selected '{item.Name}' item: result - {result}", LOG_CATEGORY);

					if (result == VSConstants.S_OK)
						return true;
				}
			}
			else
			{
				_log.LogMessage("File must be a part of currently opened solution", LOG_CATEGORY);
			}

			return false;
		}

		/// <summary>
		/// Collapses hierarchy recursively.
		/// </summary>
		private void CollapseAllRecursively(UIHierarchy hierarchy, UIHierarchyItem parentItem)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var parentItems = parentItem.UIHierarchyItems;
			if ((parentItems == null) || (parentItems.Count == 0))
				return;

			var canRecurse = true;

			if (parentItem.Object is Project)
			{
				var dteProject = GetProjectInternal(parentItem.Object, false);
				if (dteProject != null)
				{
					try
					{
						if ((dteProject.Kind != EnvDTE.Constants.vsProjectKindSolutionItems) &&
								!IsCodeModelProject(dteProject))
							canRecurse = false;
					}
					catch (NotImplementedException)
					{
						canRecurse = false;
					}

					if (!canRecurse)
					{
						string projectName;
						try
						{
							// Item name retrieval raises an exception on unavailable project items
							projectName = dteProject.Name;
						}
						catch
						{
							projectName = null;
						}

						if (projectName != null)
							_log.LogMessage($"Collapse all projects - project {projectName} won't be processed any further", LOG_CATEGORY);
						else
							_log.LogMessage("Collapse all projects - project won't be processed any further", LOG_CATEGORY);
					}
				}
			} // project check

			if (canRecurse)
			{
				foreach (UIHierarchyItem item in parentItems)
				{
					UIHierarchyItems items;
					try
					{
						items = item.UIHierarchyItems;
					}
					catch
					{
						string itemName;
						try
						{
							// Item name retrieval raises an exception on unavailable project items
							itemName = item.Name;
						}
						catch
						{
							itemName = null;
						}

						if (itemName != null)
							_log.LogMessage($"Collapse all projects - error processing {itemName} item", LOG_CATEGORY);
						else
							_log.LogMessage("Collapse all projects - error processing item", LOG_CATEGORY);

						throw;
					}

					if ((items != null) && (items.Count > 0))
					{
						CollapseAllRecursively(hierarchy, item);

						// Collapse sub-item
						items.Expanded = false;

						// Workaround for solution folders collapse bug
						if (items.Expanded)
						{
							item.Select(vsUISelectionType.vsUISelectionTypeSelect);
							hierarchy.DoDefaultAction();
						}
					}
				} // foreach
			} // if (canRecurse)

			// Collapse parent item
			if (parentItems.Count > 0)
				parentItems.Expanded = false;
		}

		/// <summary>
		/// Returns true if project doesn't implement or support code model.
		/// </summary>
		private bool IsKnownNoCodeModelProject(Project project)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			if ((!IsWebProject(project)) && (project.Kind == PROJECT_ITEM_KIND_SETUP))
				return true;
			else
				return false;
		}

		/// <summary>
		/// Retrieves project's code model language. For web items it loads the site,
		/// which causes the code model to get setup. Returns null if code model is not supported.
		/// </summary>
		private string GetProjectLanguage(Project project, int waitMSecs)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			// C++ project check, which may not have its code model fully loaded
			if (IsCppProject(project))
			{
				var languageSet = _languageService.GetLanguage(LanguageType.CPP);
				if (languageSet.Type != LanguageType.Unknown)
					return languageSet.Language;
				else
					return string.Empty;
			}

			EnvDTE.CodeModel codeModel;
			try
			{
				if (IsKnownNoCodeModelProject(project))
					return null;

				codeModel = project.CodeModel;
			}
			catch (Exception ex)
			{
				if (ex is NotImplementedException) // This is an acceptable condition
				{
					_log.LogMessage("Project doesn't implement code model", LOG_CATEGORY);
					return string.Empty;
				}

				_log.LogMessage($"Project '{project.Name}' code model is not yet available.", LOG_CATEGORY);

				// Project could still be loading
				if (waitMSecs > 0)
					System.Threading.Thread.Sleep(waitMSecs);

				codeModel = null;
			}

			if ((codeModel == null) && IsWebProject(project))
			{
				_log.LogMessage($"Loading web project: {project.Name}", LOG_CATEGORY);

				try
				{
					WebProjectLoad(project);

					codeModel = project.CodeModel;
				}
				catch (Exception ex)
				{
					_log.LogMessage($"Project '{project.Name}' code model is not available.", ex, LOG_CATEGORY);
					return string.Empty;
				}

				if (codeModel == null)
					_log.LogMessage($"Project '{project.Name}' code model is not available.", LOG_CATEGORY);
			}

			if ((codeModel != null) && (codeModel.Language != null))
				return codeModel.Language;
			else
				return string.Empty;
		}

		/// <summary>
		/// Loads web project code model.
		/// </summary>
		private void WebProjectLoad(Project project)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			if (_webInteropAvailable)
				WebProjectLoadInternal(project);
		}

		/// <summary>
		/// Loads web project code model.
		/// </summary>
		private void WebProjectLoadInternal(Project project)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			if ((project?.Object != null) && (project.Object is VSWebSite))
			{
				// Try to load web site
				((VSWebSite)project.Object).WaitUntilReady();
			}
		}

		/// <summary>
		/// Checks whether given project is a C++ one.
		/// </summary>
		private bool IsCppProject(Project project)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			if (_cppInteropAvailable)
				return IsCppProjectInternal(project);
			else
				return false;
		}

		/// <summary>
		/// Checks whether given project is a C++ one.
		/// </summary>
		private bool IsCppProjectInternal(Project project)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			return (project != null) && (project.Object is VCProject);
		}

		private static bool IsCppFileInternal(object item)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var dteItem = item as ProjectItem;
			return (dteItem != null) && (dteItem.Object is VCFile);
		}

		/// <summary>
		/// Returns true if a given shell context is active.
		/// </summary>
		private bool IsContextActiveInternal(Guid contextGuid)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var monitorSelection = _serviceProvider.GetService<IVsMonitorSelection, SVsShellMonitorSelection>();
			if (monitorSelection.GetCmdUIContextCookie(ref contextGuid, out uint contextCookie) != VSConstants.S_OK)
				return false;

			monitorSelection.IsCmdUIContextActive(contextCookie, out int active);
			return Convert.ToBoolean(active);
		}

		/// <summary>
		/// Opens item's editor either in primary or designer view.
		/// </summary>
		private void ActivateItem(IExtensibilityItem item, bool activate, bool alternativeView)
		{
			if (item == null)
				return;

			ThreadHelper.ThrowIfNotOnUIThread();

			var dteItem = GetProjectItemInternal(item.ProjectItem);
			if (dteItem == null)
				return;

			bool isActivated = false;

			if (dteItem.Document != null)
			{
				var windows = dteItem.Document.Windows;

				for (int index = 1; index <= windows.Count; index++)
				{
					var window = windows.Item(index);

					if (alternativeView)
					{
						if (GetFileTypeResolver().IsWinDesignSubType(item.ItemSubType) && (window.Object is IDesignerHost))
						{
							if (activate || !window.Visible)
								window.Activate();

							isActivated = true;
							break;
						}
					}
					else if (!(window.Object is IDesignerHost))
					{
						if (activate || !window.Visible)
							window.Activate();

						isActivated = true;
						break;
					}
				}
			}

			if (!isActivated)
				ActivateWindow(item, alternativeView);
		}

		/// <summary>
		/// Activates item's primary or designer view.
		/// </summary>
		private void ActivateWindow(IExtensibilityItem item, bool alternativeView)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var dteItem = GetProjectItemInternal(item.ProjectItem);
			if (dteItem == null)
				return;

			Window window;

			try
			{
				if (dteItem.Kind.Equals(EnvDTE.Constants.vsProjectItemKindSolutionItems, StringComparison.OrdinalIgnoreCase))
				{
					window = GetDTEInternal().OpenFile(EnvDTE.Constants.vsViewKindPrimary, dteItem.FileNames[1]);
				}
				else if (alternativeView &&
								(GetFileTypeResolver().IsWinDesignSubType(item.ItemSubType) || GetFileTypeResolver().IsDesignAndCodeSubType(item.ParentSubType)))
				{
					window = dteItem.Open(EnvDTE.Constants.vsViewKindDesigner);
				}
				else
				{
					if (GetFileTypeResolver().IsCodeSubType(item.ItemSubType))
					{
						window = dteItem.Open(EnvDTE.Constants.vsViewKindCode);
					}
					else
					{
						if (alternativeView)
						{
							if (GetFileTypeResolver().IsDesignAndCodeSubType(item.ItemSubType))
								window = dteItem.Open(EnvDTE.Constants.vsViewKindTextView);
							else
								window = dteItem.Open(EnvDTE.Constants.vsViewKindPrimary);
						}
						else
						{
							window = dteItem.Open(EnvDTE.Constants.vsViewKindPrimary);
						}
					}
				}
			}
			catch (Exception ex)
			{
				_log.LogMessage($"File '{item.Name}' is not found", ex, LOG_CATEGORY);
				return;
			}

			if (dteItem.Document != null)
				dteItem.Document.Activate();

			ActivateWindow(window, alternativeView);
		}

		private void ActivateWindow(Window window, bool designView)
		{
			if (window == null)
				return;

			ThreadHelper.ThrowIfNotOnUIThread();

			window.Activate();

			// Don't try to get the text document for this window *if* one is requested
			if (!designView && (window.Object != null) && (!(window.Object is IDesignerHost)) && (window.Document != null))
			{
				var textDocument = window.Document.Object(TEXT_DOCUMENT) as TextDocument;
				if (textDocument != null)
				{
					var startPoint = textDocument.StartPoint;
					if (startPoint != null)
					{
						try
						{
							startPoint.TryToShow(vsPaneShowHow.vsPaneShowTop, null);
						}
						catch
						{
							// TS database projects thrown an error here on .sql files
							window.Document.Activate();
						}
					}
				}
			}
		}

		/// <summary>
		/// Returns file's text view.
		/// </summary>
		private IVsTextView GetTextView(string fileName)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var viewGuid = VSConstants.LOGVIEWID_Primary;
			var openDocument = _serviceProvider.GetService<IVsUIShellOpenDocument, SVsUIShellOpenDocument>();
			if (openDocument.OpenDocumentViaProject(fileName, ref viewGuid, out _, out _, out _, out IVsWindowFrame windowFrame) != VSConstants.S_OK)
				return null;

			if (windowFrame.Show() != VSConstants.S_OK)
				return null;

			var textView = VsShellUtilities.GetTextView(windowFrame);
			return textView;
		}

		/// <summary>
		/// Use a well know command in the Global scope to resolve scope's localized name.
		/// </summary>
		private string GetGlobalScopeName(DTE dte)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var scopeCommand = dte.Commands.Item("File.Exit", 1);
			if (scopeCommand == null)
			{
				_log.LogMessage("Failed to retrieve scope command", LOG_CATEGORY);
				return null;
			}

			var scopes = (object[])scopeCommand.Bindings;
			if ((scopes == null) || (scopes.Length == 0))
			{
				_log.LogMessage("Scope command has no bindings", LOG_CATEGORY);
				return null;
			}

			var rawScope = scopes[0]?.ToString();
			if (rawScope == null)
			{
				_log.LogMessage("Scope command binding cannot be determined", LOG_CATEGORY);
				return null;
			}

			var scope = rawScope.Substring(0, rawScope.IndexOf("::"));
			return scope;
		}

		#endregion
	}
}