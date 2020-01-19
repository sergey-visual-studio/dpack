using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DPackRx.UI.Behaviors
{
	/// <summary>
	/// Automatically resized designated list view column.
	/// </summary>
	public static class ListViewAutoSizeColumn
	{
		#region Fields

		private static readonly ConcurrentDictionary<ListView, int> _controls = new ConcurrentDictionary<ListView, int>();

		#endregion

		#region Properties

		/// <summary>
		/// Exposed behavior property.
		/// </summary>
		public static readonly DependencyProperty ColumnProperty =
			DependencyProperty.RegisterAttached("Column", typeof(int), typeof(ListViewAutoSizeColumn),
				new PropertyMetadata(-1, ColumnPropertyChanged));

		#endregion

		#region Public Methods

		/// <summary>
		/// Behavior property getter.
		/// </summary>
		[AttachedPropertyBrowsableForChildren(IncludeDescendants = false)]
		[AttachedPropertyBrowsableForType(typeof(ListView))]
		public static int GetColumn(DependencyObject obj)
		{
			return (int)obj.GetValue(ColumnProperty);
		}

		/// <summary>
		/// Behavior property setter.
		/// </summary>
		public static void SetColumn(DependencyObject obj, int value)
		{
			obj.SetValue(ColumnProperty, value);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Sets up event handlers on property change.
		/// </summary>
		private static void ColumnPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			var listView = obj as ListView;
			if ((listView != null) && !DesignerProperties.GetIsInDesignMode(listView))
			{
				var columnIndex = (int)e.NewValue;
				if (columnIndex >= 0)
				{
					if (!_controls.ContainsKey(listView))
						_controls.TryAdd(listView, columnIndex);

					listView.Loaded += Loaded;
					listView.Unloaded += Unloaded;
				}
			}
		}

		/// <summary>
		/// List view loaded event handler.
		/// </summary>
		private static void Loaded(object sender, RoutedEventArgs e)
		{
			var listView = sender as ListView;
			if (listView != null)
			{
				listView.SizeChanged += SizeChanged;
				ResizeColumns(listView);
			}
		}

		/// <summary>
		/// List view unloaded event handler.
		/// </summary>
		private static void Unloaded(object sender, RoutedEventArgs e)
		{
			var listView = sender as ListView;
			if (listView != null)
			{
				if (_controls.ContainsKey(listView))
					_controls.TryRemove(listView, out _);

				listView.Loaded -= Loaded;
				listView.Unloaded -= Unloaded;
			}
		}

		/// <summary>
		/// List view size changed event handler.
		/// </summary>
		private static void SizeChanged(object sender, SizeChangedEventArgs e)
		{
			var listView = sender as ListView;
			if (listView != null)
				ResizeColumns(listView);
		}

		/// <summary>
		/// Resized list view auto-size column.
		/// </summary>
		private static void ResizeColumns(ListView listView)
		{
			if (!_controls.ContainsKey(listView))
				return;

			var columnIndex = _controls[listView];
			var gridView = listView.View as GridView;

			if ((gridView == null) || (columnIndex < 0) || (columnIndex >= gridView.Columns.Count))
				return;

			var scrollWidth = SystemParameters.VerticalScrollBarWidth; // use scrollbar width regardless of whether it's visible or not
			var column = gridView.Columns[columnIndex];
			var columnsWidth = gridView.Columns.Where(c => c != column).Sum(c => c.ActualWidth);
			var separatorWidth = gridView.Columns.Count - 1;
			var offsetWidth = 4; // some arbitrary offset

			column.Width = listView.ActualWidth - columnsWidth - separatorWidth - scrollWidth - offsetWidth;
		}

		#endregion
	}
}