using System;
using System.Windows;

using DPackRx.Extensions;
using DPackRx.UI;

using Microsoft.VisualStudio.PlatformUI;

namespace DPackRx.Services
{
	/// <summary>
	/// Modal dialogs service.
	/// </summary>
	public class ModalDialogService : IModalDialogService
	{
		#region Fields

		private readonly IServiceProvider _serviceProvider;
		private readonly IAsyncTaskService _asyncTaskService;

		#endregion

		public ModalDialogService(IServiceProvider serviceProvider, IAsyncTaskService asyncTaskService)
		{
			_serviceProvider = serviceProvider;
			_asyncTaskService = asyncTaskService;
		}

		#region IModalDialogService Members

		/// <summary>
		/// Display modal dialog.
		/// </summary>
		/// <typeparam name="TWindow">Dialog type.</typeparam>
		/// <typeparam name="TViewModel">View model type.</typeparam>
		/// <param name="message">Wait message.</param>
		/// <returns>Returns true if dialog's been successfully shown, or False if wait cancellation's been requested.</returns>
		public bool ShowDialog<TWindow, TViewModel>(string message) where TWindow : Window where TViewModel : ViewModelBase
		{
			return ShowDialog<TWindow, TViewModel>(message, null);
		}

		/// <summary>
		/// Display modal dialog.
		/// </summary>
		/// <typeparam name="TWindow">Dialog type.</typeparam>
		/// <typeparam name="TViewModel">View model type.</typeparam>
		/// <param name="message">Wait message.</param>
		/// <param name="argument">Optional wait argument.</param>
		/// <returns>Returns true if dialog's been successfully shown, or False if wait cancellation's been requested.</returns>
		public bool ShowDialog<TWindow, TViewModel>(string message, object argument) where TWindow : Window where TViewModel : ViewModelBase
		{
			return ShowDialog<TWindow, TViewModel, bool>(message, argument);
		}

		/// <summary>
		/// Display modal dialog.
		/// </summary>
		/// <typeparam name="TWindow">Dialog type.</typeparam>
		/// <typeparam name="TViewModel">View model type.</typeparam>
		/// <typeparam name="TResult">Result type.</typeparam>
		/// <param name="message">Wait message.</param>
		/// <param name="argument">Optional wait argument.</param>
		/// <returns>Returns true if dialog's been successfully shown, or False if wait cancellation's been requested.</returns>
		public TResult ShowDialog<TWindow, TViewModel, TResult>(string message, object argument) where TWindow : Window where TViewModel : ViewModelBase
		{
			var viewModel = _serviceProvider.GetService<TViewModel>();
			if (!_asyncTaskService.RunOnMainThread(viewModel.OnInitialize, argument, message))
				return default;

			var dialog = Activator.CreateInstance(typeof(TWindow)) as Window; // use default constructor w/o use of service container
			dialog.Owner = Application.Current?.MainWindow;
			dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
			dialog.DataContext = viewModel;
			bool dialogResult;
			if (typeof(DialogWindow).IsAssignableFrom(typeof(TWindow)))
				dialogResult = (dialog as DialogWindow).ShowModal().Value;
			else
				dialogResult = dialog.ShowDialog().Value;

			viewModel.OnClose(dialogResult);

			if (typeof(TResult) == typeof(bool))
				return (TResult)(object)dialogResult;

			if (!(viewModel is IModalDialogResult<TResult> result))
				return default;

			return result.Result;
		}

		#endregion
	}
}