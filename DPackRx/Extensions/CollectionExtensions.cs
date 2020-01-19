using System.Collections.Generic;

namespace DPackRx.Extensions
{
	/// <summary>
	/// Collection extensions.
	/// </summary>
	public static class CollectionExtensions
	{
		/// <summary>
		/// Custom: adds items to collection.
		/// </summary>
		/// <typeparam name="T">Item type.</typeparam>
		/// <param name="collection">Collection.</param>
		/// <param name="items">Items.</param>
		public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
		{
			if (items == null)
				return;

			foreach (var item in items)
			{
				collection.Add(item);
			}
		}
	}
}