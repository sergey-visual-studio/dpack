using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DPackRx.Services;

namespace DPackRx.Features.Bookmarks
{
	/// <summary>
	/// Bookmarks service.
	/// </summary>
	public class BookmarksService : IBookmarksService, ISolutionEvents, IDisposable // TODO: consider adding bookmarks solution persistence
	{
		#region Fields

		private readonly ILog _log;
		private readonly IShellEventsService _shellEventsService;
		private readonly IShellStatusBarService _shellStatusBarService;
		private readonly IShellSelectionService _shellSelectionService;
		private readonly IShellHelperService _shellHelperService;
		/// <summary>
		/// File bookmarks, 1-10 local per file and 1-10 global ones for the entire solution.
		/// </summary>
		private readonly Dictionary<string, List<Bookmark>> _bookmarks = new Dictionary<string, List<Bookmark>>(StringComparer.OrdinalIgnoreCase);
		private readonly Dictionary<string, IBookmarkCallbackClient> _bookmarkCallbacks = new Dictionary<string, IBookmarkCallbackClient>(StringComparer.OrdinalIgnoreCase);
		private readonly List<Bookmark> _emptyBookmarks = new List<Bookmark>();
		private bool _solutionEvents;

		private const string LOG_CATEGORY = "Bookmarks";

		#endregion

		public BookmarksService(ILog log, IShellEventsService shellEventsService,
			IShellStatusBarService shellStatusBarService, IShellSelectionService shellSelectionService, IShellHelperService shellHelperService)
		{
			if (log == null)
				throw new ArgumentNullException(nameof(log));

			if (shellEventsService == null)
				throw new ArgumentNullException(nameof(shellEventsService));

			if (shellStatusBarService == null)
				throw new ArgumentNullException(nameof(shellStatusBarService));

			if (shellSelectionService == null)
				throw new ArgumentNullException(nameof(shellSelectionService));

			if (shellHelperService == null)
				throw new ArgumentNullException(nameof(shellHelperService));

			_log = log;
			_shellEventsService = shellEventsService;
			_shellStatusBarService = shellStatusBarService;
			_shellSelectionService = shellSelectionService;
			_shellHelperService = shellHelperService;
		}

		#region IDisposable Members

		public void Dispose()
		{
			if (_shellEventsService != null)
			{
				if (_solutionEvents)
				{
					_solutionEvents = false;
					_shellEventsService.UnsubscribeSolutionEvents(this);
				}
			}
		}

		#endregion

		#region IBookmarksService Members

		/// <summary>
		/// Bookmark count.
		/// </summary>
		public int BookmarkCount
		{
			get { return _bookmarks.Values.Sum(b => b.Count); }
		}

		/// <summary>
		/// Bookmark callback count.
		/// </summary>
		public int BookmarkCallbackCount
		{
			get { return _bookmarkCallbacks.Count; }
		}

		/// <summary>
		/// Raised when bookmarks change.
		/// </summary>
		public event BookmarksEventHandler Changed;

		/// <summary>
		/// Registers callback client.
		/// </summary>
		/// <param name="client">Client.</param>
		/// <param name="fileName">Document's file name.</param>
		public void RegisterCallback(IBookmarkCallbackClient client, string fileName)
		{
			if (client == null)
				throw new ArgumentNullException(nameof(client));

			if (string.IsNullOrEmpty(fileName))
				throw new ArgumentNullException(nameof(fileName));

			if (_bookmarkCallbacks.ContainsKey(fileName) || _bookmarkCallbacks.ContainsValue(client))
			{
				_log.LogMessage($"Callback client has already been registered: {Path.GetFileName(fileName)}", LOG_CATEGORY);
				return;
			}

			_log.LogMessage($"File '{Path.GetFileName(fileName)}' registered for bookmarks callback", LOG_CATEGORY);
			_bookmarkCallbacks.Add(fileName, client);
		}

