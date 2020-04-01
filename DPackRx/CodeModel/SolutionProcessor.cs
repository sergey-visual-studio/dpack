using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DPackRx.Extensions;
using DPackRx.Language;
using DPackRx.Services;

using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace DPackRx.CodeModel
{
	/// <summary>
	/// Solution project processor.
	/// </summary>
	public class SolutionProcessor : ISolutionProcessor
	{
		#region Fields

		private readonly ILog _log;
		private readonly ILanguageService _languageService;
		private readonly IShellProjectService _shellProjectService;
		private readonly IProjectProcessor _projectProcessor;

		#endregion

		public SolutionProcessor(ILog log, ILanguageService languageService, IShellProjectService shellProjectService,
			IProjectProcessor projectProcessor)
		{
			_log = log;
			_languageService = languageService;
			_shellProjectService = shellProjectService;
			_projectProcessor = projectProcessor;
		}

		#region ISolutionProcessor Members

		/// <summary>
		/// Returns solution projects.
		/// </summary>
		/// <param name="flags">Processor flags.</param>
		/// <param name="filter">Code model filter.</param>
		/// <returns>Projects.</returns>
		public SolutionModel GetProjects(ProcessorFlags flags, CodeModelFilterFlags filter = CodeModelFilterFlags.All)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var model = new SolutionModel();
			GetProjectsInternal(model, flags, filter);
			return model;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Returns solution projects.
		/// </summary>
		private void GetProjectsInternal(SolutionModel model, ProcessorFlags flags, CodeModelFilterFlags filter)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var dte = _shellProjectService.GetDTE() as DTE;

			_log.LogMessage("Resolving solution name");

			var solution = dte.Solution?.FileName;
			if (!string.IsNullOrEmpty(solution))
				solution = Path.GetFileNameWithoutExtension(solution);
			model.SolutionName = solution;

			_log.LogMessage("Collecting solution projects");

			var projects = dte.Solution.Projects;
			for (int index = 1; index <= projects.Count; index++)
			{
				Project project = projects.Item(index);

				if (_shellProjectService.IsProjectLoadDeferred(project, out bool loaded))
					continue;

				// Reacquire project reference if it's been reloaded
				if (loaded)
					project = projects.Item(index);

				if (ProcessProject(project, model.Projects, flags, filter))
					ProcessProjectRecursively(project, model.Projects, flags, filter);
			}

			model.Files = model.Projects.SelectMany(p => p.Files).ToList();

			if (flags.HasFlag(ProcessorFlags.GroupLinkedFiles))
			{
				// Get all grouped, ie duplicate, files, keep the first code file and remove the rest
				var groupedFiles = model.Files.GroupBy(f => f.FileNameWithPath).Where(g => g.Count() > 1).Select(g => g);
				foreach (var group in groupedFiles)
				{
					var file = group.FirstOrDefault(f => f.Project.Language.SupportsCompileBuildAction && _shellProjectService.CompileBuildAction(f.ProjectItem));
					if (file != null)
					{
						group.ForEach(f =>
						{
							if (f != file)
							{
								model.Files.Remove(f);
								f.Project.Files.Remove(f);
								AddProjectName(file, f.ProjectName);
							}
						});
					}
				}
			}

			_log.LogMessage($"Collected {model.Projects.Count} solution projects");
		}

		/// <summary>
		/// Processes individual project.
		/// </summary>
		/// <returns>Whether project's been processed or not.</returns>
		private bool ProcessProject(Project project, ICollection<ProjectModel> model, ProcessorFlags flags, CodeModelFilterFlags filter)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var process = true;
			var language = string.Empty;

			// Safety check & language retrieval
			try
			{
				var items = project.ProjectItems;
				if ((items != null) && (items.Count > 0))
				{
					if (_shellProjectService.IsCodeModelProject(project))
					{
						language = _shellProjectService.GetProjectLanguage(project);
						if (string.IsNullOrEmpty(language))
							process = false;
					}
					else
					{
						language = _shellProjectService.GetNoCodeModelProjectLanguage(project);
						if (string.IsNullOrEmpty(language))
							process = false;
					}
				}
			}
			catch (NotImplementedException) // This is an acceptable condition - keep on going
			{
				process = false;
				_log.LogMessage($"Project '{project?.Name}' doesn't implement code model");
			}
			catch (Exception ex) // But this is a legit problem - keep on going anyways
			{
				process = false;
				_log.LogMessage("Failed to retrieve project code model / language", ex);
			}

			string projectFullName = string.Empty;
			if (process)
			{
				_shellProjectService.GetProjectPath(project, out string projectPath, out projectFullName);
				if ((string.IsNullOrEmpty(projectPath) || string.IsNullOrEmpty(projectFullName)) && _shellProjectService.IsProject(project))
				{
					process = false;
					_log.LogMessage($"Project '{project?.Name}' path is not available");
				}
			}

			if (process)
			{
				var projectModel = new ProjectModel
				{
					Project = project,
					Name = project.Name,
					FriendlyName = _shellProjectService.GetFriendlyProjectName(project),
					FileName = projectFullName,
					Language = _languageService.GetLanguage(language)
				};

				if (flags.HasFlag(ProcessorFlags.IncludeFiles))
				{
					var files = _projectProcessor.GetFiles(project, flags, filter);
					if (files != null)
					{
						projectModel.Files.AddRange(files);
						files.ForEach(f => f.Project = projectModel);
					}
				}

				model.Add(projectModel);
			}

			return process;
		}

		/// <summary>
		/// Walks sub-project and processes all of its qualified items and sub-projects.
		/// </summary>
		private void ProcessProjectRecursively(Project project, ICollection<ProjectModel> model, ProcessorFlags flags, CodeModelFilterFlags filter)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			if (project == null)
				return;

			var items = project.ProjectItems;
			if (items == null)
				return;

			foreach (ProjectItem item in items)
			{
				Project subProject;
				try
				{
					subProject = item.SubProject;
				}
				catch
				{
					subProject = null;
				}

				if ((subProject != null) && (subProject.ProjectItems != null))
				{
					if (ProcessProject(subProject, model, flags, filter))
						ProcessProjectRecursively(subProject, model, flags, filter);
				}
			}
		}

		/// <summary>
		/// Adds linked file project name.
		/// </summary>
		private void AddProjectName(FileModel file, string projectName)
		{
			if (!string.IsNullOrEmpty(file.ProjectName) && !string.IsNullOrEmpty(projectName))
				file.ProjectName = $"{file.ProjectName}, {projectName}";
			else
				file.ProjectName = projectName;
		}

		#endregion
	}
}