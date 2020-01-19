using System;
using System.Collections.Generic;

using Moq;
using NUnit.Framework;

using DPackRx.Language;
using DPackRx.Services;

namespace DPackRx.Tests.Language
{
	/// <summary>
	/// LanguageService tests.
	/// </summary>
	[TestFixture]
	public class LanguageServiceTests
	{
		#region Fields

		private Mock<ILog> _logMock;
		private Mock<ILanguageRegistrationService> _languageRegistrationServiceMock;
		private LanguageSettings _languageSet;

		#endregion

		#region Tests Setup

		[SetUp]
		public void Setup()
		{
			_logMock = new Mock<ILog>();
			_logMock.Setup(l => l.LogMessage(It.IsAny<string>(), It.IsAny<string>())).Verifiable();

			_languageSet = new LanguageSettings("test", "test") { WebNames = new string[] { "webtest" }, WebLanguage = "web", Extensions = new Dictionary<string, bool> { { "test", true }, { "test2", false } } };
			var languages = new List<LanguageSettings> {
				_languageSet,
				new LanguageSettings("test2", "test2")
			};

			_languageRegistrationServiceMock = new Mock<ILanguageRegistrationService>();
			_languageRegistrationServiceMock.Setup(l => l.GetLanguages()).Returns(languages).Verifiable();
		}

