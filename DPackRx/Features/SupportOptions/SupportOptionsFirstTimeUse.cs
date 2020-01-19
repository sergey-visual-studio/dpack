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
		private readonly IMessageService _messageService;
		private bool _firstTimeUseChecked;

		private const string WELCOME = "Welcome";
		private const string WELCOME_TEXT =
			"Thank you for choosing {0}!\r\n\r\n" +
			"{0} extends Visual Studio {1} by adding various features to its Tools and editor context menu. Tools|{0} menu should be your starting point. Tools|{0}|Options menu item allows you to customize many of {0}'s features.\r\n\r\n" +
			"Don't hesitate to contact support if you have any questions or run into any problems. Enjoy.\r\n\r\n" +
			"Sergey Mishkovskiy @ USysWare, Inc.\r\n\r\n\r\n" +
			"For best {0} experience it's recommended to let {0} assign its keyboard shortcuts. It can also be done later via Tools|{0}|Options menu item.\r\n\r\n" +
			"Would you like to assign {0} keyboard shortcuts now?";

		#endregion

		public SupportOptionsFirstTimeUse(ILog log, IOptionsService optionsService,
			IPackageService packageService, IShellHelperService shellHelperService, IMessageService messageService)
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

				if (_messageService.ShowQuestion(string.Format(WELCOME_TEXT, _packageService.ProductName, _packageService.VSKnownVersion)))
					_shellHelperService.AssignShortcuts();
			}
			finally
			{
				_firstTimeUseChecked = true;
				_optionsService.SetBoolOption(KnownFeature.SupportOptions, WELCOME, true);
			}
		}

		#endregion
	}
}