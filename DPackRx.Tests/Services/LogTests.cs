using System.IO;

using Moq;
using NUnit.Framework;

using DPackRx.Package;
using DPackRx.Services;

namespace DPackRx.Tests.Services
{
	/// <summary>
	/// Log tests.
	/// </summary>
	[TestFixture]
	public class LogTests
	{
		#region Fields

		private Mock<IPackageService> _packageServiceMock;

		#endregion

		#region Tests Setup

		[SetUp]
		public void Setup()
		{
			_packageServiceMock = new Mock<IPackageService>();
			_packageServiceMock.SetupGet(p => p.VSVersion).Returns("test").Verifiable();
			_packageServiceMock.SetupGet(p => p.VSKnownVersion).Returns("test").Verifiable();
		}

		[TearDown]
		public void TearDown()
		{
			_packageServiceMock = null;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Returns test service instance.
		/// </summary>
		private ILog GetService()
		{
			return new Log(_packageServiceMock.Object);
		}

		#endregion

		#region Tests

		[Test]
		public void Enabled()
		{
			var service = GetService();

			Assert.That(service.Enabled, Is.False);
		}

		[Test]
		public void FileName()
		{
			var service = GetService();

			try
			{
				service.Enabled = true;

				Assert.That(service.Enabled, Is.True);
				Assert.That(service.FileName, Is.Not.Null.Or.Empty);
				Assert.That(File.Exists(service.FileName), Is.True);
				_packageServiceMock.VerifyGet(p => p.VSVersion);
				_packageServiceMock.VerifyGet(p => p.VSKnownVersion);
			}
			finally
			{
				service.Enabled = false;

				Assert.That(service.Enabled, Is.False);

				var path = Path.GetDirectoryName(service.FileName);
				if (Directory.Exists(path))
					Directory.Delete(path, true);
			}
		}

		[Test]
		public void LogMessage()
		{
			var service = GetService();

			try
			{
				service.Enabled = true;

				var size1 = new FileInfo(service.FileName).Length;

				service.LogMessage("test");

				var size2 = new FileInfo(service.FileName).Length;

				Assert.That(size1, Is.GreaterThan(0));
				Assert.That(size2, Is.GreaterThan(0));
				Assert.That(size2, Is.GreaterThan(size1));
			}
			finally
			{
				service.Enabled = false;

				Assert.That(service.Enabled, Is.False);

				var path = Path.GetDirectoryName(service.FileName);
				if (Directory.Exists(path))
					Directory.Delete(path, true);
			}
		}

		#endregion
	}
}