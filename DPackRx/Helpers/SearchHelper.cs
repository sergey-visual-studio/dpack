using System.Linq;
using System.Text;

namespace DPackRx.Helpers
{
	/// <summary>
	/// Search helper.
	/// </summary>
	public static class SearchHelper
	{
		#region Fields

		private static readonly char[] _suffixChars = new[] { '<', '(' };

		#endregion

		#region Public Methods

		/// <summary>
		/// Returns a Pascal cased string.
		/// For instance:
		/// 'ThisIsSomeText' would result in 'TISM'
		/// 'SomeType.SomeMember' would result in 'ST.SM'
		/// 'SomeFolder\SomeFileName.Extension' would result in 'SM\SFN.E'
		/// </summary>
		/// <param name="data">String to process.</param>
		/// <returns>Pascal cased string.</returns>
		public static string GetPascalCasedString(string data)
		{
			if (string.IsNullOrEmpty(data))
				return string.Empty;

			var result = string.Empty;
			var processed = false;

			// Do some caching for larger strings
			StringBuilder sb = null;
			if (data.Length >= 10)
				sb = new StringBuilder(10);

			// Get first character of either all uppercase or all lowercase
			// underscore-separated string
			if (data.Length > 1)
			{
				var anyLowerCase = data.Any(c => char.IsLower(c));
				var anyUpperCase = data.Any(c => char.IsUpper(c));
				if (!(anyLowerCase && anyUpperCase))
				{
					var items = data.Split('_');
					if (items.Length > 1)
					{
						foreach (var item in items)
						{
							if (item.Length > 0)
							{
								var chr = item[0];
								if (sb != null)
									sb.Append(chr);
								else
									result = result + chr;
							}
						}
						processed = true;
					}
				}
			}

			// Regular camel case string fall back processing
			if (!processed)
			{
				for (var index = 0; index < data.Length; index++)
				{
					var chr = data[index];

					var suffixChar = false;
					for (int suffixIndex = 0; suffixIndex < _suffixChars.Length; suffixIndex++)
					{
						if (_suffixChars[suffixIndex] == chr)
						{
							suffixChar = true;
							break;
						}
					}
					if (suffixChar)
						break;

					if (char.IsUpper(chr)) // || (chr == '.') || (chr == System.IO.Path.DirectorySeparatorChar))
					{
						if (sb != null)
							sb.Append(chr);
						else
							result = result + chr;
					}
				}
			}

			if (sb != null)
				result = sb.ToString();

			return result;
		}

		#endregion
	}
}