using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using DPackRx.CodeModel;
using DPackRx.Extensions;
using DPackRx.Services;

namespace DPackRx.UI.Behaviors
{
	/// <summary>
	/// Redirects some text box key actions to associated list view control.
	/// </summary>
	public static class TextBoxInputRedirectToListView
	{
		#region Fields

		private static readonly ConcurrentDictionary<TextBox, string> _controlNames = new ConcurrentDictionary<TextBox, string>();
		private static readonly ConcurrentDictionary<TextBox, ListView> _controls = new ConcurrentDictionary<TextBox, ListView>();

		#endregion

		#region Properties

		/// <summary>
		/// Exposed behavior property.
		/// </summary>
		public static readonly DependencyProperty ControlProperty =
			DependencyProperty.RegisterAttached("Control", typeof(string), typeof(TextBoxInputRedirectToListView),
				new PropertyMetadata(null, ControlPropertyChanged));

		#endregion

		#region Public Methods

		/// <summary>
		/// Behavior property getter.
		/// </summary>
		[AttachedPropertyBrowsableForChildren(IncludeDescendants = false)]
		[AttachedPropertyBrowsableForType(typeof(TextBox))]
		public static string GetControl(DependencyObject obj)
		{
			return (string)obj.GetValue(ControlProperty);
		}

		/// <summary>
		/// Behavior property setter.
		/// </summary>
		public static void SetControl(DependencyObject obj, string value)
		{
			obj.SetValue(ControlProperty, value);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Sets up event handlers on property change.
		/// </summary>
		private static void ControlPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			var textBox = obj as TextBox;
			if ((textBox != null) && !DesignerProperties.GetIsInDesignMode(textBox))
			{
				var name = (string)e.NewValue;
				if (!string.IsNullOrEmpty(name))
				{
					if (!_controlNames.ContainsKey(textBox))
						_controlNames.TryAdd(textBox, name);

					textBox.Loaded += Loaded;
					textBox.Unloaded += Unloaded;
					textBox.PreviewKeyDown += PreviewKeyDown;
				}
			}
		}

