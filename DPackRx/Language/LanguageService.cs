using System;
using System.Collections.Generic;

using DPackRx.Services;

namespace DPackRx.Language
{
	/// <summary>
	/// Language service.
	/// </summary>
	public class LanguageService : ILanguageService
	{
		#region Fields

		private readonly ILog _log;
		private readonly ILanguageRegistrationService _languageRegistrationService;
		private bool _cached;
		private readonly object _cacheLock = new object();
		private readonly Dictionary<string, LanguageSettings> _languageFromId = new Dictionary<string, LanguageSettings>();
		private readonly Dictionary<string, LanguageSettings> _languageFromFriendlyName = new Dictionary<string, LanguageSettings>();
		private readonly Dictionary<string, LanguageSettings> _languageFromWebName = new Dictionary<string, LanguageSettings>();
		private readonly Dictionary<string, LanguageSettings> _languageFromWebLanguage = new Dictionary<string, LanguageSettings>();
		private readonly Dictionary<string, LanguageSettings> _languageFromNormalizedExtension =
			new Dictionary<string, LanguageSettings>(StringComparer.OrdinalIgnoreCase);
		private readonly Dictionary<string, LanguageSettings> _languageFromDenormalizedExtension =
			new Dictionary<string, LanguageSettings>(StringComparer.OrdinalIgnoreCase);

		private const string LOG_CATEGORY = "Language";

		#endregion

		public LanguageService(ILog log, ILanguageRegistrationService languageRegistrationService)
		{
			_log = log;
			_languageRegistrationService = languageRegistrationService;
		}

		#region ILanguageService Members

		/// <summary>
		/// Returns language reference for a given language name.
		/// </summary>
		/// <param name="language">Language name.</param>
		/// <returns>Language reference or unknown language if one is not found.</returns>
		public LanguageSettings GetLanguage(string language)
		{
			if (string.IsNullOrEmpty(language))
				return LanguageSettings.UnknownLanguage;

			if (!_cached)
				CacheLanguages();

			if (!_languageFromId.ContainsKey(language))
				return LanguageSettings.UnknownLanguage;

			return _languageFromId[language];
		}

		/// <summary>
		/// Returns language reference for a given language type.
		/// </summary>
		/// <param name="language">Language type.</param>
		/// <returns>Language reference or unknown language if one is not found.</returns>
		public LanguageSettings GetLanguage(LanguageType language)
		{
			if (language == LanguageType.Unknown)
				return LanguageSettings.UnknownLanguage;

			if (!_cached)
				CacheLanguages();

			foreach (var languageSet in _languageFromId.Values)
			{
				if (languageSet.Type == language)
					return languageSet;
			}

			return LanguageSettings.UnknownLanguage;
		}

		/// <summary>
		/// Returns language reference for a given web language name.
		/// </summary>
		/// <param name="name">Web language name.</param>
		/// <returns>Language reference or unknown language if one is not found.</returns>
		public LanguageSettings GetWebNameLanguage(string name)
		{
			if (string.IsNullOrEmpty(name))
				return LanguageSettings.UnknownLanguage;

			if (!_cached)
				CacheLanguages();

			if (_languageFromWebName.ContainsKey(name))
				return _languageFromWebName[name];
			else
				return LanguageSettings.UnknownLanguage;
		}

		/// <summary>
		/// Returns language reference for a given web language.
		/// </summary>
		/// <param name="language">Web language.</param>
		/// <returns>Language reference or unknown language if one is not found.</returns>
		public LanguageSettings GetWebLanguage(string language)
		{
			if (string.IsNullOrEmpty(language))
				return LanguageSettings.UnknownLanguage;

			if (!_cached)
				CacheLanguages();

			if (_languageFromWebLanguage.ContainsKey(language))
				return _languageFromWebLanguage[language];
			else
				return LanguageSettings.UnknownLanguage;
		}

