using System;

namespace DPackRx.Package
{
	/// <summary>
	/// Must match VSCT resources.
	/// </summary>
	public static class GUIDs
	{
		/// <summary>
		/// Must match VSIX manifest Product ID.
		/// </summary>
		public const string PRODUCT_ID = "EE57E48A-8422-4CEC-B415-3E3BA31F1199";

		public static readonly Guid CommandSet = new Guid("{BD0B0377-DC68-4C27-A9A3-D240F33367D4}");

		public const string GUID_XAML_EDITOR_FACTORY = "4C87B692-1202-46AA-B64C-EF01FAEC53DA";
		public const string GUID_XAML_LANGUAGE_SERVICE = "C9164055-039B-4669-832D-F257BD5554D4";
		public const string GUID_NEW_XAML_LANGUAGE_SERVICE = "CD53C9A1-6BC2-412B-BE36-CC715ED8DD41";
		public const string GUID_XAML_DESIGNER = "2E06F766-183C-46F9-B8AC-BC4FB78480A7";

		public const string OPTIONS_GENERAL = "F26A055B-47EC-4642-839F-3240A2594742";
		public const string OPTIONS_FILE_BROWSER = "B998A0CD-69D6-4980-9240-5EC65F3B6E02";
	}
}