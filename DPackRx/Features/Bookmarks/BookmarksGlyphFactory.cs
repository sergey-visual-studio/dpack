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

		private readonly IWpfTextViewMargin _margin;

		#endregion

		public BookmarksGlyphFactory(IWpfTextViewMargin margin)
		{
			_margin = margin;
		}

		#region IGlyphFactory Members

		public UIElement GenerateGlyph(IWpfTextViewLine line, IGlyphTag tag) // TODO: consider drawing bookmarks directly
		{
			if (!(tag is BookmarkTag))
				return null;

			var bookmarkTag = (BookmarkTag)tag;

			var image = new Image { Width = 16, Height = 16 };
			image.Source = new BitmapImage(GetImageUri(bookmarkTag));
			image.ToolTip = GetDisplayName(bookmarkTag);
			return image;
		}

		#endregion

		#region Private Methods

		private Uri GetImageUri(BookmarkTag bookmarkTag)
		{
			var prefix = bookmarkTag.Type == BookmarkType.Global ? "Global" : string.Empty;
			return new Uri($"pack://application:,,,/DPackRx;component/Features/Bookmarks/Images/{prefix}Bookmark{bookmarkTag.Number}.png");
		}

		private string GetDisplayName(BookmarkTag bookmarkTag)
		{
			if (bookmarkTag.Type == BookmarkType.Local)
				return $"Bookmark {bookmarkTag.Number}";
			else
				return $"Global bookmark {bookmarkTag.Number}";
		}

		#endregion
	}
}