using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;

namespace DPackRx.Features.Bookmarks
{
	/// <summary>
	/// Provides the visual element that will appear in the glyph margin.
	/// </summary>
	public class BookmarksGlyphFactory : IGlyphFactory
	{
		#region Fields

		private const string IMAGE_RESOURCE = "pack://application:,,,/DPackRx;component/Features/Bookmarks/Images/{0}Bookmark{1}.png";

		#endregion

		#region IGlyphFactory Members

		public UIElement GenerateGlyph(IWpfTextViewLine line, IGlyphTag tag) // TODO: consider drawing bookmarks directly
		{
			if (!(tag is BookmarkTag))
				return null;

			var bookmarkTag = (BookmarkTag)tag;
			var prefix = bookmarkTag.Type == BookmarkType.Global ? "Global" : string.Empty;

			var image = new Image { Width = 16, Height = 16 };
			image.Source = new BitmapImage(new Uri(string.Format(IMAGE_RESOURCE, prefix, bookmarkTag.Number)));
			return image;
		}

		#endregion
	}
}