		/// <summary>
		/// Returns language reference for a given file extension.
		/// </summary>
		/// <param name="extension">Extension.</param>
		/// <returns>Language reference or unknown language if one is not found.</returns>
		public LanguageSettings GetExtensionLanguage(string extension)
		{
			if (string.IsNullOrEmpty(extension))
				return LanguageSettings.UnknownLanguage;

			if (!_cached)
				CacheLanguages();

			var ext = GetDenormalizedExtension(extension);
			if (_languageFromDenormalizedExtension.ContainsKey(ext))
				return _languageFromDenormalizedExtension[ext];

			ext = GetNormalizedExtension(extension);
			if (_languageFromNormalizedExtension.ContainsKey(ext))
				return _languageFromNormalizedExtension[ext];

			return LanguageSettings.UnknownLanguage;
		}

		/// <summary>
		/// Returns extension w/o a leading '.'.
		/// </summary>
		/// <param name="extension">Extension.</param>
		/// <returns>Normalized extension.</returns>
		public string GetNormalizedExtension(string extension)
		{
			if (string.IsNullOrEmpty(extension))
				return extension;

			if (extension.StartsWith(".", StringComparison.OrdinalIgnoreCase))
				return extension.Substring(1, extension.Length - 1);
			else
				return extension;
		}

		/// <summary>
		/// Returns extension with a leading '.'.
		/// </summary>
		/// <param name="extension">Extension.</param>
		/// <returns>Denormalized extension.</returns>
		public string GetDenormalizedExtension(string extension)
		{
			if (string.IsNullOrEmpty(extension))
				return extension;

			if (!extension.StartsWith(".", StringComparison.OrdinalIgnoreCase))
				return "." + extension;
			else
				return extension;
		}

		/// <summary>
		/// Checks whether extension is available and enabled for a given language definition.
		/// </summary>
		/// <param name="languageSet">Language reference.</param>
		/// <param name="extension">Extension.</param>
		/// <returns>Extension availability status.</returns>
		public bool IsValidExtension(LanguageSettings languageSet, string extension)
		{
			if (languageSet == null)
				throw new ArgumentNullException(nameof(languageSet));

			if (string.IsNullOrEmpty(extension))
				return false;

			if (extension.StartsWith(".", StringComparison.OrdinalIgnoreCase))
				extension = extension.Substring(1, extension.Length - 1);

			var extensions = languageSet.Extensions;
			if (extensions.ContainsKey(extension))
				return extensions[extension];
			else
				return false;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Caches languages definitions.
		/// </summary>
		private void CacheLanguages()
		{
			if (_cached)
				return;

			lock (_cacheLock)
			{
				if (_cached)
					return;

				var languages = _languageRegistrationService.GetLanguages();

				foreach (var language in languages)
				{
					if (!_languageFromId.ContainsKey(language.Language))
						_languageFromId.Add(language.Language, language);

					// Support for newer project that don't expose project language type via DTE in the conventional form
					if (!string.IsNullOrEmpty(language.ProjectGuid) && !_languageFromId.ContainsKey(language.ProjectGuid))
						_languageFromId.Add(language.ProjectGuid, language);

					if (!_languageFromFriendlyName.ContainsKey(language.FriendlyName))
						_languageFromFriendlyName.Add(language.FriendlyName, language);

					foreach (var webName in language.WebNames)
					{
						if (!string.IsNullOrEmpty(webName) && !_languageFromWebName.ContainsKey(webName))
							_languageFromWebName.Add(webName, language);
					}

					if (!string.IsNullOrEmpty(language.WebLanguage) && !_languageFromWebLanguage.ContainsKey(language.WebLanguage))
						_languageFromWebLanguage.Add(language.WebLanguage, language);

					foreach (var extension in language.Extensions.Keys)
					{
						if (!string.IsNullOrEmpty(extension) && language.Extensions[extension])
						{
							var ext = GetNormalizedExtension(extension);
							if (!_languageFromNormalizedExtension.ContainsKey(ext))
								_languageFromNormalizedExtension.Add(ext, language);

							ext = GetDenormalizedExtension(ext);
							if (!_languageFromDenormalizedExtension.ContainsKey(ext))
								_languageFromDenormalizedExtension.Add(ext, language);
						}
					}
				}

				_cached = true;

				if (_languageFromId.Count == 0)
					throw new ApplicationException("No language definitions were found");

				_log.LogMessage($"Cached {_languageFromId.Count} language definitions", LOG_CATEGORY);
			} // lock
		}

		#endregion
	}
}