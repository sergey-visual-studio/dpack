using System;

using DPackRx.Options;
using DPackRx.Package;
using DPackRx.Services;

namespace DPackRx.Features.SupportOptions
{
	public class SupportOptionsFirstTimeUse : ISolutionEvents
	{
		#region Fields

		private readonly ILog _log;
		private readonly IOptionsService _optionsService;
		private readonly IPackageService _packageService;
		private readonly IShellHelperService _shellHelperService;
		private readonly IShellInfoBarService _shellInfoBarService;
		private readonly IMessageService _messageService;
		private bool _firstTimeUseChecked;

		private const string WELCOME = "Welcome";
		private const string WELCOME_TEXT =
			"Thank you for choosing {0}!  " +
			"{0} features are accessible from Tools and editor context menu, and can be customized via Options dialog.  " +
			"Don't hesitate to contact support if you have any questions or run into any issues.";

		#endregion

		public SupportOptionsFirstTimeUse(ILog log, IOptionsService optionsService,
			IPackageService packageService, IShellHelperService shellHelperService, IShellInfoBarService shellInfoBarService,
			IMessageService messageService)
		{
			if (log == null)
				throw new ArgumentNullException(nameof(log));

			if (optionsService == null)
				throw new ArgumentNullException(nameof(optionsService));

			if (shellHelperService == null)
				throw new ArgumentNullException(nameof(shellHelperService));

			if (messageService == null)
				throw new ArgumentNullException(nameof(messageService));

			_log = log;
			_optionsService = optionsService;
			_packageService = packageService;
			_shellHelperService = shellHelperService;
			_shellInfoBarService = shellInfoBarService;
			_messageService = messageService;
		}

		#region ISolutionEvents Members

		public void SolutionClosed()
		{
		}

		public void SolutionClosing()
		{
		}

		public void SolutionOpened(bool allProjectsLoaded)
		{
			ShowFirstTimeUse();
		}

		public void SolutionRenamed(string oldName, string newName)
		{
		}

		public void SolutionSaved()
		{
		}

		public void ProjectAdded(object project)
		{
		}

		public void ProjectDeleted(object project)
		{
		}

		public void ProjectRenamed(object project)
		{
		}

		public void ProjectUnloaded(object project)
		{
		}

		public void FileAdded(string[] fileNames, object project)
		{
		}

		public void FileChanged(string fileName, object projectItem)
		{
		}

		public void FileClosed(string fileName, object project)
		{
		}

		public void FileDeleted(string[] fileNames, object project)
		{
		}

		public void FileOpened(string fileName, object projectItem)
		{
		}

		public void FileRenamed(string[] oldFileNames, string[] newFileNames, object project)
		{
		}

		public void FileSaved(string fileName, object projectItem)
		{
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Shows first time use message.
		/// </summary>
		private void ShowFirstTimeUse()
		{
			if (_firstTimeUseChecked)
				return;

			try
			{
				if (_optionsService.GetBoolOption(KnownFeature.SupportOptions, WELCOME))
					return;

				var message = string.Format(WELCOME_TEXT, _packageService.ProductName, _packageService.VSKnownVersion);
				_shellInfoBarService.ShowInfoBar(message, "Assign Keyboard Shortcuts", AssignShortcuts, true);
			}
			finally
			{
				_firstTimeUseChecked = true;
				_optionsService.SetBoolOption(KnownFeature.SupportOptions, WELCOME, true);
			}
		}

		private void AssignShortcuts()
		{
			if (_shellHelperService.AssignShortcuts())
				_messageService.ShowMessage("Successfully assigned default keyboard shortcuts.");
			else
				_messageService.ShowError("Failed to assign default keyboard shortcuts.");
		}

		#endregion
	}
}