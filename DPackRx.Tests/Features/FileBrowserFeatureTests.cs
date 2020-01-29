using System;

using Moq;
using NUnit.Framework;

using DPackRx.Features;
using DPackRx.Features.FileBrowser;
using DPackRx.Options;
using DPackRx.Package;
using DPackRx.Services;

namespace DPackRx.Tests.Features
{
	/// <summary>
	/// FileBrowserFeature tests.
	/// </summary>
	[TestFixture]
	public class FileBrowserFeatureTests
	{
		#region Fields

		private Mock<IServiceProvider> _serviceProviderMock;
		private Mock<ILog> _logMock;
		private Mock<IOptionsService> _optionsServiceMock;
		private Mock<IShellSelectionService> _shellSelectionServiceMock;
		private Mock<IModalDialogService> _modalDialogServiceMock;

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

			_modalDialogServiceMock = new Mock<IModalDialogService>();
		}

		[TearDown]
		public void TearDown()
		{
			_serviceProviderMock = null;
			_logMock = null;
			_optionsServiceMock = null;
			_shellSelectionServiceMock = null;
			_modalDialogServiceMock = null;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Returns test feature instance.
		/// </summary>
		private IFeature GetFeature()
		{
			return new FileBrowserFeature(_serviceProviderMock.Object, _logMock.Object, _optionsServiceMock.Object,
				_shellSelectionServiceMock.Object, _modalDialogServiceMock.Object);
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
			Assert.That(commands.Count, Is.EqualTo(1));
			Assert.That(commands, Contains.Item(CommandIDs.FILE_BROWSER));
		}

		[TestCase(CommandIDs.FILE_BROWSER, true)]
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

		[Test]
		public void IsValidContext_NoContext()
		{
			var feature = GetFeature();

			_shellSelectionServiceMock.Setup(s => s.IsContextActive(ContextType.SolutionHasProjects)).Returns(false).Verifiable();

			var result = feature.IsValidContext(CommandIDs.FILE_BROWSER);

			Assert.That(result, Is.EqualTo(false));
			_shellSelectionServiceMock.Verify(s => s.IsContextActive(It.IsAny<ContextType>()));
		}

		[Test]
		public void Execute()
		{
			var feature = GetFeature();

			_modalDialogServiceMock
				.Setup(d => d.ShowDialog<FileBrowserWindow, FileBrowserViewModel>(It.IsNotNull<string>()))
				.Returns(true)
				.Verifiable();

			var result = feature.Execute(CommandIDs.FILE_BROWSER);

			Assert.That(result, Is.True);
			_modalDialogServiceMock.Verify(d => d.ShowDialog<FileBrowserWindow, FileBrowserViewModel>(It.IsNotNull<string>()));
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