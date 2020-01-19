using System;
using System.Collections.Generic;
using System.Diagnostics;

using DPackRx.Options;
using DPackRx.Package;
using DPackRx.Services;

namespace DPackRx.Features.SupportOptions
{
	/// <summary>
	/// Support options feature.
	/// </summary>
	[KnownFeature(KnownFeature.SupportOptions)]
	[OptionsDefaults("Logging", false)]
	public class SupportOptionsFeature : Feature, IDisposable
	{
		#region Fields

		private readonly IPackageService _packageService;
		private readonly IShellEventsService _shellEventsService;
		private readonly IShellHelperService _shellHelperService;
		private readonly IMessageService _messageService;
		private SupportOptionsFirstTimeUse _firstTimeUse;

		private const string EMAIL = "mailto:{0}?subject={1} v{2} for {3}" + 
			"&body=Please include extension log file when submitting a problem report.";

		#endregion

		public SupportOptionsFeature(IServiceProvider serviceProvider, ILog log, IOptionsService optionsService,
			IPackageService packageService, IShellEventsService shellEventsService, IShellHelperService shellHelperService,
			IMessageService messageService) : base(serviceProvider, log, optionsService)
		{
			_packageService = packageService;
			_shellEventsService = shellEventsService;
			_shellHelperService = shellHelperService;
			_messageService = messageService;
		}

		// Test constructor
		protected internal SupportOptionsFeature() : base()
		{
		}

		#region IDisposable Members

		public void Dispose()
		{
			if (_firstTimeUse != null)
			{
				_shellEventsService.UnsubscribeSolutionEvents(_firstTimeUse);
				_firstTimeUse = null;
			}
		}

		#endregion

		#region Feature Overrides

		/// <summary>
		/// One time initialization.
		/// </summary>
		public override void Initialize()
		{
			base.Initialize();

			_firstTimeUse = new SupportOptionsFirstTimeUse(this.Log, this.OptionsService, _packageService, _shellHelperService, _messageService);
			_shellEventsService.SubscribeSolutionEvents(_firstTimeUse);
		}

		/// <summary>
		/// Returns all commands.
		/// </summary>
		/// <returns>Command Ids.</returns>
		public override ICollection<int> GetCommandIds()
		{
			return new List<int>(new[] {
				CommandIDs.PROJECT_HOME,
				CommandIDs.SUPPORT_EMAIL,
				CommandIDs.OPTIONS
			});
		}

		/// <summary>
		/// Checks if command is available or not.
		/// </summary>
		/// <param name="commandId">Command Id.</param>
		/// <returns>Command status.</returns>
		public override bool IsValidContext(int commandId)
		{
			switch (commandId)
			{
				case CommandIDs.PROJECT_HOME:
				case CommandIDs.SUPPORT_EMAIL:
				case CommandIDs.OPTIONS:
					return true;
				default:
					return base.IsValidContext(commandId);
			}
		}

		/// <summary>
		/// Executes a command.
		/// </summary>
		/// <param name="commandId">Command Id.</param>
		/// <returns>Execution status.</returns>
		public override bool Execute(int commandId)
		{
			switch (commandId)
			{
				case CommandIDs.PROJECT_HOME:
					OpenUrl(_packageService.GetResourceString(IDs.URL));
					return true;
				case CommandIDs.SUPPORT_EMAIL:
					EmailUs();
					return true;
				case CommandIDs.OPTIONS:
					ShowOptionsDialog();
					return true;
				default:
					return base.Execute(commandId);
			}
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Opens a given Url using default browser.
		/// </summary>
		private void OpenUrl(string url)
		{
			if (!string.IsNullOrEmpty(url))
				Process.Start(url);
		}

		/// <summary>
		/// Opens new email using default email client.
		/// </summary>
		private void EmailUs()
		{
			var email = _packageService.GetResourceString(IDs.EMAIL);
			if (!string.IsNullOrEmpty(email))
				Process.Start(
					string.Format(EMAIL,
						email,
						_packageService.GetResourceString(IDs.PRODUCT),
						_packageService.Version,
						_packageService.VSKnownVersion));
		}

		/// <summary>
		/// Shows extension options page.
		/// </summary>
		private void ShowOptionsDialog()
		{
			_shellHelperService.ShowOptions<OptionsGeneral>();
		}

		#endregion
	}
}