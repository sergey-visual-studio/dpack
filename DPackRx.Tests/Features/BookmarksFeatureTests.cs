using System;
using System.Collections.Generic;

using Moq;
using NUnit.Framework;

using DPackRx.Features;
using DPackRx.Features.Bookmarks;
using DPackRx.Options;
using DPackRx.Package;
using DPackRx.Services;

namespace DPackRx.Tests.Features
{
	/// <summary>
	/// BookmarksFeature tests.
	/// </summary>
	[TestFixture]
	public class BookmarksFeatureTests
	{
		#region Fields

		private Mock<IServiceProvider> _serviceProviderMock;
		private Mock<ILog> _logMock;
		private Mock<IOptionsService> _optionsServiceMock;
		private Mock<IBookmarksService> _bookmarksServiceMock;
		private Mock<IShellSelectionService> _shellSelectionServiceMock;

		#endregion

		#region Tests Setup

		[SetUp]
		public void Setup()
		{
			_serviceProviderMock = new Mock<IServiceProvider>();

			_logMock = new Mock<ILog>();
			_logMock.Setup(l => l.LogMessage(It.IsAny<string>(), It.IsAny<string>())).Verifiable();

			_optionsServiceMock = new Mock<IOptionsService>();

			_bookmarksServiceMock = new Mock<IBookmarksService>();

			_shellSelectionServiceMock = new Mock<IShellSelectionService>();
			_shellSelectionServiceMock.Setup(s => s.IsContextActive(It.IsAny<ContextType>())).Returns(true).Verifiable();
		}

