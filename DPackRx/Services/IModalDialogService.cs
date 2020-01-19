using System.Windows;

using DPackRx.UI;

namespace DPackRx.Services
{
	/// <summary>
	/// Modal dialogs service.
	/// </summary>
	public interface IModalDialogService
	{
		/// <summary>
		/// Display modal dialog.
		/// </summary>
		/// <typeparam name="TWindow">Dialog type.</typeparam>
		/// <typeparam name="TViewModel">View model type.</typeparam>
		/// <param name="message">Wait message.</param>
		/// <returns>Returns true if dialog's been successfully shown, or False if wait cancellation's been requested.</returns>
		bool ShowDialog<TWindow, TViewModel>(string message) where TWindow : Window where TViewModel : ViewModelBase;

		/// <summary>
		/// Display modal dialog.
		/// </summary>
		/// <typeparam name="TWindow">Dialog type.</typeparam>
		/// <typeparam name="TViewModel">View model type.</typeparam>
		/// <param name="message">Wait message.</param>
		/// <param name="argument">Optional wait argument.</param>
		/// <returns>Returns true if dialog's been successfully shown, or False if wait cancellation's been requested.</returns>
		bool ShowDialog<TWindow, TViewModel>(string message, object argument) where TWindow : Window where TViewModel : ViewModelBase;
	}
}