namespace DPackRx.Features.Bookmarks
{
	/// <summary>
	/// Creates bookmark tags for the text buffer.
	/// </summary>
	public interface IBookmarksSimpleTagger : IBookmarkCallbackClient
	{
		/// <summary>
		/// Bookmarks service instance.
		/// </summary>
		IBookmarksService BookmarksService { get; }

		/// <summary>
		/// Whether bookmark service callback's been registered.
		/// </summary>
		bool CallbackRegistered { get; }

		/// <summary>
		/// Number of file bookmarks.
		/// </summary>
		int BookmarkCount { get; }
	}
}