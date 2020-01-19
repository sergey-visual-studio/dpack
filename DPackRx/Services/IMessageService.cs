using System;

namespace DPackRx.Services
{
	/// <summary>
	/// Message services.
	/// </summary>
	public interface IMessageService
	{
		/// <summary>
		/// Displays an error message.
		/// </summary>
		/// <param name="error">Error message.</param>
		/// <param name="log">Whether to log the error or not.</param>
		void ShowError(string error, bool log = true);

		/// <summary>
		/// Displays exception error message.
		/// </summary>
		/// <param name="ex">Exception.</param>
		void ShowError(Exception ex);

		/// <summary>
		/// Displays an error message.
		/// </summary>
		/// <param name="error">Error message.</param>
		/// <param name="ex">Exception to log.</param>
		void ShowError(string error, Exception ex);

		/// <summary>
		/// Displays a message.
		/// </summary>
		/// <param name="message">Message.</param>
		void ShowMessage(string message);

		/// <summary>
		/// Displays a confirmation message.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <returns>True if Yes is selected, False otherwise.</returns>
		bool ShowQuestion(string message);
	}
}