using System.ComponentModel.Composition;

using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace DPackRx.Features.Bookmarks
{
	/// <summary>
	/// Provides bookmark glyph factory.
	/// </summary>
	[Export(typeof(IGlyphFactoryProvider))]
	[Name("NumberedBookmark")]
	[Order(After = "VsTextMarker")]
	[ContentType("any")]
	[TagType(typeof(BookmarkTag))]
	public class BookmarksGlyphFactoryProvider : IGlyphFactoryProvider
	{
		#region IGlyphFactoryProvider Members

		public IGlyphFactory GetGlyphFactory(IWpfTextView view, IWpfTextViewMargin margin)
		{
			return new BookmarksGlyphFactory();
		}

		#endregion
	}
}