using System;

using Moq;
using NUnit.Framework;

using DPackRx.CodeModel;
using DPackRx.Features;
using DPackRx.Features.CodeBrowser;
using DPackRx.Language;
using DPackRx.Options;
using DPackRx.Package;
using DPackRx.Services;

namespace DPackRx.Tests.Features
{
	/// <summary>
	/// CodeBrowserFeature tests.
	/// </summary>
	[TestFixture]
	public class CodeBrowserFeatureTests
	{
		#region Fields

		private Mock<IServiceProvider> _serviceProviderMock;
		private Mock<ILog> _logMock;
		private Mock<IOptionsService> _optionsServiceMock;
		private Mock<IShellSelectionService> _shellSelectionServiceMock;
		private Mock<IModalDialogService> _modalDialogServiceMock;
		private Mock<ILanguageService> _languageServiceMock;
		private Mock<IMessageService> _messageServiceMock;

		#endregion

		#region Tests Setup

		[SetUp]
		public void Setup()
		{
			_serviceProviderMock = new Mock<IServiceProvider>();

			_logMock = new Mock<ILog>();
			_logMock.Setup(l => l.LogMessage(It.IsAny<string>(), It.IsAny<string>())).Verifiable();

			_optionsServiceMock = new Mock<IOptionsService>();

			_shellSelectionServiceMock = new Mock<IShellSelectionService>();
			_shellSelectionServiceMock.Setup(s => s.IsContextActive(It.IsAny<ContextType>())).Returns(true).Verifiable();
			_shellSelectionServiceMock.Setup(s => s.GetActiveFileName()).Returns("test.cs").Verifiable();

			_modalDialogServiceMock = new Mock<IModalDialogService>();
			_modalDialogServiceMock
				.Setup(d => d.ShowDialog<CodeBrowserWindow, CodeBrowserViewModel>(It.IsNotNull<string>(), It.IsAny<CodeModelFilterFlags>()))
				.Returns(true)
				.Verifiable();

			_languageServiceMock = new Mock<ILanguageService>();
			_languageServiceMock
				.Setup(l => l.GetExtensionLanguage(It.IsAny<string>()))
				.Returns(new LanguageSettings("C#", "C#") { Type = LanguageType.CSharp } )
				.Verifiable();

			_messageServiceMock = new Mock<IMessageService>();
		}

		[TearDown]
		public void TearDown()
		{
			_serviceProviderMock = null;
			_logMock = null;
			_optionsServiceMock = null;
			_shellSelectionServiceMock = null;
			_modalDialogServiceMock = null;
			_languageServiceMock = null;
			_messageServiceMock = null;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Returns test feature instance.
		/// </summary>
		private IFeature GetFeature()
		{
			return new CodeBrowserFeature(_serviceProviderMock.Object, _logMock.Object, _optionsServiceMock.Object,
				_shellSelectionServiceMock.Object, _modalDialogServiceMock.Object, _languageServiceMock.Object, _messageServiceMock.Object);
		}

		#endregion

		#region Tests

		[Test]
		public void Initialize()
		{
			var feature = GetFeature();

			feature.Initialize();

			Assert.That(feature.Initialized, Is.True);
		}

		[Test]
		public void GetCommandIds()
		{
			var feature = GetFeature();

			var commands = feature.GetCommandIds();

			Assert.That(commands, Is.Not.Null);
			Assert.That(commands.Count, Is.EqualTo(4));
			Assert.That(commands, Contains.Item(CommandIDs.CODE_BROWSER));
			Assert.That(commands, Contains.Item(CommandIDs.CODE_BROWSER_CI));
			Assert.That(commands, Contains.Item(CommandIDs.CODE_BROWSER_M));
			Assert.That(commands, Contains.Item(CommandIDs.CODE_BROWSER_P));
		}

		[TestCase(CommandIDs.CODE_BROWSER, true)]
		[TestCase(CommandIDs.CODE_BROWSER_CI, true)]
		[TestCase(CommandIDs.CODE_BROWSER_M, true)]
		[TestCase(CommandIDs.CODE_BROWSER_P, true)]
		[TestCase(0, false)]
		public void IsValidContext(int commandId, bool expectedResult)
		{
			var feature = GetFeature();

			var result = feature.IsValidContext(commandId);

			Assert.That(result, Is.EqualTo(expectedResult));
			if (expectedResult)
				_shellSelectionServiceMock.Verify(s => s.IsContextActive(ContextType.SolutionHasProjects));
			else
				_shellSelectionServiceMock.Verify(s => s.IsContextActive(ContextType.SolutionHasProjects), Times.Never);
		}

		[TestCase(CommandIDs.CODE_BROWSER, CodeModelFilterFlags.All)]
		[TestCase(CommandIDs.CODE_BROWSER_CI, CodeModelFilterFlags.ClassesInterfaces)]
		[TestCase(CommandIDs.CODE_BROWSER_M, CodeModelFilterFlags.MethodsConstructors)]
		[TestCase(CommandIDs.CODE_BROWSER_P, CodeModelFilterFlags.Properties)]
		public void Execute(int commandId, CodeModelFilterFlags filter)
		{
			var feature = GetFeature();

			var result = feature.Execute(commandId);

			Assert.That(result, Is.True);
			_shellSelectionServiceMock.Verify(s => s.GetActiveFileName());
			_languageServiceMock.Verify(l => l.GetExtensionLanguage(It.IsAny<string>()));
			_modalDialogServiceMock.Verify(d => d.ShowDialog<CodeBrowserWindow, CodeBrowserViewModel>(It.IsNotNull<string>(), filter));
		}

		[Test]
		public void Execute_NoActiveFile()
		{
			var feature = GetFeature();

			_shellSelectionServiceMock.Setup(s => s.GetActiveFileName()).Returns(string.Empty).Verifiable();

			var result = feature.Execute(CommandIDs.CODE_BROWSER);

			Assert.That(result, Is.False);
			_shellSelectionServiceMock.Verify(s => s.GetActiveFileName());
			_languageServiceMock.Verify(l => l.GetExtensionLanguage(It.IsAny<string>()), Times.Never);
			_modalDialogServiceMock.Verify(d => d.ShowDialog<CodeBrowserWindow, CodeBrowserViewModel>(
				It.IsNotNull<string>(), It.IsAny<CodeModelFilterFlags>()), Times.Never);
		}

		[Test]
		public void Execute_UnknownLanguage()
		{
			var feature = GetFeature();

			_languageServiceMock
				.Setup(l => l.GetExtensionLanguage(It.IsAny<string>()))
				.Returns(LanguageSettings.UnknownLanguage)
				.Verifiable();

			var result = feature.Execute(CommandIDs.CODE_BROWSER);

			Assert.That(result, Is.True);
			_shellSelectionServiceMock.Verify(s => s.GetActiveFileName());
			_languageServiceMock.Verify(l => l.GetExtensionLanguage(It.IsAny<string>()));
			_modalDialogServiceMock.Verify(d => d.ShowDialog<CodeBrowserWindow, CodeBrowserViewModel>(
				It.IsNotNull<string>(), It.IsAny<CodeModelFilterFlags>()), Times.Never);
		}

		#endregion
	}
}