using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using System.Windows.Media;

using DPackRx.CodeModel;
using DPackRx.Extensions;
using DPackRx.Package;

using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace DPackRx.Services
{
	/// <summary>
	/// Shell image service.
	/// </summary>
	public class ShellImageService : IShellImageService
	{
		#region Fields

		private readonly IPackageService _packageService;
		private IVsImageService2 _imageService;

		[Import]
		internal IGlyphService _glyphService = null;

		#endregion

		public ShellImageService(IPackageService packageService)
		{
			_packageService = packageService;
		}

		#region IShellImageService Members

		/// <summary>
		/// Returns file image.
		/// </summary>
		/// <param name="fileName">File mame.</param>
		/// <returns>Image.</returns>
		public ImageSource GetFileImage(string fileName)
		{
			if (string.IsNullOrEmpty(fileName))
				return null;

			ThreadHelper.ThrowIfNotOnUIThread();

			var imageService = GetImageService();
			var moniker = imageService.GetImageMonikerForFile(fileName);
			var attributes = GetImageAttributes();

			var image = imageService.GetImage(moniker, attributes);
			if (image == null)
				return null;

			image.get_Data(out object data);
			return (ImageSource)data;
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
			ThreadHelper.ThrowIfNotOnUIThread();

			StandardGlyphItem item;
			if (isStatic)
			{
				item = StandardGlyphItem.GlyphItemPublic;
			}
			else
			{
				switch (modifier)
				{
					case Modifier.Public:
						item = StandardGlyphItem.GlyphItemPublic;
						break;
					case Modifier.Internal:
						item = StandardGlyphItem.GlyphItemInternal;
						break;
					case Modifier.Protected:
						item = StandardGlyphItem.GlyphItemProtected;
						break;
					case Modifier.ProtectedInternal:
						item = StandardGlyphItem.GlyphItemFriend;
						break;
					case Modifier.Private:
						item = StandardGlyphItem.GlyphItemPrivate;
						break;
					default:
						item = StandardGlyphItem.GlyphItemFriend;
						break;
				}
			}

			StandardGlyphGroup group;
			switch (kind)
			{
				case Kind.Class:
					group = StandardGlyphGroup.GlyphGroupClass;
					break;
				case Kind.Module:
					group = StandardGlyphGroup.GlyphGroupModule;
					break;
				case Kind.Struct:
					group = StandardGlyphGroup.GlyphGroupStruct;
					break;
				case Kind.Enum:
					group = StandardGlyphGroup.GlyphGroupEnum;
					break;
				case Kind.Interface:
					group = StandardGlyphGroup.GlyphGroupInterface;
					break;
				case Kind.Method:
					group = StandardGlyphGroup.GlyphGroupMethod;
					break;
				case Kind.Property:
					group = StandardGlyphGroup.GlyphGroupProperty;
					break;
				case Kind.Variable:
					group = StandardGlyphGroup.GlyphGroupVariable;
					break;
				case Kind.Event:
					group = StandardGlyphGroup.GlyphGroupEvent;
					break;
				case Kind.Delegate:
					group = StandardGlyphGroup.GlyphGroupDelegate;
					break;
				default:
					group = StandardGlyphGroup.GlyphGroupUnknown;
					break;
			}

			var image = GetGlyphService().GetGlyph(group, item);
			return image;
		}

		/// <summary>
		/// Returns well known image.
		/// </summary>
		/// <param name="image">Image type.</param>
		/// <returns>Image.</returns>
		public ImageSource GetWellKnownImage(WellKnownImage knownImage)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			int id;
			switch (knownImage)
			{
				case WellKnownImage.AllFiles:
					id = KnownImageIds.ShowAllFiles;
					break;
				case WellKnownImage.AllCode:
					id = KnownImageIds.ShowAllCode;
					break;
				case WellKnownImage.Members:
					id = KnownImageIds.ListMembers;
					break;
				case WellKnownImage.Classes:
					id = KnownImageIds.ClassPublic;
					break;
				case WellKnownImage.Methods:
					id = KnownImageIds.MethodPublic;
					break;
				case WellKnownImage.Properties:
					id = KnownImageIds.PropertyPublic;
					break;
				default:
					throw new ApplicationException($"Unknown image {knownImage}");
			}
			var moniker = new ImageMoniker { Guid = KnownImageIds.ImageCatalogGuid, Id = id };

			var imageService = GetImageService();
			var attributes = GetImageAttributes();
			var image = imageService.GetImage(moniker, attributes);
			if (image == null)
				return null;

			image.get_Data(out object data);
			return (ImageSource)data;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Returns shell image service.
		/// </summary>
		private IVsImageService2 GetImageService()
		{
			if (_imageService == null)
				_imageService = _packageService.GetService<IVsImageService2, SVsImageService>();

			return _imageService;
		}

		/// <summary>
		/// Returns shell glyph service.
		/// </summary>
		private IGlyphService GetGlyphService()
		{
			if (_glyphService == null)
				this.SatisfyImportsOnce();

			return _glyphService;
		}

		private ImageAttributes GetImageAttributes()
		{
			return new ImageAttributes
				{
					StructSize = Marshal.SizeOf(typeof(ImageAttributes)),
					Format = (uint)_UIDataFormat.DF_WPF,
					LogicalHeight = 16,
					LogicalWidth = 16,
					Flags = (uint)_ImageAttributesFlags.IAF_RequiredFlags,
					ImageType = (uint)_UIImageType.IT_Bitmap
				};
		}

		#endregion
	}
}