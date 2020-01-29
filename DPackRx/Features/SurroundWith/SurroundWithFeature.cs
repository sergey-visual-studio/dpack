using System;
using System.Collections.Generic;
using System.Windows.Input;

using DPackRx.CodeModel;
using DPackRx.Language;
using DPackRx.Options;
using DPackRx.Package;
using DPackRx.Services;

namespace DPackRx.Features.SurroundWith
{
	/// <summary>
	/// Surround with feature.
	/// </summary>
	[KnownFeature(KnownFeature.SurroundWith)]
	[OptionsDefaults("Logging", false)]
	public class SurroundWithFeature : Feature
	{
		#region Fields

		private readonly IShellHelperService _shellHelperService;
		private readonly IShellSelectionService _shellSelectionService;
		private readonly IFileTypeResolver _fileTypeResolver;
		private readonly IKeyboardService _keyboardService;

		protected internal const string SURROUND_WITH_COMMAND = "Edit.SurroundWith";
		protected internal const string SNIPPET_TRY_CATCH = "try";
		protected internal const string SNIPPET_TRY_FINALLY = "tryf";
		protected internal const string SNIPPET_FOR = "for";
		protected internal const string SNIPPET_FOR_EACH = "foreach";
		protected internal const string SNIPPET_REGION = "#region";

		#endregion

		public SurroundWithFeature(IServiceProvider serviceProvider, ILog log, IOptionsService optionsService,
			IShellHelperService shellHelperService, IShellSelectionService shellSelectionService,
			IFileTypeResolver fileTypeResolver, IKeyboardService keyboardService) : base(serviceProvider, log, optionsService)
		{
			_shellHelperService = shellHelperService;
			_shellSelectionService = shellSelectionService;
			_fileTypeResolver = fileTypeResolver;
			_keyboardService = keyboardService;
		}

		// Test constructor
		protected internal SurroundWithFeature() : base()
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
				CommandIDs.SW_TRY_CATCH,
				CommandIDs.SW_TRY_FINALLY,
				CommandIDs.SW_FOR,
				CommandIDs.SW_FOR_EACH,
				CommandIDs.SW_REGION
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
				case CommandIDs.SW_TRY_CATCH:
				case CommandIDs.SW_TRY_FINALLY:
				case CommandIDs.SW_FOR:
				case CommandIDs.SW_FOR_EACH:
				case CommandIDs.SW_REGION:
					return
						_shellSelectionService.IsContextActive(ContextType.SolutionExists) && (
						_shellSelectionService.IsContextActive(ContextType.TextEditor) ||
						_shellSelectionService.IsContextActive(ContextType.XMLTextEditor) ||
						_shellSelectionService.IsContextActive(ContextType.XamlEditor) ||
						_shellSelectionService.IsContextActive(ContextType.NewXamlEditor) ||
						_shellSelectionService.IsContextActive(ContextType.HTMLSourceEditor) ||
						_shellSelectionService.IsContextActive(ContextType.CSSTextEditor));
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
				case CommandIDs.SW_TRY_CATCH:
					return ExecuteSnippet(SNIPPET_TRY_CATCH);
				case CommandIDs.SW_TRY_FINALLY:
					return ExecuteSnippet(SNIPPET_TRY_FINALLY);
				case CommandIDs.SW_FOR:
					return ExecuteSnippet(SNIPPET_FOR);
				case CommandIDs.SW_FOR_EACH:
					return ExecuteSnippet(SNIPPET_FOR_EACH);
				case CommandIDs.SW_REGION:
					return ExecuteSnippet(SNIPPET_REGION);
				default:
					return base.Execute(commandId);
			}
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Executes a given surround with snippet.
		/// </summary>
		private bool ExecuteSnippet(string snippet)
		{
			var project = _shellSelectionService.GetActiveProject();
			var languageSet = _fileTypeResolver.GetCurrentLanguage(project, out _);
			if ((languageSet?.Type == LanguageType.Unknown) || !languageSet.SurroundWith)
				return true;

			_shellHelperService.ExecuteCommand(SURROUND_WITH_COMMAND);
			_keyboardService.Type(snippet);
			_keyboardService.Type(Key.Enter);
			return true;
		}

		#endregion
	}
}