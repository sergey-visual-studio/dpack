using System;

using Moq;
using NUnit.Framework;

using DPackRx.CodeModel;
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
		private Mock<IShellEventsService> _shellEventsServiceMock;

		#endregion

		#region Tests Setup

		[SetUp]
		public void Setup()
		{
			_serviceProviderMock = new Mock<IServiceProvider>();

			_logMock = new Mock<ILog>();
			_logMock.Setup(l => l.LogMessage(It.IsAny<string>(), It.IsAny<string>())).Verifiable();
			_logMock.Setup(l => l.LogMessage(It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<string>())).Verifiable();

			_optionsServiceMock = new Mock<IOptionsService>();

			_shellSelectionServiceMock = new Mock<IShellSelectionService>();
			_shellSelectionServiceMock.Setup(s => s.IsContextActive(It.IsAny<ContextType>())).Returns(true).Verifiable();

			_modalDialogServiceMock = new Mock<IModalDialogService>();

			_shellEventsServiceMock = new Mock<IShellEventsService>();
			_shellEventsServiceMock.Setup(s => s.SubscribeSolutionEvents(It.IsAny<ISolutionEvents>())).Verifiable();
			_shellEventsServiceMock.Setup(s => s.UnsubscribeSolutionEvents(It.IsAny<ISolutionEvents>())).Verifiable();
		}

		[TearDown]
		public void TearDown()
		{
			_serviceProviderMock = null;
			_logMock = null;
			_optionsServiceMock = null;
			_shellSelectionServiceMock = null;
			_modalDialogServiceMock = null;
			_shellEventsServiceMock = null;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Returns test feature instance.
		/// </summary>
		private FileBrowserFeature GetFeature()
		{
			return new FileBrowserFeature(_serviceProviderMock.Object, _logMock.Object, _optionsServiceMock.Object,
				_shellSelectionServiceMock.Object, _modalDialogServiceMock.Object, _shellEventsServiceMock.Object);
		}

		#endregion

		#region Tests

		[Test]
		public void Initialize()
		{
			var feature = GetFeature();

			feature.Initialize();

			Assert.That(feature.Initialized, Is.True);
			_shellEventsServiceMock.Verify(s => s.SubscribeSolutionEvents(It.IsNotNull<ISolutionEvents>()), Times.Once);
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
		public void Execute([Values] bool isCashed)
		{
			var feature = GetFeature();
			var cache = new SolutionModel();
			if (isCashed)
				feature.Cache = cache;

			_modalDialogServiceMock
				.Setup(d => d.ShowDialog<FileBrowserWindow, FileBrowserViewModel, SolutionModel>(It.IsNotNull<string>(), It.IsAny<object>()))
				.Returns(cache)
				.Verifiable();

			var result = feature.Execute(CommandIDs.FILE_BROWSER);

			Assert.That(result, Is.True);
			Assert.That(feature.Cache, Is.Not.Null);
			_modalDialogServiceMock.Verify(d => d.ShowDialog<FileBrowserWindow, FileBrowserViewModel, SolutionModel>(It.IsNotNull<string>(), It.IsAny<object>()));
			_logMock.Verify(l => l.LogMessage(It.IsNotNull<string>(), null, It.IsNotNull<string>()), Times.Once);
		}

		[Test]
		public void Execute_InvalidCommand()
		{
			var feature = GetFeature();

			var result = feature.Execute(0);

			Assert.That(result, Is.False);
		}

		[Test]
		public void SolutionOpened()
		{
			var feature = GetFeature();
			var featureEvents = (ISolutionEvents)feature;

			feature.Execute(0);
			featureEvents.SolutionOpened(false);

			Assert.That(feature.Cache, Is.Null);
			_logMock.Verify(l => l.LogMessage(It.IsNotNull<string>(), It.IsNotNull<string>()), Times.Once);
		}

		[Test]
		public void SolutionClosing()
		{
			var feature = GetFeature();
			var featureEvents = (ISolutionEvents)feature;

			Assert.DoesNotThrow(() => featureEvents.SolutionClosing());
		}

		[Test]
		public void SolutionClosed()
		{
			var feature = GetFeature();
			var featureEvents = (ISolutionEvents)feature;

			feature.Execute(0);
			featureEvents.SolutionClosed();

			Assert.That(feature.Cache, Is.Null);
			_logMock.Verify(l => l.LogMessage(It.IsNotNull<string>(), It.IsNotNull<string>()), Times.Once);
		}

		[Test]
		public void SolutionSaved()
		{
			var feature = GetFeature();
			var featureEvents = (ISolutionEvents)feature;

			Assert.DoesNotThrow(() => featureEvents.SolutionSaved());
		}

		[Test]
		public void SolutionRenamed()
		{
			var feature = GetFeature();
			var featureEvents = (ISolutionEvents)feature;

			Assert.DoesNotThrow(() => featureEvents.SolutionRenamed(null, null));
		}

		[Test]
		public void ProjectAdded()
		{
			var feature = GetFeature();
			var featureEvents = (ISolutionEvents)feature;

			feature.Execute(0);
			featureEvents.ProjectAdded(null);

			Assert.That(feature.Cache, Is.Null);
			_logMock.Verify(l => l.LogMessage(It.IsNotNull<string>(), It.IsNotNull<string>()), Times.Once);
		}

		[Test]
		public void ProjectDeleted()
		{
			var feature = GetFeature();
			var featureEvents = (ISolutionEvents)feature;

			feature.Execute(0);
			featureEvents.ProjectDeleted(null);

			Assert.That(feature.Cache, Is.Null);
			_logMock.Verify(l => l.LogMessage(It.IsNotNull<string>(), It.IsNotNull<string>()), Times.Once);
		}

		[Test]
		public void ProjectRenamed()
		{
			var feature = GetFeature();
			var featureEvents = (ISolutionEvents)feature;

			feature.Execute(0);
			featureEvents.ProjectRenamed(null);

			Assert.That(feature.Cache, Is.Null);
			_logMock.Verify(l => l.LogMessage(It.IsNotNull<string>(), It.IsNotNull<string>()), Times.Once);
		}

		[Test]
		public void ProjectUnloaded()
		{
			var feature = GetFeature();
			var featureEvents = (ISolutionEvents)feature;

			feature.Execute(0);
			featureEvents.ProjectUnloaded(null);

			Assert.That(feature.Cache, Is.Null);
			_logMock.Verify(l => l.LogMessage(It.IsNotNull<string>(), It.IsNotNull<string>()), Times.Once);
		}

		[Test]
		public void FileAdded()
		{
			var feature = GetFeature();
			var featureEvents = (ISolutionEvents)feature;

			feature.Execute(0);
			featureEvents.FileAdded(null, null);

			Assert.That(feature.Cache, Is.Null);
			_logMock.Verify(l => l.LogMessage(It.IsNotNull<string>(), It.IsNotNull<string>()), Times.Once);
		}

		[Test]
		public void FileDeleted()
		{
			var feature = GetFeature();
			var featureEvents = (ISolutionEvents)feature;

			feature.Execute(0);
			featureEvents.FileDeleted(null, null);

			Assert.That(feature.Cache, Is.Null);
			_logMock.Verify(l => l.LogMessage(It.IsNotNull<string>(), It.IsNotNull<string>()), Times.Once);
		}

		[Test]
		public void FileRenamed()
		{
			var feature = GetFeature();
			var featureEvents = (ISolutionEvents)feature;

			feature.Execute(0);
			featureEvents.FileRenamed(null, null, null);

			Assert.That(feature.Cache, Is.Null);
			_logMock.Verify(l => l.LogMessage(It.IsNotNull<string>(), It.IsNotNull<string>()), Times.Once);
		}

		[Test]
		public void FileChanged()
		{
			var feature = GetFeature();
			var featureEvents = (ISolutionEvents)feature;

			Assert.DoesNotThrow(() => featureEvents.FileChanged(null, null));
		}

		[Test]
		public void FileOpened()
		{
			var feature = GetFeature();
			var featureEvents = (ISolutionEvents)feature;

			Assert.DoesNotThrow(() => featureEvents.FileOpened(null, null));
		}

		[Test]
		public void FileClosed()
		{
			var feature = GetFeature();
			var featureEvents = (ISolutionEvents)feature;

			Assert.DoesNotThrow(() => featureEvents.FileClosed(null, null));
		}

		[Test]
		public void FileSaved()
		{
			var feature = GetFeature();
			var featureEvents = (ISolutionEvents)feature;

			Assert.DoesNotThrow(() => featureEvents.FileSaved(null, null));
		}

		[Test]
		public void Dispose()
		{
			var feature = GetFeature();

			((IDisposable)feature).Dispose();

			Assert.That(feature.Initialized, Is.False);
			_shellEventsServiceMock.Verify(s => s.UnsubscribeSolutionEvents(It.IsNotNull<ISolutionEvents>()), Times.Once);
		}

		#endregion
	}
}