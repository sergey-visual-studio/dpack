using System;
using System.Linq;

using Moq;
using NUnit.Framework;

using DPackRx.Features.Bookmarks;
using DPackRx.Services;

namespace DPackRx.Tests.Features
{
	/// <summary>
	/// BookmarksService tests.
	/// </summary>
	[TestFixture]
	public class BookmarksServiceTests
	{
		#region Fields

		private Mock<ILog> _logMock;
		private Mock<IShellEventsService> _shellEventsServiceMock;
		private Mock<IShellStatusBarService> _shellStatusBarServiceMock;
		private Mock<IShellSelectionService> _shellSelectionServiceMock;
		private Mock<IShellHelperService> _shellHelperServiceMock;
		private Mock<IBookmarkCallbackClient> _bookmarkCallbackClientMock;

		#endregion

		#region Tests Setup

		[SetUp]
		public void Setup()
		{
			_logMock = new Mock<ILog>();
			_logMock.Setup(l => l.LogMessage(It.IsAny<string>(), It.IsAny<string>())).Verifiable();

			_shellEventsServiceMock = new Mock<IShellEventsService>();

			_shellStatusBarServiceMock = new Mock<IShellStatusBarService>();
			_shellStatusBarServiceMock.Setup(s => s.SetStatusBarText(It.IsNotNull<string>())).Verifiable();

			_shellSelectionServiceMock = new Mock<IShellSelectionService>();
			_shellSelectionServiceMock.Setup(s => s.GetActiveFileName()).Returns("test").Verifiable();
			_shellSelectionServiceMock.Setup(s => s.GetActiveFilePosition()).Returns(new Position(12, 34)).Verifiable();
			_shellSelectionServiceMock.Setup(s => s.SetActiveFilePosition(It.IsAny<int>(), It.IsAny<int>())).Returns(true).Verifiable();

			_shellHelperServiceMock = new Mock<IShellHelperService>();
			_shellHelperServiceMock.Setup(s => s.OpenFileAt(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())).Returns(true).Verifiable();

			_bookmarkCallbackClientMock = new Mock<IBookmarkCallbackClient>();
			_bookmarkCallbackClientMock.Setup(c => c.GetBookmarkLine(It.IsAny<Bookmark>())).Returns(123).Verifiable();
		}

