using System;
using System.IO;

using DPackRx.Language;
using DPackRx.Services;

using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.VCProjectEngine;

namespace DPackRx.CodeModel
{
	/// <summary>
	/// Resolves project item types and subtypes.
	/// </summary>
	public class FileTypeResolver : IFileTypeResolver
	{
		#region Fields

		private readonly ILog _log;
		private readonly ILanguageService _languageService;
		private readonly IShellProjectService _shellProjectService;

		// DTE "SubType" values:
		// "" - miscellaneous file of unknown type
		// "Code" - code file
		// "Form" - WinForms - code file, WebForms - non-code file
		// "Component" - non-web - component code file, web - non-code file
		// "UserControl" - non-web - component code file, web - non-code file
		// "ASPXCodeBehind" - web code file
		// "Designer" - file containing generic designer - not supported
		// "Preview" - file containing VC/VB 6 code - not supported
		// private const string SUB_TYPE_CODE = "Code"; // .asax for web
		private const string SUB_TYPE_FORM = "Form"; // .aspx for web 
		private const string SUB_TYPE_COMPONENT = "Component"; // .asax for web
		private const string SUB_TYPE_USER_CONTROL = "UserControl"; // .ascx for web

		// private const string PROP_IS_DEPENDENT_FILE = "IsDependentFile";

		private const string DESIGNER_EXT = ".designer";

		#endregion

		public FileTypeResolver(ILog log, ILanguageService languageService, IShellProjectService shellProjectService)
		{
			_log = log;
			_languageService = languageService;
			_shellProjectService = shellProjectService;
		}

		#region IFileTypeResolver Members

		/// <summary>
		/// Returns project's language.
		/// </summary>
		/// <param name="project">Project. Untyped extensibility link (name matches the actual type).</param>
		/// <param name="isWebProject">Whether it's a web project.</param>
		/// <returns>Language.</returns>
		public LanguageSettings GetCurrentLanguage(object project, out bool isWebProject)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			isWebProject = false;

			if (project == null)
				return LanguageSettings.UnknownLanguage;

			var dteProject = project as Project;
			if (dteProject == null)
				throw new ArgumentNullException(nameof(project));

			try
			{
				var projectName = dteProject.Name;
				if (projectName != null)
				{
					if (!string.IsNullOrEmpty(projectName))
						_log.LogMessage($"Check '{projectName}' project language");
					else
						_log.LogMessage("Check unnamed project language");
				}

				var languageSet = LanguageSettings.UnknownLanguage;
				var language = _shellProjectService.GetProjectLanguage(project);
				if (language != null)
				{
					if (language == string.Empty)
						language = _shellProjectService.GetNoCodeModelProjectLanguage(project);

					if (!string.IsNullOrEmpty(language))
					{
						languageSet = _languageService.GetLanguage(language);

						if (languageSet?.Type != LanguageType.Unknown)
						{
							// C++ doesn't expose project type
							if (languageSet.Type != LanguageType.CPP)
								isWebProject = _shellProjectService.IsWebProject(project);
						}
					}
				}

				_log.LogMessage($"'{projectName}' project language is {languageSet?.FriendlyName}");
				return languageSet;
			}
			catch (NotImplementedException) // This is an acceptable condition
			{
				_log.LogMessage("Project doesn't implement code model");
				return LanguageSettings.UnknownLanguage;
			}
			catch (Exception ex) // But this is a legit problem - raise an exception
			{
				_log.LogMessage("Failed to check project language", ex);
				throw;
			}
		}

