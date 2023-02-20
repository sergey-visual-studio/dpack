using System;
using System.Collections.Generic;
using Microsoft.Win32;

using DPackRx.Package;
using DPackRx.Services;

namespace DPackRx.Language
{
	/// <summary>
	/// Language registration service.
	/// </summary>
	public class LanguageRegistrationService : ILanguageRegistrationService
	{
		#region Fields

		private readonly ILog _log;
		private readonly IPackageService _packageService;

		private const string LANGUAGES_KEY = "Languages";
		private const string EXTENSIONS_KEY = "Extensions";
		private const string COMMENTS_KEY = "Comments";
		private const string PROJECT_GUID_VALUE = "ProjectGuid";
		private const string WEB_LANGUAGE_VALUE = "WebLanguage";
		private const string LANGUAGES_VALUE = "Languages";
		private const string SMART_FORMAT_VALUE = "SmartFormat";
		private const string XML_DOC = "XmlDoc";
		private const string XML_DOC_SURROUND = "XmlDocSurround";
		private const string DESIGNER_FILES = "DesignerFiles";
		private const string IMPORTS = "Imports";
		private const string IGNORE_CODE_TYPE = "IgnoreCodeType";
		private const string CHECK_DUPLICATE_NAMES = "CheckDuplicateNames";
		private const string PARENTLESS_FULL_NAME = "ParentlessFullName";
		private const string LOG_CATEGORY = "Language Registry";

		#endregion

		public LanguageRegistrationService(ILog log, IPackageService packageService)
		{
			_log = log;
			_packageService = packageService;
		}

		#region ILanguageRegistrationService Members

		/// <summary>
		/// Returns all registered languages.
		/// </summary>
		/// <returns>Languages.</returns>
		public ICollection<LanguageSettings> GetLanguages()
		{
			List<LanguageSettings> languageSettings = new List<LanguageSettings>(10);

			RegistryKey dpackKey;
			try
			{
				dpackKey = _packageService.GetAppRegistryRootKey(LANGUAGES_KEY);
			}
			catch
			{
				dpackKey = null;
			}
			if (dpackKey == null)
				throw new ApplicationException(
					$"Language definitions registry key {LANGUAGES_KEY} is missing. {_packageService.ProductName} must be re-installed.");

			using (dpackKey)
			{
				var languageKeys = dpackKey.GetSubKeyNames();

				for (var index = 0; index < languageKeys.Length; index++)
				{
					var id = languageKeys[index];

					var friendlyName = string.Empty;
					var projectGuid = string.Empty;
					var webLanguage = string.Empty;
					var languages = string.Empty;
					var smartFormat = true;
					var xmlDoc = (string)null;
					var xmlDocSurround = false;
					var ignoreCodeType = false;
					var checkDuplicateNames = false;
					var parentlessFullName = false;
					var designerFiles = LanguageDesignerFiles.NotSupported;
					var imports = LanguageImports.NotSupported;

					var langKey = dpackKey.OpenSubKey(id);
					if (langKey != null)
					{
						using (langKey)
						{
							friendlyName = (string)langKey.GetValue(string.Empty, friendlyName);
							projectGuid = (string)langKey.GetValue(PROJECT_GUID_VALUE, projectGuid);
							webLanguage = (string)langKey.GetValue(WEB_LANGUAGE_VALUE, webLanguage);
							languages = (string)langKey.GetValue(LANGUAGES_VALUE, languages);
							smartFormat = Convert.ToBoolean(
								(int)langKey.GetValue(SMART_FORMAT_VALUE, Convert.ToInt32(smartFormat)));
							xmlDoc = (string)langKey.GetValue(XML_DOC, xmlDoc);
							xmlDocSurround = Convert.ToBoolean(
								(int)langKey.GetValue(XML_DOC_SURROUND, Convert.ToInt32(xmlDocSurround)));
							designerFiles = (LanguageDesignerFiles)langKey.GetValue(DESIGNER_FILES, (int)designerFiles);
							imports = (LanguageImports)langKey.GetValue(IMPORTS, (int)imports);
							ignoreCodeType = Convert.ToBoolean(
								(int)langKey.GetValue(IGNORE_CODE_TYPE, Convert.ToInt32(ignoreCodeType)));
							checkDuplicateNames = Convert.ToBoolean(
								(int)langKey.GetValue(CHECK_DUPLICATE_NAMES, Convert.ToInt32(checkDuplicateNames)));
							parentlessFullName = Convert.ToBoolean(
								(int)langKey.GetValue(PARENTLESS_FULL_NAME, Convert.ToInt32(parentlessFullName)));
						}
					}
					if (string.IsNullOrEmpty(friendlyName))
					{
						_log.LogMessage($"Skipped language {id} definition w/o a friendly name", LOG_CATEGORY);
						continue;
					}

					 var language = new LanguageSettings(id, friendlyName, xmlDoc)
					{
						ProjectGuid = projectGuid,
						WebNames = !string.IsNullOrEmpty(languages) ? languages.Split(',') : new string[0],
						WebLanguage = webLanguage,
						SmartFormat = smartFormat,
						XmlDocSurround = xmlDocSurround,
						DesignerFiles = designerFiles,
						Imports = imports,
						IgnoreCodeType = ignoreCodeType,
						CheckDuplicateNames = checkDuplicateNames,
						ParentlessFullName = parentlessFullName
					};

					var extKey = dpackKey.OpenSubKey(id + "\\" + EXTENSIONS_KEY);
					if (extKey != null)
					{
						using (extKey)
						{
							var extensions = extKey.GetValueNames();
							for (int extIndex = 0; extIndex < extensions.Length; extIndex++)
							{
								var ext = extensions[extIndex];

								var enabled = Convert.ToBoolean((int)extKey.GetValue(ext, 1));
								language.Extensions.Add(ext, enabled);
							}
						}
					}

					var commentKey = dpackKey.OpenSubKey(id + "\\" + COMMENTS_KEY);
					if (commentKey != null)
					{
						using (commentKey)
						{
							var comments = commentKey.GetValueNames();
							for (int commentIndex = 0; commentIndex < comments.Length; commentIndex++)
							{
								var comment = comments[commentIndex];

								var enabled = Convert.ToBoolean((int)commentKey.GetValue(comment, 1));
								language.Comments.Add(comment, enabled);
							}
						}
					}

					languageSettings.Add(language);
				} // for (index)
			} // using

			return languageSettings;
		}

		#endregion
	}
}