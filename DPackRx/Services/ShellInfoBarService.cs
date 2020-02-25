using System;
using System.Collections.Generic;

using DPackRx.Extensions;
using DPackRx.Options;

using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace DPackRx.Services
{
	/// <summary>
	/// System InfoBar service. Accesses Visual Studio InfoBar and DTE APIs and thus is untestable.
	/// </summary>
	public class ShellInfoBarService : IShellInfoBarService, IVsInfoBarUIEvents
	{
		#region Fields

		private readonly IServiceProvider _serviceProvider;
		private readonly ILog _log;
		private readonly IMessageService _messageService;
		private readonly IShellHelperService _shellHelperService;
		private uint _cookie;
		private Action _action;

		private const string CONTEXT_ACTION = "action";
		private const string CONTEXT_OPTIONS = "options";

		#endregion

		public ShellInfoBarService(IServiceProvider serviceProvider, ILog log,
			IMessageService messageService, IShellHelperService shellHelperService)
		{
			_serviceProvider = serviceProvider;
			_log = log;
			_messageService = messageService;
			_shellHelperService = shellHelperService;
		}

		#region IShellInfoBarService Members

		/// <summary>
		/// Shows system InfoBar.
		/// </summary>
		/// <param name="message">Text message to show.</param>
		/// <param name="actionText">Action button text.</param>
		/// <param name="action">Action to execute.</param>
		/// <param name="showOption">Whether to show Options dialog access button.</param>
		public void ShowInfoBar(string message, string actionText, Action action, bool showOption)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			if (string.IsNullOrEmpty(message))
				throw new ArgumentNullException(nameof(message));

			if (string.IsNullOrEmpty(actionText))
				throw new ArgumentNullException(nameof(actionText));

			if (action == null)
				throw new ArgumentNullException(nameof(action));

			if (_cookie > 0)
			{
				_log.LogMessage($"InfoBar is already in use: {_cookie}");
				return;
			}

			var shell = _serviceProvider.GetService<IVsShell, SVsShell>();

			shell.GetProperty((int)__VSSPROPID7.VSSPROPID_MainWindowInfoBarHost, out var obj);
			var host = obj as IVsInfoBarHost;
			if (host == null)
			{
				_log.LogMessage("InfoBar host is not available");
				return;
			}

			var actionLink = new InfoBarHyperlink(actionText, CONTEXT_ACTION);
			var actions = new List<InfoBarActionItem> { actionLink };
			if (showOption)
			{
				var showOptionsLink = new InfoBarHyperlink("Options", CONTEXT_OPTIONS);
				actions.Add(showOptionsLink);
			}
			var textSpan = new InfoBarTextSpan(message);
			var textSpans = new List<InfoBarTextSpan> { textSpan };
			var infoBarModel = new InfoBarModel(textSpans, actions, KnownMonikers.StatusInformation, true);

			var factory = _serviceProvider.GetService<IVsInfoBarUIFactory, SVsInfoBarUIFactory>();
			var element = factory.CreateInfoBar(infoBarModel);
			element.Advise(this, out _cookie);
			host.AddInfoBar(element);
			_action = action;

			_log.LogMessage($"InfoBar is set: {_cookie}");
		}

		#endregion

		#region IVsInfoBarUIEvents Members

		public void OnClosed(IVsInfoBarUIElement infoBarUIElement)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			infoBarUIElement?.Unadvise(_cookie);
			_cookie = 0;
			_action = null;
		}

		public void OnActionItemClicked(IVsInfoBarUIElement infoBarUIElement, IVsInfoBarActionItem actionItem)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			try
			{
				var context = actionItem.ActionContext as string;

				if (context == CONTEXT_ACTION)
				{
					_action();
				}
				else if (context == CONTEXT_OPTIONS)
				{
					_shellHelperService.ShowOptions<OptionsGeneral>();
				}
				else
				{
					_log.LogMessage($"InfoBar invalid context: {context}");
				}
			}
			catch (Exception ex)
			{
				_messageService.ShowError(ex);
			}
		}

		#endregion
	}
}