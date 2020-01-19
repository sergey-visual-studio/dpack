using System;
using System.Diagnostics;

namespace DPackRx.Features.Bookmarks
{
	[DebuggerDisplay("#{Number}, {Type}, {Line}:{Column}")]
	public class Bookmark
	{
		public Bookmark(BookmarkType type, int number, int line, int column)
		{
			if (number <= 0)
				throw new ArgumentOutOfRangeException(nameof(number));

			if (line <= 0)
				throw new ArgumentOutOfRangeException(nameof(line));

			if (column <= 0)
				throw new ArgumentOutOfRangeException(nameof(column));

			this.Type = type;
			this.Number = number;
			this.Line = line;
			this.Column = column;
		}

		/// <summary>
		/// Bookmark number 1-10.
		/// </summary>
		public int Number { get; private set; }

		/// <summary>
		/// 1-based line number.
		/// </summary>
		public int Line { get; set; }

		/// <summary>
		/// 1-based column number.
		/// </summary>
		public int Column { get; set; }

		/// <summary>
		/// Bookmark type.
		/// </summary>
		public BookmarkType Type { get; private set; }
	}

	#region BookmarkType enum

	/// <summary>
	/// Bookmark type.
	/// </summary>
	[Flags]
	public enum BookmarkType
	{
		Local = 1,
		Global = 2,
		Any = Local | Global
	}

	#endregion
}