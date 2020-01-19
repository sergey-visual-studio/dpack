using System;

namespace DPackRx.CodeModel
{
	/// <summary>
	/// Match item.
	/// </summary>
	public interface IMatchItem : IComparable, IComparable<IMatchItem>
	{
		/// <summary>
		/// Data used for matching.
		/// </summary>
		string Data { get; }

		/// <summary>
		/// Data optional ending index.
		/// Matching uses that to treat partial match up to index as an exact one.
		/// It must be greater than 0.
		/// </summary>
		int DataEndingIndex { get; }

		/// <summary>
		/// Optional pascal cased version of Data used for matching.
		/// </summary>
		string PascalCasedData { get; }

		/// <summary>
		/// Item code type, whenever's applicable.
		/// </summary>
		FileSubType ItemSubType { get; }

		/// <summary>
		/// Item's match result.
		/// </summary>
		bool Matched { get; set; }

		/// <summary>
		/// Match rank 0 and up, the higher the better matched item it is.
		/// </summary>
		int Rank { get; set; }
	}
}