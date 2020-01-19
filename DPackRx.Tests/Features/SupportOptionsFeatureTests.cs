using System;

using Moq;
using NUnit.Framework;

using DPackRx.Features;
using DPackRx.Features.SupportOptions;
using DPackRx.Options;
using DPackRx.Package;
using DPackRx.Services;

namespace DPackRx.Tests.Features
{
	/// <summary>
	/// SupportOptionsFeature tests.
	/// </summary>
	[TestFixture]
	public class SupportOptionsFeatureTests
	{
		#region Fields

		private Mock<IServiceProvider> _serviceProviderMock;
		private Mock<ILog> _logMock;
		private Mock<IOptionsService> _optionsServiceMock;
		private Mock<IPackageService> _packageServiceMock;
		private Mock<IShellEventsService> _shellEventsServiceMock;
		private Mock<IShellHelperService> _shellHelperServiceMock;
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

			_packageServiceMock = new Mock<IPackageService>();
			_packageServiceMock.Setup(p => p.GetResourceString(It.IsAny<int>())).Returns(string.Empty).Verifiable();

			_shellEventsServiceMock = new Mock<IShellEventsService>();
			_shellEventsServiceMock.Setup(s => s.SubscribeSolutionEvents(It.IsAny<ISolutionEvents>())).Verifiable();

			_shellHelperServiceMock = new Mock<IShellHelperService>();
			_shellHelperServiceMock.Setup(s => s.ShowOptions<OptionsGeneral>()).Verifiable();

			_messageServiceMock = new Mock<IMessageService>();
		}

		[TearDown]
		public void TearDown()
		{
			_serviceProviderMock = null;
			_logMock = null;
			_optionsServiceMock = null;
			_packageServiceMock = null;
			_shellEventsServiceMock = null;
			_shellHelperServiceMock = null;
			_messageServiceMock = null;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Returns test feature instance.
		/// </summary>
		private IFeature GetFeature()
		{
			return new SupportOptionsFeature(_serviceProviderMock.Object, _logMock.Object, _optionsServiceMock.Object,
				_packageServiceMock.Object, _shellEventsServiceMock.Object, _shellHelperServiceMock.Object, _messageServiceMock.Object);
		}

		#endregion

		#region Tests

		[Test]
		public void Initialize()
		{
			var feature = GetFeature();

			feature.Initialize();

			Assert.That(feature.Initialized, Is.True);
			_shellEventsServiceMock.Verify(s => s.SubscribeSolutionEvents(It.IsNotNull<ISolutionEvents>()));
		}

		[Test]
		public void GetCommandIds()
		{
			var feature = GetFeature();

			var commands = feature.GetCommandIds();

			Assert.That(commands, Is.Not.Null);
			Assert.That(commands.Count, Is.EqualTo(3));
			Assert.That(commands, Contains.Item(CommandIDs.PROJECT_HOME));
			Assert.That(commands, Contains.Item(CommandIDs.SUPPORT_EMAIL));
			Assert.That(commands, Contains.Item(CommandIDs.OPTIONS));
		}

		[TestCase(CommandIDs.PROJECT_HOME, true)]
		[TestCase(CommandIDs.SUPPORT_EMAIL, true)]
		[TestCase(CommandIDs.OPTIONS, true)]
		[TestCase(0, false)]
		public void IsValidContext(int commandId, bool expectedResult)
		{
			var feature = GetFeature();

			var result = feature.IsValidContext(commandId);

			Assert.That(result, Is.EqualTo(expectedResult));
		}

		[Test]
		public void Execute_Home()
		{
			var feature = GetFeature();

			var result = feature.Execute(CommandIDs.PROJECT_HOME);

			Assert.That(result, Is.True);
			_packageServiceMock.Verify(p => p.GetResourceString(It.IsAny<int>()));
		}

		[Test]
		public void Execute_Email()
		{
			var feature = GetFeature();

			var result = feature.Execute(CommandIDs.SUPPORT_EMAIL);

			Assert.That(result, Is.True);
			_packageServiceMock.Verify(p => p.GetResourceString(It.IsAny<int>()));
		}

		[Test]
		public void Execute_Options()
		{
			var feature = GetFeature();

			var result = feature.Execute(CommandIDs.OPTIONS);

			Assert.That(result, Is.True);
			_shellHelperServiceMock.Verify(s => s.ShowOptions<OptionsGeneral>());
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