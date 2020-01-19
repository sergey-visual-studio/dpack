using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using DPackRx.CodeModel;

namespace DPackRx.UI.Behaviors
{
	/// <summary>
	/// Sets up list view sorter to defer item comparison to its <see cref="IMatchItem"/>.
	/// </summary>
	public static class ListViewMatchItemComparer
	{
		#region MatchItemSorter class

		/// <summary>
		/// Sort comparer that defers comparison to IMatchItem.
		/// </summary>
		private class MatchItemSorter : IComparer
		{
			public int Compare(object x, object y)
			{
				var itemX = x as IMatchItem;
				var itemY = y as IMatchItem;

				if ((itemX == null) || (itemY == null))
					return 0;

				return itemX.CompareTo(itemY);
			}
		}

		#endregion

		#region Properties

		/// <summary>
		/// Exposed behavior property.
		/// </summary>
		public static readonly DependencyProperty EnabledProperty =
			DependencyProperty.RegisterAttached("Enabled", typeof(bool), typeof(ListViewMatchItemComparer),
				new PropertyMetadata(false, EnabledPropertyChanged));

		#endregion

		#region Public Methods

		/// <summary>
		/// Behavior property getter.
		/// </summary>
		[AttachedPropertyBrowsableForChildren(IncludeDescendants = false)]
		[AttachedPropertyBrowsableForType(typeof(ListView))]
		public static bool GetEnabled(DependencyObject obj)
		{
			return (bool)obj.GetValue(EnabledProperty);
		}

		/// <summary>
		/// Behavior property setter.
		/// </summary>
		public static void SetEnabled(DependencyObject obj, bool value)
		{
			obj.SetValue(EnabledProperty, value);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Sets up event handlers on property change.
		/// </summary>
		private static void EnabledPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			var listView = obj as ListView;
			if ((listView != null) && !DesignerProperties.GetIsInDesignMode(listView))
			{
				if ((bool)e.NewValue)
				{
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
				var collectionView = CollectionViewSource.GetDefaultView(listView.ItemsSource) as ListCollectionView;
				if (collectionView != null)
					collectionView.CustomSort = new MatchItemSorter();

				// This is subject to Loaded event invocation order and may not work correctly
				if (listView.Items.Count > 0)
					listView.SelectedIndex = 0;
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
				listView.Loaded -= Loaded;
				listView.Unloaded -= Unloaded;
			}
		}

		#endregion
	}
}