using System;
using System.Windows.Input;

using Moq;
using NUnit.Framework;

using DPackRx.CodeModel;
using DPackRx.Features;
using DPackRx.Features.SurroundWith;
using DPackRx.Language;
using DPackRx.Options;
using DPackRx.Package;
using DPackRx.Services;

namespace DPackRx.Tests.Features
{
	/// <summary>
	/// SurroundWithFeature tests.
	/// </summary>
	[TestFixture]
	public class SurroundWithFeatureTests
	{
		#region Fields

		private Mock<IServiceProvider> _serviceProviderMock;
		private Mock<ILog> _logMock;
		private Mock<IOptionsService> _optionsServiceMock;
		private Mock<IShellHelperService> _shellHelperServiceMock;
		private Mock<IShellSelectionService> _shellSelectionServiceMock;
		private Mock<IFileTypeResolver> _fileTypeResolverMock;
		private Mock<IKeyboardService> _keyboardServiceMock;
		private LanguageSettings _settings;

		#endregion

		#region Tests Setup

		[SetUp]
		public void Setup()
		{
			_serviceProviderMock = new Mock<IServiceProvider>();

			_logMock = new Mock<ILog>();
			_logMock.Setup(l => l.LogMessage(It.IsAny<string>(), It.IsAny<string>())).Verifiable();

			_optionsServiceMock = new Mock<IOptionsService>();

			_shellHelperServiceMock = new Mock<IShellHelperService>();
			_shellHelperServiceMock.Setup(s => s.ExecuteCommand(It.IsNotNull<string>(), null)).Verifiable();

			_shellSelectionServiceMock = new Mock<IShellSelectionService>();
			_shellSelectionServiceMock.Setup(s => s.IsContextActive(It.IsAny<ContextType>())).Returns(true).Verifiable();
			_shellSelectionServiceMock.Setup(s => s.GetActiveProject()).Returns(new object()).Verifiable();

			_settings = new LanguageSettings("C#", "C#") { Type = LanguageType.CSharp, SurroundWith = true };
			var webProject = false;
			_fileTypeResolverMock = new Mock<IFileTypeResolver>();
			_fileTypeResolverMock.Setup(f => f.GetCurrentLanguage(It.IsAny<object>(), out webProject)).Returns(_settings).Verifiable();

			_keyboardServiceMock = new Mock<IKeyboardService>();
			_keyboardServiceMock.Setup(k => k.Type(It.IsAny<Key>())).Verifiable();
			_keyboardServiceMock.Setup(k => k.Type(It.IsAny<string>())).Verifiable();
		}

