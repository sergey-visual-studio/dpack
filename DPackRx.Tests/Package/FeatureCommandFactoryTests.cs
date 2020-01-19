using System;
using System.Collections.Generic;
using System.ComponentModel.Design;

using Moq;
using NUnit.Framework;

using DPackRx.Features;
using DPackRx.Package;
using DPackRx.Services;

namespace DPackRx.Tests.Package
{
	/// <summary>
	/// FeatureCommandFactory tests.
	/// </summary>
	[TestFixture]
	public class FeatureCommandFactoryTests
	{
		#region Fields

		private Mock<IMenuCommandService> _menuCommandServiceMock;
		private Mock<ILog> _logMock;
		private Mock<IMessageService> _messageServiceMock;
		private Mock<IUtilsService> _utilsServiceMock;

		#endregion

		#region TestFeature class

		private class TestFeature : IFeature
		{
			public string Name => throw new NotImplementedException();

			public KnownFeature KnownFeature => KnownFeature.Miscellaneous;

			public void Initialize()
			{
				this.Initialized = true;
			}

			public bool Execute(int commandId)
			{
				throw new NotImplementedException();
			}

			public ICollection<int> GetCommandIds()
			{
				throw new NotImplementedException();
			}

			public bool IsValidContext(int commandId)
			{
				throw new NotImplementedException();
			}

			public bool Initialized { get; set; }
		}

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
		}

		[TearDown]
		public void TearDown()
		{
			_menuCommandServiceMock = null;
			_logMock = null;
			_messageServiceMock = null;
			_utilsServiceMock = null;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Returns test service instance.
		/// </summary>
		private IFeatureCommandFactory GetService()
		{
			return new FeatureCommandFactory(_logMock.Object, _menuCommandServiceMock.Object, _messageServiceMock.Object, _utilsServiceMock.Object);
		}

		/// <summary>
		/// Returns command instance.
		/// </summary>
		private IFeatureCommand GetCommand()
		{
			return new FeatureCommand(_logMock.Object, _menuCommandServiceMock.Object, _messageServiceMock.Object, _utilsServiceMock.Object);
		}

		#endregion

		#region Tests

		[Test]
		public void CreateCommand()
		{
			var service = GetService();

			var feature = new TestFeature();
			var command = service.CreateCommand(feature, 123);

			Assert.That(command, Is.Not.Null);
			Assert.That(command.CommandId, Is.EqualTo(123));
			Assert.That(command.Feature, Is.EqualTo(KnownFeature.Miscellaneous));
			Assert.That(command.Initialized, Is.True);
			_menuCommandServiceMock.Verify(m => m.AddCommand(It.IsNotNull<MenuCommand>()));
		}

		#endregion
	}
}