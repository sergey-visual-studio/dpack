using System;
using System.Collections;
using System.Collections.Generic;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

using Moq;
using NUnit.Framework;

using DPackRx.Features.Bookmarks;
using DPackRx.Services;

namespace DPackRx.Tests.Features
{
	/// <summary>
	/// BookmarksSimpleTagger tests.
	/// </summary>
	[TestFixture]
	public class BookmarksSimpleTaggerTests
	{
		#region Fields

		private Bookmark _bookmark;
		private Mock<IBookmarksService> _bookmarksServiceMock;
		private Mock<ISharedServiceProvider> _sharedServiceProviderMock;
		private Mock<ITextDocument> _textDocumentMock;
		private Mock<ITextBuffer> _textBufferMock;

		#endregion

		#region TestNormalizedTextChangeCollection class

		private class TestNormalizedTextChangeCollection : INormalizedTextChangeCollection
		{
			private readonly List<ITextChange> _changes = new List<ITextChange>();

			public ITextChange this[int index]
			{
				get { return _changes[index]; }
				set { _changes[index] = value; }
			}

			public bool IncludesLineChanges { get { return _changes.Count > 0; } }

			public int Count { get { return _changes.Count; } }

			public bool IsReadOnly => false;

			public void Add(ITextChange item)
			{
				_changes.Add(item);
			}

			public void Clear()
			{
				_changes.Clear();
			}

			public bool Contains(ITextChange item)
			{
				return _changes.Contains(item);
			}

			public void CopyTo(ITextChange[] array, int arrayIndex)
			{
			}

			public IEnumerator<ITextChange> GetEnumerator()
			{
				return _changes.GetEnumerator();
			}

			public int IndexOf(ITextChange item)
			{
				return _changes.IndexOf(item);
			}

			public void Insert(int index, ITextChange item)
			{
			}

			public bool Remove(ITextChange item)
			{
				return _changes.Remove(item);
			}

			public void RemoveAt(int index)
			{
				_changes.RemoveAt(index);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return _changes.GetEnumerator();
			}
		}

		#endregion

		#region Tests Setup

		[SetUp]
		public void Setup()
		{
			_bookmark = new Bookmark(BookmarkType.Local, number: 1, line: 12, column: 34); // line # must be less than # of lines in the snapshot

			_bookmarksServiceMock = new Mock<IBookmarksService>();
			_bookmarksServiceMock.Setup(b => b.RegisterCallback(It.IsNotNull<IBookmarkCallbackClient>(), It.IsNotNull<string>())).Verifiable();
			_bookmarksServiceMock.Setup(b => b.UnregisterCallback(It.IsNotNull<IBookmarkCallbackClient>())).Verifiable();
			_bookmarksServiceMock.Setup(b => b.GetCallbackFileName(It.IsAny<IBookmarkCallbackClient>())).Returns("test").Verifiable();
			_bookmarksServiceMock.Setup(b => b.UpdateBookmark("test", It.IsAny<int>(), It.IsAny<BookmarkType>())).Verifiable();
			_bookmarksServiceMock.Setup(b => b.RemoveBookmark("test", It.IsAny<int>(), It.IsAny<BookmarkType>())).Verifiable();

			_sharedServiceProviderMock = new Mock<ISharedServiceProvider>();
			_sharedServiceProviderMock.Setup(s => s.GetService(typeof(IBookmarksService))).Returns(_bookmarksServiceMock.Object).Verifiable();
			_sharedServiceProviderMock.Setup(s => s.GetService(typeof(ILog))).Returns(new Mock<ILog>().Object).Verifiable();

			_textDocumentMock = new Mock<ITextDocument>();
			_textDocumentMock.SetupGet(d => d.FilePath).Returns("test").Verifiable();

			var properties = new PropertyCollection();
			properties.AddProperty(typeof(ITextDocument), _textDocumentMock.Object);

			var snapshotMock = new Mock<ITextSnapshot>();
			var lineSnapshotMock = new Mock<ITextSnapshotLine>();
			var trackingSpanMock = new Mock<ITrackingSpan>();
			snapshotMock.SetupGet(s => s.Length).Returns(100);
			snapshotMock.SetupGet(s => s.LineCount).Returns(20);
			lineSnapshotMock.SetupGet(l => l.LineNumber).Returns(_bookmark.Line - 1); // line is stored 0-based
			lineSnapshotMock.SetupGet(l => l.Start).Returns(new SnapshotPoint(snapshotMock.Object, 56));
			snapshotMock.Setup(s => s.GetLineFromLineNumber(It.IsAny<int>())).Returns(lineSnapshotMock.Object);
			snapshotMock.Setup(s => s.GetLineFromPosition(It.IsAny<int>())).Returns(lineSnapshotMock.Object);
			snapshotMock.Setup(s => s.CreateTrackingSpan(It.IsAny<Span>(), It.IsAny<SpanTrackingMode>())).Returns(trackingSpanMock.Object);

			_textBufferMock = new Mock<ITextBuffer>();
			_textBufferMock.SetupGet(b => b.Properties).Returns(properties).Verifiable();
			_textBufferMock.SetupGet(b => b.CurrentSnapshot).Returns(snapshotMock.Object).Verifiable();

			var point = new SnapshotPoint(_textBufferMock.Object.CurrentSnapshot, 56);
			trackingSpanMock.SetupGet(s => s.TextBuffer).Returns(_textBufferMock.Object).Verifiable();
			trackingSpanMock.Setup(s => s.GetStartPoint(It.IsAny<ITextSnapshot>())).Returns(point).Verifiable();
		}

