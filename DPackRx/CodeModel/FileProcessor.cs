using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml.XPath;

using DPackRx.Language;
using DPackRx.Services;

using EnvDTE;
using EnvDTE80;

using Microsoft.VisualStudio.Shell;

namespace DPackRx.CodeModel
{
	/// <summary>
	/// File code model processor.
	/// </summary>
	public class FileProcessor : IFileProcessor
	{
		#region Fields

		private readonly ILog _log;
		private readonly ILanguageService _languageService;
		private readonly IShellSelectionService _shellSelectionService;
		private readonly IShellProjectService _shellProjectService;
		private readonly IShellCodeModelService _shellCodeModelService;
		private readonly IFileTypeResolver _fileTypeResolver;

		private const string UNNAMED_NAME = "<Unnamed>";
		private const string DESTRUCTOR_PREFIX = "~";
		private const string DOC_TAG = "doc";

		private readonly CodeGenericsPair[] _codePairs = new[] { new CodeGenericsPair('<', '>'), new CodeGenericsPair('(', ')') };

		#endregion

		#region CodeGenericsPair class

		private class CodeGenericsPair
		{
			public CodeGenericsPair(char begin, char end)
			{
				this.Being = begin;
				this.End = end;
			}

			public char Being { get; private set; }
			public char End { get; private set; }
		}

		#endregion

		public FileProcessor(ILog log, ILanguageService languageService, IShellSelectionService shellSelectionService,
			IShellProjectService shellProjectService, IShellCodeModelService shellCodeModelService, IFileTypeResolver fileTypeResolver)
		{
			_log = log;
			_languageService = languageService;
			_shellSelectionService = shellSelectionService;
			_shellProjectService = shellProjectService;
			_shellCodeModelService = shellCodeModelService;
			_fileTypeResolver = fileTypeResolver;
		}

		#region IFileProcessor Members

		/// <summary>
		/// Returns whether document is valid for code model collection.
		/// </summary>
		/// <param name="document">Open document or null for active document. Untyped extensibility link (name matches the actual type).</param>
		/// <param name="projectItem">Item associated with the document. Untyped extensibility link (name matches the actual type).</param>
		/// <returns>Code model collection status.</returns>
		public bool IsDocumentValid(object document, out object projectItem)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var doc = document as Document;
			if (document == null)
				doc = _shellSelectionService.GetActiveDocument() as Document;

			var dteItem = doc != null ? doc.ProjectItem : GetSelectedProjectItem(null);
			var languageSet = _fileTypeResolver.GetCurrentLanguage(dteItem?.ContainingProject, out bool isWebProject);
			var fileName = string.Empty;

			// Xaml specific logic - use code behind file instead of designer one
			if ((dteItem != null) && _fileTypeResolver.IsXamlItem(dteItem, languageSet, isWebProject))
			{
				var items = dteItem.ProjectItems;
				if (items?.Count > 0)
				{
					foreach (ProjectItem i in items)
					{
						if (string.Compare(i.Kind, Constants.vsProjectItemKindPhysicalFile, StringComparison.OrdinalIgnoreCase) == 0)
						{
							var itemFileName = i.get_FileNames(1);
							if (!string.IsNullOrEmpty(itemFileName))
							{
								var itemLanguageSet = _languageService.GetExtensionLanguage(Path.GetExtension(itemFileName));
								if (itemLanguageSet?.Type != LanguageType.Unknown)
								{
									dteItem = i;
									break;
								}
							}
						}
					}
				}
			}

			var itemIsJavaScript = false;
			var itemIsXml = false;

			// Validate the selected file
			if (dteItem != null)
			{
				var project = dteItem.ContainingProject;
				if (project != null)
				{
					var webCodeItem = false;

					var activeWindow = _shellSelectionService.GetActiveWindow() as Window;
					if (activeWindow?.Object is HTMLWindow)
					{
						itemIsJavaScript = _fileTypeResolver.IsJavaScriptItem(dteItem, languageSet, isWebProject);
						if (!itemIsJavaScript)
						{
							// Check for web code item, like generic handler for instance
							var itemSubType = _fileTypeResolver.GetSubType(dteItem, languageSet, isWebProject);
							webCodeItem = _fileTypeResolver.IsWebCodeOnlySubType(itemSubType, languageSet, isWebProject);
							if (!webCodeItem)
								dteItem = null;
						}
					}

					if (dteItem != null)
					{
						try
						{
							fileName = dteItem.get_FileNames(1);
							if (!webCodeItem)
								languageSet = _languageService.GetExtensionLanguage(Path.GetExtension(fileName));

							if (!itemIsJavaScript)
								itemIsJavaScript = languageSet?.Type == LanguageType.JavaScript;
							if (!itemIsJavaScript)
								itemIsXml = languageSet?.Type == LanguageType.Xml;
						}
						catch
						{
							dteItem = null;
						}
					}
				}
			}

