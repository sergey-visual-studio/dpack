using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using DPackRx.Extensions;
using DPackRx.Services;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace DPackRx.Features.Bookmarks
{
	/// <summary>
	/// Creates bookmark tags for the text buffer.
	/// </summary>
	public class BookmarksSimpleTagger : SimpleTagger<BookmarkTag>, IBookmarksSimpleTagger, IDisposable
	{
		#region Fields

		private ISharedServiceProvider _serviceProvider;
		private ITextBuffer _buffer;
		private ILog _log;
		private readonly Dictionary<Bookmark, TrackingTagSpan<BookmarkTag>> _bookmarkSpans = new Dictionary<Bookmark, TrackingTagSpan<BookmarkTag>>(10);

		private const string LOG_CATEGORY = "Bookmark Tagger";

		#endregion

		public BookmarksSimpleTagger(ISharedServiceProvider serviceProvider, ITextBuffer buffer) : base(buffer)
		{
			if (serviceProvider == null)
				throw new ArgumentNullException(nameof(serviceProvider));

			if (buffer == null)
				throw new ArgumentNullException(nameof(buffer));

			_serviceProvider = serviceProvider;
			_serviceProvider.Initialized += ServiceProvider_Initialized;

			_buffer = buffer;
			_buffer.Changed += Buffer_Changed;

			DoChanged(0, BookmarkType.Any);
		}

		#region IDisposable Members

		public void Dispose()
		{
			if (_buffer != null)
			{
				if ((_bookmarkSpans.Count > 0) && (this.BookmarksService != null))
				{
					var fileName = this.BookmarksService.GetCallbackFileName(this); // ITextDocument file name retrieval isn't possible at this stage
					if (!string.IsNullOrEmpty(fileName))
						_bookmarkSpans?.Keys.ForEach(b => this.BookmarksService?.UpdateBookmark(fileName, b.Number, b.Type)); // report updated line numbers back
					_bookmarkSpans.Clear();
				}

				if (this.BookmarksService != null)
				{
					this.BookmarksService.Changed -= BookmarksService_Changed;
					if (this.CallbackRegistered)
					{
						this.BookmarksService.UnregisterCallback(this);
						this.CallbackRegistered = false;
					}
					this.BookmarksService = null;
				}

				if (_serviceProvider != null)
				{
					_serviceProvider.Initialized -= ServiceProvider_Initialized;
					_serviceProvider = null;
				}

				if (_buffer != null)
				{
					_buffer.Changed -= Buffer_Changed;
					_buffer = null;
				}

				_log = null;
			}
		}

		#endregion

		#region IBookmarkCallbackClient Members

		/// <summary>
		/// Returns current bookmark line number.
		/// </summary>
		/// <param name="bookmark">Bookmark.</param>
		/// <returns>Line number or 0 if bookmark's not found.</returns>
		public int GetBookmarkLine(Bookmark bookmark)
		{
			if (_bookmarkSpans.ContainsKey(bookmark))
			{
				var span = _bookmarkSpans[bookmark].Span;
				var line = span.GetStartPoint(span.TextBuffer.CurrentSnapshot).GetContainingLine().LineNumber + 1; // position is 0-based
				return line;
			}
			else
			{
				_log?.LogMessage($"{bookmark.Type} bookmark {bookmark.Number} tag is invalid", LOG_CATEGORY);

				return 0; // upstream it should fail using this number
			}
		}

		#endregion

		#region IBookmarksSimpleTagger Members

		/// <summary>
		/// Bookmarks service instance.
		/// </summary>
		public IBookmarksService BookmarksService { get; private set; }

		/// <summary>
		/// Whether bookmark service callback's been registered.
		/// </summary>
		public bool CallbackRegistered { get; private set; }

		/// <summary>
		/// Number of file bookmarks.
		/// </summary>
		public int BookmarkCount
		{
			get { return _bookmarkSpans.Count; }
		}

		#endregion

		#region Private Methods

		private void ServiceProvider_Initialized(object sender, EventArgs e)
		{
			// Work around bookmarks service not being under MEF when package loads *after* file with bookmarks is opened
			InitializeServices(true);
		}

		private void BookmarksService_Changed(object sender, BookmarkEventArgs e)
		{
			// Check if bookmark change happened for our file
			if (!string.IsNullOrEmpty(e.FileName))
			{
				var fileName = GetFileName();
				if (!string.IsNullOrEmpty(fileName) && fileName.Equals(e.FileName, StringComparison.OrdinalIgnoreCase))
					DoChanged(e.Number, e.Type);
			}
		}

		/// <summary>
		/// Initializes bookmarks service.
		/// </summary>
		private bool InitializeServices(bool throwOnError)
		{
			if (this.BookmarksService == null)
			{
				this.BookmarksService = _serviceProvider.GetService<IBookmarksService>(throwOnError);
				if (this.BookmarksService != null)
					this.BookmarksService.Changed += BookmarksService_Changed;
			}

			if (!this.CallbackRegistered && (_buffer != null) && (this.BookmarksService != null))
			{
				var fileName = GetFileName();
				if (!string.IsNullOrEmpty(fileName))
				{
					this.BookmarksService.RegisterCallback(this, fileName);
					this.CallbackRegistered = true;
				}
			}

			if (_log == null)
				_log = _serviceProvider.GetService<ILog>(throwOnError);

			return this.BookmarksService != null;
		}

		/// <summary>
		/// Returns buffer's file name or null if required service is not available.
		/// </summary>
		private string GetFileName()
		{
			return _buffer?.GetService<ITextDocument>()?.FilePath;
		}

		/// <summary>
		/// Reconciles bookmark service bookmarks on hand with bookmark spans.
		/// </summary>
		private void DoChanged(int number, BookmarkType type)
		{
			if (!InitializeServices(false))
				return;

			var fileName = GetFileName();
			if (string.IsNullOrEmpty(fileName))
				return;

			_log?.LogMessage($"File '{Path.GetFileName(fileName)}' bookmark {number} change ({type})", LOG_CATEGORY);

			try
			{
				var bookmarks = this.BookmarksService?.GetFileBookmarks(fileName);
				if ((bookmarks.Count() == 0) && (_bookmarkSpans.Count == 0))
					return;

				using (Update())
				{
					// Remove changed bookmark so that it gets re-added further down
					if (number > 0)
					{
						var changedBookmark = _bookmarkSpans.Keys.FirstOrDefault(b => (b.Number == number) && type.HasFlag(b.Type));
						if (changedBookmark != null)
						{
							_log?.LogMessage($"Removed changed {changedBookmark.Type} bookmark {changedBookmark.Number} tag", LOG_CATEGORY);

							if (!RemoveTagSpan(_bookmarkSpans[changedBookmark]))
								_log?.LogMessage($"Failed to remove changed {changedBookmark.Type} bookmark {changedBookmark.Number} tag span", LOG_CATEGORY);
							_bookmarkSpans.Remove(changedBookmark);
						}
					}

					var newBookmarks = bookmarks.Where(b => !_bookmarkSpans.ContainsKey(b)).ToList();
					newBookmarks.ForEach(b =>
					{
						if ((b.Line >= 1) && (b.Line <= _buffer.CurrentSnapshot.LineCount))
						{
							var line = _buffer.CurrentSnapshot.GetLineFromLineNumber(b.Line - 1);
							var span = _buffer.CurrentSnapshot.CreateTrackingSpan(new SnapshotSpan(line.Start, 1), SpanTrackingMode.EdgeExclusive); // can't be span of 0 length
							var trackingSpan = CreateTagSpan(span, new BookmarkTag(b.Number, b.Type));
							_bookmarkSpans.Add(b, trackingSpan);

							_log?.LogMessage($"Added {b.Type} bookmark {b.Number} tag on line {b.Line}", LOG_CATEGORY);
						}
						else
						{
							_log?.LogMessage($"{b.Type} bookmark {b.Number} invalid tag line {b.Line}", LOG_CATEGORY);
						}
					});

					var deleteBookmarks = _bookmarkSpans.Keys.Where(b => !bookmarks.Contains(b)).ToList();
					deleteBookmarks.ForEach(b =>
					{
						_log?.LogMessage($"Removed deleted {b.Type} bookmark {b.Number} tag", LOG_CATEGORY);

						if (!RemoveTagSpan(_bookmarkSpans[b]))
							_log?.LogMessage($"Failed to remove deleted {b.Type} bookmark {b.Number} tag span", LOG_CATEGORY);
						_bookmarkSpans.Remove(b);
					});
				}
			}
			catch (Exception ex)
			{
				if (_log != null)
					_log.LogMessage(ex, LOG_CATEGORY);
				else
					Trace.TraceError(ex.ToString());
			}
		}

		/// <summary>
		/// Delete qualified bookmarks on text change.
		/// </summary>
		/// <remarks>For performance reasons consider line type changes only.</remarks>
		private void Buffer_Changed(object sender, TextContentChangedEventArgs e)
		{
			if (!e.Changes.IncludesLineChanges || (_bookmarkSpans.Count == 0))
				return;

			var lineChanges = e.Changes.Where(c => c.LineCountDelta < 0);
			if (lineChanges.Count() == 0)
				return;

			List<Bookmark> bookmarksToDelete = null;
			foreach (var span in _bookmarkSpans.Values)
			{
				if (span.Span.GetSpan(_buffer.CurrentSnapshot).Length == 0)
				{
					if (bookmarksToDelete == null)
						bookmarksToDelete = new List<Bookmark>();

					var bookmark = _bookmarkSpans.Keys.First(b => _bookmarkSpans[b] == span);
					bookmarksToDelete.Add(bookmark);
				}
			}

			if ((bookmarksToDelete != null) && (bookmarksToDelete.Count > 0))
			{
				using (Update())
				{
					var fileName = GetFileName();

					bookmarksToDelete.ForEach(b =>
					{
						_log?.LogMessage($"Removed deleted text line {b.Type} bookmark {b.Number} tag", LOG_CATEGORY);

						RemoveTagSpan(_bookmarkSpans[b]);
						_bookmarkSpans.Remove(b);
						this.BookmarksService.RemoveBookmark(fileName, b.Number, b.Type);
					});
				}
			}
		}

		#endregion
	}
}