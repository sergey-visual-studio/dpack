using System;

using DPackRx.Package;

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;

namespace DPackRx.Services
{
	/// <summary>
	/// Async tasks service.
	/// </summary>
	public class AsyncTaskService : IAsyncTaskService
	{
		#region Fields

		private readonly IPackageService _packageService;
		private readonly IMessageService _messageService;
		private readonly JoinableTaskFactory _taskFactory;

		/// <summary>
		/// Minimum delay that is not perceived as long wait.
		/// </summary>
		private const double DELAY_MSCES = 750;

		#endregion

		public AsyncTaskService(IPackageService packageService, IMessageService messageService, JoinableTaskFactory taskFactory)
		{
			_packageService = packageService;
			_messageService = messageService;
			_taskFactory = taskFactory;
		}

		#region IAsyncTaskService Members

		/// <summary>
		/// Executes a given action on the main UI thread.
		/// </summary>
		/// <param name="action">Action to execute.</param>
		/// <param name="argument">Optional action argument.</param>
		/// <param name="message">Wait message.</param>
		/// <returns></returns>
		public bool RunOnMainThread(Action<object> action, object argument, string message)
		{
			if (action == null)
				throw new ArgumentNullException(nameof(action));

			var cancelled = false;

			_taskFactory.Run(
				_packageService.ProductName,
				async (progress, cancellationToken) =>
				{
					await _taskFactory.SwitchToMainThreadAsync(cancellationToken);

					progress.Report(new ThreadedWaitDialogProgressData(message, isCancelable: true));
					try
					{
						action(argument);

						cancelled = cancellationToken.IsCancellationRequested;
					}
					catch (Exception ex)
					{
						_messageService.ShowError(ex);
						cancelled = true;
					}
				},
				TimeSpan.FromMilliseconds(DELAY_MSCES));

			return !cancelled;
		}

		#endregion
	}
}