			if (!string.IsNullOrEmpty(fileName))
				_log.LogMessage($"Collecting file code model: {fileName}");

			if ((languageSet?.Type == LanguageType.Unknown) || string.IsNullOrEmpty(fileName))
				dteItem = null;

			if ((dteItem != null) && !itemIsJavaScript && !_fileTypeResolver.IsCodeItem(dteItem, languageSet, isWebProject))
				dteItem = null;

			projectItem = dteItem;

			if (dteItem != null)
			{
				if (!itemIsJavaScript && !itemIsXml)
				{
					var itemFileCodeModel = _shellProjectService.GetFileCodeModel(dteItem);
					if (itemFileCodeModel == null)
						return false;
				}

				return true;
			}

			return false;
		}

		/// <summary>
		/// Returns current document code members.
		/// </summary>
		/// <param name="flags">Processor flags.</param>
		/// <param name="filter">Code model filter.</param>
		/// <returns>Code members.</returns>
		public FileCodeModel GetMembers(ProcessorFlags flags, CodeModelFilterFlags filter = CodeModelFilterFlags.All)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var item = _shellSelectionService.GetActiveItem();
			if (item == null)
				return new FileCodeModel();

			return GetMembers(item, flags, filter);
		}

		/// <summary>
		/// Returns item code members.
		/// </summary>
		/// <param name="projectItem">Item. Untyped extensibility link (name matches the actual type).</param>
		/// <param name="flags">Processor flags.</param>
		/// <param name="filter">Code model filter.</param>
		/// <returns>Code members.</returns>
		public FileCodeModel GetMembers(object projectItem, ProcessorFlags flags, CodeModelFilterFlags filter = CodeModelFilterFlags.All)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var model = new List<MemberCodeModel>(10);
			GetMembersInternal(projectItem, model, flags, filter);
			return new FileCodeModel { FileName = _shellProjectService.GetItemFileName(projectItem), Members = model };
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Returns project item code members.
		/// </summary>
		private void GetMembersInternal(object projectItem, List<MemberCodeModel> model, ProcessorFlags flags, CodeModelFilterFlags filter = CodeModelFilterFlags.All)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var dteItem = projectItem as ProjectItem;
			if (dteItem == null)
				throw new ArgumentNullException(nameof(projectItem));

			_log.LogMessage($"Collecting code members for '{dteItem.Name}'");

			var languageSet = _fileTypeResolver.GetCurrentLanguage(dteItem.ContainingProject, out _);

			CodeElements elements;
			try
			{
				elements = (_shellProjectService.GetFileCodeModel(dteItem) as EnvDTE.FileCodeModel)?.CodeElements;
			}
			catch (Exception ex)
			{
				_log.LogMessage($"Error collecting code members for '{dteItem.Name}': {ex.Message}", ex);
				throw;
			}

			if (elements == null)
			{
				_log.LogMessage($"Code member information is not available for '{dteItem.Name}'");
				return;
			}

			try
			{
				var dteFilter = FilterToDTEElementKind(filter);

				foreach (CodeElement element in elements)
				{
					ProcessCodeElement(dteItem, element, model, flags, languageSet, dteFilter, filter, false);
				}
			}
			catch (Exception ex)
			{
				_log.LogMessage("Error refreshing code model for '{dteItem.Name}'", ex);
				return;
			}

			_log.LogMessage($"Collected {model.Count} '{dteItem.Name}' code members");
		}

		/// <summary>
		/// Recursively processes qualified code elements.
		/// </summary>
		private void ProcessCodeElement(ProjectItem item, CodeElement element, List<MemberCodeModel> model, ProcessorFlags flags,
			LanguageSettings languageSet, ICollection<vsCMElement> dteFilter, CodeModelFilterFlags filter, bool parented)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			if (element.Kind == vsCMElement.vsCMElementNamespace)
			{
				var members = ((CodeNamespace)element).Members;

				foreach (CodeElement member in members)
				{
					ProcessCodeElement(item, member, model, flags, languageSet, dteFilter, filter, false);
				}
			}
			else
			if (element.IsCodeType || ((languageSet != null) && languageSet.IgnoreCodeType))
			{
				// Add Class, Interface, Struct, Enum and Delegate name
				// as 1st level element entry w/o a '.' prefix
				if ((element.Kind == vsCMElement.vsCMElementClass) ||
						(element.Kind == vsCMElement.vsCMElementModule) ||
						(element.Kind == vsCMElement.vsCMElementInterface) ||
						(element.Kind == vsCMElement.vsCMElementStruct) ||
						(element.Kind == vsCMElement.vsCMElementEnum) ||
						(element.Kind == vsCMElement.vsCMElementDelegate))
				{
					AddCodeElement(item, null, element, model, flags, languageSet, dteFilter, filter);
				}

				// Add members that don't reside in the parented member, 
				// like class or interface for instance
				if (!parented && (
						(element.Kind == vsCMElement.vsCMElementFunction) ||
						(element.Kind == vsCMElement.vsCMElementProperty) ||
						(element.Kind == vsCMElement.vsCMElementVariable)))
				{
					AddCodeElement(item, null, element, model, flags, languageSet, dteFilter, filter);
				}

				// Don't expand on Enum or Delegate since we don't wanna list its members
				if (element.IsCodeType &&
						(element.Kind != vsCMElement.vsCMElementEnum) &&
						(element.Kind != vsCMElement.vsCMElementDelegate))
				{
					var members = ((CodeType)element).Members;
					if (members == null)
						throw new Exception($"Members information is not available: {element.Name}");

					foreach (CodeElement member in members)
					{
						// Don't add nested Class, Interface or Struct name
						// It'll be added as part of this method recursive call
						if ((member.Kind != vsCMElement.vsCMElementClass) &&
								(member.Kind != vsCMElement.vsCMElementModule) &&
								(member.Kind != vsCMElement.vsCMElementInterface) &&
								(member.Kind != vsCMElement.vsCMElementStruct))
						{
							AddCodeElement(item, element, member, model, flags, languageSet, dteFilter, filter);
						}

						// Don't expand on Enum or Delegate since we don't wanna list its members
						if ((member.Kind != vsCMElement.vsCMElementEnum) &&
								(member.Kind != vsCMElement.vsCMElementDelegate))
						{
							ProcessCodeElement(item, member, model, flags, languageSet, dteFilter, filter, true);
						}
					}
				}
			}
		}

		/// <summary>
		/// Creates code model element.
		/// </summary>
		private void AddCodeElement(ProjectItem item, CodeElement parentElement, CodeElement element, List<MemberCodeModel> model, ProcessorFlags flags,
			LanguageSettings languageSet, ICollection<vsCMElement> dteFilter, CodeModelFilterFlags filter)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			EditPoint editPoint;

			// Line number retrieval uses EditPoints, which is expensive to process
			try
			{
				// Not all elements support header part
				TextPoint startPoint;
				if ((element.Kind == vsCMElement.vsCMElementClass) ||
						(element.Kind == vsCMElement.vsCMElementModule) ||
						(element.Kind == vsCMElement.vsCMElementInterface) ||
						(element.Kind == vsCMElement.vsCMElementStruct) ||
						(element.Kind == vsCMElement.vsCMElementEnum) ||
						(element.Kind == vsCMElement.vsCMElementFunction))
					startPoint = element.GetStartPoint(vsCMPart.vsCMPartHeader);
				else
					startPoint = element.StartPoint;

				if (startPoint == null)
				{
					_log.LogMessage($"{element.Name} element's of {element.Kind} kind StartPoint cannot be determined");
					return;
				}

				// Check if this code model element belongs to the current project item
				if (element.ProjectItem != null)
				{
					var elementItem = element.ProjectItem;
					if ((item != null) && (item != elementItem))
					{
						string itemFileName = item.get_FileNames(1);
						string epiFileName = elementItem.get_FileNames(1);

						if (string.IsNullOrEmpty(itemFileName) ||
								string.IsNullOrEmpty(epiFileName) ||
								(string.Compare(itemFileName, epiFileName, StringComparison.OrdinalIgnoreCase) != 0))
						{
							_log.LogMessage($"Project item mismatch: '{item.Name}' expected but '{elementItem.Name}' found instead - {element.Name}");
							return;
						}
						else
						{
							// This is a work around for project item reference change where essentially new reference 
							// still points at the same file but reference wise these two are no longer equal
							item = elementItem;
						}
					}
				}

				editPoint = startPoint.CreateEditPoint();
			}
			catch (Exception ex)
			{
				// Swallow C++ exception as some invalid code model functions raise it sometimes
				if (languageSet?.Language != CodeModelLanguageConstants.vsCMLanguageVC)
					_log.LogMessage($"Error adding code member: {element.Name}", ex);

				return;
			}

			var line = editPoint.Line;

			// Decide whether the element is to be filtered out or not
			bool add;
			if (dteFilter?.Count == 0)
			{
				add = true;
			}
			else
			{
				add = dteFilter.Contains(element.Kind);
				if (!add)
				{
					// TODO: resurrect next statement if need be - used to treat read-only fields as properties
					//if (dteFilter.Contains(vsCMElement.vsCMElementProperty) && (element.Kind == vsCMElement.vsCMElementVariable))
					//{
					//	var varElt = (CodeVariable2)element;
					//	if (varElt.ConstKind == vsCMConstKind.vsCMConstKindReadOnly)
					//		add = true;
					//}
				}
			}

			if (add)
			{
				GetConstructorDestructorInfo(languageSet, parentElement, element, out bool constructor, out bool destructor);

				if ((filter != CodeModelFilterFlags.Constructors) ||
						(filter.HasFlag(CodeModelFilterFlags.Constructors) && (constructor || destructor)))
				{
					var name = element.Name;
					string parentFullName;
					var fullName = flags.HasFlag(ProcessorFlags.IncludeMemeberFullName) ? element.FullName : string.Empty;
					var code = string.Empty;

					// Setup element's full name information
					if (string.IsNullOrEmpty(name))
						name = UNNAMED_NAME;
					if (parentElement == null)
					{
						if (!flags.HasFlag(ProcessorFlags.IncludeMemeberFullName) && (languageSet != null) && languageSet.ParentlessFullName)
							parentFullName = element.FullName;
						else
							parentFullName = name;
					}
					else
					if ((parentElement.Kind == vsCMElement.vsCMElementClass) ||
							(parentElement.Kind == vsCMElement.vsCMElementModule) ||
							(parentElement.Kind == vsCMElement.vsCMElementInterface) ||
							(parentElement.Kind == vsCMElement.vsCMElementStruct))
						parentFullName = parentElement.Name + "." + name;
					else
						parentFullName = name;

					// Check for duplicate names
					if ((languageSet != null) && languageSet.CheckDuplicateNames)
					{
						foreach (var modelItem in model)
						{
							if ((modelItem.Line == line) &&
									(modelItem.CodeModelElementKind == (int)element.Kind) &&
									(modelItem.ParentFullName == parentFullName))
							{
								// Duplicate found
								add = false;
								break;
							}
						}
					}

					if (add)
					{
						// Setup element's first code line information
						if (editPoint != null)
						{
							int lineLength = editPoint.LineLength - editPoint.LineCharOffset + 1;
							if (lineLength > 1)
								code = editPoint.GetText(lineLength).TrimStart(' ', '\t');
						}

						try
						{
							var returnTypeName = constructor || destructor || (element == null) ? string.Empty : GetElementReturnTypeName(element);
							var xmlDoc = flags.HasFlag(ProcessorFlags.IncludeMemberXmlDoc) ? GetXmlDoc(element, languageSet) : string.Empty;

							char? genericSuffix = null;
							if ((languageSet != null) && languageSet.SupportsGenerics)
							{
								var suffix = _shellCodeModelService.GetGenericsSuffix(element);

								if (!string.IsNullOrEmpty(suffix))
								{
									name = name + suffix;
									parentFullName = parentFullName + suffix;
									genericSuffix = suffix[0];
								}
							}

							var member = new MemberCodeModel
							{
								ProjectItem = element.ProjectItem,
								Name = name,
								FullName = fullName,
								ParentFullName = parentFullName,
								CodeModelElementKind = (int)element.Kind,
								ElementKind = _shellCodeModelService.GetElementKind(element),
								ElementModifier = _shellCodeModelService.GetElementModifier(element),
								IsConstant = _shellCodeModelService.IsConstant(element),
								IsStatic = _shellCodeModelService.IsStatic(element),
								SupportsGenerics = languageSet.SupportsGenerics,
								GenericsSuffix = _shellCodeModelService.GetGenericsSuffix(element),
								Line = line,
								Code = code,
								ReturnTypeName = returnTypeName,
								XmlDoc = xmlDoc
							};

							model.Add(member);
						}
						catch (COMException ex)
						{
							_log.LogMessage($"Ignored COM code model error for {name}", ex);
						}
					} // if (add)
				} // if (memberFilter)
			} // if (add)
		}

		/// <summary>
		/// Returns code item in the first selected project.
		/// </summary>
		private ProjectItem GetSelectedProjectItem(Project currentProject)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var dte = _shellProjectService.GetDTE() as DTE;

			// Get the first file from the first active project
			var projects = dte.ActiveSolutionProjects as Array;
			if (projects?.Length > 0)
			{
				var project = projects.GetValue(0) as Project;

				if (project?.ProjectItems != null)
					return GetFirstQualifiedItem(project, currentProject);
			}

			return null;
		}

		/// <summary>
		/// Finds first code item in the project.
		/// </summary>
		private ProjectItem GetFirstQualifiedItem(Project project, Project currentProject)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			if (project != currentProject)
			{
				// We used to switch to project's language here... this doesn't appear to be necessary anymore
			}

			var languageSet = _fileTypeResolver.GetCurrentLanguage(project, out bool isWebProject);
			if (languageSet?.Type == LanguageType.Unknown)
				return null;

			var items = project.ProjectItems;
			if (items == null)
				return null;

			// Get the first file from the first active project
			foreach (ProjectItem item in items)
			{
				if ((string.Compare(item.Kind, Constants.vsProjectItemKindPhysicalFolder, StringComparison.OrdinalIgnoreCase) == 0) ||
						(string.Compare(item.Kind, Constants.vsProjectItemKindVirtualFolder, StringComparison.OrdinalIgnoreCase) == 0))
				{
					if (item.ProjectItems != null)
					{
						foreach (ProjectItem subItem in item.ProjectItems)
						{
							if (_fileTypeResolver.IsCodeItem(subItem, languageSet, isWebProject) || _fileTypeResolver.IsXamlItem(item, languageSet, isWebProject))
								return subItem;
						}
					}
				}
				else if (_fileTypeResolver.IsCodeItem(item, languageSet, isWebProject) || _fileTypeResolver.IsXamlItem(item, languageSet, isWebProject))
				{
					return item;
				}
			}

			return null;
		}

		/// <summary>
		/// Returns constructor and destructor information for the code elements.
		/// </summary>
		private void GetConstructorDestructorInfo(LanguageSettings languageSet, CodeElement parentElement, CodeElement element,
			out bool constructor, out bool destructor)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			constructor = false;
			destructor = false;

			if (element.Kind == vsCMElement.vsCMElementFunction)
			{
				if (parentElement != null)
				{
					if ((parentElement.Kind == vsCMElement.vsCMElementClass) ||
							(parentElement.Kind == vsCMElement.vsCMElementModule) ||
							(parentElement.Kind == vsCMElement.vsCMElementStruct))
					{
						if (parentElement.Name == element.Name)
							constructor = true;
						else
						if (element.Name.StartsWith(DESTRUCTOR_PREFIX))
							destructor = true;
					}
				}
				else if ((languageSet != null) && languageSet.ParentlessFullName) // C++ projects
				{
					if ((element is CodeFunction) && element.IsCodeType)
					{
						var func = (CodeFunction)element;
						if ((func.FunctionKind & vsCMFunction.vsCMFunctionConstructor) == vsCMFunction.vsCMFunctionConstructor)
							constructor = true;
						else if ((func.FunctionKind & vsCMFunction.vsCMFunctionDestructor) == vsCMFunction.vsCMFunctionDestructor)
							destructor = true;
					}
				}
			}
		}

		/// <summary>
		/// Converts members type to vsCMElement one.
		/// </summary>
		private ICollection<vsCMElement> FilterToDTEElementKind(CodeModelFilterFlags filter)
		{
			var result = new List<vsCMElement>(4);

			if (filter.HasFlag(CodeModelFilterFlags.Classes))
				result.AddRange(new[] { vsCMElement.vsCMElementClass, vsCMElement.vsCMElementModule });
			if (filter.HasFlag(CodeModelFilterFlags.Interfaces))
				result.Add(vsCMElement.vsCMElementInterface);
			if (filter.HasFlag(CodeModelFilterFlags.Structs))
				result.Add(vsCMElement.vsCMElementStruct);
			if (filter.HasFlag(CodeModelFilterFlags.Enums))
				result.Add(vsCMElement.vsCMElementEnum);
			if (filter.HasFlag(CodeModelFilterFlags.Methods) || filter.HasFlag(CodeModelFilterFlags.Constructors))
				result.Add(vsCMElement.vsCMElementFunction);
			if (filter.HasFlag(CodeModelFilterFlags.Properties))
				result.Add(vsCMElement.vsCMElementProperty);
			if (filter.HasFlag(CodeModelFilterFlags.Fields))
				result.Add(vsCMElement.vsCMElementVariable);
			if (filter.HasFlag(CodeModelFilterFlags.Events))
				result.Add(vsCMElement.vsCMElementEvent);
			if (filter.HasFlag(CodeModelFilterFlags.Delegates))
				result.Add(vsCMElement.vsCMElementDelegate);

			return result;
		}

		/// <summary>
		/// Returns element's Xml document information text w/o all supporting/wrapping Xml tags.
		/// </summary>
		private string GetXmlDoc(CodeElement element, LanguageSettings languageSet)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			if (languageSet?.Type == LanguageType.Unknown)
				return string.Empty;

			// Check if Xml documentation is supported
			var xmlDocs = languageSet.XmlDocs;
			if (xmlDocs == null)
				return string.Empty;

			string xmlDoc;
			string comment;

			switch (element.Kind)
			{
				case vsCMElement.vsCMElementVariable:
					if (!(element is CodeVariable))
						return string.Empty;

					var codeField = (CodeVariable)element;
					xmlDoc = codeField.DocComment;
					comment = codeField.Comment;
					break;
				case vsCMElement.vsCMElementProperty:
					if (!(element is CodeProperty))
						return string.Empty;

					var codeProperty = (CodeProperty)element;
					xmlDoc = codeProperty.DocComment;
					comment = codeProperty.Comment;
					break;
				case vsCMElement.vsCMElementFunction:
					if (!(element is CodeFunction))
						return string.Empty;

					var codeMethod = (CodeFunction)element;
					xmlDoc = codeMethod.DocComment;
					comment = codeMethod.Comment;
					break;
				case vsCMElement.vsCMElementEvent:
					if (!(element is CodeEvent))
						return string.Empty;

					var codeEvent = (CodeEvent)element;
					xmlDoc = codeEvent.DocComment;
					comment = codeEvent.Comment;
					break;
				case vsCMElement.vsCMElementDelegate:
					if (!(element is CodeDelegate))
						return string.Empty;

					var codeDelegate = (CodeDelegate)element;
					xmlDoc = codeDelegate.DocComment;
					comment = codeDelegate.Comment;
					break;
				case vsCMElement.vsCMElementEnum:
					if (!(element is CodeEnum))
						return string.Empty;

					var codeEnum = (CodeEnum)element;
					xmlDoc = codeEnum.DocComment;
					comment = codeEnum.Comment;
					break;
				case vsCMElement.vsCMElementClass:
				case vsCMElement.vsCMElementModule:
					if (!(element is CodeClass))
						return string.Empty;

					var codeClass = (CodeClass)element;
					xmlDoc = codeClass.DocComment;
					comment = codeClass.Comment;
					break;
				case vsCMElement.vsCMElementInterface:
					if (!(element is CodeInterface))
						return string.Empty;

					var codeInterface = (CodeInterface)element;
					xmlDoc = codeInterface.DocComment;
					comment = codeInterface.Comment;
					break;
				case vsCMElement.vsCMElementStruct:
					if (!(element is CodeStruct))
						return string.Empty;

					var codeStruct = (CodeStruct)element;
					xmlDoc = codeStruct.DocComment;
					comment = codeStruct.Comment;
					break;
				default:
					return string.Empty;
			}

			// Safety check
			if (xmlDoc == null)
			{
				if (!string.IsNullOrEmpty(comment))
					return comment;
				else
					return string.Empty;
			}

			if (comment == null)
				comment = string.Empty;

			if (xmlDoc == string.Empty)
			{
				xmlDoc = comment;
			}
			else
			{
				// Check if Xml documentation processing is required
				if ((xmlDocs.Length == 1) && string.IsNullOrEmpty(xmlDocs[0]))
					return xmlDoc;

				if (languageSet.XmlDocSurround)
					xmlDoc = $"<{DOC_TAG}>{xmlDoc}<{DOC_TAG}/>";

				// Extract Xml documentation 'summary' tag contents
				using (var reader = new StringReader(xmlDoc))
				{
					XPathDocument doc;
					try
					{
						doc = new XPathDocument(reader);
					}
					catch // Raised when Xml is malformed
					{
						return string.Empty;
					}

					var nav = doc.CreateNavigator();
					var processed = false;

					foreach (string xd in xmlDocs)
					{
						var iterator = languageSet.XmlDocSurround ? nav.Select($"/{DOC_TAG}") : nav.Select(xd);

						if ((iterator != null) && iterator.MoveNext())
						{
							xmlDoc = iterator.Current.InnerXml;

							// Strip out leading and trailing CrLf
							var leadingCrLf = xmlDoc.StartsWith(Environment.NewLine, StringComparison.OrdinalIgnoreCase);
							var trailingCrLf = xmlDoc.EndsWith(Environment.NewLine, StringComparison.OrdinalIgnoreCase);
							if (leadingCrLf && trailingCrLf)
								xmlDoc = xmlDoc.Substring(Environment.NewLine.Length, xmlDoc.Length - Environment.NewLine.Length * 2);
							else if (leadingCrLf)
								xmlDoc = xmlDoc.Substring(Environment.NewLine.Length);
							else if (trailingCrLf)
								xmlDoc = xmlDoc.Substring(0, xmlDoc.Length - Environment.NewLine.Length);

							processed = true;
							break;
						}
					}
					if (!processed)
						xmlDoc = string.Empty;
				}
			}

			return xmlDoc;
		}

		/// <summary>
		/// Returns element's type name declaration.
		/// </summary>
		private string GetElementReturnTypeName(CodeElement element)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			CodeTypeRef typeRef;
			try
			{
				switch (element.Kind)
				{
					case vsCMElement.vsCMElementFunction:
						typeRef = ((CodeFunction)element).Type;
						break;
					case vsCMElement.vsCMElementProperty:
						typeRef = ((CodeProperty)element).Type;
						break;
					case vsCMElement.vsCMElementVariable:
						typeRef = ((CodeVariable)element).Type;
						break;
					case vsCMElement.vsCMElementDelegate:
						typeRef = ((CodeDelegate)element).Type;
						break;
					case vsCMElement.vsCMElementEvent:
						typeRef = ((CodeEvent)element).Type;
						break;
					default:
						typeRef = null;
						break;
				}
			}
			catch (Exception ex)
			{
				_log.LogMessage($"Error collecting {element.Name} return type", ex);
				typeRef = null;
			}

			if (typeRef == null)
				return string.Empty;

			var typeName = typeRef.AsString;
			if (typeName == null)
			{
				typeName = string.Empty;
			}
			else
			{
				// Generics processing
				foreach (CodeGenericsPair pair in _codePairs)
				{
					var genericsBegin = typeName.IndexOf(pair.Being);
					if (genericsBegin <= 0)
						continue;
					var genericsEnd = typeName.LastIndexOf(pair.End);
					if (genericsEnd <= 0)
						continue;

					var genericsTypeName = typeName.Substring(genericsBegin);
					var genericsIndex = genericsTypeName.LastIndexOf('.');
					if ((genericsIndex >= 0) && (genericsIndex < genericsTypeName.Length - 1))
					{
						var baseTypeName = typeName.Substring(0, genericsBegin + 1);

						var spaceIndex = genericsTypeName.IndexOf(' ');
						var genericsTypeNamePrefix = spaceIndex > 0 ? genericsTypeName.Substring(1, spaceIndex) : string.Empty;

						genericsTypeName = genericsTypeName.Substring(genericsIndex + 1);
						typeName = baseTypeName + genericsTypeNamePrefix + genericsTypeName;
					}

					// Once we find a matching pair, we are done
					break;
				} // foreach

				var index = typeName.LastIndexOf('.');
				if ((index >= 0) && (index < typeName.Length - 1))
					typeName = typeName.Substring(index + 1);
			}

			return typeName;
		}

		#endregion
	}
}