		[TearDown]
		public void TearDown()
		{
			_serviceProviderMock = null;
			_logMock = null;
			_optionsServiceMock = null;
			_bookmarksServiceMock = null;
			_shellSelectionServiceMock = null;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Returns test feature instance.
		/// </summary>
		private IFeature GetFeature()
		{
			return new BookmarksFeature(_serviceProviderMock.Object, _logMock.Object, _optionsServiceMock.Object,
				_bookmarksServiceMock.Object, _shellSelectionServiceMock.Object);
		}

		#endregion

		#region Tests

		[Test]
		public void Initialize()
		{
			var feature = GetFeature();

			feature.Initialize();

			Assert.That(feature.Initialized, Is.True);
		}

		[Test]
		public void GetCommandIds()
		{
			var feature = GetFeature();

			var commands = feature.GetCommandIds();

			var commandIds = new List<int>(new[] {
				CommandIDs.BOOKMARK_GET_1, CommandIDs.BOOKMARK_GET_2, CommandIDs.BOOKMARK_GET_3, CommandIDs.BOOKMARK_GET_4, CommandIDs.BOOKMARK_GET_5,
				CommandIDs.BOOKMARK_GET_6, CommandIDs.BOOKMARK_GET_7, CommandIDs.BOOKMARK_GET_8, CommandIDs.BOOKMARK_GET_9, CommandIDs.BOOKMARK_GET_0,
				CommandIDs.BOOKMARK_SET_1, CommandIDs.BOOKMARK_SET_2, CommandIDs.BOOKMARK_SET_3, CommandIDs.BOOKMARK_SET_4, CommandIDs.BOOKMARK_SET_5,
				CommandIDs.BOOKMARK_SET_6, CommandIDs.BOOKMARK_SET_7, CommandIDs.BOOKMARK_SET_8, CommandIDs.BOOKMARK_SET_9, CommandIDs.BOOKMARK_SET_0,
				CommandIDs.BOOKMARK_GET_GLB_1, CommandIDs.BOOKMARK_GET_GLB_2, CommandIDs.BOOKMARK_GET_GLB_3, CommandIDs.BOOKMARK_GET_GLB_4, CommandIDs.BOOKMARK_GET_GLB_5,
				CommandIDs.BOOKMARK_GET_GLB_6, CommandIDs.BOOKMARK_GET_GLB_7, CommandIDs.BOOKMARK_GET_GLB_8, CommandIDs.BOOKMARK_GET_GLB_9, CommandIDs.BOOKMARK_GET_GLB_0,
				CommandIDs.BOOKMARK_SET_GLB_1, CommandIDs.BOOKMARK_SET_GLB_2, CommandIDs.BOOKMARK_SET_GLB_3, CommandIDs.BOOKMARK_SET_GLB_4, CommandIDs.BOOKMARK_SET_GLB_5,
				CommandIDs.BOOKMARK_SET_GLB_6, CommandIDs.BOOKMARK_SET_GLB_7, CommandIDs.BOOKMARK_SET_GLB_8, CommandIDs.BOOKMARK_SET_GLB_9, CommandIDs.BOOKMARK_SET_GLB_0,
				CommandIDs.BOOKMARK_CLEAR_F, CommandIDs.BOOKMARK_CLEAR_S
			});

			Assert.That(commands, Is.Not.Null);
			Assert.That(commands.Count, Is.EqualTo(commandIds.Count));
			commandIds.ForEach(c => Assert.That(commands, Contains.Item(c)));
		}

		[TestCase(CommandIDs.BOOKMARK_GET_1, true)]
		[TestCase(CommandIDs.BOOKMARK_GET_2, true)]
		[TestCase(CommandIDs.BOOKMARK_GET_3, true)]
		[TestCase(CommandIDs.BOOKMARK_GET_4, true)]
		[TestCase(CommandIDs.BOOKMARK_GET_5, true)]
		[TestCase(CommandIDs.BOOKMARK_GET_6, true)]
		[TestCase(CommandIDs.BOOKMARK_GET_7, true)]
		[TestCase(CommandIDs.BOOKMARK_GET_8, true)]
		[TestCase(CommandIDs.BOOKMARK_GET_9, true)]
		[TestCase(CommandIDs.BOOKMARK_GET_0, true)]
		[TestCase(CommandIDs.BOOKMARK_SET_1, true)]
		[TestCase(CommandIDs.BOOKMARK_SET_2, true)]
		[TestCase(CommandIDs.BOOKMARK_SET_3, true)]
		[TestCase(CommandIDs.BOOKMARK_SET_4, true)]
		[TestCase(CommandIDs.BOOKMARK_SET_5, true)]
		[TestCase(CommandIDs.BOOKMARK_SET_6, true)]
		[TestCase(CommandIDs.BOOKMARK_SET_7, true)]
		[TestCase(CommandIDs.BOOKMARK_SET_8, true)]
		[TestCase(CommandIDs.BOOKMARK_SET_9, true)]
		[TestCase(CommandIDs.BOOKMARK_GET_GLB_1, true)]
		[TestCase(CommandIDs.BOOKMARK_GET_GLB_2, true)]
		[TestCase(CommandIDs.BOOKMARK_GET_GLB_3, true)]
		[TestCase(CommandIDs.BOOKMARK_GET_GLB_4, true)]
		[TestCase(CommandIDs.BOOKMARK_GET_GLB_5, true)]
		[TestCase(CommandIDs.BOOKMARK_GET_GLB_6, true)]
		[TestCase(CommandIDs.BOOKMARK_GET_GLB_7, true)]
		[TestCase(CommandIDs.BOOKMARK_GET_GLB_8, true)]
		[TestCase(CommandIDs.BOOKMARK_GET_GLB_9, true)]
		[TestCase(CommandIDs.BOOKMARK_GET_GLB_0, true)]
		[TestCase(CommandIDs.BOOKMARK_SET_GLB_1, true)]
		[TestCase(CommandIDs.BOOKMARK_SET_GLB_2, true)]
		[TestCase(CommandIDs.BOOKMARK_SET_GLB_3, true)]
		[TestCase(CommandIDs.BOOKMARK_SET_GLB_4, true)]
		[TestCase(CommandIDs.BOOKMARK_SET_GLB_5, true)]
		[TestCase(CommandIDs.BOOKMARK_SET_GLB_6, true)]
		[TestCase(CommandIDs.BOOKMARK_SET_GLB_7, true)]
		[TestCase(CommandIDs.BOOKMARK_SET_GLB_8, true)]
		[TestCase(CommandIDs.BOOKMARK_SET_GLB_9, true)]
		[TestCase(CommandIDs.BOOKMARK_SET_GLB_0, true)]
		[TestCase(CommandIDs.BOOKMARK_CLEAR_F, true)]
		[TestCase(CommandIDs.BOOKMARK_CLEAR_S, true)]
		[TestCase(0, false)]
		public void IsValidContext(int commandId, bool expectedResult)
		{
			var feature = GetFeature();

			var result = feature.IsValidContext(commandId);

			Assert.That(result, Is.EqualTo(expectedResult));
			if (expectedResult)
				_shellSelectionServiceMock.Verify(s => s.IsContextActive(It.IsAny<ContextType>()), Times.AtLeast(2));
			else
				_shellSelectionServiceMock.Verify(s => s.IsContextActive(It.IsAny<ContextType>()), Times.Never);
		}

		[Test]
		public void IsValidContext_NoContext()
		{
			var feature = GetFeature();

			_shellSelectionServiceMock.Setup(s => s.IsContextActive(ContextType.SolutionExists)).Returns(false).Verifiable();

			var result = feature.IsValidContext(CommandIDs.BOOKMARK_GET_1);

			Assert.That(result, Is.EqualTo(false));
			_shellSelectionServiceMock.Verify(s => s.IsContextActive(It.IsAny<ContextType>()));
		}

		[TestCase(CommandIDs.BOOKMARK_GET_1)]
		[TestCase(CommandIDs.BOOKMARK_GET_2)]
		[TestCase(CommandIDs.BOOKMARK_GET_3)]
		[TestCase(CommandIDs.BOOKMARK_GET_4)]
		[TestCase(CommandIDs.BOOKMARK_GET_5)]
		[TestCase(CommandIDs.BOOKMARK_GET_6)]
		[TestCase(CommandIDs.BOOKMARK_GET_7)]
		[TestCase(CommandIDs.BOOKMARK_GET_8)]
		[TestCase(CommandIDs.BOOKMARK_GET_9)]
		[TestCase(CommandIDs.BOOKMARK_GET_0)]
		public void Execute_GetBookmark(int commandId)
		{
			var feature = GetFeature();

			_bookmarksServiceMock.Setup(b => b.GoToBookmark(It.IsInRange<int>(1, 10, Range.Inclusive))).Returns(true).Verifiable();

			var result = feature.Execute(commandId);

			Assert.That(result, Is.True);
			_bookmarksServiceMock.Verify(b => b.GoToBookmark(It.IsInRange<int>(1, 10, Range.Inclusive)));
		}

		[TestCase(CommandIDs.BOOKMARK_SET_1)]
		[TestCase(CommandIDs.BOOKMARK_SET_2)]
		[TestCase(CommandIDs.BOOKMARK_SET_3)]
		[TestCase(CommandIDs.BOOKMARK_SET_4)]
		[TestCase(CommandIDs.BOOKMARK_SET_5)]
		[TestCase(CommandIDs.BOOKMARK_SET_6)]
		[TestCase(CommandIDs.BOOKMARK_SET_7)]
		[TestCase(CommandIDs.BOOKMARK_SET_8)]
		[TestCase(CommandIDs.BOOKMARK_SET_9)]
		[TestCase(CommandIDs.BOOKMARK_SET_0)]
		public void Execute_SetBookmark(int commandId)
		{
			var feature = GetFeature();

			_bookmarksServiceMock.Setup(b => b.SetBookmark(It.IsInRange<int>(1, 10, Range.Inclusive))).Returns(true).Verifiable();

			var result = feature.Execute(commandId);

			Assert.That(result, Is.True);
			_bookmarksServiceMock.Verify(b => b.SetBookmark(It.IsInRange<int>(1, 10, Range.Inclusive)));
		}

		[TestCase(CommandIDs.BOOKMARK_GET_GLB_1)]
		[TestCase(CommandIDs.BOOKMARK_GET_GLB_2)]
		[TestCase(CommandIDs.BOOKMARK_GET_GLB_3)]
		[TestCase(CommandIDs.BOOKMARK_GET_GLB_4)]
		[TestCase(CommandIDs.BOOKMARK_GET_GLB_5)]
		[TestCase(CommandIDs.BOOKMARK_GET_GLB_6)]
		[TestCase(CommandIDs.BOOKMARK_GET_GLB_7)]
		[TestCase(CommandIDs.BOOKMARK_GET_GLB_8)]
		[TestCase(CommandIDs.BOOKMARK_GET_GLB_9)]
		[TestCase(CommandIDs.BOOKMARK_GET_GLB_0)]
		public void Execute_GetGlobalBookmark(int commandId)
		{
			var feature = GetFeature();

			_bookmarksServiceMock.Setup(b => b.GoToGlobalBookmark(It.IsInRange<int>(1, 10, Range.Inclusive))).Returns(true).Verifiable();

			var result = feature.Execute(commandId);

			Assert.That(result, Is.True);
			_bookmarksServiceMock.Verify(b => b.GoToGlobalBookmark(It.IsInRange<int>(1, 10, Range.Inclusive)));
		}

		[TestCase(CommandIDs.BOOKMARK_SET_GLB_1)]
		[TestCase(CommandIDs.BOOKMARK_SET_GLB_2)]
		[TestCase(CommandIDs.BOOKMARK_SET_GLB_3)]
		[TestCase(CommandIDs.BOOKMARK_SET_GLB_4)]
		[TestCase(CommandIDs.BOOKMARK_SET_GLB_5)]
		[TestCase(CommandIDs.BOOKMARK_SET_GLB_6)]
		[TestCase(CommandIDs.BOOKMARK_SET_GLB_7)]
		[TestCase(CommandIDs.BOOKMARK_SET_GLB_8)]
		[TestCase(CommandIDs.BOOKMARK_SET_GLB_9)]
		[TestCase(CommandIDs.BOOKMARK_SET_GLB_0)]
		public void Execute_SetGlobalBookmark(int commandId)
		{
			var feature = GetFeature();

			_bookmarksServiceMock.Setup(b => b.SetGlobalBookmark(It.IsInRange<int>(1, 10, Range.Inclusive))).Returns(true).Verifiable();

			var result = feature.Execute(commandId);

			Assert.That(result, Is.True);
			_bookmarksServiceMock.Verify(b => b.SetGlobalBookmark(It.IsInRange<int>(1, 10, Range.Inclusive)));
		}

		[Test]
		public void Execute_ClearFileBookmarks()
		{
			var feature = GetFeature();

			_bookmarksServiceMock.Setup(b => b.ClearFileBookmarks()).Returns(true).Verifiable();

			var result = feature.Execute(CommandIDs.BOOKMARK_CLEAR_F);

			Assert.That(result, Is.True);
			_bookmarksServiceMock.Verify(b => b.ClearFileBookmarks());
		}

		[Test]
		public void Execute_ClearSolutionBookmarks()
		{
			var feature = GetFeature();

			_bookmarksServiceMock.Setup(b => b.ClearAllBookmarks()).Returns(true).Verifiable();

			var result = feature.Execute(CommandIDs.BOOKMARK_CLEAR_S);

			Assert.That(result, Is.True);
			_bookmarksServiceMock.Verify(b => b.ClearAllBookmarks());
		}

		#endregion
	}
}