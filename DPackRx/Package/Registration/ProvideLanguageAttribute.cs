using System;
using System.Resources;

using DPackRx.Language;

using Microsoft.VisualStudio.Shell;

namespace DPackRx.Package.Registration
{
	/// <summary>
	/// Registers language definition.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
	public class ProvideLanguageAttribute : RegistrationAttribute
	{
		#region Fields

		private readonly string _languageGuid;
		private readonly string _name;
		private readonly string[] _extensions;
		private static string _productName;

		#endregion

		public ProvideLanguageAttribute(string productName, string languageGuid, string name, string[] extensions)
		{
			if (string.IsNullOrEmpty(productName))
				throw new ArgumentNullException(nameof(productName));

			if (string.IsNullOrEmpty(languageGuid))
				throw new ArgumentNullException(nameof(languageGuid));

			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name));

			_languageGuid = languageGuid;
			_name = name;
			_extensions = extensions;

			// Cache product name just once
			if (string.IsNullOrEmpty(_productName))
			{
				if (productName.StartsWith("#", StringComparison.OrdinalIgnoreCase))
				{
					var productId = productName.Substring(1);
					var resourceManager = new ResourceManager("VSPackage", this.GetType().Assembly);
					_productName = resourceManager.GetString(productId);

					if (string.IsNullOrEmpty(_productName))
						throw new ArgumentException($"Invalid registration product name {productId}");
				}
				else
				{
					_productName = productName;
				}
			}
		}

		public ProvideLanguageAttribute(string productName, string languageGuid, string projectGuid, string name, string[] extensions) :
			this(productName, languageGuid, name, extensions)
		{

			if (string.IsNullOrEmpty(projectGuid))
				throw new ArgumentNullException(nameof(projectGuid));

			this.ProjectGuid = projectGuid;
		}

		#region Properties

		public string ProjectGuid { get; set; }

		public string WebLanguage { get; set; }

		public string Languages { get; set; }

		public string[] Comments { get; set; }

		public string XmlDoc { get; set; }

		public bool XmlDocSurround { get; set; }

		public LanguageDesignerFiles DesignerFiles { get; set; } = LanguageDesignerFiles.NotSupported;

		public LanguageImports Imports { get; set; } = LanguageImports.NotSupported;

		public bool IgnoreCodeType { get; set; }

		public bool CheckDuplicateNames { get; set; }

		public bool SmartFormat { get; set; } = true;

		public bool ParentlessFullName { get; set; }

		public bool SurroundWith { get; set; }

		public string SurroundWithLanguageName { get; set; }

		internal string RegKeyName
		{
			get { return string.Format(@"{0}\Languages\{1}", _productName, _languageGuid); }
		}

		internal string RegKeyNameExtensions
		{
			get { return this.RegKeyName + @"\Extensions"; }
		}

		internal string RegKeyNameComments
		{
			get { return this.RegKeyName + @"\Comments"; }
		}

		#endregion

		#region RegistrationAttribute Overrides

		public override void Register(RegistrationContext context)
		{
			using (var key = context.CreateKey(this.RegKeyName))
			{
				key.SetValue("", _name);

				if (!string.IsNullOrEmpty(this.ProjectGuid))
					key.SetValue(nameof(this.ProjectGuid), this.ProjectGuid);
				if (!string.IsNullOrEmpty(this.WebLanguage))
					key.SetValue(nameof(this.WebLanguage), this.WebLanguage);
				if ((this.Languages != null) && (this.Languages.Length > 0))
					key.SetValue(nameof(this.Languages), string.Join(",", this.Languages));
				if (!string.IsNullOrEmpty(this.XmlDoc))
					key.SetValue(nameof(this.XmlDoc), this.XmlDoc);
				if (this.XmlDocSurround)
					key.SetValue(nameof(this.XmlDocSurround), Convert.ToInt32(this.XmlDocSurround));
				if (this.DesignerFiles != LanguageDesignerFiles.NotSupported)
					key.SetValue(nameof(this.DesignerFiles), (int)this.DesignerFiles);
				if (this.Imports != LanguageImports.NotSupported)
					key.SetValue(nameof(this.Imports), (int)this.Imports);
				if (this.IgnoreCodeType)
					key.SetValue(nameof(this.IgnoreCodeType), Convert.ToInt32(this.IgnoreCodeType));
				if (this.CheckDuplicateNames)
					key.SetValue(nameof(this.CheckDuplicateNames), Convert.ToInt32(this.CheckDuplicateNames));
				if (this.SmartFormat)
					key.SetValue(nameof(this.SmartFormat), Convert.ToInt32(this.SmartFormat));
				if (this.ParentlessFullName)
					key.SetValue(nameof(this.ParentlessFullName), Convert.ToInt32(this.ParentlessFullName));
				if (this.SurroundWith)
					key.SetValue(nameof(this.SurroundWith), Convert.ToInt32(this.SurroundWith));
				if (!string.IsNullOrEmpty(this.SurroundWithLanguageName))
					key.SetValue(nameof(this.SurroundWithLanguageName), this.SurroundWithLanguageName);

				if ((_extensions != null) && (_extensions.Length > 0))
				{
					using (var keyExt = context.CreateKey(this.RegKeyNameExtensions))
					{
						foreach (string extension in _extensions)
						{
							keyExt.SetValue(extension, Convert.ToInt32(true));
						}
					}
				}

				if ((this.Comments != null) && (this.Comments.Length > 0))
				{
					using (var keyCmt = context.CreateKey(this.RegKeyNameComments))
					{
						foreach (string comment in this.Comments)
						{
							keyCmt.SetValue(comment, Convert.ToInt32(true));
						}
					}
				}
			}
		}

		public override void Unregister(RegistrationContext context)
		{
			context.RemoveKey(this.RegKeyName);
		}

		#endregion
	}
}