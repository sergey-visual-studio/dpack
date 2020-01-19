namespace DPackRx.CodeModel
{
	/// <summary>
	/// Known file sub-types.
	/// </summary>
	public enum FileSubType
	{
		None = 0, // used for initialization only
		Misc,
		Code,
		WinForm,
		Component,
		UserControl,
		WebForm,
		WebFormCode,
		WebControl,
		WebControlCode,
		WebService,
		WebServiceCode,
		WebAppFile,
		WebAppFileCode,
		ConfigFile,
		ResourceFile,
		Bitmap,
		Icon,
		Cursor,
		ImageFile, // generic image file
		XmlFile,
		XmlSchema,
		XslFile,
		XsxFile,
		HtmlFile,
		StyleSheet,
		JScript,
		WinHostScript,
		Settings,
		ClassDiagram,
		WebGenericHandler,
		WebMasterPage,
		WebMasterPageCode,
		WebSiteMap,
		XamlFile,
		SqlFile
	}
}