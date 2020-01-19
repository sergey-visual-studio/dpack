using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DPackRx.Extensions
{
	/// <summary>
	/// Enumerable extensions.
	/// </summary>
	public static class EnumerableExtensions
	{
		/// <summary>
		/// Custom: items iterator.
		/// </summary>
		/// <typeparam name="T">Item type.</typeparam>
		/// <param name="items">Items.</param>
		/// <param name="action">Action.</param>
		public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
		{
			if (action == null)
				return;

			foreach (var item in items)
			{
				action(item);
			}
		}

		/// <summary>
		/// Custom: items iterator.
		/// </summary>
		/// <typeparam name="T">Item type.</typeparam>
		/// <param name="items">Items.</param>
		/// <param name="action">Action.</param>
		public static void ForEach<T>(this IEnumerable items, Action<T> action)
		{
			if (action == null)
				return;

			foreach (var item in items)
			{
				if (item is T)
					action((T)item);
			}
		}

		/// <summary>
		/// Custom: immutable items iterator.
		/// </summary>
		/// <typeparam name="T">Item type.</typeparam>
		/// <param name="items">Items.</param>
		/// <param name="action">Action.</param>
		public static void For<T>(this IEnumerable<T> items, Action<T> action)
		{
			if (action == null)
				return;

			foreach (var item in items.ToArray())
			{
				action(item);
			}
		}
	}
}