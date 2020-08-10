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
	/// SupportOptionsFirstTimeUse tests.
	/// </summary>
	[TestFixture]
	public class SupportOptionsFirstTimeUseTests
	{
		#region Fields

		private Mock<IOptionsService> _optionsServiceMock;
		private Mock<IPackageService> _packageServiceMock;
		private Mock<IShellHelperService> _shellHelperServiceMock;
		private Mock<IShellInfoBarService> _shellInfoBarServiceMock;
		private Mock<IMessageService> _messageServiceMock;

		#endregion

		#region Tests Setup

		[SetUp]
		public void Setup()
		{
			_optionsServiceMock = new Mock<IOptionsService>();
			_optionsServiceMock.Setup(o => o.GetBoolOption(It.IsAny<KnownFeature>(), It.IsAny<string>(), false)).Returns(false).Verifiable();
			_optionsServiceMock.Setup(o => o.SetBoolOption(It.IsAny<KnownFeature>(), It.IsAny<string>(), true)).Verifiable();

			_packageServiceMock = new Mock<IPackageService>();
			_packageServiceMock.Setup(p => p.GetResourceString(It.IsAny<int>())).Returns(string.Empty).Verifiable();

			_shellHelperServiceMock = new Mock<IShellHelperService>();
			_shellHelperServiceMock.Setup(s => s.ShowOptions<OptionsGeneral>()).Verifiable();
			_shellHelperServiceMock.Setup(s => s.AssignShortcuts()).Verifiable();

			_shellInfoBarServiceMock = new Mock<IShellInfoBarService>();
			_shellInfoBarServiceMock.Setup(s => s.ShowInfoBar(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<bool>())).Verifiable();

			_messageServiceMock = new Mock<IMessageService>();
			_messageServiceMock.Setup(m => m.ShowQuestion(It.IsAny<string>())).Returns(true).Verifiable();
		}

		[TearDown]
		public void TearDown()
		{
			_optionsServiceMock = null;
			_packageServiceMock = null;
			_shellHelperServiceMock = null;
			_messageServiceMock = null;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Returns test feature instance.
		/// </summary>
		private ISolutionEvents GetFeature()
		{
			return new SupportOptionsFirstTimeUse(_optionsServiceMock.Object,
				_packageServiceMock.Object, _shellHelperServiceMock.Object, _shellInfoBarServiceMock.Object,
				_messageServiceMock.Object);
		}

		#endregion

		#region Tests

		[TestCase(true)]
		[TestCase(false)]
		public void SolutionOpened(bool alreadyProcessed)
		{
			var feature = GetFeature();

			_optionsServiceMock.Setup(o => o.GetBoolOption(It.IsAny<KnownFeature>(), It.IsAny<string>(), false)).Returns(alreadyProcessed);

			feature.SolutionOpened(true);

			_optionsServiceMock.Verify(o => o.GetBoolOption(It.IsAny<KnownFeature>(), It.IsAny<string>(), false));
			_optionsServiceMock.Verify(o => o.SetBoolOption(It.IsAny<KnownFeature>(), It.IsAny<string>(), true));

			if (alreadyProcessed)
			{
				_shellInfoBarServiceMock.Verify(s => s.ShowInfoBar(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<bool>()), Times.Never);
			}
			else
			{
				_shellInfoBarServiceMock.Verify(s => s.ShowInfoBar(It.IsNotNull<string>(), It.IsNotNull<string>(), It.IsNotNull<Action>(), true), Times.Once);
			}
		}

		[Test]
		public void SolutionOpened_JustOnce()
		{
			var feature = GetFeature();

			feature.SolutionOpened(true);
			feature.SolutionOpened(true);

			_shellInfoBarServiceMock.Verify(s => s.ShowInfoBar(It.IsNotNull<string>(), It.IsNotNull<string>(), It.IsNotNull<Action>(), true), Times.Once);
		}

		[Test]
		public void SolutionOpened_NoShortcutAssignment()
		{
			var feature = GetFeature();

			_messageServiceMock.Setup(m => m.ShowQuestion(It.IsAny<string>())).Returns(false).Verifiable();

			feature.SolutionOpened(true);

			_shellInfoBarServiceMock.Verify(s => s.ShowInfoBar(It.IsNotNull<string>(), It.IsNotNull<string>(), It.IsNotNull<Action>(), true), Times.Once);
			_optionsServiceMock.Verify(o => o.GetBoolOption(It.IsAny<KnownFeature>(), It.IsAny<string>(), false));
			_optionsServiceMock.Verify(o => o.SetBoolOption(It.IsAny<KnownFeature>(), It.IsAny<string>(), true));
		}

		#endregion
	}
}