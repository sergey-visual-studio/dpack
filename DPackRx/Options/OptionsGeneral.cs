using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;

using DPackRx.Extensions;
using DPackRx.Features;
using DPackRx.Package;
using DPackRx.Services;
using DPackRx.UI.Commands;

namespace DPackRx.Options
{
	/// <summary>
	/// General Tools|Options page.
	/// </summary>
	[Guid(GUIDs.OptionsGeneral)]
	[ComVisible(true)]
	[ClassInterface(ClassInterfaceType.AutoDual)]
	[ToolboxItem(false)]
	public class OptionsGeneral : OptionsBase
	{
		#region Fields

		private bool _logging;
		private ICommand _assignShortcutsCommand;
		private ICommand _loggingCommand;
		private ICommand _logFolderCommand;
		private ICommand _resetAllCommand;

		#endregion

		public OptionsGeneral() : base(KnownFeature.SupportOptions)
		{
		}

		#region Properties

		public bool LoggingEnabled
		{
			get
			{
#if BETA
				return false;
#else
				return true;
#endif
			}
		}

		public bool Logging
		{
			get { return _logging; }
			set
			{
				_logging = value;
				RaisePropertyChanged(nameof(this.Logging));
			}
		}

		#endregion

		#region Commands

		public ICommand AssignShortcutsCommand
		{
			get
			{
				if (_assignShortcutsCommand == null)
					_assignShortcutsCommand = new RelayCommand(this.MessageService, OnAssignShortcuts);

				return _assignShortcutsCommand;
			}
		}

		public ICommand LoggingCommand
		{
			get
			{
				if (_loggingCommand == null)
					_loggingCommand = new RelayCommand(this.MessageService, OnLogging);

				return _loggingCommand;
			}
		}

		public ICommand LogFolderCommand
		{
			get
			{
				if (_logFolderCommand == null)
					_logFolderCommand = new RelayCommand(this.MessageService, OnLogFolder);

				return _logFolderCommand;
			}
		}

		public ICommand ResetAllCommand
		{
			get
			{
				if (_resetAllCommand == null)
					_resetAllCommand = new RelayCommand(this.MessageService, OnResetAll);

				return _resetAllCommand;
			}
		}

		#endregion

		#region OptionsBase Overrides

		/// <summary>
		/// Returns page view.
		/// </summary>
		/// <returns>View.</returns>
		protected override UIElement GetView()
		{
			return new OptionsGeneralControl { DataContext = this };
		}

		/// <summary>
		/// Called on page load.
		/// </summary>
		protected override void OnLoad()
		{
			_logging = this.OptionsService.GetBoolOption(this.Feature, "Logging", false);
#if BETA
			if (!this.Logging)
				_logging = true;
#endif

			Refresh();
		}

		/// <summary>
		/// Called on page save.
		/// </summary>
		protected override void OnSave()
		{
			this.OptionsService.SetBoolOption(this.Feature, "Logging", _logging);

			var log = this.ServiceProvider.GetService<ILog>();
			log.Enabled = _logging;
		}

		#endregion

		#region Private Methods

		private void Refresh()
		{
			RaisePropertyChanged(nameof(this.Logging));
		}

		private void OnAssignShortcuts(object parameter)
		{
			if (this.MessageService.ShowQuestion("Would you like to assign default keyboard shortcuts?"))
			{
				if (this.ShellHelperService.AssignShortcuts())
					this.MessageService.ShowMessage("Successfully assigned default keyboard shortcuts.");
				else
					this.MessageService.ShowError("Failed to assign default keyboard shortcuts.");
			}
		}

		private void OnLogging(object obj)
		{
			this.OptionsService.SetBoolOption(this.Feature, "Logging", obj != null ? Convert.ToBoolean(obj) : false);
		}

		private void OnLogFolder(object obj)
		{
			var log = this.ServiceProvider.GetService<ILog>();
			if (!string.IsNullOrEmpty(log.FileName))
			{
				var path = Path.GetDirectoryName(log.FileName);
				if (Directory.Exists(path))
					Process.Start(path);
			}
		}

		private void OnResetAll(object obj)
		{
			if (this.MessageService.ShowQuestion("Settings changes will take effect right away.\r\nAre you sure to reset all settings?"))
			{
				this.OptionsService.ResetAll();

				this.MessageService.ShowMessage("Successfully reset all settings.");
			}
		}

		#endregion
	}
}