using System;

namespace DPackRx.Services
{
	/// <summary>
	/// Async tasks service.
	/// </summary>
	public interface IAsyncTaskService
	{
		/// <summary>
		/// Executes a given action on the main UI thread.
		/// </summary>
		/// <param name="action">Action to execute.</param>
		/// <param name="argument">Optional action argument.</param>
		/// <param name="message">Wait message.</param>
		/// <returns></returns>
		bool RunOnMainThread(Action<object> action, object argument, string message);
	}
}