using System;
using System.Collections.Generic;
using System.Linq;

using DPackRx.CodeModel;
using DPackRx.Extensions;

namespace DPackRx.Services
{
	/// <summary>
	/// Search match service.
	/// </summary>
	public class SearchMatchService : ISearchMatchService
	{
		#region Fields

		private readonly IFileTypeResolver _fileTypeResolver;
		private readonly IWildcardMatch _wildcardMatch;

		protected internal const int RANK_EXACT = 50;
		protected internal const int RANK_FROM_START = RANK_EXACT - 5;
		protected internal const int RANK_NOT_FROM_START = 10;
		protected internal const int RANK_PASCAL_CASE_EXACT = RANK_EXACT;
		protected internal const int RANK_PASCAL_CASE_START_SINGLE = RANK_FROM_START;
		protected internal const int RANK_PASCAL_CASE_START_MULTIPLE = RANK_FROM_START - 10;
		protected internal const int RANK_PASCAL_CASE_NOT_FROM_START_SINGLE = RANK_NOT_FROM_START;
		protected internal const int RANK_PASCAL_CASE_NOT_FROM_START_MULTIPLE = RANK_NOT_FROM_START;
		protected internal const int RANK_CODE = 1;

		#endregion

		public SearchMatchService(IFileTypeResolver fileTypeResolver, IWildcardMatch wildcardMatch)
		{
			_fileTypeResolver = fileTypeResolver;
			_wildcardMatch = wildcardMatch;
		}

		#region ISearchMatchService Members

		/// <summary>
		/// Updates items match rank based on search filter criteria.
		/// </summary>
		/// <param name="filter">Search filter.</param>
		/// <param name="items">Items.</param>
		public void MatchItems(string filter, IEnumerable<IMatchItem> items)
		{
			if ((items == null) || (items.Count() == 0))
				return;

			if (string.IsNullOrEmpty(filter))
			{
				ResetItems(items);
				return;
			}

			var tokens = GetSearchTokens(filter);
			if (tokens.Count == 0)
			{
				ResetItems(items);
				return;
			}

			foreach (var item in items)
			{
				item.Matched = MatchItem(tokens, item, out int rank);

				if (_fileTypeResolver.IsCodeSubType(item.ItemSubType))
					rank += RANK_CODE;

				item.Rank = rank;
			}
		}

		/// <summary>
		/// Resets item rank and match status.
		/// </summary>
		/// <param name="items">Items.</param>
		public void ResetItems(IEnumerable<IMatchItem> items)
		{
			items?.ForEach(i =>
			{
				i.Rank = 0;
				i.Matched = true;
			});
		}

		/// <summary>
		/// Breaks down search filter onto multiple search tokens.
		/// </summary>
		/// <param name="filter">Search filter.</param>
		/// <returns>List of search tokens.</returns>
		public IList<SearchToken> GetSearchTokens(string filter)
		{
			var tokens = new List<SearchToken>(2);

			if (!string.IsNullOrEmpty(filter))
			{
				var splitFilters = filter.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

				splitFilters
					.Where(f => !tokens.Any(t => t.Filter == f))
					.ForEach(f => tokens.Add(new SearchToken(_wildcardMatch, f)));
			}

			return tokens;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Matches an item to the filter and sets match rank.
		/// </summary>
		private bool MatchItem(IList<SearchToken> tokens, IMatchItem item, out int rank)
		{
			rank = 0;
			if ((item == null) || (tokens?.Count == 0))
				return false;

			var match = false;

			foreach (var token in tokens)
			{
				match = false;

				var filter = token.Filter;

				// Check the actual filter (this should never happen)
				if (string.IsNullOrEmpty(filter))
				{
					match = true;
					continue;
				}

				var wildcard = token.Wildcard;
				var filterRank = 0;
				var filterPascalRank = 0;

				// Match the actual filter
				var itemData = item.Data;
				if (wildcard.WildcardPresent)
				{
					match = wildcard.Match(itemData);
				}
				else
				{
					var pos = itemData.IndexOf(filter, StringComparison.OrdinalIgnoreCase);
					match = pos != -1;
					if (match)
					{
						if (pos == 0)
						{
							if ((filter.Length == itemData.Length) ||
									((item.DataEndingIndex > 0) && (filter.Length == item.DataEndingIndex)))
								filterRank += RANK_EXACT;
							else
								filterRank += RANK_FROM_START;
						}
						else
						{
							filterRank += GetMatchRank(pos, filter.Length, itemData.Length, RANK_NOT_FROM_START);
						}
					}
				}

				// Pascal case match
				var itemPascalCasedData = item.PascalCasedData;
				if (!string.IsNullOrEmpty(itemPascalCasedData))
				{
					var pos = itemPascalCasedData.IndexOf(filter, StringComparison.OrdinalIgnoreCase);
					if (pos >= 0)
					{
						match = true;
						if (pos == 0)
						{
							if (itemPascalCasedData.Length == filter.Length)
								filterPascalRank += RANK_PASCAL_CASE_EXACT;
							else if (itemPascalCasedData.Length == 1)
								filterPascalRank += RANK_PASCAL_CASE_START_SINGLE;
							else
								filterPascalRank += RANK_PASCAL_CASE_START_MULTIPLE;
						}
						else
						{
							if (itemPascalCasedData.Length == 1)
								filterPascalRank += RANK_PASCAL_CASE_NOT_FROM_START_SINGLE;
							else
								filterPascalRank += GetMatchRank(pos, filter.Length, itemPascalCasedData.Length,
									RANK_PASCAL_CASE_NOT_FROM_START_MULTIPLE);
						}
					}
				}

				// Pick the highest rank
				if ((filterRank > 0) || (filterPascalRank > 0))
				{
					// Ranks are applied on either/or basis
					if (filterRank >= filterPascalRank)
						rank += filterRank;
					else
						rank += filterPascalRank;
				}

				if (!match)
					break;
			} // for index

			return match;
		}

		private int GetMatchRank(int matchPosition, int filterLength, int itemLength, int rankMultiplier)
		{
			double rankRatio = 0.0;

			// Match position within item:
			// * the closer to the start the higher the rank
			// * take 1/Ns portion only as it's a less reliable component
			rankRatio += (1 - ((double)(matchPosition + 1) / itemLength)) / 3;

			// Match length in comparison to item:
			// * the longer the filter the higher the rank
			// * take xN as it's a heavier weight component
			rankRatio += ((double)filterLength / itemLength) * 2;

			// Convert combined ratio rank to avoid double rounding
			return Convert.ToInt32(rankMultiplier * rankRatio);
		}

		#endregion
	}
}