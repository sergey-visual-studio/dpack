namespace DPackRx.Language
{
	/// <summary>
	/// Language service.
	/// </summary>
	public interface ILanguageService
	{
		/// <summary>
		/// Returns language reference for a given language name.
		/// </summary>
		/// <param name="language">Language name.</param>
		/// <returns>Language reference or unknown language if one is not found.</returns>
		LanguageSettings GetLanguage(string language);

		/// <summary>
		/// Returns language reference for a given language type.
		/// </summary>
		/// <param name="language">Language type.</param>
		/// <returns>Language reference or unknown language if one is not found.</returns>
		LanguageSettings GetLanguage(LanguageType language);

		/// <summary>
		/// Returns language reference for a given web language name.
		/// </summary>
		/// <param name="name">Web language name.</param>
		/// <returns>Language reference or unknown language if one is not found.</returns>
		LanguageSettings GetWebNameLanguage(string name);

		/// <summary>
		/// Returns language reference for a given web language.
		/// </summary>
		/// <param name="language">Web language.</param>
		/// <returns>Language reference or unknown language if one is not found.</returns>
		LanguageSettings GetWebLanguage(string language);

		/// <summary>
		/// Returns language reference for a given file extension.
		/// </summary>
		/// <param name="extension">Extension.</param>
		/// <returns>Language reference or unknown language if one is not found.</returns>
		LanguageSettings GetExtensionLanguage(string extension);

		/// <summary>
		/// Returns extension w/o a leading '.'.
		/// </summary>
		/// <param name="extension">Extension.</param>
		/// <returns>Normalized extension.</returns>
		string GetNormalizedExtension(string extension);

		/// <summary>
		/// Returns extension with a leading '.'.
		/// </summary>
		/// <param name="extension">Extension.</param>
		/// <returns>Denormalized extension.</returns>
		string GetDenormalizedExtension(string extension);

		/// <summary>
		/// Checks whether extension is available and enabled for a given language definition.
		/// </summary>
		/// <param name="languageSet">Language reference.</param>
		/// <param name="extension">Extension.</param>
		/// <returns>Extension availability status.</returns>
		bool IsValidExtension(LanguageSettings languageSet, string extension);
	}
}