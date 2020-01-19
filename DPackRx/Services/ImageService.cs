using System;
using System.Collections.Concurrent;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using DPackRx.CodeModel;
using DPackRx.Helpers;
using DPackRx.Package;

namespace DPackRx.Services
{
	/// <summary>
	/// Image access service.
	/// </summary>
	public class ImageService : IImageService, IDisposable
	{
		#region Fields

		private readonly IPackageService _packageService;
		private readonly IShellImageService _shellImageService;
		private readonly ConcurrentDictionary<string, ImageSource> _fileImages =
			new ConcurrentDictionary<string, ImageSource>(StringComparer.OrdinalIgnoreCase);
		private readonly ConcurrentDictionary<string, ImageSource> _memberImages =
			new ConcurrentDictionary<string, ImageSource>(StringComparer.OrdinalIgnoreCase);

		private const string SHELL_FILES = "ShellFileAssociations";

		#endregion

		public ImageService(IPackageService packageService, IShellImageService shellImageService)
		{
			_packageService = packageService;
			_shellImageService = shellImageService;
		}

		#region IImageService Members

		/// <summary>
		/// Returns associated file image.
		/// </summary>
		/// <param name="fileName">File name.</param>
		/// <returns>Image.</returns>
		public ImageSource GetImage(string fileName)
		{
			if (string.IsNullOrEmpty(fileName))
				return null;

			var extension = Path.GetExtension(fileName);
			if (_fileImages.ContainsKey(extension))
				return _fileImages[extension];

			var image = _shellImageService.GetFileImage(fileName);
			if ((image == null) && !string.IsNullOrEmpty(extension)) // TODO: investigate if this old execution path is still needed
				image = GetExtensionImage(extension);

			_fileImages.TryAdd(extension, image);

			return image;
		}

		/// <summary>
		/// Returns member image.
		/// </summary>
		/// <param name="modifier">Member visibility modifier.</param>
		/// <param name="kind">Member kind.</param>
		/// <param name="isStatic">Whether member is static.</param>
		/// <returns>Image.</returns>
		public ImageSource GetMemberImage(Modifier modifier, Kind kind, bool isStatic)
		{
			var key = $"{(isStatic ? "static-" : string.Empty)}{modifier}-{kind}";
			if (_memberImages.ContainsKey(key))
				return _memberImages[key];

			var image = _shellImageService.GetMemberImage(modifier, kind, isStatic);
			if ((image != null) && isStatic)
			{
				var overlayImage = new BitmapImage(new Uri($"pack://application:,,,/DPackRx;component/Resources/OverlayStatic.png"));

				var imageGroup = new DrawingGroup();
				imageGroup.Children.Add(new ImageDrawing(image, new Rect(0, 0, image.Width, image.Height)));
				imageGroup.Children.Add(new ImageDrawing(overlayImage, new Rect(0, 0, image.Width, image.Height)));

				Image combinedImage = new Image { Source = new DrawingImage(imageGroup) };
				image = combinedImage.Source;
			}

			_memberImages.TryAdd(key, image);

			return image;
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			_fileImages?.Clear();
			_memberImages?.Clear();
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Get extended extension key from VS file associations.
		/// </summary>
		private string GetStudioExtension(string extension)
		{
			try
			{
				var regKeyShellExt = _packageService.GetSystemRegistryRootKey(SHELL_FILES + "\\" + extension);

				if (regKeyShellExt != null)
				{
					using (regKeyShellExt)
					{
						return regKeyShellExt.GetValue(string.Empty, null) as string;
					}
				}
			}
			catch
			{
			}

			return null;
		}

		/// <summary>
		/// Returns file extension image.
		/// </summary>
		private ImageSource GetExtensionImage(string extension)
		{
			var icon = ShellImageHelper.GetShellAssociatedIcon(extension);
			if (icon == null)
			{
				var studioExtension = GetStudioExtension(extension);

				if (!string.IsNullOrEmpty(studioExtension))
					icon = ShellImageHelper.GetClassRootIcon(extension, true);

				if (icon == null)
					icon = ShellImageHelper.GetClassRootIcon(extension, false);
			}

			if (icon == null)
			{
				// TODO: add default no-icon handling
				return null;
			}

			var image = ShellImageHelper.IconToImage(icon);
			return image;
		}

		#endregion
	}
}