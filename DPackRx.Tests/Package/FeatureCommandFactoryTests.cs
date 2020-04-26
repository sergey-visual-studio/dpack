using System;
using System.Collections.Generic;

using Moq;
using NUnit.Framework;

using DPackRx.Features;
using DPackRx.Package;

namespace DPackRx.Tests.Package
{
	/// <summary>
	/// FeatureCommandFactory tests.
	/// </summary>
	[TestFixture]
	public class FeatureCommandFactoryTests
	{
		#region Fields

		private Mock<IServiceProvider> _serviceProviderMock;
		private Mock<IFeatureCommand> _featureCommandMock;

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
			_featureCommandMock = new Mock<IFeatureCommand>();
			_featureCommandMock.Setup(c => c.Initialize(It.IsAny<IFeature>(), It.IsAny<int>())).Verifiable();

			_serviceProviderMock = new Mock<IServiceProvider>();
			_serviceProviderMock.Setup(s => s.GetService(typeof(IFeatureCommand))).Returns(_featureCommandMock.Object).Verifiable();
		}

		[TearDown]
		public void TearDown()
		{
			_serviceProviderMock = null;
			_featureCommandMock = null;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Returns test service instance.
		/// </summary>
		private IFeatureCommandFactory GetService()
		{
			return new FeatureCommandFactory(_serviceProviderMock.Object);
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
			_serviceProviderMock.Verify(s => s.GetService(typeof(IFeatureCommand)));
			_featureCommandMock.Verify(c => c.Initialize(feature, 123));
		}

		#endregion
	}
}