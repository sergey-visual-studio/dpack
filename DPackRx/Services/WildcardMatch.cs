using System;
using System.Text;
using System.Text.RegularExpressions;

namespace DPackRx.Services
{
	/// <summary>
	/// Performs case insensitive wildcard match using * and ? characters.
	/// </summary>
	public class WildcardMatch : IWildcardMatch
	{
		#region Field

		private string _filter = string.Empty;

		#endregion

		#region IWildcardMatch Members

		/// <summary>
		/// Indicates whether '*' or '?' wildcard character is present in the Filter.
		/// </summary>
		public bool WildcardPresent { get; private set; }

		/// <summary>
		/// Initializes wild card instance.
		/// </summary>
		/// <param name="filter">Filter value.</param>
		public void Initialize(string filter)
		{
			string filterValue = null;

			if (!string.IsNullOrEmpty(filter))
			{
				// Strip all repeating * or Regex.IsMatch() locks up on a long input string
				if ((filter.Length > 0) && (filter.IndexOf("**", StringComparison.OrdinalIgnoreCase) != -1))
				{
					var value = new StringBuilder(30);
					var lastStar = false;
					char current;
					for (int index = 0; index < filter.Length; index++)
					{
						current = filter[index];
						if (lastStar && (current == '*'))
							continue;
						value.Append(current);
						lastStar = (current == '*');
					}
					filterValue = value.ToString();
				}
				else
				{
					filterValue = filter;
				}

				_filter = Regex.Escape(filterValue);    // for compatibility
				_filter = _filter.Replace(@"\?", ".");  // for ? wildcard
				_filter = _filter.Replace(@"\*", ".*"); // for * wildcard
			}

			this.WildcardPresent = !string.IsNullOrEmpty(filterValue) && ((filterValue.IndexOf('*') != -1) || (filterValue.IndexOf('?') != -1));
		}

		/// <summary>
		/// Matches data to filter w/o case sensitivity.
		/// </summary>
		/// <param name="data">Data to match.</param>
		/// <returns>Returns true if data matches.</returns>
		public bool Match(string data)
		{
			bool result;
			if (string.IsNullOrEmpty(_filter))
				result = data == string.Empty;
			else if (string.IsNullOrEmpty(data))
				result = data == string.Empty;
			else
				result = Regex.IsMatch(data, _filter, RegexOptions.IgnoreCase);

			return result;
		}

		#endregion
	}
}