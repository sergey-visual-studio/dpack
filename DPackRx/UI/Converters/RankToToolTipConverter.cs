using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

using DPackRx.CodeModel;

namespace DPackRx.UI.Converters
{
	/// <summary>
	/// Converts <see cref="IMatchItem.Rank"/> to string type for Debug builds only.
	/// </summary>
	[ValueConversion(typeof(IMatchItem), typeof(object))]
	public class RankToToolTipConverter : MarkupExtension, IValueConverter
	{
		#region MarkupExtension Overrides

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return this;
		}

		#endregion

		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
#if DEBUG
			if (!(value is IMatchItem))
				return null;

			var item = (IMatchItem)value;
			return item.Rank.ToString();
#else
			return null;
#endif
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}

		#endregion
	}
}