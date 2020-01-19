namespace DPackRx.Package
{
	/// <summary>
	/// Supported language constants.
	/// </summary>
	/// <remarks>Must use '{' and '}' brackets for GUIDs here.</remarks>
	public static class LanguageConsts
	{
		// EnvDTE.CodeModelLanguageConstants.vsCMLanguageCSharp
		public const string vsCMLanguageCSharp = "{B5E9BD34-6D3E-4B5D-925E-8A43B79820B4}";

		// EnvDTE.CodeModelLanguageConstants.vsCMLanguageVB
		public const string vsCMLanguageVB = "{B5E9BD33-6D3E-4B5D-925E-8A43B79820B4}";

		// EnvDTE.CodeModelLanguageConstants.vsCMLanguageVC
		public const string vsCMLanguageVC = "{B5E9BD32-6D3E-4B5D-925E-8A43B79820B4}";

		/// <summary>
		/// Unknown language friendly name.
		/// </summary>
		public const string vsLanguageUnknown = "Unknown";

		/// <summary>
		/// Fake J/S language Guid.
		/// </summary>
		public const string vsLanguageJavaScript = "{FFFFFFFF-0001-FFFF-FFFF-FFFFFFFFFFFF}";

		/// <summary>
		/// Fake Xml language Guid.
		/// </summary>
		public const string vsLanguageXml = "{FFFFFFFF-0002-FFFF-FFFF-FFFFFFFFFFFF}";

		/// <summary>
		/// Fake solution items project language Guid.
		/// </summary>
		public const string vsLanguageSolutionItems = "{FFFFFFFF-0000-FFFF-FFFF-FFFFFFFFFFFF}";
	}
}