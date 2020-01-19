using System;
using System.Diagnostics;

namespace DPackRx.Services
{
	[DebuggerDisplay("{Filter}")]
	public class SearchToken
	{
		public SearchToken(IWildcardMatch wildcardMatch, string filter)
		{
			if (wildcardMatch == null)
				throw new ArgumentNullException(nameof(wildcardMatch));

			if (string.IsNullOrEmpty(filter))
				throw new ArgumentNullException(nameof(filter));

			this.Filter = filter;
			this.Wildcard = wildcardMatch;
			this.Wildcard.Initialize(filter);
		}

		#region Properties

		/// <summary>
		/// Search filter.
		/// </summary>
		public string Filter { get; private set; }

		/// <summary>
		/// Wildcard match.
		/// </summary>
		public IWildcardMatch Wildcard { get; private set; }

		#endregion
	}
}