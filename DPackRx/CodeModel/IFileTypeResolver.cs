using DPackRx.Language;

namespace DPackRx.CodeModel
{
	/// <summary>
	/// Resolves project item types and subtypes.
	/// </summary>
	public interface IFileTypeResolver
	{
		/// <summary>
		/// Returns project's language.
		/// </summary>
		/// <param name="project">Project. Untyped extensibility link (name matches the actual type).</param>
		/// <param name="isWebProject">Whether it's a web project.</param>
		/// <returns>Language.</returns>
		LanguageSettings GetCurrentLanguage(object project, out bool isWebProject);

		/// <summary>
		/// Returns item's file sub-type.
		/// </summary>
		/// <param name="projectItem">Project item. Untyped extensibility link (name matches the actual type).</param>
		/// <param name="languageSet">Language.</param>
		/// <param name="isWebProject">Whether item is part of web project.</param>
		/// <returns>File sub-type.</returns>
		FileSubType GetSubType(object projectItem, LanguageSettings languageSet, bool isWebProject);

		/// <summary>
		/// Returns item's file sub-type based on file's extension.
		/// </summary>
		/// <param name="projectItem">Project item. Untyped extensibility link (name matches the actual type).</param>
		/// <param name="languageSet">Language.</param>
		/// <param name="isWebProject">Whether item is part of web project.</param>
		/// <returns>File sub-type.</returns>
		FileSubType GetExtensionSubType(object projectItem, LanguageSettings languageSet, bool isWebProject);

		/// <summary>
		/// Checks whether a given file sub-type is a code one.
		/// </summary>
		/// <param name="itemSubType">File sub-type.</param>
		/// <returns>File sub-type status.</returns>
		bool IsCodeSubType(FileSubType itemSubType);

		/// <summary>
		/// Checks whether a given file sub-type is a web one with both design and code views.
		/// </summary>
		/// <param name="itemSubType">File sub-type.</param>
		/// <returns>File sub-type status.</returns>
		bool IsDesignAndCodeSubType(FileSubType itemSubType);

		/// <summary>
		/// Checks whether a given file sub-type is WinForms that supports design surface.
		/// </summary>
		/// <param name="itemSubType">File sub-type.</param>
		/// <returns>File sub-type status.</returns>
		bool IsWinDesignSubType(FileSubType itemSubType);

		/// <summary>
		/// Checks whether a given file sub-type is for web file that is represented by a single code file only
		/// (like Generic Handler web file, for instance).
		/// </summary>
		/// <param name="itemSubType">File sub-type.</param>
		/// <param name="languageSet">Language.</param>
		/// <param name="isWebProject">Whether item is part of web project.</param>
		/// <returns>File sub-type status.</returns>
		bool IsWebCodeOnlySubType(FileSubType itemSubType, LanguageSettings languageSet, bool isWebProject);

		/// <summary>
		/// Checks whether a given file is a .Designer generated file.
		/// </summary>
		/// <param name="projectItem">Project item. Untyped extensibility link (name matches the actual type).</param>
		/// <param name="itemSubType">File sub-type.</param>
		/// <param name="languageSet">Language.</param>
		/// <param name="isWebProject">Whether item is part of web project.</param>
		/// <returns>File status.</returns>
		bool IsDesignerItem(object projectItem, FileSubType itemSubType, LanguageSettings languageSet, bool isWebProject);

		/// <summary>
		/// Checks whether a given file sub-type is a code one.
		/// </summary>
		/// <param name="projectItem">Project item. Untyped extensibility link (name matches the actual type).</param>
		/// <param name="languageSet">Language.</param>
		/// <param name="isWebProject">Whether item is part of web project.</param>
		/// <returns>File sub-type status.</returns>
		bool IsCodeItem(object projectItem, LanguageSettings languageSet, bool isWebProject);

		/// <summary>
		/// Checks whether a given file sub-type is an Xml one.
		/// </summary>
		/// <param name="projectItem">Project item. Untyped extensibility link (name matches the actual type).</param>
		/// <param name="languageSet">Language.</param>
		/// <param name="isWebProject">Whether item is part of web project.</param>
		/// <returns>File sub-type status.</returns>
		bool IsXamlItem(object projectItem, LanguageSettings languageSet, bool isWebProject);

		/// <summary>
		/// Checks whether a given file sub-type is a JavaScript one.
		/// </summary>
		/// <param name="projectItem">Project item. Untyped extensibility link (name matches the actual type).</param>
		/// <param name="languageSet">Language.</param>
		/// <param name="isWebProject">Whether item is part of web project.</param>
		/// <returns>File sub-type status.</returns>
		bool IsJavaScriptItem(object projectItem, LanguageSettings languageSet, bool isWebProject);
	}
}