using System;
using System.Collections.Generic;
using System.IO;

using DPackRx.CodeModel;
using DPackRx.Language;
using DPackRx.Options;
using DPackRx.Package;
using DPackRx.Services;

namespace DPackRx.Features.CodeBrowser
{
	/// <summary>
	/// Code Browser feature.
	/// </summary>
	[KnownFeature(KnownFeature.CodeBrowser)]
	public class CodeBrowserFeature : Feature
	{
		#region Fields

		private readonly IShellSelectionService _shellSelectionService;
		private readonly IModalDialogService _dialogService;
		private readonly IFileProcessor _fileProcessor;
		private readonly ILanguageService _languageService;
		private readonly IMessageService _messageService;

		#endregion

		public CodeBrowserFeature(IServiceProvider serviceProvider, ILog log, IOptionsService optionsService,
			IShellSelectionService shellSelectionService, IModalDialogService dialogService, IFileProcessor fileProcessor,
			ILanguageService languageService, IMessageService messageService) : base(serviceProvider, log, optionsService)
		{
			_shellSelectionService = shellSelectionService;
			_dialogService = dialogService;
			_fileProcessor = fileProcessor;
			_languageService = languageService;
			_messageService = messageService;
		}

		// Test constructor
		protected internal CodeBrowserFeature() : base()
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
				CommandIDs.CODE_BROWSER,
				CommandIDs.CODE_BROWSER_CI,
				CommandIDs.CODE_BROWSER_M,
				CommandIDs.CODE_BROWSER_P
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
				case CommandIDs.CODE_BROWSER:
				case CommandIDs.CODE_BROWSER_CI:
				case CommandIDs.CODE_BROWSER_M:
				case CommandIDs.CODE_BROWSER_P:
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
			CodeModelFilterFlags filter;
			switch (commandId)
			{
				case CommandIDs.CODE_BROWSER:
					filter = CodeModelFilterFlags.All;
					break;
				case CommandIDs.CODE_BROWSER_CI:
					filter = CodeModelFilterFlags.ClassesInterfaces;
					break;
				case CommandIDs.CODE_BROWSER_M:
					filter = CodeModelFilterFlags.MethodsConstructors;
					break;
				case CommandIDs.CODE_BROWSER_P:
					filter = CodeModelFilterFlags.Properties;
					break;
				default:
					return base.Execute(commandId);
			}

			return ShowBrowser(filter);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Shows code browser window with provided filter selection.
		/// </summary>
		private bool ShowBrowser(CodeModelFilterFlags filter)
		{
			var fileName = _shellSelectionService.GetActiveFileName();
			if (string.IsNullOrEmpty(fileName))
				return false;

			var languageSet = _languageService.GetExtensionLanguage(Path.GetExtension(fileName));
			if ((languageSet == null) || (languageSet.Type == LanguageType.Unknown))
			{
				_messageService.ShowError($"{this.Name} request cannot be processed.\r\n\r\nThis non-code file type is not supported.", false);
				return true;
			}

			if (!_fileProcessor.IsDocumentValid(null, out _))
			{
				_messageService.ShowError($"{this.Name} request cannot be processed.\r\n\r\nFile code model cannot be determined.", false);
				return true;
			}

			_dialogService.ShowDialog<CodeBrowserWindow, CodeBrowserViewModel>($"{this.Name}'s collecting file information...", filter);
			return true;
		}

		#endregion
	}
}