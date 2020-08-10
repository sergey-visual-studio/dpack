using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

using DPackRx.Extensions;
using DPackRx.Options;
using DPackRx.Package;
using DPackRx.Services;

namespace DPackRx.Features.Miscellaneous
{
	/// <summary>
	/// Miscellaneous feature.
	/// </summary>
	[KnownFeature(KnownFeature.Miscellaneous)]
	public class MiscellaneousFeature : Feature
	{
		#region Fields

		private readonly IPackageService _packageService;
		private readonly IUtilsService _utilsService;
		private readonly IShellHelperService _shellHelperService;
		private readonly IShellStatusBarService _shellStatusBarService;
		private readonly IShellReferenceService _shellReferenceService;
		private readonly IShellSelectionService _shellSelectionService;

		private const char PROJECT_SEP = '|';
		private const string PROJECT_PREFIX = "Project=";

		#endregion

		public MiscellaneousFeature(IServiceProvider serviceProvider, ILog log, IOptionsService optionsService,
			IPackageService packageService, IUtilsService utilsService, IShellHelperService shellHelperService,
			IShellStatusBarService shellStatusBarService, IShellReferenceService shellReferenceService,
			IShellSelectionService shellSelectionService) : base(serviceProvider, log, optionsService)
		{
			_packageService = packageService;
			_utilsService = utilsService;
			_shellHelperService = shellHelperService;
			_shellStatusBarService = shellStatusBarService;
			_shellReferenceService = shellReferenceService;
			_shellSelectionService = shellSelectionService;
		}

		// Test constructor
		protected internal MiscellaneousFeature() : base()
		{
		}

		#region Feature Overrides

		/// <summary>
		/// Returns all commands.
		/// </summary>
		/// <returns>Command Ids.</returns>
		public override ICollection<int> GetCommandIds()
		{
			return new List<int>(new[] {
				CommandIDs.COLLAPSE_SOLUTION_CONTEXT,
				CommandIDs.COPY_REFERENCES_CONTEXT,
				CommandIDs.PASTE_REFERENCES_CONTEXT,
				CommandIDs.COPY_REFERENCE_CONTEXT,
				CommandIDs.LOCATE_IN_SOLUTION_EXPLORER_CONTEXT,
				CommandIDs.COMMAND_PROMPT,
				CommandIDs.COPY_FULL_PATH
			});
		}

		/// <summary>
		/// Checks if command is available or not.
		/// </summary>
		/// <param name="commandId">Command Id.</param>
		/// <returns>Command status.</returns>
		public override bool IsValidContext(int commandId)
		{
			switch (commandId)
			{
				case CommandIDs.COLLAPSE_SOLUTION_CONTEXT:
					if (_shellSelectionService.IsContextActive(ContextType.SolutionHasProjects))
						return true;
					else
						return false;
				case CommandIDs.COPY_REFERENCES_CONTEXT:
				case CommandIDs.PASTE_REFERENCES_CONTEXT:
				case CommandIDs.COPY_REFERENCE_CONTEXT:
					return true;
				case CommandIDs.LOCATE_IN_SOLUTION_EXPLORER_CONTEXT:
					return true;
				case CommandIDs.COMMAND_PROMPT:
					return true;
				case CommandIDs.COPY_FULL_PATH:
					return true;
				default:
					return base.IsValidContext(commandId);
			}
		}

