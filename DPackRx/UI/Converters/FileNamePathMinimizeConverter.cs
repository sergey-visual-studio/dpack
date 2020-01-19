using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace DPackRx.UI.Converters
{
	/// <summary>
	/// Converts file name path to string that fits into a given control replacing leading omitted parts with ...
	/// </summary>
	[ValueConversion(typeof(object[]), typeof(string))]
	public class FileNamePathMinimizeConverter : MarkupExtension, IMultiValueConverter
	{
		#region Fields

		private const string FILLER = "...";

		#endregion

		#region MarkupExtension Overrides

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return this;
		}

		#endregion

		#region IMultiValueConverter Members

		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values?.Length != 7)
				return null;

			var fileName = values[0] as string;
			if (string.IsNullOrEmpty(fileName))
				return null;

			var minimizedFileName = MinimizeFileName(fileName, values);
			return minimizedFileName;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			return null;
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Minimizes file path.
		/// </summary>
		/// <param name="fileName">File name with path.</param>
		/// <param name="values">Converter input values from Xaml.</param>
		/// <returns>Minimized file name.</returns>
		private string MinimizeFileName(string fileName, object[] values)
		{
			var textWidth = GetTextWidth(fileName, values);
			var minWidth = (double)values[1];

			if (textWidth < minWidth)
				return fileName;

			var root = Path.GetPathRoot(fileName);
			var path = Path.GetDirectoryName(fileName);
			var file = Path.GetFileName(fileName);
			var sep = Path.DirectorySeparatorChar;

			if (root != string.Empty)
				path = path.Substring(root.Length);

			string newFileName;
			int index;

			while (true)
			{
				index = path.IndexOf(sep);
				if (index == -1)
					break;

				path = path.Substring(index + 1);
				newFileName = $"{root}{FILLER}{sep}{path}{sep}{file}";
				textWidth = GetTextWidth(newFileName, values);

				if (textWidth < minWidth)
					return newFileName;
			}

			return fileName;
		}

		/// <summary>
		/// Returns actual text width.
		/// </summary>
		private double GetTextWidth(string text, object[] values)
		{
			return new FormattedText(text,
				CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
				new Typeface((FontFamily)values[2], (FontStyle)values[3], (FontWeight)values[4], (FontStretch)values[5]),
				(double)values[6], Brushes.Black).Width;
		}

		#endregion
	}
}