		[TearDown]
		public void TearDown()
		{
			_logMock = null;
			_shellEventsServiceMock = null;
			_shellStatusBarServiceMock = null;
			_shellSelectionServiceMock = null;
			_shellHelperServiceMock = null;
			_bookmarkCallbackClientMock = null;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Returns test instance.
		/// </summary>
		private IBookmarksService GetService()
		{
			return new BookmarksService(_logMock.Object, _shellEventsServiceMock.Object, _shellStatusBarServiceMock.Object,
				_shellSelectionServiceMock.Object, _shellHelperServiceMock.Object);
		}

		private ISolutionEvents GetSolutionEvents(IBookmarksService service)
		{
			return (ISolutionEvents)service;
		}

		#endregion

		#region Tests

		[Test]
		public void BookmarkCount()
		{
			var service = GetService();

			service.SetBookmark(1);

			Assert.That(service.BookmarkCount, Is.EqualTo(1));
		}

		[Test]
		public void GlobalBookmarkCount()
		{
			var service = GetService();

			service.SetGlobalBookmark(1);

			Assert.That(service.BookmarkCount, Is.EqualTo(1));
		}

		[Test]
		public void RegisterCallback()
		{
			var service = GetService();
			service.SetBookmark(1);

			service.RegisterCallback(_bookmarkCallbackClientMock.Object, "test");

			Assert.That(service.BookmarkCallbackCount, Is.EqualTo(1));
		}

		[Test]
		public void RegisterCallback_MoreThanOnce()
		{
			var service = GetService();
			service.SetBookmark(1);

			service.RegisterCallback(_bookmarkCallbackClientMock.Object, "test");
			service.RegisterCallback(_bookmarkCallbackClientMock.Object, "test");

			Assert.That(service.BookmarkCallbackCount, Is.EqualTo(1));
		}

		[Test]
		public void RegisterCallback_ErrorHandling()
		{
			var service = GetService();

			Assert.Throws<ArgumentNullException>(() => service.RegisterCallback(null, "test"));
			Assert.Throws<ArgumentNullException>(() => service.RegisterCallback(_bookmarkCallbackClientMock.Object, null));
			Assert.Throws<ArgumentNullException>(() => service.RegisterCallback(_bookmarkCallbackClientMock.Object, ""));
		}

		[Test]
		public void UnegisterCallback()
		{
			var service = GetService();
			service.SetBookmark(1);
			service.RegisterCallback(_bookmarkCallbackClientMock.Object, "test");

			service.UnregisterCallback(_bookmarkCallbackClientMock.Object);

			Assert.That(service.BookmarkCallbackCount, Is.EqualTo(0));
		}

		[Test]
		public void UnegisterCallback_MoreThanOnce()
		{
			var service = GetService();
			service.SetBookmark(1);
			service.RegisterCallback(_bookmarkCallbackClientMock.Object, "test");

			service.UnregisterCallback(_bookmarkCallbackClientMock.Object);
			service.UnregisterCallback(_bookmarkCallbackClientMock.Object);

			Assert.That(service.BookmarkCallbackCount, Is.EqualTo(0));
		}

		[Test]
		public void UnregisterCallback_ErrorHandling()
		{
			var service = GetService();

			Assert.Throws<ArgumentNullException>(() => service.UnregisterCallback(null));
		}

		[Test]
		public void GetCallbackFileName()
		{
			var service = GetService();
			service.SetBookmark(1);
			service.RegisterCallback(_bookmarkCallbackClientMock.Object, "test");

			var fileName = service.GetCallbackFileName(_bookmarkCallbackClientMock.Object);

			Assert.That(fileName, Is.EqualTo("test"));
		}

		[Test]
		public void GetCallbackFileName_InvalidCallbackClient()
		{
			var service = GetService();
			service.SetBookmark(1);
			service.RegisterCallback(_bookmarkCallbackClientMock.Object, "test");

			var invalidCallbackClientMock = new Mock<IBookmarkCallbackClient>();

			var fileName = service.GetCallbackFileName(invalidCallbackClientMock.Object);

			Assert.That(fileName, Is.Null);
		}

		[Test]
		public void GetCallbackFileName_ErrorHandling()
		{
			var service = GetService();

			Assert.Throws<ArgumentNullException>(() => service.GetCallbackFileName(null));
		}

		[Test]
		public void GetFileBookmarks()
		{
			var service = GetService();

			_shellSelectionServiceMock.Setup(s => s.GetActiveFilePosition()).Returns(new Position(1, 2));
			service.SetBookmark(1);

			_shellSelectionServiceMock.Setup(s => s.GetActiveFilePosition()).Returns(new Position(3, 4));
			service.SetBookmark(2);

			_shellSelectionServiceMock.Setup(s => s.GetActiveFilePosition()).Returns(new Position(5, 6));
			service.SetGlobalBookmark(1);

			_shellSelectionServiceMock.Setup(s => s.GetActiveFilePosition()).Returns(new Position(7, 8));
			service.SetGlobalBookmark(2);

			var bookmarks = service.GetFileBookmarks("test");

			Assert.That(bookmarks, Is.Not.Null);
			Assert.That(bookmarks.Count(), Is.EqualTo(4));
			Assert.That(bookmarks.Count(b => b.Type == BookmarkType.Local), Is.EqualTo(2));
			Assert.That(bookmarks.Count(b => b.Type == BookmarkType.Global), Is.EqualTo(2));
		}

		[TestCase(null)]
		[TestCase("")]
		public void GetFileBookmarks_ErrorHandling(string fileName)
		{
			var service = GetService();

			var result = service.GetFileBookmarks(fileName);

			Assert.That(result, Is.Null);
		}

		[Test]
		public void SetBookmark()
		{
			var service = GetService();
			var changed = false;
			var type = BookmarkType.Any;
			service.Changed += (sender, e) => { changed = true; type = e.Type; };

			var result = service.SetBookmark(1);

			Assert.That(result, Is.True);
			Assert.That(changed, Is.True);
			Assert.That(type, Is.EqualTo(BookmarkType.Local));
			_shellSelectionServiceMock.Verify(s => s.GetActiveFileName());
			_shellSelectionServiceMock.Verify(s => s.GetActiveFilePosition());
			_shellStatusBarServiceMock.Verify(s => s.SetStatusBarText(It.IsNotNull<string>()));
		}

		[Test]
		public void SetBookmark_ErrorHandling()
		{
			var service = GetService();

			Assert.Throws<ArgumentOutOfRangeException>(() => service.SetBookmark(0));
			Assert.Throws<ArgumentOutOfRangeException>(() => service.SetBookmark(11));
		}

		[Test]
		public void SetBookmark_NoFile()
		{
			var service = GetService();

			_shellSelectionServiceMock.Setup(s => s.GetActiveFileName()).Returns(string.Empty).Verifiable();

			var result = service.SetBookmark(1);

			Assert.That(result, Is.False);
			_shellSelectionServiceMock.Verify(s => s.GetActiveFileName());
			_shellSelectionServiceMock.Verify(s => s.GetActiveFilePosition(), Times.Never);
			_shellStatusBarServiceMock.Verify(s => s.SetStatusBarText(It.IsNotNull<string>()), Times.Never);
		}

		[Test]
		public void SetBookmark_NoPoint()
		{
			var service = GetService();

			_shellSelectionServiceMock.Setup(s => s.GetActiveFilePosition()).Returns(new Position(0, 0)).Verifiable();

			var result = service.SetBookmark(1);

			Assert.That(result, Is.False);
			_shellSelectionServiceMock.Verify(s => s.GetActiveFileName());
			_shellSelectionServiceMock.Verify(s => s.GetActiveFilePosition());
			_shellStatusBarServiceMock.Verify(s => s.SetStatusBarText(It.IsNotNull<string>()), Times.Never);
		}

		[Test]
		public void SetBookmark_Update()
		{
			var service = GetService();

			_shellSelectionServiceMock.Setup(s => s.GetActiveFilePosition()).Returns(new Position(1, 2));
			service.SetBookmark(1);
			var bookmark = service.GetFileBookmarks("test").FirstOrDefault();

			Assert.That(bookmark.Number, Is.EqualTo(1));
			Assert.That(bookmark.Line, Is.EqualTo(1));
			Assert.That(bookmark.Column, Is.EqualTo(2));

			_shellSelectionServiceMock.Setup(s => s.GetActiveFilePosition()).Returns(new Position(3, 4));
			service.SetBookmark(1);
			bookmark = service.GetFileBookmarks("test").FirstOrDefault();

			Assert.That(service.BookmarkCount, Is.EqualTo(1));
			Assert.That(bookmark.Number, Is.EqualTo(1));
			Assert.That(bookmark.Line, Is.EqualTo(3));
			Assert.That(bookmark.Column, Is.EqualTo(4));
		}

		[Test]
		public void SetBookmark_SameLine()
		{
			var service = GetService();

			var result = service.SetBookmark(5);
			var repeatResult = service.SetBookmark(1);

			var bookmarks = service.GetFileBookmarks("test");

			Assert.That(result, Is.True);
			Assert.That(repeatResult, Is.True);
			Assert.That(bookmarks, Is.Not.Null);
			Assert.That(bookmarks.Count(), Is.EqualTo(2));
		}

		[Test]
		public void GoToBookmark()
		{
			var service = GetService();

			service.SetBookmark(1);
			var result = service.GoToBookmark(1);

			Assert.That(result, Is.True);
			_shellSelectionServiceMock.Verify(s => s.GetActiveFileName());
			_shellSelectionServiceMock.Verify(s => s.GetActiveFilePosition());
			_shellSelectionServiceMock.Verify(s => s.SetActiveFilePosition(It.IsAny<int>(), It.IsAny<int>()));
			_shellStatusBarServiceMock.Verify(s => s.SetStatusBarText(It.IsNotNull<string>()));
		}

		[Test]
		public void GetBookmark_CallbackClient()
		{
			var service = GetService();

			service.RegisterCallback(_bookmarkCallbackClientMock.Object, "test");
			service.SetBookmark(1);

			var result = service.GoToBookmark(1);

			Assert.That(result, Is.True);
			Assert.That(service.BookmarkCallbackCount, Is.EqualTo(1));
			_bookmarkCallbackClientMock.Verify(c => c.GetBookmarkLine(It.IsAny<Bookmark>()));
			_shellSelectionServiceMock.Verify(s => s.SetActiveFilePosition(123, It.IsAny<int>()));
		}

		[Test]
		public void GoToBookmark_ErrorHandling()
		{
			var service = GetService();

			Assert.Throws<ArgumentOutOfRangeException>(() => service.GoToBookmark(0));
			Assert.Throws<ArgumentOutOfRangeException>(() => service.GoToBookmark(11));
		}

		[TestCase(null)]
		[TestCase("")]
		public void GoToBookmark_NoFile(string fileName)
		{
			var service = GetService();

			service.SetBookmark(1);

			_shellStatusBarServiceMock.Reset();
			_shellSelectionServiceMock.Reset();
			_shellSelectionServiceMock.Setup(s => s.GetActiveFileName()).Returns(fileName).Verifiable();

			var result = service.GoToBookmark(1);

			Assert.That(result, Is.False);
			_shellSelectionServiceMock.Verify(s => s.GetActiveFileName());
			_shellSelectionServiceMock.Verify(s => s.GetActiveFilePosition(), Times.Never);
			_shellSelectionServiceMock.Verify(s => s.SetActiveFilePosition(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
			_shellStatusBarServiceMock.Verify(s => s.SetStatusBarText(It.IsNotNull<string>()), Times.Never);
		}

		[Test]
		public void GoToBookmark_NoBookmark()
		{
			var service = GetService();

			service.SetBookmark(1);
			var result = service.GoToBookmark(2);

			Assert.That(result, Is.False);
			_shellSelectionServiceMock.Verify(s => s.GetActiveFileName());
			_shellSelectionServiceMock.Verify(s => s.GetActiveFilePosition());
			_shellSelectionServiceMock.Verify(s => s.SetActiveFilePosition(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
			_shellStatusBarServiceMock.Verify(s => s.SetStatusBarText(It.IsNotNull<string>()));
		}

		[Test]
		public void GoToBookmark_InvalidLine()
		{
			var service = GetService();
			_shellSelectionServiceMock.Setup(s => s.SetActiveFilePosition(It.IsAny<int>(), It.IsAny<int>())).Returns(false).Verifiable();

			service.SetBookmark(1);
			var result = service.GoToBookmark(1);

			Assert.That(result, Is.False);
			_shellSelectionServiceMock.Verify(s => s.GetActiveFileName());
			_shellSelectionServiceMock.Verify(s => s.GetActiveFilePosition());
			_shellSelectionServiceMock.Verify(s => s.SetActiveFilePosition(It.IsAny<int>(), It.IsAny<int>()));
			_shellStatusBarServiceMock.Verify(s => s.SetStatusBarText(It.IsNotNull<string>()));
		}

		[Test]
		public void GoToBookmark_NoFileBookmarks()
		{
			var service = GetService();

			var result = service.GoToBookmark(1);

			Assert.That(result, Is.False);
			_shellSelectionServiceMock.Verify(s => s.GetActiveFileName());
			_shellStatusBarServiceMock.Verify(s => s.SetStatusBarText(It.IsNotNull<string>()));
		}

		[Test]
		public void SetGlobalBookmark()
		{
			var service = GetService();
			var changed = false;
			var type = BookmarkType.Any;
			service.Changed += (sender, e) => { changed = true; type = e.Type; };

			var result = service.SetGlobalBookmark(1);

			Assert.That(result, Is.True);
			Assert.That(changed, Is.True);
			Assert.That(type, Is.EqualTo(BookmarkType.Global));
			_shellSelectionServiceMock.Verify(s => s.GetActiveFileName());
			_shellSelectionServiceMock.Verify(s => s.GetActiveFilePosition());
			_shellStatusBarServiceMock.Verify(s => s.SetStatusBarText(It.IsNotNull<string>()));
		}

		[Test]
		public void SetGlobalBookmark_ErrorHandling()
		{
			var service = GetService();

			Assert.Throws<ArgumentOutOfRangeException>(() => service.SetGlobalBookmark(0));
			Assert.Throws<ArgumentOutOfRangeException>(() => service.SetGlobalBookmark(11));
		}

		[Test]
		public void SetGlobalBookmark_NoFile()
		{
			var service = GetService();

			_shellSelectionServiceMock.Setup(s => s.GetActiveFileName()).Returns(string.Empty).Verifiable();

			var result = service.SetGlobalBookmark(1);

			Assert.That(result, Is.False);
			_shellSelectionServiceMock.Verify(s => s.GetActiveFileName());
			_shellSelectionServiceMock.Verify(s => s.GetActiveFilePosition(), Times.Never);
			_shellStatusBarServiceMock.Verify(s => s.SetStatusBarText(It.IsNotNull<string>()), Times.Never);
		}

		[Test]
		public void SetGlobalBookmark_NoPoint()
		{
			var service = GetService();

			_shellSelectionServiceMock.Setup(s => s.GetActiveFilePosition()).Returns(new Position(0, 0)).Verifiable();

			var result = service.SetGlobalBookmark(1);

			Assert.That(result, Is.False);
			_shellSelectionServiceMock.Verify(s => s.GetActiveFileName());
			_shellSelectionServiceMock.Verify(s => s.GetActiveFilePosition());
			_shellStatusBarServiceMock.Verify(s => s.SetStatusBarText(It.IsNotNull<string>()), Times.Never);
		}

		[Test]
		public void SetGlobalBookmark_FileUpdate()
		{
			var service = GetService();
			var changed = 0;
			service.Changed += (sender, e) => { changed++; };

			_shellSelectionServiceMock.Setup(s => s.GetActiveFilePosition()).Returns(new Position(1, 2));
			service.SetGlobalBookmark(1);
			var bookmark = service.GetFileBookmarks("test").FirstOrDefault();

			Assert.That(bookmark.Number, Is.EqualTo(1));
			Assert.That(bookmark.Line, Is.EqualTo(1));
			Assert.That(bookmark.Column, Is.EqualTo(2));

			_shellSelectionServiceMock.Setup(s => s.GetActiveFileName()).Returns("test2").Verifiable();
			_shellSelectionServiceMock.Setup(s => s.GetActiveFilePosition()).Returns(new Position(3, 4));
			service.SetGlobalBookmark(1);
			bookmark = service.GetFileBookmarks("test2").FirstOrDefault();

			Assert.That(service.BookmarkCount, Is.EqualTo(1));
			Assert.That(bookmark.Number, Is.EqualTo(1));
			Assert.That(bookmark.Line, Is.EqualTo(3));
			Assert.That(bookmark.Column, Is.EqualTo(4));
			Assert.That(changed, Is.EqualTo(3));
		}

		[Test]
		public void SetGlobalBookmark_SameLine()
		{
			var service = GetService();

			var result = service.SetGlobalBookmark(5);
			var repeatResult = service.SetGlobalBookmark(1);

			var bookmarks = service.GetFileBookmarks("test");

			Assert.That(result, Is.True);
			Assert.That(repeatResult, Is.True);
			Assert.That(bookmarks, Is.Not.Null);
			Assert.That(bookmarks.Count(), Is.EqualTo(2));
		}

		[Test]
		public void GoToGlobalBookmark()
		{
			var service = GetService();

			service.SetGlobalBookmark(1);
			var result = service.GoToGlobalBookmark(1);

			Assert.That(result, Is.True);
			_shellHelperServiceMock.Verify(s => s.OpenFileAt("test", It.IsAny<int>(), It.IsAny<int>()));
			_shellStatusBarServiceMock.Verify(s => s.SetStatusBarText(It.IsNotNull<string>()));
		}

		[Test]
		public void GoToGlobalBookmark_CallbackClient()
		{
			var service = GetService();

			service.RegisterCallback(_bookmarkCallbackClientMock.Object, "test");
			service.SetGlobalBookmark(1);

			var result = service.GoToGlobalBookmark(1);

			Assert.That(result, Is.True);
			Assert.That(service.BookmarkCallbackCount, Is.EqualTo(1));
			_bookmarkCallbackClientMock.Verify(c => c.GetBookmarkLine(It.IsAny<Bookmark>()));
			_shellHelperServiceMock.Verify(s => s.OpenFileAt("test", 123, It.IsAny<int>()));
		}

		[Test]
		public void GoToGlobalBookmark_ErrorHandling()
		{
			var service = GetService();

			Assert.Throws<ArgumentOutOfRangeException>(() => service.GoToGlobalBookmark(0));
			Assert.Throws<ArgumentOutOfRangeException>(() => service.GoToGlobalBookmark(11));
		}

		[Test]
		public void GoToGlobalBookmark_NoBookmark()
		{
			var service = GetService();

			service.SetGlobalBookmark(1);
			var result = service.GoToGlobalBookmark(2);

			Assert.That(result, Is.False);
			_shellHelperServiceMock.Verify(s => s.OpenFileAt(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
			_shellStatusBarServiceMock.Verify(s => s.SetStatusBarText(It.IsNotNull<string>()));
		}

		[Test]
		public void GoToGlobalBookmark_InvalidLine()
		{
			var service = GetService();
			_shellHelperServiceMock.Setup(s => s.OpenFileAt(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())).Returns(false).Verifiable();

			service.SetGlobalBookmark(1);
			var result = service.GoToGlobalBookmark(1);

			Assert.That(result, Is.False);
			_shellHelperServiceMock.Verify(s => s.OpenFileAt(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()));
			_shellStatusBarServiceMock.Verify(s => s.SetStatusBarText(It.IsNotNull<string>()));
		}

		[Test]
		public void ClearFileBookmarks()
		{
			var service = GetService();

			service.SetBookmark(1);
			service.SetGlobalBookmark(1);

			var countBefore = service.BookmarkCount;

			var result = service.ClearFileBookmarks();

			var countAfter = service.BookmarkCount;

			Assert.That(result, Is.True);
			Assert.That(countBefore, Is.EqualTo(2));
			Assert.That(countAfter, Is.EqualTo(0));
			_shellStatusBarServiceMock.Verify(s => s.SetStatusBarText(It.IsNotNull<string>()));
		}

		[TestCase("", false)]
		[TestCase("test2", true)]
		public void ClearFileBookmarks_NoBookmarks(string fileName, bool expectedResult)
		{
			var service = GetService();

			service.SetBookmark(1);
			service.SetGlobalBookmark(1);

			_shellStatusBarServiceMock.Reset();
			_shellSelectionServiceMock.Setup(s => s.GetActiveFileName()).Returns(fileName).Verifiable();

			var result = service.ClearFileBookmarks();

			Assert.That(result, Is.EqualTo(expectedResult));
			Assert.That(service.BookmarkCount, Is.EqualTo(2));
			if (expectedResult)
				_shellStatusBarServiceMock.Verify(s => s.SetStatusBarText(It.IsNotNull<string>()), Times.Once);
			else
				_shellStatusBarServiceMock.Verify(s => s.SetStatusBarText(It.IsNotNull<string>()), Times.Never);
		}

		[Test]
		public void ClearAllBookmarks()
		{
			var service = GetService();
			var changed = 0;
			service.Changed += (sender, e) => { changed++; };

			service.SetBookmark(1);
			service.SetGlobalBookmark(1);

			var countBefore = service.BookmarkCount;

			var result = service.ClearAllBookmarks();

			var countAfter = service.BookmarkCount;

			Assert.That(result, Is.True);
			Assert.That(countBefore, Is.EqualTo(2));
			Assert.That(countAfter, Is.EqualTo(0));
			Assert.That(changed, Is.EqualTo(3), "One for each bookmark and one for file clear");
			_shellStatusBarServiceMock.Verify(s => s.SetStatusBarText(It.IsNotNull<string>()));
		}

		[Test]
		public void ClearAllBookmarks_NoBookmarks()
		{
			var service = GetService();
			var changed = 0;
			service.Changed += (sender, e) => { changed++; };

			var result = service.ClearAllBookmarks();

			Assert.That(result, Is.True);
			Assert.That(changed, Is.EqualTo(0));
			_shellStatusBarServiceMock.Verify(s => s.SetStatusBarText(It.IsNotNull<string>()));
		}

		[Test]
		public void UpdateBookmark()
		{
			var service = GetService();
			service.RegisterCallback(_bookmarkCallbackClientMock.Object, "test");
			service.SetBookmark(1);

			service.UpdateBookmark("test", 1, BookmarkType.Local);
			service.GoToBookmark(1);

			Assert.That(service.BookmarkCallbackCount, Is.EqualTo(1));
			_bookmarkCallbackClientMock.Verify(c => c.GetBookmarkLine(It.IsAny<Bookmark>()));
			_shellSelectionServiceMock.Verify(s => s.SetActiveFilePosition(123, It.IsAny<int>()));
		}

		[Test]
		public void UpdateBookmark_ErrorHandling()
		{
			var service = GetService();

			Assert.Throws<ArgumentNullException>(() => service.UpdateBookmark(null, 1, BookmarkType.Any));
			Assert.Throws<ArgumentNullException>(() => service.UpdateBookmark(string.Empty, 1, BookmarkType.Any));
			Assert.Throws<ArgumentOutOfRangeException>(() => service.UpdateBookmark("test", 0, BookmarkType.Any));
			Assert.Throws<ArgumentOutOfRangeException>(() => service.UpdateBookmark("test", 11, BookmarkType.Any));
		}

		[Test]
		public void UpdateBookmark_Invalid()
		{
			var service = GetService();
			service.SetBookmark(1);
			service.SetGlobalBookmark(1);

			Assert.DoesNotThrow(() => service.UpdateBookmark("bad", 1, BookmarkType.Local));
		}

		[Test]
		public void RemoveBookmark()
		{
			var service = GetService();
			service.SetBookmark(1);
			service.SetGlobalBookmark(1);

			service.RemoveBookmark("test", 1, BookmarkType.Local);
			service.RemoveBookmark("test", 1, BookmarkType.Global);

			Assert.That(service.BookmarkCount, Is.EqualTo(0));
		}

		[Test]
		public void RemoveBookmark_ErrorHandling()
		{
			var service = GetService();

			Assert.Throws<ArgumentNullException>(() => service.RemoveBookmark(null, 1, BookmarkType.Any));
			Assert.Throws<ArgumentNullException>(() => service.RemoveBookmark(string.Empty, 1, BookmarkType.Any));
			Assert.Throws<ArgumentOutOfRangeException>(() => service.RemoveBookmark("test", 11, BookmarkType.Any));
		}

		[Test]
		public void RemoveBookmark_Invalid()
		{
			var service = GetService();
			service.SetBookmark(1);
			service.SetGlobalBookmark(1);

			Assert.DoesNotThrow(() => service.RemoveBookmark("bad", 1, BookmarkType.Local));
		}

		[Test]
		public void SolutionClosed()
		{
			var service = GetService();
			var events = GetSolutionEvents(service);
			var changed = 0;
			var type = BookmarkType.Local;

			service.RegisterCallback(_bookmarkCallbackClientMock.Object, "test");
			service.SetBookmark(1);
			service.SetGlobalBookmark(1);
			service.Changed += (sender, e) => { changed++; type = e.Type; };

			events.SolutionClosed();

			Assert.That(service.BookmarkCount, Is.EqualTo(0));
			Assert.That(service.BookmarkCallbackCount, Is.EqualTo(0));
			Assert.That(changed, Is.EqualTo(1), "One notification per solution");
			Assert.That(type, Is.EqualTo(BookmarkType.Any));
		}

		[Test]
		public void FileDeleted()
		{
			var service = GetService();
			var events = GetSolutionEvents(service);
			var changed = 0;
			var type = BookmarkType.Local;

			service.SetBookmark(1);
			service.SetGlobalBookmark(1);
			service.Changed += (sender, e) => { changed++; type = e.Type; };

			events.FileDeleted(new string[] { "test" }, null);

			Assert.That(service.BookmarkCount, Is.EqualTo(0));
			Assert.That(changed, Is.EqualTo(1), "One notification per file");
			Assert.That(type, Is.EqualTo(BookmarkType.Any));
		}

		[Test]
		public void FileRenamed()
		{
			var service = GetService();
			var events = GetSolutionEvents(service);
			var changed = 0;

			service.SetBookmark(1);
			service.SetGlobalBookmark(1);
			service.Changed += (sender, e) => { changed++; };

			events.FileRenamed(new string[] { "test" }, new string[] { "test2" }, null);

			Assert.That(service.BookmarkCount, Is.EqualTo(2));
			Assert.That(service.GetFileBookmarks("test2").Count(), Is.EqualTo(2));
			Assert.That(changed, Is.EqualTo(0), "No notifications");
		}

		#endregion
	}
}