		/// <summary>
		/// Executes a command.
		/// </summary>
		/// <param name="commandId">Command Id.</param>
		/// <returns>Execution status.</returns>
		public override bool Execute(int commandId)
		{
			switch (commandId)
			{
				case CommandIDs.COLLAPSE_SOLUTION_CONTEXT:
					return CollapseAllProjects();
				case CommandIDs.COPY_REFERENCES_CONTEXT:
					return CopyReferences(true);
				case CommandIDs.PASTE_REFERENCES_CONTEXT:
					return PasteReferences();
				case CommandIDs.COPY_REFERENCE_CONTEXT:
					return CopyReferences(false);
				case CommandIDs.LOCATE_IN_SOLUTION_EXPLORER_CONTEXT:
					return LocateInSolutionExplorer();
				case CommandIDs.COMMAND_PROMPT:
					return OpenCommandPrompt();
				case CommandIDs.COPY_FULL_PATH:
					return CopyProjectFullPath();
				default:
					return base.Execute(commandId);
			}
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Collapses all projects.
		/// </summary>
		private bool CollapseAllProjects()
		{
			_shellHelperService.CollapseAllProjects();
			return true;
		}

		/// <summary>
		/// Copies project references to the clipboard.
		/// </summary>
		private bool CopyReferences(bool copyAll)
		{
			this.Log.LogMessage(this.KnownFeature, $"Copy references - check selection - {copyAll}");

			var references = _shellReferenceService.GetProjectReferences(!copyAll);
			var referenceCount = 0;

			// Build reference string
			var sb = new StringBuilder();
			foreach (var reference in references)
			{
				if (!string.IsNullOrEmpty(reference.Path))
				{
					string name = reference.Name;

					if (!string.IsNullOrEmpty(reference.ReferencingProjectName))
					{
						sb.Append(PROJECT_PREFIX);
						sb.Append(reference.ReferencingProjectName);
						sb.Append(PROJECT_SEP);
					}

					sb.Append(reference.Path);
					sb.Append(Environment.NewLine);

					referenceCount++;
				}
			}
			_utilsService.SetClipboardData(sb.ToString());

			_shellStatusBarService.SetStatusBarText($"Copied {referenceCount} references");

			this.Log.LogMessage(this.KnownFeature, $"Copy references - copied selection - {copyAll} - {referenceCount}");
			return true;
		}

		/// <summary>
		/// Pastes project references from the clipboard.
		/// </summary>
		private bool PasteReferences()
		{
			this.Log.LogMessage(this.KnownFeature, "Paste references - check selection");

			if (!_utilsService.GetClipboardData(out string refs) || string.IsNullOrEmpty(refs))
				return false;

			var references = refs.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
			if ((references == null) || (references.Length == 0))
				return false;

			// Collect invalid file name chars
			var invalidFileNameChars = new List<char>(10);
			var chars = Path.GetInvalidFileNameChars();
			if (chars != null)
				invalidFileNameChars.AddRange(chars);
			// Collect invalid path chars
			var invalidPathChars = new List<char>(10);
			chars = Path.GetInvalidPathChars();
			if (chars != null)
				invalidPathChars.AddRange(chars);

			var referenceCount = 0;

			// Add references
			foreach (var reference in references)
			{
				var isProject = false;
				var fileName = reference;
				var projectName = string.Empty;

				// Name validation
				if (fileName == null)
					continue;
				fileName = fileName.Trim();
				if (string.IsNullOrEmpty(fileName))
					continue;

				// Name massaging
				if (fileName.EndsWith(Environment.NewLine))
					fileName = fileName.Substring(0, fileName.Length - Environment.NewLine.Length);

				var names = fileName.Split(PROJECT_SEP);
				if ((names != null) && (names.Length == 2) && (!string.IsNullOrEmpty(names[0])) &&
						(names[0].StartsWith(PROJECT_PREFIX)))
				{
					projectName = names[0].Substring(PROJECT_PREFIX.Length);
					fileName = names[1];
					isProject = true;

					// Name massaging
					if (fileName.EndsWith(Environment.NewLine))
						fileName = fileName.Substring(0, fileName.Length - Environment.NewLine.Length);
					if ((!string.IsNullOrEmpty(projectName)) && (projectName.EndsWith(Environment.NewLine)))
						projectName = projectName.Substring(0, projectName.Length - Environment.NewLine.Length);
				}

				if (!isProject) // Assembly reference processing
				{
					var invalidFileName = false;

					// Check file path
					var sepIndex = fileName.LastIndexOf(Path.DirectorySeparatorChar);
					if (sepIndex > 0)
					{
						for (var charIndex = 0; charIndex < sepIndex; charIndex++)
						{
							if (invalidPathChars.Contains(fileName[charIndex]))
							{
								invalidFileName = true;
								break;
							}
						}
					}

					// Check file name
					if (!invalidFileName)
					{
						if (sepIndex == -1)
							sepIndex = 0;
						else
							sepIndex++;

						for (var charIndex = sepIndex; charIndex < fileName.Length; charIndex++)
						{
							if (invalidFileNameChars.Contains(fileName[charIndex]))
							{
								invalidFileName = true;
								break;
							}
						}
					}

					if (invalidFileName)
					{
						this.Log.LogMessage(this.KnownFeature, $"Paste references - ignored invalid file name - {fileName}");
						continue;
					}

					if (_shellReferenceService.AddAssemblyReference(fileName))
						referenceCount++;
				}
				else // Project reference processing
				if (!string.IsNullOrEmpty(projectName) && !string.IsNullOrEmpty(fileName))
				{
					if (_shellReferenceService.AddProjectReference(projectName))
						referenceCount++;
				}
			} // for (references)

			_shellStatusBarService.SetStatusBarText($"Pasted {referenceCount} reference(s)");

			this.Log.LogMessage(this.KnownFeature, $"Paste references - pasted - {referenceCount}");
			return true;
		}

		/// <summary>
		/// Selects current document in Solution Explorer.
		/// </summary>
		private bool LocateInSolutionExplorer()
		{
			if (!_shellHelperService.SelectSolutionExplorerDocument(out string documentName))
			{
				_shellStatusBarService.SetStatusBarText($"Unable to locate '{documentName}' file in Solution Explorer");
				return false;
			}

			return true;
		}

		/// <summary>
		/// Opens command prompt.
		/// </summary>
		private bool OpenCommandPrompt()
		{
			var path = _shellHelperService.GetSelectedItemPath();
			if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
				return false;

			var commandToolsPath = _packageService.VSInstallDir;
			if (!string.IsNullOrEmpty(commandToolsPath))
			{
				commandToolsPath = Path.Combine(commandToolsPath, @"Common7\Tools", "vsdevcmd.bat");
			}
			if (string.IsNullOrEmpty(commandToolsPath) || !File.Exists(commandToolsPath))
			{
				this.Log.LogMessage(this.KnownFeature, $"Command Prompt - failed to find command tools: {commandToolsPath}");
				return false;
			}

			var startInfo = new ProcessStartInfo("cmd.exe", "/K \"" + commandToolsPath + "\"") { WorkingDirectory = path };
			if (_utilsService.ControlKeyDown())
				startInfo.Verb = "runas";

			if (Process.Start(startInfo) != null)
				this.Log.LogMessage(this.KnownFeature, $"Command Prompt - failed to start: {startInfo.Arguments}");

			return true;
		}

		/// <summary>
		/// Copies project full path to the clipboard.
		/// </summary>
		private bool CopyProjectFullPath()
		{
			var path = _shellHelperService.GetCurrentProjectPath();
			if (string.IsNullOrEmpty(path))
			{
				_shellStatusBarService.SetStatusBarText("Unable to copy project file path");
				return false;
			}

			_utilsService.SetClipboardData(path);
			_shellStatusBarService.SetStatusBarText(path);
			return true;
		}

		#endregion
	}
}