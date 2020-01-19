using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;

namespace DPackRx.UI.Behaviors
{
	/// <summary>
	/// Focuses first available child control on user control load.
	/// </summary>
	public static class UserControlFocusOnLoad
	{
		#region Properties

		/// <summary>
		/// Exposed behavior property.
		/// </summary>
		public static readonly DependencyProperty EnabledProperty =
			DependencyProperty.RegisterAttached("Enabled", typeof(bool), typeof(UserControlFocusOnLoad),
				new PropertyMetadata(false, EnabledPropertyChanged));

		#endregion

		#region Public Methods

		/// <summary>
		/// Behavior property getter.
		/// </summary>
		[AttachedPropertyBrowsableForChildren(IncludeDescendants = false)]
		[AttachedPropertyBrowsableForType(typeof(UserControl))]
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
			var userControl = obj as UserControl;
			if ((userControl != null) && !DesignerProperties.GetIsInDesignMode(userControl))
			{
				if ((bool)e.NewValue)
				{
					userControl.Loaded += Loaded;
					userControl.Unloaded += Unloaded;
				}
			}
		}

		/// <summary>
		/// User control loaded event handler.
		/// </summary>
		private static void Loaded(object sender, RoutedEventArgs e)
		{
			var userControl = e.OriginalSource as UserControl;
			if (userControl != null)
			{
				var child = GetFirstFocusableChild(userControl);
				if (child != null)
				{
					ThreadHelper.JoinableTaskFactory.Run(async delegate
					{
						await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
						Keyboard.Focus(child);
					});
				}
			}
		}

		/// <summary>
		/// User control unloaded event handler.
		/// </summary>
		private static void Unloaded(object sender, RoutedEventArgs e)
		{
			var userControl = sender as UserControl;
			if (userControl != null)
			{
				userControl.Loaded -= Loaded;
				userControl.Unloaded -= Unloaded;
			}
		}

		/// <summary>
		/// Returns first focusable UI child element.
		/// </summary>
		private static UIElement GetFirstFocusableChild(DependencyObject obj)
		{
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
			{
				DependencyObject child = VisualTreeHelper.GetChild(obj, i);
				if ((child is UIElement) && ((UIElement)child).Focusable)
					return (UIElement)child;

				child = GetFirstFocusableChild(child);
				if (child != null)
					return (UIElement)child;
			}

			return null;
		}

		#endregion
	}
}