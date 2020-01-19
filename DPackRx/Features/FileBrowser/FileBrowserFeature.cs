using System;
using System.Collections.Generic;

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
	public class FileBrowserFeature : Feature
	{
		#region Fields

		private readonly IShellSelectionService _shellSelectionService;
		private readonly IModalDialogService _dialogService;

		#endregion

		public FileBrowserFeature(IServiceProvider serviceProvider, ILog log, IOptionsService optionsService,
			IShellSelectionService shellSelectionService, IModalDialogService dialogService) : base(serviceProvider, log, optionsService)
		{
			_shellSelectionService = shellSelectionService;
			_dialogService = dialogService;
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

		#region Private Methods

		private void ShowBrowser()
		{
			_dialogService.ShowDialog<FileBrowserWindow, FileBrowserViewModel>($"{this.Name}'s collecting solution information...");
		}

		#endregion
	}
}