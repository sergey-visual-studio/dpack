using System;

using Moq;
using NUnit.Framework;

using DPackRx.Features;
using DPackRx.Features.Miscellaneous;
using DPackRx.Features.SupportOptions;
using DPackRx.Services;

namespace DPackRx.Tests.Services
{
	/// <summary>
	/// FeatureFactory tests.
	/// </summary>
	[TestFixture]
	public class FeatureFactoryTests
	{
		#region Fields

		private Mock<IServiceProvider> _serviceProviderMock;
		private Mock<ILog> _logMock;
		private Mock<IMessageService> _messageServiceMock;

		#endregion

		#region Tests Setup

		[SetUp]
		public void Setup()
		{
			_serviceProviderMock = new Mock<IServiceProvider>();
			_serviceProviderMock.Setup(s => s.GetService(typeof(SupportOptionsFeature))).Returns(new SupportOptionsFeature()).Verifiable();
			_serviceProviderMock.Setup(s => s.GetService(typeof(MiscellaneousFeature))).Returns(new MiscellaneousFeature()).Verifiable();

			_logMock = new Mock<ILog>();
			_logMock.Setup(l => l.LogMessage(It.IsAny<string>(), It.IsAny<string>())).Verifiable();

			_messageServiceMock = new Mock<IMessageService>();
			_messageServiceMock.Setup(m => m.ShowError(It.IsAny<string>(), true)).Verifiable();
		}

		[TearDown]
		public void TearDown()
		{
			_serviceProviderMock = null;
			_logMock = null;
			_messageServiceMock = null;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Returns test service instance.
		/// </summary>
		private IFeatureFactory GetService()
		{
			return new FeatureFactory(_serviceProviderMock.Object, _logMock.Object, _messageServiceMock.Object);
		}

		#endregion

		#region Tests

		[Test]
		public void GetFeatureName()
		{
			var service = GetService();

			var name = service.GetFeatureName(KnownFeature.SupportOptions);

			Assert.That(name, Is.Not.Null.And.Not.Empty);
		}

		[Test]
		public void GetFeature()
		{
			var service = GetService();

			var feature = service.GetFeature(KnownFeature.SupportOptions);

			Assert.That(feature, Is.Not.Null);
			Assert.That(feature.KnownFeature, Is.EqualTo(KnownFeature.SupportOptions));
		}

		[Test]
		public void GetFeature_SameInstance()
		{
			var service = GetService();

			var feature1 = service.GetFeature(KnownFeature.SupportOptions);
			var feature2 = service.GetFeature(KnownFeature.SupportOptions);

			Assert.That(feature1, Is.Not.Null);
			Assert.That(feature2, Is.Not.Null);
			Assert.That(feature1, Is.EqualTo(feature2));
		}

		[Test]
		public void GetAllFeatures()
		{
			var service = GetService();

			var features = service.GetAllFeatures();

			Assert.That(features, Is.Not.Null);
			Assert.That(features.Count, Is.EqualTo(2), "Feature count must match test setup features");
		}

		#endregion
	}
}