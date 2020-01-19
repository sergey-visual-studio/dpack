using System;
using System.ComponentModel.Design;

namespace DPackRx.Services
{
	/// <summary>
	/// Service provider shared between two DI frameworks MEF and 3rd party one.
	/// </summary>
	public interface ISharedServiceProvider : IServiceProvider
	{
		/// <summary>
		/// Returns whether instance's been initialized.
		/// </summary>
		bool IsInitialized { get; }

		/// <summary>
		/// Raised when initialization's done.
		/// </summary>
		event EventHandler Initialized;

		/// <summary>
		/// Initialize instance.
		/// </summary>
		/// <param name="mefContainer">MEF container.</param>
		/// <param name="diContainer">3rd party container.</param>
		void Initialize(IServiceContainer mefContainer, LightInject.IServiceContainer diContainer);
	}
}