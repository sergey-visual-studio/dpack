using System;
using System.ComponentModel.Design;

using Moq;
using NUnit.Framework;

using DPackRx.Features;
using DPackRx.Package;
using DPackRx.Services;

namespace DPackRx.Tests.Package
{
	/// <summary>
	/// FeatureCommand tests.
	/// </summary>
	[TestFixture]
	public class FeatureCommandTests
	{
		#region Fields

		private Mock<IMenuCommandService> _menuCommandServiceMock;
		private Mock<ILog> _logMock;
		private Mock<IMessageService> _messageServiceMock;
		private Mock<IUtilsService> _utilsServiceMock;
		private Mock<IFeature> _featureMock;

		#endregion

		#region Tests Setup

		[SetUp]
		public void Setup()
		{
			_menuCommandServiceMock = new Mock<IMenuCommandService>();
			_menuCommandServiceMock.Setup(m => m.AddCommand(It.IsAny<MenuCommand>())).Verifiable();

			_logMock = new Mock<ILog>();
			_logMock.Setup(l => l.LogMessage(It.IsAny<string>(), It.IsAny<string>())).Verifiable();

			_messageServiceMock = new Mock<IMessageService>();
			_messageServiceMock.Setup(m => m.ShowError(It.IsAny<string>(), true)).Verifiable();

			_utilsServiceMock = new Mock<IUtilsService>();

			_featureMock = new Mock<IFeature>();
			_featureMock.SetupGet(f => f.KnownFeature).Returns(KnownFeature.Miscellaneous);
			_featureMock.Setup(f => f.IsValidContext(It.IsAny<int>())).Returns(true).Verifiable();
			_featureMock.Setup(f => f.Execute(It.IsAny<int>())).Returns(true).Verifiable();
		}

		[TearDown]
		public void TearDown()
		{
			_menuCommandServiceMock = null;
			_logMock = null;
			_messageServiceMock = null;
			_utilsServiceMock = null;
			_featureMock = null;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Returns test service instance.
		/// </summary>
		private IFeatureCommand GetCommand(bool initialize = true)
		{
			var feature = new FeatureCommand(_logMock.Object, _menuCommandServiceMock.Object, _messageServiceMock.Object, _utilsServiceMock.Object);
			if (initialize)
				feature.Initialize(_featureMock.Object, 123);
			return feature;
		}

		#endregion

		#region Tests

		[Test]
		public void Initialize()
		{
			var command = GetCommand();

			Assert.That(command.CommandId, Is.EqualTo(123));
			Assert.That(command.Feature, Is.EqualTo(KnownFeature.Miscellaneous));
			_menuCommandServiceMock.Verify(m => m.AddCommand(It.IsNotNull<MenuCommand>()));
		}

		[Test]
		public void Initialize_ErrorHandling()
		{
			var command = GetCommand(false);

			Assert.Throws<ArgumentNullException>(() => command.Initialize(null, 123));
			Assert.Throws<ArgumentException>(() => command.Initialize(_featureMock.Object, 0));
			_menuCommandServiceMock.Verify(m => m.AddCommand(It.IsNotNull<MenuCommand>()), Times.Never);
		}

		[Test]
		public void IsValidContext()
		{
			var command = GetCommand();

			var result = command.IsValidContext();

			Assert.That(result, Is.True);
			_featureMock.Verify(f => f.IsValidContext(It.IsAny<int>()));
		}

		[Test]
		public void IsValidContext_NotInitialized()
		{
			var command = GetCommand(false);

			var result = command.IsValidContext();

			Assert.That(result, Is.False);
			_featureMock.Verify(f => f.IsValidContext(It.IsAny<int>()), Times.Never);
		}

		[Test]
		public void Execute()
		{
			var command = GetCommand();

			var result = command.Execute();

			Assert.That(result, Is.True);
			_featureMock.Verify(f => f.Execute(It.IsAny<int>()));
		}

		[Test]
		public void Execute_NotInitialized()
		{
			var command = GetCommand(false);

			var result = command.Execute();

			Assert.That(result, Is.False);
			_featureMock.Verify(f => f.Execute(It.IsAny<int>()), Times.Never);
		}

		#endregion
	}
}