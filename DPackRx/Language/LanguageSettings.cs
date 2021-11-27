using System;
using System.Collections.Generic;
using System.Diagnostics;

using DPackRx.Package;

namespace DPackRx.Language
{
	/// <summary>
	/// Language definition.
	/// </summary>
	[DebuggerDisplay("{FriendlyName}")]
	public class LanguageSettings
	{
		public LanguageSettings(string language, string friendlyName, string xmlDoc = null)
		{
			this.Language = language;
			this.FriendlyName = friendlyName;

			if (xmlDoc?.IndexOf(",", StringComparison.OrdinalIgnoreCase) >= 0)
				this.XmlDocs = xmlDoc.Split(',');
			else
				this.XmlDocs = new string[] { xmlDoc };

			switch (this.Language)
			{
				case LanguageConsts.VS_CM_LANGUAGE_CSHARP:
					this.Type = LanguageType.CSharp;
					this.SupportsCompileBuildAction = true;
					this.SupportsGenerics = true;
					break;
				case LanguageConsts.VS_CM_LANGUAGE_VB:
					this.Type = LanguageType.VB;
					this.SupportsCompileBuildAction = true;
					this.SupportsGenerics = true;
					break;
				case LanguageConsts.VS_CM_LANGUAGE_VC:
					this.Type = LanguageType.CPP;
					break;
				case LanguageConsts.VS_LANGUAGE_JAVA_SCRIPT:
					this.Type = LanguageType.JavaScript;
					this.SupportsStatistics = false;
					break;
				case LanguageConsts.VS_LANGUAGE_XML:
					this.Type = LanguageType.Xml;
					this.SupportsStatistics = false;
					break;
				case LanguageConsts.VS_LANGUAGE_SOLUTION_ITEMS:
					this.Type = LanguageType.SolutionItems;
					break;
				default:
					this.Type = LanguageType.Unknown;
					break;
			}
		}

		#region Properties

		/// <summary>
		/// Unknown language.
		/// </summary>
		public static LanguageSettings UnknownLanguage { get; } = new LanguageSettings(string.Empty, "Unknown");

		/// <summary>
		/// Language type.
		/// </summary>
		public LanguageType Type { get; set; }

		/// <summary>
		/// Language name.
		/// </summary>
		public string Language { get; set; } = string.Empty;

		/// <summary>
		/// Language friendly name.
		/// </summary>
		public string FriendlyName { get; set; } = string.Empty;

		/// <summary>
		/// Optional project language type Guid.
		/// </summary>
		public string ProjectGuid { get; set; } = string.Empty;

		/// <summary>
		/// Language web names.
		/// </summary>
		public string[] WebNames { get; set; } = new string[0];

		/// <summary>
		/// Default web language name.
		/// </summary>
		public string WebLanguage { get; set; } = string.Empty;

		/// <summary>
		/// Language file extensions and their availability status.
		/// </summary>
		public IDictionary<string, bool> Extensions { get; set; } = new Dictionary<string, bool>(10, StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Language file comments and their availability status.
		/// </summary>
		public IDictionary<string, bool> Comments { get; set; } = new Dictionary<string, bool>(2, StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Language smart format.
		/// </summary>
		/// <remarks>Used to be used by surround with feature.</remarks>
		public bool SmartFormat { get; set; }

		/// <summary>
		/// Non-null value indicates that languages supports Xml documentation.
		/// Empty value indicates that documentation doesn't require any Xml processing.
		/// </summary>
		public string[] XmlDocs { get; set; }

		/// <summary>
		/// Indicates whether to surround raw Xml documentation with 'doc' tag.
		/// </summary>
		public bool XmlDocSurround { get; set; }

		/// <summary>
		/// Level of .Designer file processing support.
		/// </summary>
		public LanguageDesignerFiles DesignerFiles { get; set; } = LanguageDesignerFiles.NotSupported;

		/// <summary>
		/// Level of imports processing support.
		/// </summary>
		public LanguageImports Imports { get; set; } = LanguageImports.NotSupported;

		/// <summary>
		/// Whether to ignore code element's IsCodeType property or not.
		/// </summary>
		public bool IgnoreCodeType { get; set; } = true;

		/// <summary>
		/// Whether to check for duplicate code element names.
		/// </summary>
		public bool CheckDuplicateNames { get; set; }

		/// <summary>
		/// Whether parentless code elements should use their full names in place of parent full names.
		/// </summary>
		public bool ParentlessFullName { get; set; }

		/// <summary>
		/// Whether language supports linked files grouping.
		/// </summary>
		public bool SupportsCompileBuildAction { get; set; }

		/// <summary>
		/// Whether language support Solution Statistics feature.
		/// </summary>
		public bool SupportsStatistics { get; set; }

		/// <summary>
		/// Whether language supports generic types.
		/// </summary>
		public bool SupportsGenerics { get; set; }

		/// <summary>
		/// Whether language supports surround with.
		/// </summary>
		public bool SurroundWith { get; set; }

		#endregion
	}

	#region LanguageType enum

	/// <summary>
	/// Maps to project type.
	/// </summary>
	public enum LanguageType
	{
		Custom = -1,
		Unknown = 0,
		CSharp,
		VB,
		CPP,
		JavaScript,
		Xml,
		SolutionItems
	}

	#endregion

	#region LanguageDesignerFiles enum

	/// <summary>
	/// Level of .Designer file processing support.
	/// </summary>
	public enum LanguageDesignerFiles
	{
		/// <summary>
		/// Language doesn't support .Designer files.
		/// </summary>
		NotSupported = 0,
		/// <summary>
		/// Language's automation model fully supports .Designer files.
		/// </summary>
		FullySupported,
		/// <summary>
		/// Language's generates .Designer files but automation model doesn't support it.
		/// </summary>
		PartiallySupported
	}

	#endregion

	#region LanguageImports enum

	/// <summary>
	/// Imports programmatic addition support.
	/// </summary>
	public enum LanguageImports
	{
		/// <summary>
		/// Language doesn't support imports.
		/// </summary>
		NotSupported = 0,
		/// <summary>
		/// Language support imports.
		/// </summary>
		Supported
	}

	#endregion
}