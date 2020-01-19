using Moq;
using NUnit.Framework;

using DPackRx.Features;
using DPackRx.Options;
using DPackRx.Package;
using DPackRx.Services;

namespace DPackRx.Tests.Options
{
	/// <summary>
	/// OptionsPersistence tests.
	/// </summary>
	[TestFixture]
	public class OptionsPersistenceServiceTests
	{
		#region Fields

		private Mock<ILog> _logMock;
		private Mock<IPackageService> _packageServiceMock;

		#endregion

		#region Tests Setup

		[SetUp]
		public void Setup()
		{
			_logMock = new Mock<ILog>();
			_logMock.Setup(l => l.LogMessage(It.IsAny<string>(), It.IsAny<string>())).Verifiable();

			_packageServiceMock = new Mock<IPackageService>();
		}

		[TearDown]
		public void TearDown()
		{
			_logMock = null;
			_packageServiceMock = null;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Returns test service instance.
		/// </summary>
		private IOptionsPersistenceService GetService()
		{
			return new OptionsPersistenceService(_logMock.Object, _packageServiceMock.Object);
		}

		#endregion

		#region Tests

		[TestCase(KnownFeature.SupportOptions, 1)]
		[TestCase(KnownFeature.Miscellaneous, 0)]
		[TestCase(KnownFeature.CodeBrowser, 0)]
		[TestCase(KnownFeature.FileBrowser, 5)]
		[TestCase(KnownFeature.Bookmarks, 0)]
		public void LoadDefaultOptions(KnownFeature feature, int expectedCount)
		{
			var service = GetService();

			var defaults = service.LoadDefaultOptions(feature);

			if (expectedCount == 0)
			{
				Assert.That(defaults, Is.Null);
			}
			else
			{
				Assert.That(defaults, Is.Not.Null);
				Assert.That(defaults.Count, Is.EqualTo(expectedCount));
			}
		}

		#endregion
	}
}