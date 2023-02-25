using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DPackRx.CodeModel;
using DPackRx.Language;
using DPackRx.Options;
using DPackRx.Package;
using DPackRx.Services;

using Newtonsoft.Json;

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

		private readonly IShellSelectionService _shellSelectionService;
		private readonly IFileTypeResolver _fileTypeResolver;
		private readonly ISurroundWithFormatterService _surroundWithFormatterService;
		private SurroundWithModel _model;

		#endregion

		public SurroundWithFeature(IServiceProvider serviceProvider, ILog log, IOptionsService optionsService,
			IShellSelectionService shellSelectionService, IFileTypeResolver fileTypeResolver,
			ISurroundWithFormatterService surroundWithFormatterService) : base(serviceProvider, log, optionsService)
		{
			_shellSelectionService = shellSelectionService;
			_fileTypeResolver = fileTypeResolver;
			_surroundWithFormatterService = surroundWithFormatterService;
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
					var project = _shellSelectionService.GetActiveProject();
					var languageSet = _fileTypeResolver.GetCurrentLanguage(project, out _);
					if ((languageSet?.Type == LanguageType.Unknown) || !languageSet.SurroundWith)
						return false;

					var result =
						_shellSelectionService.IsContextActive(ContextType.SolutionExists) && (
						_shellSelectionService.IsContextActive(ContextType.TextEditor) ||
						_shellSelectionService.IsContextActive(ContextType.XMLTextEditor) ||
						_shellSelectionService.IsContextActive(ContextType.XamlEditor) ||
						_shellSelectionService.IsContextActive(ContextType.NewXamlEditor) ||
						_shellSelectionService.IsContextActive(ContextType.HTMLSourceEditor) ||
						_shellSelectionService.IsContextActive(ContextType.CSSTextEditor));
					return result;
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
			SurroundWithLanguage language = GetSurroundWithLanguage();
			IEnumerable<SurroundWithLanguageModel> langugeModels = GetSurroundWithModel(language);
			if (langugeModels == null)
				return false;

			SurroundWithType type;
			switch (commandId)
			{
				case CommandIDs.SW_TRY_CATCH:
					type = SurroundWithType.TryCatch;
					break;
				case CommandIDs.SW_TRY_FINALLY:
					type = SurroundWithType.TryFinally;
					break;
				case CommandIDs.SW_FOR:
					type = SurroundWithType.For;
					break;
				case CommandIDs.SW_FOR_EACH:
					type = SurroundWithType.ForEach;
					break;
				case CommandIDs.SW_REGION:
					type = SurroundWithType.Region;
					break;
				default:
					return base.Execute(commandId);
			}

			SurroundWithLanguageModel model = langugeModels.FirstOrDefault(m => m.Type == type);
			if (model == null)
				return base.Execute(commandId);

			_surroundWithFormatterService.Format(model);
			return true;
		}

		#endregion

		#region Private Methods

		private SurroundWithLanguage GetSurroundWithLanguage()
		{
			var project = _shellSelectionService.GetActiveProject();
			var languageSet = _fileTypeResolver.GetCurrentLanguage(project, out _);

			switch (languageSet.Type)
			{
				case LanguageType.CSharp:
					return SurroundWithLanguage.CSharp;
				case LanguageType.VB:
					return SurroundWithLanguage.VB;
				case LanguageType.CPP:
					return SurroundWithLanguage.CPP;
				default:
					return SurroundWithLanguage.None;
			}
		}

		private IEnumerable<SurroundWithLanguageModel> GetSurroundWithModel(SurroundWithLanguage language)
		{
			if (_model == null)
			{
				using (Stream stream = GetType().Assembly.GetManifestResourceStream($"{GetType().Namespace}.SurroundWith.json"))
				{
					using (StreamReader reader = new StreamReader(stream))
					{
						string content = reader.ReadToEnd();
						_model = JsonConvert.DeserializeObject<SurroundWithModel>(content);
					}
				}
			}

			return _model?.Models.Where(m => m.Language == language);
		}

		#endregion
	}
}