		/// <summary>
		/// Unregisters callback client.
		/// </summary>
		/// <param name="client">Client.</param>
		public void UnregisterCallback(IBookmarkCallbackClient client)
		{
			if (client == null)
				throw new ArgumentNullException(nameof(client));

			if (!_bookmarkCallbacks.ContainsValue(client))
			{
				_log.LogMessage("Callback client hasn't been registered", LOG_CATEGORY);
				return;
			}

			var fileName = _bookmarkCallbacks.First(b => b.Value == client).Key;
			if (!string.IsNullOrEmpty(fileName))
				_bookmarkCallbacks.Remove(fileName);
			_log.LogMessage($"File '{Path.GetFileName(fileName)}' unregistered for bookmarks callback", LOG_CATEGORY);
		}

		/// <summary>
		/// Returns callback client associated file name.
		/// </summary>
		/// <param name="client">Client.</param>
		/// <returns>File name.</returns>
		public string GetCallbackFileName(IBookmarkCallbackClient client)
		{
			if (client == null)
				throw new ArgumentNullException(nameof(client));

			if (_bookmarkCallbacks.ContainsValue(client))
				return _bookmarkCallbacks.First(b => b.Value == client).Key;
			else
				return null;
		}

		/// <summary>
		/// Returns all file bookmarks.
		/// </summary>
		/// <param name="fileName">File name.</param>
		/// <returns>Bookmarks.</returns>
		public IEnumerable<Bookmark> GetFileBookmarks(string fileName)
		{
			if (string.IsNullOrEmpty(fileName))
				return null;

			if (_bookmarks.ContainsKey(fileName))
				return _bookmarks[fileName];
			else
				return _emptyBookmarks;
		}

		/// <summary>
		/// Sets current file bookmark.
		/// </summary>
		/// <param name="number">Bookmark number.</param>
		/// <returns>Whether bookmark's been set or not.</returns>
		public bool SetBookmark(int number)
		{
			if ((number < 1) || (number > 10))
				throw new ArgumentOutOfRangeException(nameof(number));

			var fileName = _shellSelectionService.GetActiveFileName();
			if (string.IsNullOrEmpty(fileName))
				return false;

			var position = _shellSelectionService.GetActiveFilePosition();
			if (position.IsEmpty())
				return false;

			_log.LogMessage($"Setting '{fileName}' bookmark # {number}", LOG_CATEGORY);
			List<Bookmark> bookmarks;
			if (_bookmarks.ContainsKey(fileName))
			{
				bookmarks = _bookmarks[fileName];
			}
			else
			{
				bookmarks = new List<Bookmark>(4);
				_bookmarks.Add(fileName, bookmarks);
			}

			var bookmark = bookmarks.FirstOrDefault(b => (b.Number == number) && (b.Type == BookmarkType.Local));
			if (bookmark != null)
			{
				var line = GetBookmarkLine(fileName, bookmark);

				if ((line == position.Line) && (bookmark.Column == position.Column))
				{
					bookmarks.Remove(bookmark);
					if (bookmarks.Count == 0)
						_bookmarks.Remove(fileName);

					_shellStatusBarService.SetStatusBarText($"Cleared bookmark {number}");
					_log.LogMessage($"Bookmark # {number} cleared", LOG_CATEGORY);
				}
				else
				{
					bookmark.Line = position.Line;
					bookmark.Column = position.Column;

					_shellStatusBarService.SetStatusBarText($"Updated bookmark {number}");
					_log.LogMessage($"Bookmark # {number} updated", LOG_CATEGORY);
				}
			}
			else
			{
				bookmarks.Add(new Bookmark(BookmarkType.Local, number, position.Line, position.Column));

				_shellStatusBarService.SetStatusBarText($"Set bookmark {number}");
				_log.LogMessage($"Bookmark # {number} set", LOG_CATEGORY);
			}

			DoChanged(fileName, number, BookmarkType.Local);
			return true;
		}

