using System.ComponentModel;
using System.Windows;

namespace DPackRx.UI.Behaviors
{
	/// <summary>
	/// Exposes Window DialogResult property.
	/// </summary>
	public static class WindowClose
	{
		#region Properties

		/// <summary>
		/// Exposed behavior property.
		/// </summary>
		public static readonly DependencyProperty DialogResultProperty =
			DependencyProperty.RegisterAttached("DialogResult", typeof(bool), typeof(WindowClose),
				new PropertyMetadata(false, DialogResultPropertyChanged));

		#endregion

		#region Public Methods

		/// <summary>
		/// Behavior property getter.
		/// </summary>
		[AttachedPropertyBrowsableForChildren(IncludeDescendants = false)]
		[AttachedPropertyBrowsableForType(typeof(Window))]
		public static bool GetDialogResult(DependencyObject obj)
		{
			return (bool)obj.GetValue(DialogResultProperty);
		}

		/// <summary>
		/// Behavior property setter.
		/// </summary>
		public static void SetDialogResult(DependencyObject obj, bool value)
		{
			obj.SetValue(DialogResultProperty, value);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Sets Window dialog result based on behavior property value.
		/// </summary>
		private static void DialogResultPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			var window = obj as Window;
			if ((window != null) && !DesignerProperties.GetIsInDesignMode(window))
			{
				window.DialogResult = (bool)e.NewValue;
			}
		}

		#endregion
	}
}