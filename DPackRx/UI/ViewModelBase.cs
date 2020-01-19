using System;
using System.ComponentModel;

namespace DPackRx.UI
{
	/// <summary>
	/// Base ViewModel.
	/// </summary>
	public abstract class ViewModelBase : INotifyPropertyChanged
	{
		#region Fields

		private bool _closeWindow;

		#endregion

		public ViewModelBase(IServiceProvider serviceProvider)
		{
			this.ServiceProvider = serviceProvider;
		}

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		#region Properties

		/// <summary>
		/// Service provider to retrieve shell services.
		/// </summary>
		public IServiceProvider ServiceProvider { get; }

		/// <summary>
		/// Indicates that window must be closed, when and if view model's bound to window.
		/// </summary>
		public bool CloseWindow
		{
			get { return _closeWindow; }
			set
			{
				_closeWindow = value;
				RaisePropertyChanged(nameof(this.CloseWindow));
			}
		}

		#endregion

		#region Abstract Methods

		/// <summary>
		/// Initializes data model before UI's shown.
		/// </summary>
		/// <param name="argument">Optional argument.</param>
		public abstract void OnInitialize(object argument);

		/// <summary>
		/// Closes UI down.
		/// <paramref name="apply">Whether to apply or process the selection or not.</paramref>
		/// </summary>
		public abstract void OnClose(bool apply);

		#endregion

		#region Protected Methods

		protected void RaisePropertyChanged(string propertyName)
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion
	}
}