		/// <summary>
		/// Navigates to current file bookmark.
		/// </summary>
		/// <param name="number">Bookmark number.</param>
		/// <returns>Whether bookmark's been navigated to or not.</returns>
		public bool GoToBookmark(int number)
		{
			if ((number < 1) || (number > 10))
				throw new ArgumentOutOfRangeException(nameof(number));

			var fileName = _shellSelectionService.GetActiveFileName();
			if (string.IsNullOrEmpty(fileName))
				return false;

			if (!_bookmarks.ContainsKey(fileName))
			{
				_shellStatusBarService.SetStatusBarText($"Bookmark {number} not found");
				_log.LogMessage($"Bookmark # {number} is not found", LOG_CATEGORY);
				return false;
			}

			_log.LogMessage($"Looking for '{fileName}' bookmark # {number}", LOG_CATEGORY);
			var bookmarks = _bookmarks[fileName];
			var bookmark = bookmarks.FirstOrDefault(b => (b.Number == number) && (b.Type == BookmarkType.Local));
			if (bookmark != null)
			{
				if (_shellSelectionService.SetActiveFilePosition(GetBookmarkLine(fileName, bookmark), bookmark.Column))
				{
					_shellStatusBarService.SetStatusBarText($"Bookmark {number}");
					_log.LogMessage($"Go to bookmark # {number}", LOG_CATEGORY);
					return true;
				}
				else
				{
					_shellStatusBarService.SetStatusBarText($"Bookmark {number} line is invalid");
					_log.LogMessage($"Bookmark # {number} line is invalid", LOG_CATEGORY);
					return false;
				}
			}
			else
			{
				_shellStatusBarService.SetStatusBarText($"Bookmark {number} not found");
				_log.LogMessage($"Bookmark # {number} is not found", LOG_CATEGORY);
				return false;
			}
		}

		/// <summary>
		/// Sets global bookmark.
		/// </summary>
		/// <param name="number">Bookmark number.</param>
		/// <returns>Whether global bookmark's been set or not.</returns>
		public bool SetGlobalBookmark(int number)
		{
			if ((number < 1) || (number > 10))
				throw new ArgumentOutOfRangeException(nameof(number));

			var fileName = _shellSelectionService.GetActiveFileName();
			if (string.IsNullOrEmpty(fileName))
				return false;

			var position = _shellSelectionService.GetActiveFilePosition();
			if (position.IsEmpty())
				return false;

			// Find existing global bookmark and file it's in
			var bookmarkFileName = string.Empty;
			Bookmark bookmark = null;
			foreach (var file in _bookmarks.Keys)
			{
				var fileBookmarks = _bookmarks[file];
				bookmark = fileBookmarks.FirstOrDefault(b => (b.Number == number) && (b.Type == BookmarkType.Global));
				if (bookmark != null)
				{
					bookmarkFileName = file;
					break;
				}
			}

			_log.LogMessage($"Setting '{fileName}' global bookmark # {number}", LOG_CATEGORY);
			List<Bookmark> bookmarks;
			if (_bookmarks.ContainsKey(fileName))
			{
				bookmarks = _bookmarks[fileName];
			}
			else
			{
				bookmarks = new List<Bookmark>(4);
				_bookmarks.Add(fileName, bookmarks);
			}

			if (bookmark != null)
			{
				var line = GetBookmarkLine(fileName, bookmark);

				if ((line == position.Line) && fileName.Equals(bookmarkFileName, StringComparison.OrdinalIgnoreCase))
				{
					bookmarks.Remove(bookmark);

					_shellStatusBarService.SetStatusBarText($"Cleared global bookmark {number}");
					_log.LogMessage($"Global bookmark # {number} cleared", LOG_CATEGORY);
				}
				else
				{
					bookmark.Line = position.Line;
					bookmark.Column = position.Column;

					if (!fileName.Equals(bookmarkFileName, StringComparison.OrdinalIgnoreCase))
					{
						_bookmarks[bookmarkFileName].Remove(bookmark);
						bookmarks.Add(bookmark);
					}

					_shellStatusBarService.SetStatusBarText($"Updated global bookmark {number}");
					_log.LogMessage($"Global bookmark # {number} updated", LOG_CATEGORY);
				}
			}
			else
			{
				bookmarks.Add(new Bookmark(BookmarkType.Global, number, position.Line, position.Column));

				_shellStatusBarService.SetStatusBarText($"Set global bookmark {number}");
				_log.LogMessage($"Global bookmark # {number} set", LOG_CATEGORY);
			}

			DoChanged(fileName, number, BookmarkType.Global);
			if (!string.IsNullOrEmpty(bookmarkFileName) && !fileName.Equals(bookmarkFileName, StringComparison.OrdinalIgnoreCase))
				DoChanged(bookmarkFileName, number, BookmarkType.Global);
			return true;
		}

