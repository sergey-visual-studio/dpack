using Microsoft.VisualStudio.Text.Editor;

namespace DPackRx.Features.Bookmarks
{
	/// <summary>
	/// Bookmark glyph tag.
	/// </summary>
	public class BookmarkTag : IGlyphTag
	{
		public BookmarkTag(int number, BookmarkType type)
		{
			this.Number = number;
			this.Type = type;
		}

		#region Properties

		/// <summary>
		/// Bookmark number.
		/// </summary>
		public int Number { get; }

		/// <summary>
		/// Bookmark type.
		/// </summary>
		public BookmarkType Type { get; }

		#endregion
	}
}