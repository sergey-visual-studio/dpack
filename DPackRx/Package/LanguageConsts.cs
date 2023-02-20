namespace DPackRx.Package
{
	/// <summary>
	/// Supported language constants.
	/// </summary>
	/// <remarks>Must use '{' and '}' brackets for GUIDs here.</remarks>
	public static class LanguageConsts
	{
		// EnvDTE.CodeModelLanguageConstants.vsCMLanguageCSharp
		public const string VS_CM_LANGUAGE_CSHARP = "{B5E9BD34-6D3E-4B5D-925E-8A43B79820B4}";

		// EnvDTE.CodeModelLanguageConstants.vsCMLanguageVB
		public const string VS_CM_LANGUAGE_VB = "{B5E9BD33-6D3E-4B5D-925E-8A43B79820B4}";

		// EnvDTE.CodeModelLanguageConstants.vsCMLanguageVC
		public const string VS_CM_LANGUAGE_VC = "{B5E9BD32-6D3E-4B5D-925E-8A43B79820B4}";

		/// <summary>
		/// Unknown language friendly name.
		/// </summary>
		public const string VS_LANGUAGE_UNKNOWN = "Unknown";

		/// <summary>
		/// Fake J/S language Guid.
		/// </summary>
		public const string VS_LANGUAGE_JAVA_SCRIPT = "{FFFFFFFF-0001-FFFF-FFFF-FFFFFFFFFFFF}";

		/// <summary>
		/// Fake Xml language Guid.
		/// </summary>
		public const string VS_LANGUAGE_XML = "{FFFFFFFF-0002-FFFF-FFFF-FFFFFFFFFFFF}";

		/// <summary>
		/// Fake solution items project language Guid.
		/// </summary>
		public const string VS_LANGUAGE_SOLUTION_ITEMS = "{FFFFFFFF-0000-FFFF-FFFF-FFFFFFFFFFFF}";

		/// <summary>
		/// VS Sql project language Guid.
		/// </summary>
		public const string VS_LANGUAGE_SQL = "{00D1A9C2-B5F0-4AF3-8072-F6C62B433612}";
	}
}