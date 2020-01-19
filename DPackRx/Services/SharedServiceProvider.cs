using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Design;

namespace DPackRx.Services
{
	/// <summary>
	/// Service provider shared between two DI frameworks MEF and 3rd party one.
	/// For link like that to work it must provide a parameterless constructor.
	/// </summary>
	[Export(typeof(ISharedServiceProvider))] // MEF export
	public class SharedServiceProvider : ISharedServiceProvider, IDisposable
	{
		#region Properties

		protected internal IServiceContainer MEFContainer { get; set; }

		protected internal LightInject.IServiceContainer DIContainer { get; set; }

		#endregion

		#region ISharedServiceProvider Members

		/// <summary>
		/// Returns whether instance's been initialized.
		/// </summary>
		public bool IsInitialized { get; private set; }

		/// <summary>
		/// Raised when initialization's done.
		/// </summary>
		public event EventHandler Initialized;

		/// <summary>
		/// Initialize instance.
		/// </summary>
		/// <param name="mefContainer">MEF container.</param>
		/// <param name="diContainer">3rd party container.</param>
		public void Initialize(IServiceContainer mefContainer, LightInject.IServiceContainer diContainer)
		{
			this.MEFContainer = mefContainer;
			this.DIContainer = diContainer;
			this.IsInitialized = true;

			Initialized?.Invoke(this, EventArgs.Empty);
		}

		public object GetService(Type serviceType)
		{
			if (!this.IsInitialized)
				return null;

			// Check 3rd party container first followed by MEF one
			var service = this.DIContainer?.TryGetInstance(serviceType);
			if (service != null)
				return service;
			else
				return this.MEFContainer?.GetService(serviceType);
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			this.MEFContainer = null;
			this.DIContainer = null;
			this.IsInitialized = false;
		}

		#endregion
	}
}