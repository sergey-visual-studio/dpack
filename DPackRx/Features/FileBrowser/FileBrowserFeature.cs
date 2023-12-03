using System;
using System.Collections.Generic;

using DPackRx.CodeModel;
using DPackRx.Extensions;
using DPackRx.Options;
using DPackRx.Package;
using DPackRx.Services;

namespace DPackRx.Features.FileBrowser
{
	/// <summary>
	/// File Browser feature.
	/// </summary>
	[KnownFeature(KnownFeature.FileBrowser)]
	[OptionsDefaults("Search", "")]
	[OptionsDefaults("AllFiles", false)]
	[OptionsDefaults("IgnoreFiles", "packages.config")]
	[OptionsDefaults("ShowFiles", "")]
	[OptionsDefaults("IgnoreFolders", "bin")]
	public class FileBrowserFeature : Feature, ISolutionEvents, IDisposable
	{
		#region Fields

		private readonly IShellSelectionService _shellSelectionService;
		private readonly IModalDialogService _dialogService;
		private readonly IShellEventsService _shellEventsService;

		#endregion

		public FileBrowserFeature(IServiceProvider serviceProvider, ILog log, IOptionsService optionsService,
			IShellSelectionService shellSelectionService, IModalDialogService dialogService, IShellEventsService shellEventsService) : base(serviceProvider, log, optionsService)
		{
			_shellSelectionService = shellSelectionService;
			_dialogService = dialogService;
			_shellEventsService = shellEventsService;

			_shellEventsService.SubscribeSolutionEvents(this);
		}

		// Test constructor
		protected internal FileBrowserFeature() : base()
		{
		}

		#region Feature Overrides

		/// <summary>
		/// Returns all commands.
		/// </summary>
		/// <returns>Command Ids.</returns>
		public override ICollection<int> GetCommandIds()
		{
			return new List<int>(new[] { CommandIDs.FILE_BROWSER });
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
				case CommandIDs.FILE_BROWSER:
					if (_shellSelectionService.IsContextActive(ContextType.SolutionHasProjects))
						return true;
					else
						return false;
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
				case CommandIDs.FILE_BROWSER:
					ShowBrowser();
					return true;
				default:
					return base.Execute(commandId);
			}
		}

		#endregion

		#region ISolutionEvents Members

		public void SolutionOpened(bool allProjectsLoaded)
		{
			ClearCache("solution opened");
		}

		public void SolutionClosing()
		{
		}

		public void SolutionClosed()
		{
			ClearCache("solution closed");
		}

		public void SolutionSaved()
		{
		}

		public void SolutionRenamed(string oldName, string newName)
		{
		}

		public void ProjectAdded(object project)
		{
			ClearCache("project added");
		}

		public void ProjectDeleted(object project)
		{
			ClearCache("project deleted");
		}

		public void ProjectRenamed(object project)
		{
			ClearCache("project renamed");
		}

		public void ProjectUnloaded(object project)
		{
			ClearCache("project unloaded");
		}

		public void FileAdded(string[] fileNames, object project)
		{
			ClearCache("file added");
		}

		public void FileDeleted(string[] fileNames, object project)
		{
			ClearCache("file deleted");
		}

		public void FileRenamed(string[] oldFileNames, string[] newFileNames, object project)
		{
			ClearCache("file renamed");
		}

		public void FileChanged(string fileName, object projectItem)
		{
		}

		public void FileOpened(string fileName, object projectItem)
		{
		}

		public void FileClosed(string fileName, object project)
		{
		}

		public void FileSaved(string fileName, object projectItem)
		{
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			_shellEventsService.UnsubscribeSolutionEvents(this);
		}

		#endregion

		#region Name

		/// <summary>
		/// Solution model cache.
		/// </summary>
		protected internal SolutionModel Cache { get; set; }

		#endregion

		#region Private Methods

		private void ShowBrowser()
		{
			this.Cache = _dialogService.ShowDialog<FileBrowserWindow, FileBrowserViewModel, SolutionModel>($"{this.Name}'s collecting solution information...", this.Cache);

			if (this.Cache != null)
				this.Log.LogMessage(KnownFeature.FileBrowser, "Saved cached solution model");
		}

		private void ClearCache(string message)
		{
			if (this.Cache != null)
			{
				this.Cache = null;

				this.Log.LogMessage(KnownFeature.FileBrowser, $"Cleared cached solution model - {message}");
			}
		}

		#endregion
	}
}