		/// <summary>
		/// Navigates to global bookmark.
		/// </summary>
		/// <param name="number">Bookmark number.</param>
		/// <returns>Whether global bookmark's been navigated to or not.</returns>
		public bool GoToGlobalBookmark(int number)
		{
			if ((number < 1) || (number > 10))
				throw new ArgumentOutOfRangeException(nameof(number));

			_log.LogMessage($"Looking for global bookmark # {number}", LOG_CATEGORY);
			foreach (var fileName in _bookmarks.Keys)
			{
				var bookmark = _bookmarks[fileName].FirstOrDefault(b => (b.Number == number) && (b.Type == BookmarkType.Global));
				if (bookmark != null)
				{
					if (_shellHelperService.OpenFileAt(fileName, GetBookmarkLine(fileName, bookmark), bookmark.Column))
					{
						_shellStatusBarService.SetStatusBarText($"Global bookmark {number}");
						_log.LogMessage($"Go to global bookmark # {number}", LOG_CATEGORY);
						return true;
					}
					else
					{
						_shellStatusBarService.SetStatusBarText($"Global bookmark {number} line is invalid");
						_log.LogMessage($"Global bookmark # {number} line is invalid", LOG_CATEGORY);
						return false;
					}
				}
			}

			_shellStatusBarService.SetStatusBarText($"Global bookmark {number} not found");
			_log.LogMessage($"Global bookmark # {number} is not found", LOG_CATEGORY);
			return false;
		}

		/// <summary>
		/// Clears all current file bookmarks.
		/// </summary>
		/// <returns>Whether all file bookmarks have been cleared or not.</returns>
		public bool ClearFileBookmarks()
		{
			var fileName = _shellSelectionService.GetActiveFileName();
			if (string.IsNullOrEmpty(fileName))
				return false;

			if (_bookmarks.ContainsKey(fileName))
				_bookmarks.Remove(fileName);
			_shellStatusBarService.SetStatusBarText("Cleared all file bookmarks");
			_log.LogMessage($"Cleared all '{fileName}' bookmarks", LOG_CATEGORY);

			DoChanged(fileName, 0, BookmarkType.Any);
			return true;
		}

		/// <summary>
		/// Clears all solution bookmarks.
		/// </summary>
		/// <returns>Whether all solution bookmarks have been cleared or not.</returns>
		public bool ClearAllBookmarks()
		{
			var fileNames = _bookmarks.Keys.ToList();
			_bookmarks.Clear();
			_shellStatusBarService.SetStatusBarText("Cleared all solution bookmarks");
			_log.LogMessage("Cleared all solution bookmarks", LOG_CATEGORY);

			fileNames.ForEach(f => DoChanged(f, 0, BookmarkType.Any));
			return true;
		}

		/// <summary>
		/// Updates bookmark in the document.
		/// </summary>
		/// <param name="fileName">File name.</param>
		/// <param name="number">Bookmark number.</param>
		/// <param name="type">Bookmark type.</param>
		public void UpdateBookmark(string fileName, int number, BookmarkType type)
		{
			if (string.IsNullOrEmpty(fileName))
				throw new ArgumentNullException(nameof(fileName));

			if ((number < 1) || (number > 10))
				throw new ArgumentOutOfRangeException(nameof(number));

			if (_bookmarks.ContainsKey(fileName) && _bookmarkCallbacks.ContainsKey(fileName))
			{
				var bookmark = _bookmarks[fileName].First(b => (b.Number == number) && (b.Type == type));
				var client = _bookmarkCallbacks[fileName];
				var line = client.GetBookmarkLine(bookmark);
				bookmark.Line = line;
			}
		}