		[TearDown]
		public void TearDown()
		{
			_serviceProviderMock = null;
			_logMock = null;
			_optionsServiceMock = null;
			_shellHelperServiceMock = null;
			_shellSelectionServiceMock = null;
			_fileTypeResolverMock = null;
			_keyboardServiceMock = null;
			_settings = null;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Returns test feature instance.
		/// </summary>
		private IFeature GetFeature()
		{
			return new SurroundWithFeature(_serviceProviderMock.Object, _logMock.Object, _optionsServiceMock.Object,
				_shellHelperServiceMock.Object, _shellSelectionServiceMock.Object, _fileTypeResolverMock.Object,
				_keyboardServiceMock.Object);
		}

		#endregion

		#region Tests

		[Test]
		public void GetCommandIds()
		{
			var feature = GetFeature();

			var commands = feature.GetCommandIds();

			Assert.That(commands, Is.Not.Null);
			Assert.That(commands.Count, Is.EqualTo(5));
			Assert.That(commands, Contains.Item(CommandIDs.SW_TRY_CATCH));
			Assert.That(commands, Contains.Item(CommandIDs.SW_TRY_FINALLY));
			Assert.That(commands, Contains.Item(CommandIDs.SW_FOR));
			Assert.That(commands, Contains.Item(CommandIDs.SW_FOR_EACH));
			Assert.That(commands, Contains.Item(CommandIDs.SW_REGION));
		}

		[TestCase(CommandIDs.SW_TRY_CATCH, true)]
		[TestCase(CommandIDs.SW_TRY_FINALLY, true)]
		[TestCase(CommandIDs.SW_FOR, true)]
		[TestCase(CommandIDs.SW_FOR_EACH, true)]
		[TestCase(CommandIDs.SW_REGION, true)]
		[TestCase(0, false)]
		public void IsValidContext(int commandId, bool expectedResult)
		{
			var feature = GetFeature();

			var result = feature.IsValidContext(commandId);

			Assert.That(result, Is.EqualTo(expectedResult));
			if (expectedResult)
				_shellSelectionServiceMock.Verify(s => s.IsContextActive(It.IsAny<ContextType>()), Times.AtLeast(2));
			else
				_shellSelectionServiceMock.Verify(s => s.IsContextActive(It.IsAny<ContextType>()), Times.Never);
		}

		[Test]
		public void IsValidContext_UnknownLanguage()
		{
			var feature = GetFeature();
			var webProject = false;
			_fileTypeResolverMock.Setup(f => f.GetCurrentLanguage(It.IsAny<object>(), out webProject))
				.Returns(LanguageSettings.UnknownLanguage).Verifiable();

			var result = feature.IsValidContext(CommandIDs.SW_REGION);

			Assert.That(result, Is.False);
			_shellSelectionServiceMock.Verify(s => s.GetActiveProject());
			_fileTypeResolverMock.Verify(f => f.GetCurrentLanguage(It.IsAny<object>(), out webProject));
			_shellSelectionServiceMock.Verify(s => s.IsContextActive(It.IsAny<ContextType>()), Times.Never);
		}

		[Test]
		public void IsValidContext_UnsupportedLanguage()
		{
			var feature = GetFeature();
			var webProject = false;
			_fileTypeResolverMock.Setup(f => f.GetCurrentLanguage(It.IsAny<object>(), out webProject))
				.Returns(new LanguageSettings("C#", "C#") { Type = LanguageType.CSharp, SurroundWith = false }).Verifiable();

			var result = feature.IsValidContext(CommandIDs.SW_REGION);

			Assert.That(result, Is.False);
			_shellSelectionServiceMock.Verify(s => s.GetActiveProject());
			_fileTypeResolverMock.Verify(f => f.GetCurrentLanguage(It.IsAny<object>(), out webProject));
			_shellSelectionServiceMock.Verify(s => s.IsContextActive(It.IsAny<ContextType>()), Times.Never);
		}

		[TestCase(CommandIDs.SW_TRY_CATCH, SurroundWithFeature.SNIPPET_TRY_CATCH)]
		[TestCase(CommandIDs.SW_TRY_FINALLY, SurroundWithFeature.SNIPPET_TRY_FINALLY)]
		[TestCase(CommandIDs.SW_FOR, SurroundWithFeature.SNIPPET_FOR)]
		[TestCase(CommandIDs.SW_FOR_EACH, SurroundWithFeature.SNIPPET_FOR_EACH)]
		[TestCase(CommandIDs.SW_REGION, SurroundWithFeature.SNIPPET_REGION)]
		public void Execute(int commandId, string command)
		{
			var feature = GetFeature();
			var webProject = false;

			var result = feature.Execute(commandId);

			Assert.That(result, Is.True);
			_shellSelectionServiceMock.Verify(s => s.GetActiveProject(), Times.Once);
			_fileTypeResolverMock.Verify(f => f.GetCurrentLanguage(It.IsAny<object>(), out webProject), Times.Once);
			_shellHelperServiceMock.Verify(s => s.ExecuteCommand(SurroundWithFeature.SURROUND_WITH_COMMAND, null), Times.Once);
			_shellHelperServiceMock.Verify(s => s.ExecuteCommand(SurroundWithFeature.ISERT_SNIPPET_COMMAND, null), Times.Never);
			_keyboardServiceMock.Verify(k => k.Type(Key.Enter), Times.Once);
			_keyboardServiceMock.Verify(k => k.Type(command), Times.Once);
		}

		[TestCase("test")]
		[TestCase("")]
		[TestCase(null)]
		public void Execute_LanguageName(string surroundWithLanguageName)
		{
			var feature = GetFeature();
			var webProject = false;
			_settings.SurroundWithLanguageName = surroundWithLanguageName;

			var result = feature.Execute(CommandIDs.SW_REGION);

			Assert.That(result, Is.True);
			_shellSelectionServiceMock.Verify(s => s.GetActiveProject(), Times.Once);
			_fileTypeResolverMock.Verify(f => f.GetCurrentLanguage(It.IsAny<object>(), out webProject), Times.Once);
			_shellHelperServiceMock.Verify(s => s.ExecuteCommand(SurroundWithFeature.SURROUND_WITH_COMMAND, null), Times.Exactly(string.IsNullOrEmpty(surroundWithLanguageName) ? 1 : 0));
			_shellHelperServiceMock.Verify(s => s.ExecuteCommand(SurroundWithFeature.ISERT_SNIPPET_COMMAND, null), Times.Exactly(string.IsNullOrEmpty(surroundWithLanguageName) ? 0 : 1));
			_keyboardServiceMock.Verify(k => k.Type(Key.Enter), Times.Exactly(string.IsNullOrEmpty(surroundWithLanguageName) ? 1 : 2));
			_keyboardServiceMock.Verify(k => k.Type(SurroundWithFeature.SNIPPET_REGION), Times.Once);
		}

		[Test]
		public void Execute_InvalidCommand()
		{
			var feature = GetFeature();

			var result = feature.Execute(0);

			Assert.That(result, Is.False);
		}

		#endregion
	}
}