using System.Collections.Generic;

using DPackRx.CodeModel;

namespace DPackRx.Services
{
	/// <summary>
	/// Search match service.
	/// </summary>
	public interface ISearchMatchService
	{
		/// <summary>
		/// Updates items match rank based on search filter criteria.
		/// </summary>
		/// <param name="filter">Search filter.</param>
		/// <param name="items">Items.</param>
		void MatchItems(string filter, IEnumerable<IMatchItem> items);

		/// <summary>
		/// Resets item rank and match status.
		/// </summary>
		/// <param name="items">Items.</param>
		void ResetItems(IEnumerable<IMatchItem> items);

		/// <summary>
		/// Breaks down search filter onto multiple search tokens.
		/// </summary>
		/// <param name="filter">Search filter.</param>
		/// <returns>List of search tokens.</returns>
		IList<SearchToken> GetSearchTokens(string filter);
	}
}