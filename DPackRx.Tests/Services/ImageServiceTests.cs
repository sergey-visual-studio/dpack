using System;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

using Moq;
using NUnit.Framework;

using DPackRx.Package;
using DPackRx.Services;
using System.Windows.Media;
using DPackRx.CodeModel;

namespace DPackRx.Tests.Services
{
	/// <summary>
	/// ImageService tests.
	/// </summary>
	[TestFixture]
	[RequiresThread(ApartmentState.STA)]
	public class ImageServiceTests
	{
		#region Fields

		private Mock<IPackageService> _packageServiceMock;
		private Mock<IShellImageService> _shellImageServiceMock;

		#endregion

		#region Tests Setup

		[SetUp]
		public void Setup()
		{
			var current = Application.Current;
			if (current != null)
				current = null;
			var scheme = System.IO.Packaging.PackUriHelper.UriSchemePack;
			if (string.IsNullOrEmpty(scheme))
				throw new ApplicationException("WPF pack scheme is not registered.");

			_packageServiceMock = new Mock<IPackageService>();
			_packageServiceMock.Setup(p => p.GetSystemRegistryRootKey(It.IsAny<string>())).Returns((RegistryKey)null).Verifiable();

			var image = new BitmapImage();
			image.BeginInit();
			image.UriSource = new Uri($"pack://application:,,,/DPackRx;component/Resources/OverlayStatic.png");
			image.EndInit();
			_shellImageServiceMock = new Mock<IShellImageService>();
			_shellImageServiceMock.Setup(i => i.GetFileImage(this.GetType().Assembly.Location)).Returns(image).Verifiable();
			_shellImageServiceMock.Setup(i => i.GetMemberImage(It.IsAny<Modifier>(), It.IsAny<Kind>(), It.IsAny<bool>())).Returns(image).Verifiable();
		}

		[TearDown]
		public void TearDown()
		{
			_packageServiceMock = null;
			_shellImageServiceMock = null;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Returns test service instance.
		/// </summary>
		private IImageService GetService()
		{
			return new ImageService(_packageServiceMock.Object, _shellImageServiceMock.Object);
		}

		#endregion

		#region Tests

		[Test]
		public void GetImage()
		{
			var service = GetService();

			var image = service.GetImage(this.GetType().Assembly.Location);

			Assert.That(image, Is.Not.Null);
			_shellImageServiceMock.Verify(i => i.GetFileImage(this.GetType().Assembly.Location));
			_packageServiceMock.Verify(p => p.GetSystemRegistryRootKey(It.IsAny<string>()), Times.Never);
		}

		[TestCase(null)]
		[TestCase("")]
		public void GetImage_ErrorHandling(string fileName)
		{
			var service = GetService();

			Assert.DoesNotThrow(() => service.GetImage(fileName));
		}

		[Test]
		public void GetImage_SameInstance()
		{
			var service = GetService();

			var image1 = service.GetImage(this.GetType().Assembly.Location);
			var image2 = service.GetImage(this.GetType().Assembly.Location);

			Assert.That(image1, Is.Not.Null);
			Assert.That(image2, Is.Not.Null);
			Assert.That(image1, Is.EqualTo(image2));
		}

		[Test]
		public void GetImage_NoShellImage()
		{
			_shellImageServiceMock.Setup(i => i.GetFileImage(It.IsAny<string>())).Returns((ImageSource)null).Verifiable();
			var service = GetService();

			var image = service.GetImage("test.MadeUpAndUncommonExtension");

			Assert.That(image, Is.Not.Null);
			_shellImageServiceMock.Verify(i => i.GetFileImage(It.IsAny<string>()));
			_packageServiceMock.Verify(p => p.GetSystemRegistryRootKey(It.IsAny<string>()), Times.Never);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void GetMemberImage(bool isStatic)
		{
			var service = GetService();

			var image = service.GetMemberImage(Modifier.Public, Kind.Class, isStatic);

			Assert.That(image, Is.Not.Null);
			_shellImageServiceMock.Verify(i => i.GetMemberImage(It.IsAny<Modifier>(), It.IsAny<Kind>(), It.IsAny<bool>()));
		}

		[Test]
		public void GetMemberImage_SameInstance()
		{
			var service = GetService();

			var image1 = service.GetMemberImage(Modifier.Public, Kind.Class, false);
			var image2 = service.GetMemberImage(Modifier.Public, Kind.Class, false);

			Assert.That(image1, Is.Not.Null);
			Assert.That(image2, Is.Not.Null);
			Assert.That(image1, Is.EqualTo(image2));
		}

		#endregion
	}
}