		/// <summary>
		/// Removes bookmark from the document.
		/// </summary>
		/// <param name="fileName">File name.</param>
		/// <param name="number">Bookmark number.</param>
		/// <param name="type">Bookmark type.</param>
		public void RemoveBookmark(string fileName, int number, BookmarkType type)
		{
			if (string.IsNullOrEmpty(fileName))
				throw new ArgumentNullException(nameof(fileName));

			if ((number < 1) || (number > 10))
				throw new ArgumentOutOfRangeException(nameof(number));

			if (_bookmarks.ContainsKey(fileName))
			{
				var bookmarks = _bookmarks[fileName];
				var bookmark = bookmarks.FirstOrDefault(b => (b.Number == number) && (b.Type == type));
				if ((bookmark != null) && bookmarks.Contains(bookmark))
					bookmarks.Remove(bookmark);
			}
		}

		#endregion

		#region ISolutionEvents Members

		public void SolutionOpened(bool allProjectsLoaded)
		{
		}

		public void SolutionClosing()
		{
		}

		public void SolutionClosed()
		{
			_bookmarks.Clear();
			_bookmarkCallbacks.Clear();
			DoChanged(null, 0, BookmarkType.Any);
		}

		public void SolutionSaved()
		{
		}

		public void SolutionRenamed(string oldName, string newName)
		{
		}

		public void ProjectAdded(object project)
		{
		}

		public void ProjectDeleted(object project)
		{
		}

		public void ProjectRenamed(object project)
		{
		}

		public void ProjectUnloaded(object project)
		{
		}

		public void FileAdded(string[] fileNames, object project)
		{
		}

		public void FileDeleted(string[] fileNames, object project)
		{
			if ((fileNames != null) && (fileNames.Length > 0))
			{
				foreach (var fileName in fileNames)
				{
					if (_bookmarks.ContainsKey(fileName))
						_bookmarks.Remove(fileName);
				}

				_log.LogMessage($"Bookmark file deletion event", LOG_CATEGORY);
				fileNames.ToList().ForEach(f => DoChanged(f, 0, BookmarkType.Any));
			}
		}

		public void FileRenamed(string[] oldFileNames, string[] newFileNames, object project)
		{
			if ((oldFileNames != null) && (newFileNames != null) && (oldFileNames.Length == newFileNames.Length))
			{
				for (int index = 0; index < oldFileNames.Length; index++)
				{
					var oldFileName = oldFileNames[index];
					if (_bookmarks.ContainsKey(oldFileName))
					{
						var bookmarks = _bookmarks[oldFileName];
						_bookmarks.Remove(oldFileName);

						var newFileName = newFileNames[index];
						_bookmarks.Add(newFileName, bookmarks);
					}

					if (_bookmarkCallbacks.ContainsKey(oldFileName))
					{
						var client = _bookmarkCallbacks[oldFileName];
						_bookmarkCallbacks.Remove(oldFileName);

						var newFileName = newFileNames[index];
						_bookmarkCallbacks.Add(newFileName, client);
					}
				}
			}
		}

		public void FileChanged(string fileName, object projectItem)
		{
		}

		public void FileOpened(string fileName, object projectItem)
		{
		}

		public void FileClosed(string fileName, object project)
		{
		}

		public void FileSaved(string fileName, object projectItem)
		{
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Raises Changed event and subscribes to and unsubscribes from solution events as necessary.
		/// </summary>
		private void DoChanged(string fileName, int number, BookmarkType type)
		{
			Changed?.Invoke(this, new BookmarkEventArgs { FileName = fileName, Number = number, Type = type });

			// Dynamically subscribe to solution events - no need to keep responding to them if this feature is dormant
			if (this.BookmarkCount > 0)
			{
				if (!_solutionEvents)
				{
					_shellEventsService.SubscribeSolutionEvents(this);
					_solutionEvents = true;
				}
			}
			else
			{
				if (_solutionEvents)
				{
					_solutionEvents = false;
					_shellEventsService.UnsubscribeSolutionEvents(this);
				}
			}
		}

		/// <summary>
		/// Returns actual bookmark line # from the open document. Otherwise uses bookmark's data.
		/// </summary>
		private int GetBookmarkLine(string fileName, Bookmark bookmark)
		{
			if (_bookmarkCallbacks.ContainsKey(fileName))
				return _bookmarkCallbacks[fileName].GetBookmarkLine(bookmark);

			return bookmark.Line;
		}

		#endregion
	}
}