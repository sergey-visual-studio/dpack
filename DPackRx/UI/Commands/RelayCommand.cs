using System;
using System.Windows.Input;

using DPackRx.Services;

namespace DPackRx.UI.Commands
{
	/// <summary>
	/// UI command.
	/// </summary>
	public class RelayCommand : ICommand
	{
		#region Fields

		private readonly IMessageService _messageService;
		private readonly Action<object> _execute;
		private readonly Func<object, bool> _canExecute;

		#endregion

		#region Constructors

		public RelayCommand(IMessageService messageService, Action<object> execute) : this(messageService, execute, null)
		{
		}

		public RelayCommand(IMessageService messageService, Action<object> execute, Func<object, bool> canExecute)
		{
			if (messageService == null)
				throw new ArgumentNullException(nameof(messageService));

			if (execute == null)
				throw new ArgumentNullException(nameof(execute));

			_messageService = messageService;
			_execute = execute;
			_canExecute = canExecute;
		}

		#endregion

		#region ICommand Members

		///<summary>
		/// Occurs when changes occur that affect whether or not the command should execute.
		///</summary>
		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		///<summary>
		/// Defines the method that determines whether the command can execute in its current state.
		///</summary>
		///<param name="parameter">Optional data used by the command.</param>
		///<returns>True if this command can be executed; otherwise, false.</returns>
		public bool CanExecute(object parameter)
		{
			if (_canExecute == null)
				return true;

			try
			{
				return _canExecute(parameter);
			}
			catch (Exception ex)
			{
				_messageService.ShowError(ex);
				return false;
			}
		}

		///<summary>
		/// Defines the method to be called when the command is invoked.
		///</summary>
		///<param name="parameter">Optional data used by the command.</param>
		public void Execute(object parameter)
		{
			try
			{
				_execute(parameter);
			}
			catch (Exception ex)
			{
				_messageService.ShowError(ex);
			}
		}

		#endregion
	}
}