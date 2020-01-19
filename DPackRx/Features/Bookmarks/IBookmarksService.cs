using System;
using System.Collections.Generic;

namespace DPackRx.Features.Bookmarks
{
	/// <summary>
	/// Bookmarks service.
	/// </summary>
	public interface IBookmarksService
	{
		/// <summary>
		/// Bookmark count.
		/// </summary>
		int BookmarkCount { get; }

		/// <summary>
		/// Bookmark callback count.
		/// </summary>
		int BookmarkCallbackCount { get; }

		/// <summary>
		/// Raised when bookmarks change.
		/// </summary>
		event BookmarksEventHandler Changed;

		/// <summary>
		/// Registers callback client.
		/// </summary>
		/// <param name="client">Client.</param>
		/// <param name="fileName">Document's file name.</param>
		void RegisterCallback(IBookmarkCallbackClient client, string fileName);

		/// <summary>
		/// Unregisters callback client.
		/// </summary>
		/// <param name="client">Client.</param>
		void UnregisterCallback(IBookmarkCallbackClient client);

		/// <summary>
		/// Returns callback client associated file name.
		/// </summary>
		/// <param name="client">Client.</param>
		/// <returns>File name.</returns>
		string GetCallbackFileName(IBookmarkCallbackClient client);

		/// <summary>
		/// Returns all file bookmarks.
		/// </summary>
		/// <param name="fileName">File name.</param>
		/// <returns>Bookmarks.</returns>
		IEnumerable<Bookmark> GetFileBookmarks(string fileName);

		/// <summary>
		/// Sets current file bookmark.
		/// </summary>
		/// <param name="number">Bookmark number.</param>
		/// <returns>Whether bookmark's been set or not.</returns>
		bool SetBookmark(int number);

		/// <summary>
		/// Navigates to current file bookmark.
		/// </summary>
		/// <param name="number">Bookmark number.</param>
		/// <returns>Whether bookmark's been navigated to or not.</returns>
		bool GoToBookmark(int number);

		/// <summary>
		/// Sets global bookmark.
		/// </summary>
		/// <param name="number">Bookmark number.</param>
		/// <returns>Whether global bookmark's been set or not.</returns>
		bool SetGlobalBookmark(int number);

		/// <summary>
		/// Navigates to global bookmark.
		/// </summary>
		/// <param name="number">Bookmark number.</param>
		/// <returns>Whether global bookmark's been navigated to or not.</returns>
		bool GoToGlobalBookmark(int number);

		/// <summary>
		/// Clears all current file bookmarks.
		/// </summary>
		/// <returns>Whether all file bookmarks have been cleared or not.</returns>
		bool ClearFileBookmarks();

		/// <summary>
		/// Clears all solution bookmarks.
		/// </summary>
		/// <returns>Whether all solution bookmarks have been cleared or not.</returns>
		bool ClearAllBookmarks();

		/// <summary>
		/// Updates bookmark in the document.
		/// </summary>
		/// <param name="fileName">File name.</param>
		/// <param name="number">Bookmark number.</param>
		/// <param name="type">Bookmark type.</param>
		void UpdateBookmark(string fileName, int number, BookmarkType type);

		/// <summary>
		/// Removes bookmark from the document.
		/// </summary>
		/// <param name="fileName">File name.</param>
		/// <param name="number">Bookmark number.</param>
		/// <param name="type">Bookmark type.</param>
		void RemoveBookmark(string fileName, int number, BookmarkType type);
	}

	public delegate void BookmarksEventHandler(object sender, BookmarkEventArgs e);

	#region BookmarkEventArgs class

	/// <summary>
	/// Bookmark event arguments.
	/// </summary>
	public class BookmarkEventArgs : EventArgs
	{
		/// <summary>
		/// File name with path. Empty value indicates the entire solution change.
		/// </summary>
		public string FileName { get; set; }

		/// <summary>
		/// Bookmark number 1 to 10. 0 value indicates the entire file change.
		/// </summary>
		public int Number { get; set; }

		/// <summary>
		/// Bookmarks type.
		/// </summary>
		public BookmarkType Type { get; set; }
	}

	#endregion
}