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
		private const string SUB_TYPE_CODE = "Code"; // .asax for web
		private const string SUB_TYPE_FORM = "Form"; // .aspx for web 
		private const string SUB_TYPE_COMPONENT = "Component"; // .asax for web
		private const string SUB_TYPE_USER_CONTROL = "UserControl"; // .ascx for web
		private const string SUB_TYPE_CODE_BEHIND = "ASPXCodeBehind"; // .aspx and .ascx for web

		private const string PROP_SUB_TYPE = "SubType";
		private const string PROP_IS_DEPENDENT_FILE = "IsDependentFile";

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

				var language = _shellProjectService.GetProjectLanguage(project);
				if (language != null)
				{
					if (language == string.Empty)
						language = _shellProjectService.GetNoCodeModelProjectLanguage(project);

					if (!string.IsNullOrEmpty(language))
					{
						var languageSet = _languageService.GetLanguage(language);

						if (languageSet?.Type != LanguageType.Unknown)
						{
							// C++ doesn't expose project type
							if (languageSet.Type != LanguageType.CPP)
								isWebProject = _shellProjectService.IsWebProject(project);
						}

						return languageSet;
					}
				}

				return LanguageSettings.UnknownLanguage;
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

			var extension = Path.GetExtension(dteItem.Name).ToLowerInvariant();
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
				try
				{
					// Various files are reported as .Designer - further file name check is required
					var props = dteItem.Properties;
					if (props == null)
						return false;

					var prop = props.Item(PROP_IS_DEPENDENT_FILE);
					if (prop != null)
						checkFileName = (bool)prop.Value;
					else
						return false;
				}
				catch
				{
					return false;
				}
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
			_log.LogMessage($"'{dteItem.Name}' sub-type - {itemSubType}");

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
				var fileExt = Path.GetExtension(dteItem.Name).ToLowerInvariant();
				var codeFile = _languageService.IsValidExtension(languageSet, fileExt);

				if (codeFile) // This is a code file of some sort
				{
					if (isWebProject) // File is part of web-based project such as web app or web service
					{
						fileExt = Path.GetExtension(Path.GetFileNameWithoutExtension(dteItem.Name)).ToLowerInvariant();
						if (string.IsNullOrEmpty(fileExt))
							return FileSubType.Code;
						else
							switch (fileExt)
							{
								case FileType.ASPX:
									return FileSubType.WebFormCode;
								case FileType.ASCX:
									return FileSubType.WebControlCode;
								case FileType.ASMX:
								case FileType.ASM:
									return FileSubType.WebServiceCode;
								case FileType.ASAX:
									return FileSubType.WebAppFileCode;
								case FileType.MASTER_PAGE:
									return FileSubType.WebMasterPageCode;
								default:
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
			ThreadHelper.ThrowIfNotOnUIThread();

			try
			{
				if (projectItem == null)
					return string.Empty;

				Properties props = projectItem.Properties;
				if (props == null)
					return string.Empty;

				Property prop = props.Item(PROP_SUB_TYPE);
				if (prop != null)
				{
					string itemType = (string) prop.Value;
					if (itemType == null)
						return string.Empty;
					else
						return itemType;
				}
				else
					return string.Empty;
			}
			catch
			{
				return string.Empty;
			}
		}

		/// <summary>
		/// Returns project item file type based on file's extension.
		/// </summary>
		private FileSubType GetExtensionSubType(string extension)
		{
			switch (extension)
			{
				case FileType.ASPX:
					return FileSubType.WebForm;
				case FileType.ASCX:
					return FileSubType.WebControl;
				case FileType.ASMX:
				case FileType.ASM:
					return FileSubType.WebService;
				case FileType.ASAX:
					return FileSubType.WebAppFile;
				case FileType.CONFIG:
					return FileSubType.ConfigFile;
				case FileType.RESX:
				case FileType.RES:
				case FileType.RC:
					return FileSubType.ResourceFile;
				case FileType.BMP:
					return FileSubType.Bitmap;
				case FileType.ICO:
					return FileSubType.Icon;
				case FileType.CUR:
					return FileSubType.Cursor;
				case FileType.JPG:
				case FileType.JPEG:
				case FileType.GIF:
				case FileType.PNG:
					return FileSubType.ImageFile;
				case FileType.XML:
					return FileSubType.XmlFile;
				case FileType.XML_SCHEMA:
					return FileSubType.XmlSchema;
				case FileType.XSL_SCHEMA:
				case FileType.XSLT_SCHEMA:
				case FileType.XSX_SCHEMA:
					return FileSubType.XslFile;
				case FileType.HTM:
				case FileType.HTML:
					return FileSubType.HtmlFile;
				case FileType.CSS:
					return FileSubType.StyleSheet;
				case FileType.ASHX:
					return FileSubType.WebGenericHandler;
				case FileType.MASTER_PAGE:
					return FileSubType.WebMasterPage;
				case FileType.SITE_MAP:
					return FileSubType.WebSiteMap;
				case FileType.JSCRIPT:
					return FileSubType.JScript;
				case FileType.WIN_HOST_SCRIPT:
					return FileSubType.WinHostScript;
				case FileType.SETTINGS:
					return FileSubType.Settings;
				case FileType.CLASS_DIAGRAM:
					return FileSubType.ClassDiagram;
				case FileType.XAML:
					return FileSubType.XamlFile;
				case FileType.SQL:
					return FileSubType.SqlFile;
				default:
					return FileSubType.Misc;
			}
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
					string fileExt = file.Extension.ToLowerInvariant();
					switch (fileExt)
					{
						case FileType.INLINE:
							return FileSubType.Code;
						case FileType.CONFIG:
							return FileSubType.ConfigFile;
						case FileType.CUR:
							return FileSubType.Cursor;
						case FileType.JPG:
						case FileType.JPEG:
						case FileType.GIF:
						case FileType.PNG:
							return FileSubType.ImageFile;
						default:
							return FileSubType.Misc;
					}
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