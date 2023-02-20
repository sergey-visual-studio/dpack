using System.Collections.Generic;

using DPackRx.Language;
using DPackRx.Package;

using NUnit.Framework;

namespace DPackRx.Tests.Language
{
	/// <summary>
	/// LanguageSettings tests.
	/// </summary>
	[TestFixture]
	public class LanguageSettingsTests
	{
		#region Tests

		[TestCase(LanguageConsts.VS_CM_LANGUAGE_CSHARP, LanguageType.CSharp)]
		[TestCase(LanguageConsts.VS_CM_LANGUAGE_VB, LanguageType.VB)]
		[TestCase(LanguageConsts.VS_CM_LANGUAGE_VC, LanguageType.CPP)]
		[TestCase(LanguageConsts.VS_LANGUAGE_JAVA_SCRIPT, LanguageType.JavaScript)]
		[TestCase(LanguageConsts.VS_LANGUAGE_XML, LanguageType.Xml)]
		[TestCase(LanguageConsts.VS_LANGUAGE_SOLUTION_ITEMS, LanguageType.SolutionItems)]
		[TestCase(LanguageConsts.VS_LANGUAGE_SQL, LanguageType.Sql)]
		[TestCase("bad", LanguageType.Unknown)]
		public void GetLanguage(string projectLanguage, LanguageType languageType)
		{
			var language = new LanguageSettings(projectLanguage, "Test")
			{
				CheckDuplicateNames = true,
				Comments =  new Dictionary<string, bool> { { "test", true } },
				DesignerFiles = LanguageDesignerFiles.FullySupported,
				Extensions = new Dictionary<string, bool> { { ".txt", true } },
				IgnoreCodeType = true,
				Imports = LanguageImports.Supported,
				ParentlessFullName = true,
				ProjectGuid = "test",
				SmartFormat = true,
				SupportsCompileBuildAction = true,
				SupportsGenerics = true,
				SupportsStatistics = true,
				SurroundWith = true,
				WebLanguage = "test",
				WebNames = new string[] { "test" },
				XmlDocs = new string[] { "test" },
				XmlDocSurround = true,
			};

			Assert.That(language.Language, Is.EqualTo(projectLanguage));
			Assert.That(language.FriendlyName, Is.EqualTo("Test"));
			Assert.That(language.Type, Is.EqualTo(languageType));
			Assert.That(language.CheckDuplicateNames, Is.True);
			Assert.That(language.Comments, Is.Not.Null.And.Count.EqualTo(1));
			Assert.That(language.DesignerFiles, Is.EqualTo(LanguageDesignerFiles.FullySupported));
			Assert.That(language.Extensions, Is.Not.Null.And.Count.EqualTo(1));
			Assert.That(language.IgnoreCodeType, Is.True);
			Assert.That(language.Imports, Is.EqualTo(LanguageImports.Supported));
			Assert.That(language.ParentlessFullName, Is.True);
			Assert.That(language.ProjectGuid, Is.EqualTo("test"));
			Assert.That(language.SmartFormat, Is.True);
			Assert.That(language.SupportsCompileBuildAction, Is.True);
			Assert.That(language.SupportsGenerics, Is.True);
			Assert.That(language.SupportsStatistics, Is.True);
			Assert.That(language.SurroundWith, Is.True);
			Assert.That(language.WebLanguage, Is.EqualTo("test"));
			Assert.That(language.WebNames, Is.Not.Null.And.Length.EqualTo(1));
			Assert.That(language.XmlDocs, Is.Not.Null.And.Length.EqualTo(1));
			Assert.That(language.XmlDocSurround, Is.True);
		}

		#endregion
	}
}