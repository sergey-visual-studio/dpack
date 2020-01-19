using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DPackRx.UI.Behaviors
{
	/// <summary>
	/// Selects all text in the text box on focus.
	/// </summary>
	public static class TextBoxSelectAllOnFocus
	{
		#region Properties

		/// <summary>
		/// Exposed behavior property.
		/// </summary>
		public static readonly DependencyProperty EnabledProperty =
			DependencyProperty.RegisterAttached("Enabled", typeof(bool), typeof(TextBoxSelectAllOnFocus),
				new PropertyMetadata(false, EnabledPropertyChanged));

		#endregion

		#region Public Methods

		/// <summary>
		/// Behavior property getter.
		/// </summary>
		[AttachedPropertyBrowsableForChildren(IncludeDescendants = false)]
		[AttachedPropertyBrowsableForType(typeof(TextBox))]
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
			var textBox = obj as TextBox;
			if ((textBox != null) && !DesignerProperties.GetIsInDesignMode(textBox))
			{
				if ((bool)e.NewValue)
				{
					textBox.Unloaded += Unloaded;
					textBox.GotKeyboardFocus += GotKeyboardFocus;
					textBox.PreviewMouseLeftButtonDown += PreviewMouseLeftButtonDown;
				}
			}
		}

		/// <summary>
		/// Text box unloaded event handler.
		/// </summary>
		private static void Unloaded(object sender, RoutedEventArgs e)
		{
			var textBox = sender as TextBox;
			if (textBox != null)
			{
				textBox.Unloaded -= Unloaded;
				textBox.GotKeyboardFocus -= GotKeyboardFocus;
				textBox.PreviewMouseLeftButtonDown -= PreviewMouseLeftButtonDown;
			}
		}

		/// <summary>
		/// Text box received focus event handler.
		/// </summary>
		private static void GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			var textBox = sender as TextBox;
			textBox?.SelectAll();
		}

		/// <summary>
		/// Text box left mouse button down event handler.
		/// </summary>
		private static void PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			var textBox = GetParent((DependencyObject)e.OriginalSource);

			if ((textBox != null) && !textBox.IsKeyboardFocusWithin)
			{
				textBox.Focus();
				e.Handled = true;
			}
		}

		/// <summary>
		/// Returns control's parent text box.
		/// </summary>
		private static TextBox GetParent(DependencyObject current)
		{
			current = VisualTreeHelper.GetParent(current);

			while (current != null)
			{
				if (current is TextBox)
					return (TextBox)current;

				current = VisualTreeHelper.GetParent(current);
			};

			return null;
		}

		#endregion
	}
}