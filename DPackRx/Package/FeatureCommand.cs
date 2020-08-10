using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using Microsoft.VisualStudio.Shell;

using DPackRx.Features;
using DPackRx.Services;

namespace DPackRx.Package
{
	/// <summary>
	/// Command handler.
	/// </summary>
	public class FeatureCommand : IFeatureCommand
	{
		#region Fields

		private readonly ILog _log;
		private readonly IMenuCommandService _menuCommandService;
		private readonly IMessageService _messageService;
		private readonly IUtilsService _utilsService;
		private IFeature _feature;

		private const string LOG_CATEGORY = "Command";

		#endregion

		public FeatureCommand(ILog log, IMenuCommandService menuCommandService, IMessageService messageService, IUtilsService utilsService)
		{
			_log = log;
			_menuCommandService = menuCommandService;
			_messageService = messageService;
			_utilsService = utilsService;
		}

		#region IFeatureCommand Members

		/// <summary>
		/// Command Id.
		/// </summary>
		public int CommandId { get; private set; }

		/// <summary>
		/// Feature.
		/// </summary>
		public KnownFeature Feature { get; private set; }

		/// <summary>
		/// Indicated whether command's been initialized.
		/// </summary>
		public bool Initialized { get; private set; }

		/// <summary>
		/// Initializes the command.
		/// </summary>
		public void Initialize(IFeature feature, int commandId)
		{
			if (this.Initialized)
				return;

			if (feature == null)
				throw new ArgumentNullException(nameof(feature));

			if (commandId <= 0)
				throw new ArgumentException($"Invalid command Id {commandId}");

			_feature = feature;
			this.Feature = feature.KnownFeature;
			this.CommandId = commandId;

			try
			{
				var menuCommandID = new CommandID(GUIDs.CommandSet, this.CommandId);
				var menuItem = new OleMenuCommand(Execute, menuCommandID);
				menuItem.BeforeQueryStatus += QueryStatus;
				_menuCommandService.AddCommand(menuItem);
			}
			finally
			{
				this.Initialized = true;
			}
		}

		/// <summary>
		/// Checks command availability in the current context.
		/// </summary>
		/// <returns>Whether command is valid in the current context.</returns>
		public bool IsValidContext()
		{
			if (!this.Initialized)
				return false;

			try
			{
				return _feature.IsValidContext(this.CommandId);
			}
			catch (Exception ex)
			{
				_log.LogMessage(ex, LOG_CATEGORY);
				return false;
			}
		}

		/// <summary>
		/// Executes command.
		/// </summary>
		/// <returns>Whether command's been execute successfully.</returns>
		public bool Execute()
		{
			if (!this.Initialized)
				return false;

#if BETA
			if ((this.Feature != KnownFeature.SupportOptions) && (DateTime.Now >= Beta.ExpirationDate))
			{
				_messageService.ShowError("This beta version has expired!\r\n\r\nPlease update to the latest version.", false);
				return false;
			}
#endif

			bool result;
			try
			{
				result = _feature.Execute(this.CommandId);

				if (!result)
					_utilsService.Beep();
			}
			catch (Exception ex)
			{
				_messageService.ShowError($"Error executing {_feature.Name} command # {this.CommandId}.\r\n\r\n{ex.Message}", ex);
				result = false;
			}

			return result;
		}

		#endregion

		#region Private Methods

		private void QueryStatus(object sender, EventArgs e)
		{
			var command = sender as OleMenuCommand;

			try
			{
				var valid = IsValidContext();

				command.Enabled = valid;
				command.Visible = valid;
			}
			catch (Exception ex)
			{
				command.Enabled = false;
				command.Visible = false;

				Trace.WriteLine(ex);
			}
		}

		private void Execute(object sender, EventArgs e)
		{
			try
			{
				Execute();
			}
			catch (Exception ex)
			{
				_log.LogMessage(ex, LOG_CATEGORY);
			}
		}

		#endregion
	}
}