using System;
using System.Collections.Generic;
using System.IO;

using DPackRx.Extensions;
using DPackRx.Language;
using DPackRx.Services;

using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace DPackRx.CodeModel
{
	/// <summary>
	/// Project files processor.
	/// </summary>
	public class ProjectProcessor : IProjectProcessor
	{
		#region Fields

		private readonly ILog _log;
		private readonly ILanguageService _languageService;
		private readonly IShellProjectService _shellProjectService;
		private readonly IFileTypeResolver _fileTypeResolver;
		private readonly IFileProcessor _fileProcessor;

		private const string WEB_APP_CODE = "App_Code";
		private const string REFERENCES_NODE = "References";

		#endregion

		public ProjectProcessor(ILog log, ILanguageService languageService, IShellProjectService shellProjectService, IFileTypeResolver fileTypeResolver,
			IFileProcessor fileProcessor)
		{
			_log = log;
			_languageService = languageService;
			_shellProjectService = shellProjectService;
			_fileTypeResolver = fileTypeResolver;
			_fileProcessor = fileProcessor;
		}

		#region IProjectProcessor Members

		/// <summary>
		/// Returns project files.
		/// </summary>
		/// <param name="project">Project. Untyped extensibility link (name matches the actual type).</param>
		/// <param name="flags">Processor flags.</param>
		/// <param name="filter">Code model filter.</param>
		/// <returns>Files.</returns>
		public ICollection<FileModel> GetFiles(object project, ProcessorFlags flags, CodeModelFilterFlags filter)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var model = new List<FileModel>();
			GetFilesInternal(project, model, flags, filter);
			return model;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Returns project files.
		/// </summary>
		private void GetFilesInternal(object project, List<FileModel> model, ProcessorFlags flags, CodeModelFilterFlags filter)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var dteProject = project as Project;
			if (dteProject == null)
				throw new ArgumentNullException(nameof(project));

			if (_shellProjectService.IsProjectLoadDeferred(project, out _))
				return;

			_log.LogMessage($"Collecting '{dteProject.Name}' files");

			// No need to process project's items if we fail to check its code model
			// The exception is made for solution folder *if* that's requested
			var languageSet = _fileTypeResolver.GetCurrentLanguage(project, out bool isWebProject);
			bool process = languageSet != null;
			if (!process)
			{
				if (flags.HasFlag(ProcessorFlags.IncludeSolutionFolderFiles) && (dteProject.Kind == Constants.vsProjectKindSolutionItems))
				{
					_log.LogMessage($"Allow '{dteProject.Name}' solution folder processing");
					process = true;
				}
			}

			if (!process && !flags.HasFlag(ProcessorFlags.KnownProjectsOnly) && _shellProjectService.IsProject(project))
			{
				_log.LogMessage($"Allow '{dteProject.Name}' unknown project type processing");
				process = true;
			}

			if (process)
			{
				ProjectItems projectItems = dteProject.ProjectItems;
				if ((projectItems != null) && (projectItems.Count > 0))
				{
					bool isRoot = true;
					ProcessProjectItems(
						model, flags, filter, languageSet,
						projectItems, isWebProject, ref isRoot, null, string.Empty);
				}
			}

			_log.LogMessage($"Collected {model.Count} '{dteProject.Name}' files");
		}

		/// <summary>
		/// Walks qualified project items and processes all of their qualified items.
		/// </summary>
		private void ProcessProjectItems(List<FileModel> model, ProcessorFlags flags, CodeModelFilterFlags filter, LanguageSettings languageSet,
			ProjectItems projectItems, bool isWebProject, ref bool isRoot, ProjectItem parentItem, string parentName)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			// Parent item is applicable or meant to be used for actual files only, i.e. file combos like .cs and .designer.cs, etc.
			if (parentItem != null)
			{
				if ((string.Compare(parentItem.Kind, Constants.vsProjectItemKindPhysicalFile, StringComparison.OrdinalIgnoreCase) != 0) &&
						(string.Compare(parentItem.Kind, Constants.vsProjectItemKindSolutionItems, StringComparison.OrdinalIgnoreCase) != 0))
					parentItem = null;
			}
			var parentSubType = FileSubType.None;
			var parentSubTypeSet = false;

			if (!string.IsNullOrEmpty(parentName) && !parentName.EndsWith("\\", StringComparison.OrdinalIgnoreCase))
				parentName = parentName + "\\";

			// Save the current language and determine the new current language
			if (isWebProject)
			{
				var webLanguageSet = GetWebFolderLanguage(isRoot, projectItems);

				// Fallback scenario - use default project language so that non-code files can be resolved
				if (webLanguageSet?.Type != LanguageType.Unknown)
					languageSet = webLanguageSet; // language should be restored for other items once we walk up the stack
			}

			// Root processing is finished after language is checked for the root project items
			isRoot = false;
			var projectName = string.Empty;

			foreach (ProjectItem projectItem in projectItems)
			{
				// Skip project references as they can't contribute any files
				var isReferences = projectItem.Object is VSLangProj.References;
				var isReference = projectItem.Object is VSLangProj.Reference;
				if (isReferences || isReference)
					continue;

				// LightSwitch type projects with virtual references node workaround
				if (!string.IsNullOrEmpty(projectItem.Name) &&
						projectItem.Name.Equals(REFERENCES_NODE, StringComparison.OrdinalIgnoreCase) &&
						(string.Compare(projectItem.Kind, Constants.vsProjectItemKindVirtualFolder, StringComparison.OrdinalIgnoreCase) == 0))
					continue;

				if ((string.Compare(projectItem.Kind, Constants.vsProjectItemKindPhysicalFile, StringComparison.OrdinalIgnoreCase) == 0) ||
						(string.Compare(projectItem.Kind, Constants.vsProjectItemKindSolutionItems, StringComparison.OrdinalIgnoreCase) == 0))
				{
					if (!parentSubTypeSet)
					{
						parentSubTypeSet = true;

						if (parentItem != null)
							parentSubType = _fileTypeResolver.GetSubType(parentItem, languageSet, isWebProject);
					}

					var itemSubType = _fileTypeResolver.GetSubType(projectItem, languageSet, isWebProject);

					// Check if this is a code file
					var add = !flags.HasFlag(ProcessorFlags.IncludeCodeFilesOnly) || _fileTypeResolver.IsCodeSubType(itemSubType);

					// Check for .Designer file
					if (add &&
							!flags.HasFlag(ProcessorFlags.IncludeDesignerFiles) &&
							_fileTypeResolver.IsDesignerItem(projectItem, itemSubType, languageSet, isWebProject))
					{
						add = false;
					}

					// Used to collect skipped files here as well... something to keep an eye out out for
					if (add)
					{
						var fileName = projectItem.get_FileNames(1);
						if (string.IsNullOrEmpty(fileName))
							add = false;

						if (add)
						{
							// Try getting a misc/simple subtype for unknown and solution projects
							if ((languageSet?.Type == LanguageType.Unknown) && (itemSubType == FileSubType.None))
								itemSubType = _fileTypeResolver.GetExtensionSubType(projectItem, languageSet, isWebProject);

							if (string.IsNullOrEmpty(projectName))
								projectName = projectItem.ContainingProject?.Name;

							var itemModel = new FileModel
							{
								ProjectItem = projectItem,
								FileName = projectItem.Name,
								ParentProjectItem = parentItem,
								ParentName = parentName,
								ParentSubType = parentSubType,
								ItemSubType = itemSubType,
								FileNameWithPath = fileName,
								ProjectName = projectName
							};

							if (flags.HasFlag(ProcessorFlags.IncludeFileCodeModel))
							{
								var codeMembers = _fileProcessor.GetMembers(projectItem, flags, filter);
								if ((codeMembers != null) && (codeMembers.Members != null))
									itemModel.Members.AddRange(codeMembers.Members);
							}

							model.Add(itemModel);
						}
					}

					ProjectItems currentProjectItems = projectItem.ProjectItems;
					if ((currentProjectItems != null) && (currentProjectItems.Count > 0))
						ProcessProjectItems(
							model, flags, filter, languageSet,
							currentProjectItems, false, ref isRoot, projectItem, parentName);
				}
				else
				{
					if ((string.Compare(projectItem.Kind, Constants.vsProjectItemKindPhysicalFolder, StringComparison.OrdinalIgnoreCase) == 0) ||
							(string.Compare(projectItem.Kind, Constants.vsProjectItemKindVirtualFolder, StringComparison.OrdinalIgnoreCase) == 0))
					{
						ProjectItems currentProjectItems = projectItem.ProjectItems;
						if ((currentProjectItems != null) && (currentProjectItems.Count > 0))
							ProcessProjectItems(
								model, flags, filter, languageSet,
								currentProjectItems, isWebProject, ref isRoot, projectItem, parentName + projectItem.Name);
					}
				}
			} // foreach (projectItem)
		}

		/// <summary>
		/// Finds first code file in the web project items folder. For web projects only.
		/// </summary>
		private LanguageSettings GetWebFolderLanguage(bool isRoot, ProjectItems projectItems)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var languageSet = LanguageSettings.UnknownLanguage;

			if (projectItems?.Count > 0)
			{
				foreach (ProjectItem projectItem in projectItems)
				{
					if ((string.Compare(projectItem.Kind, Constants.vsProjectItemKindPhysicalFile, StringComparison.OrdinalIgnoreCase) == 0) ||
							(string.Compare(projectItem.Kind, Constants.vsProjectItemKindSolutionItems, StringComparison.OrdinalIgnoreCase) == 0))
					{
						languageSet = GetLanguageRecursively(projectItem);
						if (languageSet?.Type != LanguageType.Unknown)
							break;
					}
				}

				// Still didn't get the language - find app code folder and get its language
				if (isRoot && ((languageSet == null) || (languageSet.Type == LanguageType.Unknown)))
				{
					foreach (ProjectItem projectItem in projectItems)
					{
						if ((string.Compare(projectItem.Kind, Constants.vsProjectItemKindPhysicalFolder, StringComparison.OrdinalIgnoreCase) == 0) ||
								(string.Compare(projectItem.Kind, Constants.vsProjectItemKindVirtualFolder, StringComparison.OrdinalIgnoreCase) == 0))
						{
							if (projectItem.Name == WEB_APP_CODE)
							{
								languageSet = GetLanguageRecursively(projectItem);
								break;
							}
						}
					}
				}
			}

			return languageSet;
		}

		/// <summary>
		/// Looks for a code file language for all project item's related files.
		/// </summary>
		private LanguageSettings GetLanguageRecursively(ProjectItem projectItem)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var fileName = projectItem.get_FileNames(1);
			if (string.IsNullOrEmpty(fileName))
				return LanguageSettings.UnknownLanguage;

			var ext = Path.GetExtension(fileName);
			var languageSet = _languageService.GetExtensionLanguage(ext);
			if (languageSet?.Type != LanguageType.Unknown)
				return languageSet;

			ProjectItems projectItems = projectItem.ProjectItems;
			if (projectItems?.Count > 0)
			{
				foreach (ProjectItem currentProjectItem in projectItems)
				{
					languageSet = GetLanguageRecursively(currentProjectItem);
					if (languageSet?.Type != LanguageType.Unknown)
						return languageSet;
				}
			}

			return LanguageSettings.UnknownLanguage;
		}

		#endregion
	}
}