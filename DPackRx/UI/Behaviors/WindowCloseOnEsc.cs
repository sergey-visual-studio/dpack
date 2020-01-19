using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace DPackRx.UI.Behaviors
{
	/// <summary>
	/// Closes Window on Esc key press.
	/// </summary>
	public static class WindowCloseOnEsc
	{
		#region Properties

		/// <summary>
		/// Exposed behavior property.
		/// </summary>
		public static readonly DependencyProperty EnabledProperty =
			DependencyProperty.RegisterAttached("Enabled", typeof(bool), typeof(WindowCloseOnEsc),
				new PropertyMetadata(false, EnabledPropertyChanged));

		#endregion

		#region Public Methods

		/// <summary>
		/// Behavior property getter.
		/// </summary>
		[AttachedPropertyBrowsableForChildren(IncludeDescendants = false)]
		[AttachedPropertyBrowsableForType(typeof(Window))]
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
			var window = obj as Window;
			if ((window != null) && !DesignerProperties.GetIsInDesignMode(window))
			{
				if ((bool)e.NewValue)
				{
					window.Unloaded += Unloaded;
					window.PreviewKeyDown += PreviewKeyDown;
				}
			}
		}

		/// <summary>
		/// Windows unloaded event handler.
		/// </summary>
		private static void Unloaded(object sender, RoutedEventArgs e)
		{
			var window = sender as Window;
			if (window != null)
			{
				window.Unloaded -= Unloaded;
				window.PreviewKeyDown -= PreviewKeyDown;
			}
		}

		/// <summary>
		/// Window key down event handler.
		/// </summary>
		private static void PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape)
			{
				var window = sender as Window;
				if (window != null)
				{
					e.Handled = true;
					window.Close();
				}
			}
		}

		#endregion
	}
}