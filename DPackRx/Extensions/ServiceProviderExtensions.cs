using System;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Shell;

namespace DPackRx.Extensions
{
	/// <summary>
	/// Service provider extensions.
	/// </summary>
	public static class ServiceProviderExtensions
	{
		/// <summary>
		/// Custom: returns an instance of service type or throws an exception if one is not available.
		/// </summary>
		/// <typeparam name="T">Service type.</typeparam>
		/// <param name="serviceProvider">Service provider.</param>
		/// <param name="throwOnError">Throw exception if service is not available or return null otherwise.</param>
		/// <returns>Service instance.</returns>
		public static T GetService<T>(this IServiceProvider serviceProvider, bool throwOnError = true)
		{
			var service = serviceProvider.GetService(typeof(T));
			if (!(service is T))
			{
				if (throwOnError)
					throw new ApplicationException($"Service of {typeof(T)} type is not available.");
				else
					return default;
			}

			return (T)service;
		}

		/// <summary>
		/// Custom: returns an instance of service type.
		/// </summary>
		/// <typeparam name="T">Service type.</typeparam>
		/// <typeparam name="TS">Service instance concrete type.</typeparam>
		/// <param name="serviceProvider">Service provider.</param>
		/// <param name="throwOnError">Throw exception if service is not available or return null otherwise.</param>
		/// <returns>Service instance.</returns>
		public static T GetService<T, TS>(this IServiceProvider serviceProvider, bool throwOnError = true)
		{
			var service = serviceProvider.GetService(typeof(TS));
			if (!(service is T))
			{
				if (throwOnError)
					throw new ApplicationException($"Service of {typeof(TS)} type is not available.");
				else
					return default;
			}

			return (T)service;
		}

		/// <summary>
		/// Custom: returns an instance of service type or throws an exception if one is not available.
		/// </summary>
		/// <typeparam name="T">Service type.</typeparam>
		/// <param name="serviceProvider">Service provider.</param>
		/// <param name="throwOnError">Throw exception if service is not available or return null otherwise.</param>
		/// <returns>Service instance.</returns>
		public static async Task<T> GetServiceAsync<T>(this IAsyncServiceProvider serviceProvider, bool throwOnError = true)
		{
			var service = await serviceProvider.GetServiceAsync(typeof(T));
			if (!(service is T))
			{
				if (throwOnError)
					throw new ApplicationException($"Service of {typeof(T)} type is not available.");
				else
					return default;
			}

			return (T)service;
		}
	}
}