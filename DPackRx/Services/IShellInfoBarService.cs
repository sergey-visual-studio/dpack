using System;

namespace DPackRx.Services
{
	/// <summary>
	/// System InfoBar service.
	/// </summary>
	public interface IShellInfoBarService
	{
		/// <summary>
		/// Shows system InfoBar.
		/// </summary>
		/// <param name="message">Text message to show.</param>
		/// <param name="actionText">Action button text.</param>
		/// <param name="action">Action to execute.</param>
		/// <param name="showOption">Whether to show Options dialog access button.</param>
		void ShowInfoBar(string message, string actionText, Action action, bool showOption);
	}
}