		/// <summary>
		/// Text box loaded event handler.
		/// </summary>
		private static void Loaded(object sender, RoutedEventArgs e)
		{
			var textBox = sender as TextBox;
			if (textBox != null)
			{
				var parent = GetParent(textBox);
				if (parent != null)
				{
					var name = _controlNames[textBox];
					var control = GetChildByName(parent, name);
					if (control != null)
						_controls.TryAdd(textBox, control);
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
				if (_controlNames.ContainsKey(textBox))
					_controlNames.TryRemove(textBox, out _);

				if (_controls.ContainsKey(textBox))
					_controls.TryRemove(textBox, out _);

				textBox.Loaded -= Loaded;
				textBox.Unloaded -= Unloaded;
				textBox.PreviewKeyDown -= PreviewKeyDown;
			}
		}

		/// <summary>
		/// Text box key down event handler.
		/// </summary>
		private static void PreviewKeyDown(object sender, KeyEventArgs e)
		{
			var textBox = sender as TextBox;
			if (textBox != null)
			{
				try
				{
					var redirected = false;

					var control = _controls.ContainsKey(textBox) ? _controls[textBox] : null;
					if ((control != null) && (control.Items.Count > 0)) // redirect input
					{
						var systemKeyDown =
							Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift) ||
							Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl) ||
							Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt);
						var shiftKeyDown = (control.SelectionMode != SelectionMode.Single) &&
							(Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) && (
							!Keyboard.IsKeyDown(Key.LeftCtrl) || !Keyboard.IsKeyDown(Key.RightCtrl) ||
							!Keyboard.IsKeyDown(Key.LeftAlt) || !Keyboard.IsKeyDown(Key.RightAlt));

						if (!systemKeyDown || shiftKeyDown) // redirect here; Shift key is used to add to selection
						{
							var viewItemIndex = -1;

							switch (e.Key)
							{
								case Key.Up:
									redirected = true;
									if (shiftKeyDown) // get first selected item index and select one preceding it
									{
										var indexes = new List<int>(control.SelectedItems.Count + 1);
										foreach (var item in control.SelectedItems)
										{
											indexes.Add(control.Items.IndexOf(item));
										}
										var index = indexes.Min();
										if (index > 0)
										{
											control.SelectedItems.Add(control.Items[index - 1]);
											viewItemIndex = index - 1;
										}
									}
									else // select previous item, or clear selection if more than 1 item's selected
									{
										if (control.SelectedIndex > 0)
										{
											if (control.SelectedItems.Count > 1)
											{
												var index = control.SelectedIndex;
												control.SelectedItems.Clear();
												control.SelectedIndex = index;
											}
											else
											{
												control.SelectedIndex--;
											}
											viewItemIndex = control.SelectedIndex;
										}
									}
									break;
								case Key.Down:
									redirected = true;
									if (shiftKeyDown) // get last selected item index and select one following it
									{
										var indexes = new List<int>(control.SelectedItems.Count + 1);
										foreach (var item in control.SelectedItems)
										{
											indexes.Add(control.Items.IndexOf(item));
										}
										var index = indexes.Max();
										if (index < control.Items.Count - 1)
										{
											control.SelectedItems.Add(control.Items[index + 1]);
											viewItemIndex = index + 1;
										}
									}
									else // select next item, or clear selection if more than 1 item's selected
									{
										if (control.SelectedIndex < control.Items.Count - 1)
										{
											if (control.SelectedItems.Count > 1)
											{
												var index = control.SelectedIndex;
												control.SelectedItems.Clear();
												control.SelectedIndex = index;
											}
											else
											{
												control.SelectedIndex++;
											}
											viewItemIndex = control.SelectedIndex;
										}
									}
									break;
								case Key.Home:
									if (!shiftKeyDown)
									{
										redirected = true;
										control.SelectedIndex = 0;
										viewItemIndex = control.SelectedIndex;
									}
									break;
								case Key.End:
									if (!shiftKeyDown)
									{
										redirected = true;
										control.SelectedIndex = control.Items.Count - 1;
										viewItemIndex = control.SelectedIndex;
									}
									break;
								case Key.PageUp:
								case Key.PageDown:
									var scrollViewer = control.GetChild<ScrollViewer>();
									if (scrollViewer != null)
									{
										redirected = true;
										if (e.Key == Key.PageUp)
											scrollViewer.PageUp();
										else
											scrollViewer.PageDown();
										control.UpdateLayout(); // the only way I could figure out for page up/down to register

										var panel = control.GetChild<VirtualizingStackPanel>();
										if (panel != null)
										{
											viewItemIndex = (int)panel.VerticalOffset;

											if (e.Key == Key.PageUp)
											{
												if (viewItemIndex == control.SelectedIndex)
													viewItemIndex = 0;
											}
											else
											{
												if (viewItemIndex <= control.SelectedIndex)
													viewItemIndex = control.Items.Count - 1;
											}

											if ((viewItemIndex >= 0) && (viewItemIndex <= control.Items.Count - 1))
												control.SelectedIndex = viewItemIndex;
										}
									}
									break;
							} // switch

							if (redirected && (viewItemIndex >= 0) && (viewItemIndex <= control.Items.Count - 1))
								control.ScrollIntoView(control.Items[viewItemIndex]);

							e.Handled = redirected;
						}
						else if (e.Key == Key.C) // Ctrl-C handling
						{
							var controlKeyDown =
								Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl) && (
								!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift) &&
								!Keyboard.IsKeyDown(Key.LeftAlt) && !Keyboard.IsKeyDown(Key.RightAlt));
							if (controlKeyDown)
							{
								var item = control.SelectedItem as IExtensibilityItem;
								if (item != null)
								{
									var serviceProvider = (textBox.DataContext as ViewModelBase)?.ServiceProvider;
									var utilsService = serviceProvider.GetService<IUtilsService>(false);
									var shellStatusBarService = serviceProvider.GetService<IShellStatusBarService>(false);

									if ((utilsService != null) && (shellStatusBarService != null))
									{
										var name = Path.GetFileName(item.Name);
										utilsService.SetClipboardData(name);
										shellStatusBarService.SetStatusBarText($"'{name}' copied to the clipboard");

										redirected = true;
										e.Handled = redirected;
									}
								}
							}
						}
						else if ((e.Key == Key.A) && (control.SelectionMode != SelectionMode.Single)) // Ctrl-A handling
						{
							var controlKeyDown =
								Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl) && (
								!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift) &&
								!Keyboard.IsKeyDown(Key.LeftAlt) && !Keyboard.IsKeyDown(Key.RightAlt));
							if (controlKeyDown)
							{
								control.SelectAll();

								redirected = true;
								e.Handled = redirected;
							}
						}
					}
				}
				catch (Exception ex)
				{
					e.Handled = true;

					var messageService = (textBox.DataContext as ViewModelBase)?.ServiceProvider.GetService<IMessageService>(false);
					messageService?.ShowError($"Unhandled exception: {ex.Message}", ex);
				}
			}
		}

		/// <summary>
		/// Returns control's parent user control.
		/// </summary>
		private static UserControl GetParent(DependencyObject current)
		{
			current = VisualTreeHelper.GetParent(current);

			while (current != null)
			{
				if (current is UserControl)
					return (UserControl)current;

				current = VisualTreeHelper.GetParent(current);
			};

			return null;
		}

		/// <summary>
		/// Returns child control by its name.
		/// </summary>
		private static ListView GetChildByName(DependencyObject obj, string name)
		{
			if (!string.IsNullOrEmpty(name))
			{
				for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
				{
					DependencyObject child = VisualTreeHelper.GetChild(obj, i);
					if ((child is ListView) && ((ListView)child).Name == name)
						return (ListView)child;

					child = GetChildByName(child, name);
					if (child != null)
						return (ListView)child;
				}
			}

			return null;
		}

		#endregion
	}
}