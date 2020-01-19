using System;
using System.ComponentModel.Design;

namespace DPackRx.Extensions
{
	/// <summary>
	/// Service container extensions.
	/// </summary>
	public static class ServiceContainerExtensions
	{
		/// <summary>
		/// Custom: adds service instance to container.
		/// </summary>
		/// <typeparam name="T">Service type.</typeparam>
		/// <param name="serviceContainer">Service container.</param>
		/// <param name="service">Service instance.</param>
		public static void AddService<T>(this IServiceContainer serviceContainer, T service)
		{
			if (service == null)
				throw new ArgumentNullException(nameof(service));

			serviceContainer.AddService(typeof(T), service);
		}

		/// <summary>
		/// Custom: adds service instance to container on demand.
		/// Service instance will be created on the first request.
		/// </summary>
		/// <typeparam name="TI">Service interface type.</typeparam>
		/// <typeparam name="TS">Service implementation type.</typeparam>
		/// <param name="serviceContainer">Service container.</param>
		/// <param name="args">Optional service constructor arguments.</param>
		public static void AddService<TI, TS>(this IServiceContainer serviceContainer, params object[] args)
		{
			if (!typeof(TI).IsAssignableFrom(typeof(TS)))
				throw new ApplicationException($"Service type {typeof(TI)} is not compatible with {typeof(TS)} type.");

			serviceContainer.AddService(typeof(TI), (sc, t) =>
			{
				if (args?.Length > 0)
					return (TI)Activator.CreateInstance(typeof(TS), args);
				else
					return (TI)Activator.CreateInstance(typeof(TS));
			});
		}
	}
}