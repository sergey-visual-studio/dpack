using System.Collections.Generic;
using System.Linq;

using DPackRx.CodeModel;
using DPackRx.Extensions;
using DPackRx.Services;

using Moq;

using NUnit.Framework;

namespace DPackRx.Tests.Services
{
	/// <summary>
	/// SearchMatchService tests.
	/// </summary>
	[TestFixture]
	public class SearchMatchServiceTests
	{
		#region Fields

		private Mock<IFileTypeResolver> _fileTypeResolverMock;
		private Mock<IWildcardMatch> _wildcardMatchMock;

		#endregion

		#region TestMatchItem class

		private class TestMatchItem : IMatchItem
		{
			public string Data { get; set; }

			public int DataEndingIndex { get; set; }

			public string PascalCasedData { get; set; }

			public FileSubType ItemSubType { get; set; }

			public bool Matched { get; set; }

			public int Rank { get; set; }

			public int CompareTo(IMatchItem other)
			{
				return 0;
			}

			public int CompareTo(object obj)
			{
				return 0;
			}
		}

		#endregion

		#region Tests Setup

		[SetUp]
		public void Setup()
		{
			_fileTypeResolverMock = new Mock<IFileTypeResolver>();
			_fileTypeResolverMock.Setup(r => r.IsCodeSubType(It.IsAny<FileSubType>(), true)).Returns(false).Verifiable();
			_fileTypeResolverMock.Setup(r => r.IsCodeSubType(FileSubType.Code, true)).Returns(true).Verifiable();

			_wildcardMatchMock = new Mock<IWildcardMatch>();
		}

		[TearDown]
		public void TearDown()
		{
			_fileTypeResolverMock = null;
			_wildcardMatchMock = null;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Returns test service instance.
		/// </summary>
		private ISearchMatchService GetService()
		{
			return new SearchMatchService(_fileTypeResolverMock.Object, _wildcardMatchMock.Object);
		}

		private static List<IMatchItem> GetItems(bool matched = false, int rank = 0)
		{
			var items = new List<IMatchItem>();
			items.Add(new TestMatchItem
			{
				ItemSubType = FileSubType.None,
				Data = "TestFile1.txt",
				PascalCasedData = "TF1",
				DataEndingIndex = 9,
				Matched = matched,
				Rank = rank
			});
			items.Add(new TestMatchItem
			{
				ItemSubType = FileSubType.Code,
				Data = "TestCodeFile2.cs",
				PascalCasedData = "TCF2",
				DataEndingIndex = 13,
				Matched = matched,
				Rank = rank
			});
			return items;
		}

		#endregion

		#region Tests

		[TestCase("test", SearchMatchService.RANK_FROM_START + SearchMatchService.RANK_CODE, SearchMatchService.RANK_FROM_START,
			"Partial match items should have code item have a higher rank")]
		[TestCase("file", 7, 8, "Partial match items should have code item have a higher rank")]
		[TestCase("code", 8, 0, "Partial match items should have code item have a higher rank")]
		[TestCase("TestFile1", SearchMatchService.RANK_CODE, SearchMatchService.RANK_EXACT, "Exact match items should have the highest rank")]
		[TestCase("TestCodeFile2", SearchMatchService.RANK_EXACT + SearchMatchService.RANK_CODE, 0, "Exact match items should have the highest rank")]
		[TestCase("TF1", SearchMatchService.RANK_CODE, SearchMatchService.RANK_PASCAL_CASE_EXACT, "Exact pascal case match items should have the highest rank")]
		[TestCase("TCF2", SearchMatchService.RANK_PASCAL_CASE_EXACT + SearchMatchService.RANK_CODE, 0, "Exact pascal case match items should have the highest rank")]
		public void MatchItems(string filter, int codeRank, int nonCodeRank, string error)
		{
			var service = GetService();
			var items = GetItems();

			service.MatchItems(filter, items);

			var codeItem = items.FirstOrDefault(i => i.ItemSubType == FileSubType.Code);
			var nonCodeItem = items.FirstOrDefault(i => i.ItemSubType != FileSubType.Code);
			Assert.That(codeItem, Is.Not.Null);
			Assert.That(nonCodeItem, Is.Not.Null);
			Assert.That(codeItem.Rank, Is.EqualTo(codeRank), $"Code item: {error}");
			Assert.That(nonCodeItem.Rank, Is.EqualTo(nonCodeRank), $"Non-code item: {error}");

			_fileTypeResolverMock.Verify(r => r.IsCodeSubType(It.IsAny<FileSubType>(), true), Times.Exactly(items.Count));
		}

		[TestCase("test", false)]
		[TestCase("test", true)]
		[TestCase(null, true)]
		[TestCase("", true)]
		public void MatchItems_ErrorHandling(string filter, bool createItems)
		{
			var service = GetService();

			Assert.DoesNotThrow(() => service.MatchItems(filter, createItems ? new List<IMatchItem>() : null));
		}

		[Test]
		public void ResetItems()
		{
			var service = GetService();
			var items = GetItems(false, 123);

			service.ResetItems(items);

			items.ForEach(i => Assert.That(i.Matched, Is.True));
			items.ForEach(i => Assert.That(i.Rank, Is.Zero));
		}

		[TestCase("test test hey", "test;hey")]
		[TestCase("test  hey", "test;hey")]
		[TestCase(" ", "")]
		public void GetSearchTokens(string filter, string expectedFilters)
		{
			var service = GetService();

			var tokens = service.GetSearchTokens(filter);

			Assert.That(tokens, Is.Not.Null);
			if (string.IsNullOrEmpty(expectedFilters))
			{
				Assert.That(tokens.Count, Is.Zero);
			}
			else
			{
				var expectedTokens = expectedFilters.Split(';').ToList();
				Assert.That(tokens.Count, Is.EqualTo(expectedTokens.Count));
				tokens.ForEach(t => Assert.That(expectedTokens.Contains(t.Filter), Is.True));
			}
		}

		[Test]
		public void SearchToken()
		{
			var token = new SearchToken(_wildcardMatchMock.Object, "test");

			Assert.That(token.Filter, Is.EqualTo("test"));
			Assert.That(token.Wildcard, Is.EqualTo(_wildcardMatchMock.Object));
		}

		#endregion
	}
}