		[TearDown]
		public void TearDown()
		{
			_logMock = null;
			_languageRegistrationServiceMock = null;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Returns test service instance.
		/// </summary>
		private ILanguageService GetService()
		{
			return new LanguageService(_logMock.Object, _languageRegistrationServiceMock.Object);
		}

		#endregion

		#region Tests

		[Test]
		public void GetLanguage()
		{
			var service = GetService();

			var language = service.GetLanguage("test");

			Assert.That(language, Is.Not.Null);
			Assert.That(language.Language, Is.EqualTo("test"));
			Assert.That(language.Type, Is.EqualTo(LanguageType.Unknown));
			_languageRegistrationServiceMock.Verify(l => l.GetLanguages());
		}

		[Test]
		public void GetLanguage_ErrorHandling()
		{
			var service = GetService();

			Assert.That(service.GetLanguage(null), Is.EqualTo(LanguageSettings.UnknownLanguage));
			Assert.That(service.GetLanguage(string.Empty), Is.EqualTo(LanguageSettings.UnknownLanguage));
			_languageRegistrationServiceMock.Verify(l => l.GetLanguages(), Times.Never);
		}

		[Test]
		public void GetLanguage_NotFound()
		{
			var service = GetService();

			var language = service.GetLanguage("bad");

			Assert.That(language, Is.EqualTo(LanguageSettings.UnknownLanguage));
			Assert.That(language.Type, Is.EqualTo(LanguageType.Unknown));
			_languageRegistrationServiceMock.Verify(l => l.GetLanguages());
		}

		[Test]
		public void GetLanguage_Type()
		{
			var service = GetService();

			var language = service.GetLanguage(LanguageType.Unknown);

			Assert.That(language, Is.Not.Null);
			Assert.That(language.Type, Is.EqualTo(LanguageType.Unknown));
			_languageRegistrationServiceMock.Verify(l => l.GetLanguages(), Times.Never);
		}

		[Test]
		public void GetLanguage_Type_NotFound()
		{
			var service = GetService();

			Assert.That(service.GetLanguage(LanguageType.Unknown), Is.EqualTo(LanguageSettings.UnknownLanguage));
			Assert.That(service.GetLanguage(LanguageType.SolutionItems), Is.EqualTo(LanguageSettings.UnknownLanguage));
			_languageRegistrationServiceMock.Verify(l => l.GetLanguages());
		}

		[Test]
		public void GetWebNameLanguage()
		{
			var service = GetService();

			var language = service.GetWebNameLanguage("webtest");

			Assert.That(language, Is.Not.Null);
			Assert.That(language.Language, Is.EqualTo("test"));
			Assert.That(language.Type, Is.EqualTo(LanguageType.Unknown));
			_languageRegistrationServiceMock.Verify(l => l.GetLanguages());
		}

		[Test]
		public void GetWebNameLanguage_ErrorHandling()
		{
			var service = GetService();

			Assert.That(service.GetWebNameLanguage(null), Is.EqualTo(LanguageSettings.UnknownLanguage));
			Assert.That(service.GetWebNameLanguage(string.Empty), Is.EqualTo(LanguageSettings.UnknownLanguage));
			_languageRegistrationServiceMock.Verify(l => l.GetLanguages(), Times.Never);
		}

		[Test]
		public void GetWebNameLanguage_NotFound()
		{
			var service = GetService();

			var language = service.GetWebNameLanguage("bad");

			Assert.That(language, Is.Not.Null);
			Assert.That(language.Type, Is.EqualTo(LanguageType.Unknown));
			_languageRegistrationServiceMock.Verify(l => l.GetLanguages());
		}

		[Test]
		public void GetWebLanguage()
		{
			var service = GetService();

			var language = service.GetWebLanguage("web");

			Assert.That(language, Is.Not.Null);
			Assert.That(language.Language, Is.EqualTo("test"));
			Assert.That(language.Type, Is.EqualTo(LanguageType.Unknown));
			_languageRegistrationServiceMock.Verify(l => l.GetLanguages());
		}

		[Test]
		public void GetWebLanguage_ErrorHandling()
		{
			var service = GetService();

			Assert.That(service.GetWebLanguage(null), Is.EqualTo(LanguageSettings.UnknownLanguage));
			Assert.That(service.GetWebLanguage(string.Empty), Is.EqualTo(LanguageSettings.UnknownLanguage));
			_languageRegistrationServiceMock.Verify(l => l.GetLanguages(), Times.Never);
		}

		[Test]
		public void GetWebLanguage_NotFound()
		{
			var service = GetService();

			var language = service.GetWebLanguage("bad");

			Assert.That(language, Is.Not.Null);
			Assert.That(language.Type, Is.EqualTo(LanguageType.Unknown));
			_languageRegistrationServiceMock.Verify(l => l.GetLanguages());
		}

		[TestCase(".test")]
		[TestCase("test")]
		public void GetExtensionLanguage(string extension)
		{
			var service = GetService();

			var language = service.GetExtensionLanguage(extension);

			Assert.That(language, Is.Not.Null);
			Assert.That(language.Language, Is.EqualTo("test"));
			Assert.That(language.Type, Is.EqualTo(LanguageType.Unknown));
			_languageRegistrationServiceMock.Verify(l => l.GetLanguages());
		}

		[Test]
		public void GetExtensionLanguage_ErrorHandling()
		{
			var service = GetService();

			Assert.That(service.GetExtensionLanguage(null), Is.EqualTo(LanguageSettings.UnknownLanguage));
			Assert.That(service.GetExtensionLanguage(string.Empty), Is.EqualTo(LanguageSettings.UnknownLanguage));
			_languageRegistrationServiceMock.Verify(l => l.GetLanguages(), Times.Never);
		}

		[Test]
		public void GetExtensionLanguage_NotFound()
		{
			var service = GetService();

			var language = service.GetExtensionLanguage("bad");

			Assert.That(language, Is.Not.Null);
			Assert.That(language.Type, Is.EqualTo(LanguageType.Unknown));
			_languageRegistrationServiceMock.Verify(l => l.GetLanguages());
		}

		[TestCase(".test", "test")]
		[TestCase("test", "test")]
		public void GetNormalizedExtension(string extension, string expectedExtension)
		{
			var service = GetService();

			var result = service.GetNormalizedExtension(extension);

			Assert.That(result, Is.Not.Null);
			Assert.That(result, Is.EqualTo(expectedExtension));
			_languageRegistrationServiceMock.Verify(l => l.GetLanguages(), Times.Never);
		}

		[Test]
		public void GetNormalizedExtension_ErrorHandling()
		{
			var service = GetService();

			Assert.That(service.GetNormalizedExtension(null), Is.Null);
			Assert.That(service.GetNormalizedExtension(string.Empty), Is.Empty);
		}

		[TestCase(".test", ".test")]
		[TestCase("test", ".test")]
		public void GetDenormalizedExtension(string extension, string expectedExtension)
		{
			var service = GetService();

			var result = service.GetDenormalizedExtension(extension);

			Assert.That(result, Is.Not.Null);
			Assert.That(result, Is.EqualTo(expectedExtension));
			_languageRegistrationServiceMock.Verify(l => l.GetLanguages(), Times.Never);
		}

		[Test]
		public void GetDenormalizedExtension_ErrorHandling()
		{
			var service = GetService();

			Assert.That(service.GetDenormalizedExtension(null), Is.Null);
			Assert.That(service.GetDenormalizedExtension(string.Empty), Is.Empty);
		}

		[TestCase(".test", true)]
		[TestCase("test", true)]
		[TestCase("bad", false)]
		[TestCase("", false)]
		[TestCase(null, false)]
		public void IsValidExtension(string extension, bool expectedResult)
		{
			var service = GetService();

			var result = service.IsValidExtension(_languageSet, extension);

			Assert.That(result, Is.EqualTo(expectedResult));
			_languageRegistrationServiceMock.Verify(l => l.GetLanguages(), Times.Never);
		}

		[Test]
		public void IsValidExtension_ErrorHandling()
		{
			var service = GetService();

			Assert.Throws<ArgumentNullException>(() => service.IsValidExtension(null, "test"));
		}

		#endregion
	}
}