		[TearDown]
		public void TearDown()
		{
			_bookmark = null;
			_bookmarksServiceMock = null;
			_sharedServiceProviderMock = null;
			_textDocumentMock = null;
			_textBufferMock = null;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Returns test instance.
		/// </summary>
		private IBookmarksSimpleTagger GetTagger()
		{
			return new BookmarksSimpleTagger(_sharedServiceProviderMock.Object, _textBufferMock.Object);
		}

		/// <summary>
		/// Returns test instance.
		/// </summary>
		private IBookmarksSimpleTagger GetTagger(ISharedServiceProvider serviceProvider, ITextBuffer textBuffer)
		{
			return new BookmarksSimpleTagger(serviceProvider, textBuffer);
		}

		#endregion

		#region Tests

		[Test]
		public void BookmarksSimpleTagger()
		{
			var tagger = GetTagger();

			Assert.That(tagger.BookmarksService, Is.Not.Null);
			Assert.That(tagger.CallbackRegistered, Is.True);
			Assert.That(tagger.BookmarkCount, Is.EqualTo(0));
			_sharedServiceProviderMock.Verify(s => s.GetService(typeof(IBookmarksService)));
			_sharedServiceProviderMock.Verify(s => s.GetService(typeof(ILog)));
			_textDocumentMock.VerifyGet(t => t.FilePath);
			_bookmarksServiceMock.Verify(b => b.RegisterCallback(It.IsNotNull<IBookmarkCallbackClient>(), It.IsNotNull<string>()));
		}

		[Test]
		public void BookmarksSimpleTagger_ErrorHandling()
		{
			Assert.Throws<ArgumentNullException>(() => GetTagger(null, _textBufferMock.Object));
			Assert.Throws<ArgumentNullException>(() => GetTagger(_sharedServiceProviderMock.Object, null));
		}

		[Test]
		public void DoChanges_Dispose()
		{
			_bookmarksServiceMock.Setup(b => b.GetFileBookmarks("test")).Returns(new List<Bookmark> { _bookmark }).Verifiable();
			var tagger = GetTagger();

			Assert.That(tagger.BookmarkCount, Is.EqualTo(1));

			((IDisposable)tagger).Dispose();

			Assert.That(tagger.BookmarksService, Is.Null);
			Assert.That(tagger.CallbackRegistered, Is.False);
			Assert.That(tagger.BookmarkCount, Is.EqualTo(0));
			_bookmarksServiceMock.Verify(b => b.UnregisterCallback(tagger));
			_bookmarksServiceMock.Verify(b => b.GetCallbackFileName(tagger));
			_bookmarksServiceMock.Verify(b => b.UpdateBookmark("test", 1, BookmarkType.Local));
		}

		[Test]
		public void ServiceProvider_Initialized()
		{
			var tagger = GetTagger();

			Assert.DoesNotThrow(() => _sharedServiceProviderMock.Raise(s => s.Initialized += null, EventArgs.Empty));
			Assert.That(tagger.BookmarksService, Is.Not.Null);
			Assert.That(tagger.CallbackRegistered, Is.True);
			_sharedServiceProviderMock.Verify(s => s.GetService(typeof(IBookmarksService)), Times.Once);
		}

		[Test]
		public void ServiceProvider_Initialized_NoService()
		{
			_sharedServiceProviderMock.Setup(s => s.GetService(typeof(IBookmarksService))).Returns(null).Verifiable();
			var tagger = GetTagger();

			Assert.Throws<ApplicationException>(() => _sharedServiceProviderMock.Raise(s => s.Initialized += null, EventArgs.Empty));
		}

		[Test]
		public void GetBookmarkLine()
		{
			_bookmarksServiceMock.Setup(b => b.GetFileBookmarks("test")).Returns(new List<Bookmark> { _bookmark }).Verifiable();
			var tagger = GetTagger();

			var line = tagger.GetBookmarkLine(_bookmark);

			Assert.That(line, Is.EqualTo(_bookmark.Line));
		}

		[Test]
		public void GetBookmarkLine_InvalidBookmarks()
		{
			_bookmarksServiceMock.Setup(b => b.GetFileBookmarks("test")).Returns(new List<Bookmark> { _bookmark }).Verifiable();
			var tagger = GetTagger();

			var line = tagger.GetBookmarkLine(new Bookmark(BookmarkType.Local, 2, 56, 78));

			Assert.That(line, Is.EqualTo(0));
		}

		[Test]
		public void Buffer_Changed_Deleted()
		{
			_bookmarksServiceMock.Setup(b => b.GetFileBookmarks("test")).Returns(new List<Bookmark> { _bookmark }).Verifiable();
			var tagger = GetTagger();
			var bookmarkCount = tagger.BookmarkCount;

			// Text buffer change setup
			var beforeSnapshotMock = new Mock<ITextSnapshot>();
			var afterSnapshotMock = new Mock<ITextSnapshot>();
			var versionMock = new Mock<ITextVersion>();
			var lineChangeMock = new Mock<ITextChange>();
			var changes = new TestNormalizedTextChangeCollection { lineChangeMock.Object };
			beforeSnapshotMock.SetupGet(s => s.Version).Returns(versionMock.Object);
			versionMock.SetupGet(v => v.Changes).Returns(changes);
			lineChangeMock.SetupGet(l => l.LineCountDelta).Returns(-1);
			_textBufferMock.Raise(b => b.Changed += null, new TextContentChangedEventArgs(beforeSnapshotMock.Object, afterSnapshotMock.Object, EditOptions.None, null));

			Assert.That(bookmarkCount, Is.EqualTo(1));
			Assert.That(tagger.BookmarkCount, Is.EqualTo(0));
			_bookmarksServiceMock.Verify(b => b.RemoveBookmark("test", _bookmark.Number, _bookmark.Type));
		}

		#endregion
	}
}