namespace DPackRx.Features.Bookmarks
{
	/// <summary>
	/// Bookmark callback client.
	/// </summary>
	/// <remarks>
	/// Bookmark location changes as user modified the text preceding the bookmark.
	/// Call back is used to report the actual bookmark line number as oppose to one initially set.
	/// </remarks>
	public interface IBookmarkCallbackClient
	{
		/// <summary>
		/// Returns current bookmark line number.
		/// </summary>
		/// <param name="bookmark">Bookmark.</param>
		/// <returns>Line number or 0 if bookmark's not found.</returns>
		int GetBookmarkLine(Bookmark bookmark);
	}
}