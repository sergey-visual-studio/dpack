namespace DPackRx.Services
{
	/// <summary>
	/// Initializes wild card instance.
	/// </summary>
	/// <param name="filter">Filter value.</param>
	public interface IWildcardMatch
	{
		/// <summary>
		/// Indicates whether '*' or '?' wildcard character is present in the Filter.
		/// </summary>
		bool WildcardPresent { get; }

		/// <summary>
		/// Initializes wild card instance.
		/// </summary>
		/// <param name="filter">Filter value.</param>
		void Initialize(string filter);

		/// <summary>
		/// Matches data to filter w/o case sensitivity.
		/// </summary>
		/// <param name="data">Data to match.</param>
		/// <returns>Returns true if data matches.</returns>
		bool Match(string data);
	}
}