		/// <summary>
		/// Returns item's file sub-type.
		/// </summary>
		/// <param name="projectItem">Project item. Untyped extensibility link (name matches the actual type).</param>
		/// <param name="languageSet">Language.</param>
		/// <param name="isWebProject">Whether item is part of web project.</param>
		/// <returns>File sub-type.</returns>
		public FileSubType GetSubType(object projectItem, LanguageSettings languageSet, bool isWebProject)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			return GetSubType(projectItem, languageSet, isWebProject, false);
		}

		/// <summary>
		/// Returns item's file sub-type based on file's extension.
		/// </summary>
		/// <param name="projectItem">Project item. Untyped extensibility link (name matches the actual type).</param>
		/// <param name="languageSet">Language.</param>
		/// <param name="isWebProject">Whether item is part of web project.</param>
		/// <returns>File sub-type.</returns>
		public FileSubType GetExtensionSubType(object projectItem, LanguageSettings languageSet, bool isWebProject)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var dteItem = projectItem as ProjectItem;
			if (dteItem == null)
				return FileSubType.None;

			var extension = Path.GetExtension(dteItem.Name);
			return GetExtensionSubType(extension);
		}

		/// <summary>
		/// Checks whether a given file sub-type is a code one.
		/// </summary>
		/// <param name="itemSubType">File sub-type.</param>
		/// <param name="miscFilesAsCode">Treat miscellaneous files as code ones.</param>
		/// <returns>File sub-type status.</returns>
		public bool IsCodeSubType(FileSubType itemSubType, bool miscFilesAsCode = true)
		{
			switch (itemSubType)
			{
				case FileSubType.Code:
				case FileSubType.WinForm:
				case FileSubType.Component:
				case FileSubType.UserControl:
				case FileSubType.WebFormCode:
				case FileSubType.WebControlCode:
				case FileSubType.WebServiceCode:
				case FileSubType.WebAppFileCode:
				case FileSubType.WebGenericHandler:
				case FileSubType.WebMasterPageCode:
					return true;
				case FileSubType.JScript:
				case FileSubType.XmlFile:
				case FileSubType.ConfigFile:
					return miscFilesAsCode;
				default:
					return false;
			}
		}

		/// <summary>
		/// Checks whether a given file sub-type is a web one with both design and code views.
		/// </summary>
		/// <param name="itemSubType">File sub-type.</param>
		/// <returns>File sub-type status.</returns>
		public bool IsDesignAndCodeSubType(FileSubType itemSubType)
		{
			switch (itemSubType)
			{
				case FileSubType.WebForm:
				case FileSubType.WebControl:
				case FileSubType.WebAppFile:
				case FileSubType.WebService:
				case FileSubType.XmlSchema:
				case FileSubType.XamlFile:
					return true;
				default:
					return false;
			}
		}

		/// <summary>
		/// Checks whether a given file sub-type is WinForms that supports design surface.
		/// </summary>
		/// <param name="itemSubType">File sub-type.</param>
		/// <returns>File sub-type status.</returns>
		public bool IsWinDesignSubType(FileSubType itemSubType)
		{
			switch (itemSubType)
			{
				case FileSubType.WinForm:
				case FileSubType.Component:
				case FileSubType.UserControl:
					return true;
				default:
					return false;
			}
		}

		/// <summary>
		/// Checks whether a given file sub-type is for web file that is represented by a single code file only
		/// (like Generic Handler web file, for instance).
		/// </summary>
		/// <param name="itemSubType">File sub-type.</param>
		/// <param name="languageSet">Language.</param>
		/// <param name="isWebProject">Whether item is part of web project.</param>
		/// <returns>File sub-type status.</returns>
		public bool IsWebCodeOnlySubType(FileSubType itemSubType, LanguageSettings languageSet, bool isWebProject)
		{
			switch (itemSubType)
			{
				case FileSubType.WebGenericHandler:
					return true;
				default:
					return false;
			}
		}

		/// <summary>
		/// Checks whether a given file is a .Designer generated file.
		/// </summary>
		/// <param name="projectItem">Project item. Untyped extensibility link (name matches the actual type).</param>
		/// <param name="itemSubType">File sub-type.</param>
		/// <param name="languageSet">Language.</param>
		/// <param name="isWebProject">Whether item is part of web project.</param>
		/// <returns>File status.</returns>
		public bool IsDesignerItem(object projectItem, FileSubType itemSubType, LanguageSettings languageSet, bool isWebProject)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			if (languageSet?.Type == LanguageType.Unknown)
				return false;

			if ((languageSet.DesignerFiles == LanguageDesignerFiles.NotSupported) || isWebProject || !IsCodeSubType(itemSubType))
				return false;

			var dteItem = projectItem as ProjectItem;
			if (dteItem == null)
				return false;

			var checkFileName = true;

			// If language is fully supported then it must expose the below property
			// Otherwise we must rely on unconditional file name check
			if (languageSet.DesignerFiles == LanguageDesignerFiles.FullySupported)
			{
				// TODO: revisit when substituted with less performance intensive API
				//try
				//{
				//	// Various files are reported as .Designer - further file name check is required
				//	var props = dteItem.Properties;
				//	if (props == null)
				//		return false;

				//	var prop = props.Item(PROP_IS_DEPENDENT_FILE);
				//	if (prop != null)
				//		checkFileName = (bool)prop.Value;
				//	else
				//		return false;
				//}
				//catch
				//{
				//	return false;
				//}
			}

			// Check if last part of the file name ends with .Designer
			if (checkFileName)
			{
				var fileName = dteItem.get_FileNames(1);
				if ((fileName != null) && (fileName != string.Empty))
				{
					fileName = Path.GetFileNameWithoutExtension(fileName);
					if (fileName.EndsWith(DESIGNER_EXT, StringComparison.OrdinalIgnoreCase))
						return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Checks whether a given file sub-type is a code one.
		/// </summary>
		/// <param name="projectItem">Project item. Untyped extensibility link (name matches the actual type).</param>
		/// <param name="languageSet">Language.</param>
		/// <param name="isWebProject">Whether item is part of web project.</param>
		/// <returns>File sub-type status.</returns>
		public bool IsCodeItem(object projectItem, LanguageSettings languageSet, bool isWebProject)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var dteItem = projectItem as ProjectItem;
			if (dteItem == null)
				return false;

			var itemSubType = GetSubType(projectItem, languageSet, isWebProject, true);
			_log.LogMessage($"'{dteItem.Name}' sub-type is {itemSubType}");

			return IsCodeSubType(itemSubType);
		}

		/// <summary>
		/// Checks whether a given file sub-type is an Xml one.
		/// </summary>
		/// <param name="projectItem">Project item. Untyped extensibility link (name matches the actual type).</param>
		/// <param name="languageSet">Language.</param>
		/// <param name="isWebProject">Whether item is part of web project.</param>
		/// <returns>File sub-type status.</returns>
		public bool IsXamlItem(object projectItem, LanguageSettings languageSet, bool isWebProject)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var dteItem = projectItem as ProjectItem;
			if (dteItem == null)
				return false;

			string fileName = dteItem.get_FileNames(1);
			if (string.IsNullOrEmpty(fileName))
				return false;

			var extension = Path.GetExtension(fileName);
			if (string.Compare(extension, FileType.XAML, StringComparison.OrdinalIgnoreCase) == 0)
				return true;
			else
				return false;
		}

		/// <summary>
		/// Checks whether a given file sub-type is a JavaScript one.
		/// </summary>
		/// <param name="projectItem">Project item. Untyped extensibility link (name matches the actual type).</param>
		/// <param name="languageSet">Language.</param>
		/// <param name="isWebProject">Whether item is part of web project.</param>
		/// <returns>File sub-type status.</returns>
		public bool IsJavaScriptItem(object projectItem, LanguageSettings languageSet, bool isWebProject)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var dteItem = projectItem as ProjectItem;
			if (dteItem == null)
				return false;

			var extension = Path.GetExtension(dteItem.get_FileNames(1));
			return _languageService.IsValidExtension(languageSet, extension);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Returns project item file type.
		/// </summary>
		private FileSubType GetSubType(object projectItem, LanguageSettings languageSet, bool isWebProject, bool ignoreMisc)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			if (languageSet?.Type == LanguageType.Unknown)
				return FileSubType.None;

			var dteItem = projectItem as ProjectItem;
			if (dteItem == null)
				throw new ArgumentNullException(nameof(projectItem));

			if (languageSet.Type == LanguageType.CPP)
			{
				return GetCppSubType(dteItem, languageSet);
			}
			else
			{
				var fileExt = Path.GetExtension(dteItem.Name);
				var codeFile = _languageService.IsValidExtension(languageSet, fileExt);

				if (codeFile) // This is a code file of some sort
				{
					if (isWebProject) // File is part of web-based project such as web app or web service
					{
						fileExt = Path.GetExtension(Path.GetFileNameWithoutExtension(dteItem.Name));
						if (string.IsNullOrEmpty(fileExt))
							return FileSubType.Code;
						else
						{
							if (fileExt.Equals(FileType.ASPX, StringComparison.OrdinalIgnoreCase))
								return FileSubType.WebFormCode;
							else if (fileExt.Equals(FileType.ASCX, StringComparison.OrdinalIgnoreCase))
								return FileSubType.WebControlCode;
							else if (fileExt.Equals(FileType.ASMX, StringComparison.OrdinalIgnoreCase) ||
											 fileExt.Equals(FileType.ASM, StringComparison.OrdinalIgnoreCase))
								return FileSubType.WebServiceCode;
							else if (fileExt.Equals(FileType.ASAX, StringComparison.OrdinalIgnoreCase))
								return FileSubType.WebAppFileCode;
							else if (fileExt.Equals(FileType.MASTER_PAGE, StringComparison.OrdinalIgnoreCase))
								return FileSubType.WebMasterPageCode;
							else
								return FileSubType.Code;
						}
					}
					else // File is part of non-web project such as win app or assembly
					{
						string subType = GetProjectItemSubType(dteItem);
						switch (subType)
						{
							case SUB_TYPE_FORM:
								return FileSubType.WinForm;
							case SUB_TYPE_COMPONENT:
								return FileSubType.Component;
							case SUB_TYPE_USER_CONTROL:
								return FileSubType.UserControl;
							default:
								return FileSubType.Code;
						}
					}
				}
				else
				{
					// This is not a code file, or file that can only be detected by its extension
					if (ignoreMisc)
					{
						switch (fileExt)
						{
							case FileType.ASHX:
								return FileSubType.WebGenericHandler;
							default:
								return FileSubType.Misc;
						}
					}
					else
						return GetExtensionSubType(fileExt);
				}
			}
		}

		/// <summary>
		/// Returns project item "SubType" property.
		/// Not used for C++ or web based projects.
		/// </summary>
		private string GetProjectItemSubType(ProjectItem projectItem)
		{
			var subType = _shellProjectService.GetProjectItemSubType(projectItem);
			if (subType == null)
				subType = string.Empty;

			return subType;
		}

		/// <summary>
		/// Returns project item file type based on file's extension.
		/// </summary>
		private FileSubType GetExtensionSubType(string extension)
		{
			if (extension.Equals(FileType.ASPX, StringComparison.OrdinalIgnoreCase))
				return FileSubType.WebForm;
			else if (extension.Equals(FileType.ASCX, StringComparison.OrdinalIgnoreCase))
				return FileSubType.WebControl;
			else if (extension.Equals(FileType.ASMX, StringComparison.OrdinalIgnoreCase) ||
							 extension.Equals(FileType.ASM, StringComparison.OrdinalIgnoreCase))
				return FileSubType.WebService;
			else if (extension.Equals(FileType.ASAX, StringComparison.OrdinalIgnoreCase))
				return FileSubType.WebAppFile;
			else if (extension.Equals(FileType.CONFIG, StringComparison.OrdinalIgnoreCase))
				return FileSubType.ConfigFile;
			else if (extension.Equals(FileType.RESX, StringComparison.OrdinalIgnoreCase) ||
							 extension.Equals(FileType.RES, StringComparison.OrdinalIgnoreCase) ||
							 extension.Equals(FileType.RC, StringComparison.OrdinalIgnoreCase))
				return FileSubType.ResourceFile;
			else if (extension.Equals(FileType.BMP, StringComparison.OrdinalIgnoreCase))
				return FileSubType.Bitmap;
			else if (extension.Equals(FileType.ICO, StringComparison.OrdinalIgnoreCase))
				return FileSubType.Icon;
			else if (extension.Equals(FileType.CUR, StringComparison.OrdinalIgnoreCase))
				return FileSubType.Cursor;
			else if (extension.Equals(FileType.JPG, StringComparison.OrdinalIgnoreCase) ||
							 extension.Equals(FileType.JPEG, StringComparison.OrdinalIgnoreCase) ||
							 extension.Equals(FileType.GIF, StringComparison.OrdinalIgnoreCase) ||
							 extension.Equals(FileType.PNG, StringComparison.OrdinalIgnoreCase))
				return FileSubType.ImageFile;
			else if (extension.Equals(FileType.XML, StringComparison.OrdinalIgnoreCase))
				return FileSubType.XmlFile;
			else if (extension.Equals(FileType.XML_SCHEMA, StringComparison.OrdinalIgnoreCase))
				return FileSubType.XmlSchema;
			else if (extension.Equals(FileType.XSLT_SCHEMA, StringComparison.OrdinalIgnoreCase) ||
							 extension.Equals(FileType.XSLT_SCHEMA, StringComparison.OrdinalIgnoreCase) ||
							 extension.Equals(FileType.XSX_SCHEMA, StringComparison.OrdinalIgnoreCase))
				return FileSubType.XslFile;
			else if (extension.Equals(FileType.HTM, StringComparison.OrdinalIgnoreCase) ||
							 extension.Equals(FileType.HTML, StringComparison.OrdinalIgnoreCase))
				return FileSubType.HtmlFile;
			else if (extension.Equals(FileType.CSS, StringComparison.OrdinalIgnoreCase))
				return FileSubType.StyleSheet;
			else if (extension.Equals(FileType.ASHX, StringComparison.OrdinalIgnoreCase))
				return FileSubType.WebGenericHandler;
			else if (extension.Equals(FileType.MASTER_PAGE, StringComparison.OrdinalIgnoreCase))
				return FileSubType.WebMasterPage;
			else if (extension.Equals(FileType.SITE_MAP, StringComparison.OrdinalIgnoreCase))
				return FileSubType.WebSiteMap;
			else if (extension.Equals(FileType.JSCRIPT, StringComparison.OrdinalIgnoreCase))
				return FileSubType.JScript;
			else if (extension.Equals(FileType.WIN_HOST_SCRIPT, StringComparison.OrdinalIgnoreCase))
				return FileSubType.WinHostScript;
			else if (extension.Equals(FileType.SETTINGS, StringComparison.OrdinalIgnoreCase))
				return FileSubType.Settings;
			else if (extension.Equals(FileType.CLASS_DIAGRAM, StringComparison.OrdinalIgnoreCase))
				return FileSubType.ClassDiagram;
			else if (extension.Equals(FileType.XAML, StringComparison.OrdinalIgnoreCase))
				return FileSubType.XamlFile;
			else if (extension.Equals(FileType.SQL, StringComparison.OrdinalIgnoreCase))
				return FileSubType.SqlFile;
			else
				return FileSubType.Misc;
		}

		/// <summary>
		/// Returns C++ project item file type.
		/// </summary>
		private FileSubType GetCppSubType(ProjectItem projectItem, LanguageSettings languageSet)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			if (!_shellProjectService.IsCppFile(projectItem))
				return FileSubType.Misc;

			var file = GetCppFile(projectItem);
			if (file == null)
				return FileSubType.Misc;

			switch (file.FileType)
			{
				case eFileType.eFileTypeCppCode:
				case eFileType.eFileTypeCppHeader:
				case eFileType.eFileTypeCppClass:
					bool codeFile = _languageService.IsValidExtension(languageSet, file.Extension);
					if (codeFile)
						return FileSubType.Code;
					else
						return FileSubType.Misc;
				case eFileType.eFileTypeCppForm:
					return FileSubType.WinForm;
				case eFileType.eFileTypeCppControl:
					return FileSubType.Component;
				case eFileType.eFileTypeAsax:
					return FileSubType.WebAppFile;
				case eFileType.eFileTypeHTML:
					if ((string.Compare(file.Extension, FileType.HTM, StringComparison.OrdinalIgnoreCase) == 0) ||
							(string.Compare(file.Extension, FileType.HTML, StringComparison.OrdinalIgnoreCase) == 0))
						return FileSubType.HtmlFile;
					else
						return FileSubType.WebService;
				case eFileType.eFileTypeCppWebService:
					return FileSubType.WebServiceCode;
				case eFileType.eFileTypeDocument:
					string fileExt = file.Extension;
					if (fileExt.Equals(FileType.INLINE, StringComparison.OrdinalIgnoreCase))
						return FileSubType.Code;
					else if (fileExt.Equals(FileType.CONFIG, StringComparison.OrdinalIgnoreCase))
						return FileSubType.ConfigFile;
					else if (fileExt.Equals(FileType.CUR, StringComparison.OrdinalIgnoreCase))
						return FileSubType.Cursor;
					else if (fileExt.Equals(FileType.JPG, StringComparison.OrdinalIgnoreCase) ||
									 fileExt.Equals(FileType.JPEG, StringComparison.OrdinalIgnoreCase) ||
									 fileExt.Equals(FileType.GIF, StringComparison.OrdinalIgnoreCase) ||
									 fileExt.Equals(FileType.PNG, StringComparison.OrdinalIgnoreCase))
						return FileSubType.ImageFile;
					else
						return FileSubType.Misc;
				case eFileType.eFileTypeResx:
				case eFileType.eFileTypeRC:
				case eFileType.eFileTypeRES:
					return FileSubType.ResourceFile;
				case eFileType.eFileTypeBMP:
					return FileSubType.Bitmap;
				case eFileType.eFileTypeICO:
					return FileSubType.Icon;
				case eFileType.eFileTypeCUR:
					return FileSubType.Cursor;
				case eFileType.eFileTypeXSD:
					return FileSubType.XmlSchema;
				case eFileType.eFileTypeXML:
					return FileSubType.XmlFile;
				case eFileType.eFileTypeCSS:
					return FileSubType.StyleSheet;
				case eFileType.eFileTypeScript:
					return FileSubType.JScript;
				case eFileType.eFileTypeClassDiagram:
					return FileSubType.ClassDiagram;
				default:
					return FileSubType.Misc;
			}
		}

		/// <summary>
		/// Converts project item to C++ file item.
		/// </summary>
		private VCFile GetCppFile(ProjectItem projectItem)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			if ((projectItem != null) && (projectItem.Object is VCFile))
				return projectItem.Object as VCFile;
			else
				return null;
		}

		#endregion
	}
}