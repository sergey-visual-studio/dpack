using System;

using DPackRx.Package;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace DPackRx.Services
{
	/// <summary>
	/// Message services.
	/// </summary>
	public class MessageService : IMessageService
	{
		#region Fields

		private readonly IServiceProvider _serviceProvider;
		private readonly ILog _log;
		private readonly IPackageService _packageService;

		#endregion

		public MessageService(IServiceProvider serviceProvider, ILog log, IPackageService packageService)
		{
			_serviceProvider = serviceProvider;
			_log = log;
			_packageService = packageService;
		}

		#region IMessageService Members

		/// <summary>
		/// Displays an error message.
		/// </summary>
		/// <param name="error">Error message.</param>
		/// <param name="log">Whether to log the error or not.</param>
		public void ShowError(string error, bool log = true)
		{
			if (string.IsNullOrEmpty(error))
				throw new ArgumentNullException(nameof(error));

			ShowErrorInternal(error, null, log);
		}

		/// <summary>
		/// Displays exception error message.
		/// </summary>
		/// <param name="ex">Exception.</param>
		public void ShowError(Exception ex)
		{
			if (ex == null)
				throw new ArgumentNullException(nameof(ex));

			ShowErrorInternal(ex.Message, ex, true);
		}

		/// <summary>
		/// Displays an error message.
		/// </summary>
		/// <param name="error">Error message.</param>
		/// <param name="ex">Exception to log.</param>
		public void ShowError(string error, Exception ex)
		{
			if (string.IsNullOrEmpty(error) && (ex == null))
				throw new ArgumentNullException();

			ShowErrorInternal(error, ex, true);
		}

		/// <summary>
		/// Displays a message.
		/// </summary>
		/// <param name="message">Message.</param>
		public void ShowMessage(string message)
		{
			if (string.IsNullOrEmpty(message))
				throw new ArgumentNullException(nameof(message));

			ShowMessageInternal(message, false);
		}

		/// <summary>
		/// Displays a confirmation message.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <returns>True if Yes is selected, False otherwise.</returns>
		public bool ShowQuestion(string message)
		{
			if (string.IsNullOrEmpty(message))
				throw new ArgumentNullException(nameof(message));

			return ShowMessageInternal(message, true);
		}

		#endregion

		#region Private Methods

		private void ShowErrorInternal(string error, Exception ex, bool log)
		{
			if (string.IsNullOrEmpty(error) && (ex == null))
				throw new ArgumentNullException();

			// Always log exceptions
			if ((ex != null) && !_log.Enabled)
				_log.Enabled = true;

			var message = string.IsNullOrEmpty(error) ? ex.Message : error;
			if (ex == null)
				_log.LogMessage(message);
			else
				_log.LogMessage(message, ex);

			if (_log.Enabled && (log || (ex != null)))
				message = $"{message}\r\n\r\nError information's been logged to:\r\n\r\n{_log.FileName}";

			VsShellUtilities.ShowMessageBox(_serviceProvider, message, _packageService.ProductName,
				OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
		}

		private bool ShowMessageInternal(string message, bool question)
		{
			var icon = question ? OLEMSGICON.OLEMSGICON_QUERY : OLEMSGICON.OLEMSGICON_INFO;
			var buttons = question ? OLEMSGBUTTON.OLEMSGBUTTON_YESNO : OLEMSGBUTTON.OLEMSGBUTTON_OK;
			var selection = question ? OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_SECOND : OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST;

			var result = VsShellUtilities.ShowMessageBox(_serviceProvider, message, _packageService.ProductName, icon, buttons, selection);

			return result == (int)VSConstants.MessageBoxResult.IDYES;
		}

		#endregion
	}
}