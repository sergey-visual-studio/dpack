using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using DPackRx.Extensions;
using DPackRx.Features;

namespace DPackRx.Services
{
	/// <summary>
	/// Feature factory.
	/// </summary>
	public sealed class FeatureFactory : IFeatureFactory, IDisposable
	{
		#region Fields

		private readonly IServiceProvider _serviceProvider;
		private readonly ILog _log;
		private readonly IMessageService _messageService;
		private readonly ConcurrentDictionary<KnownFeature, IFeature> _features = new ConcurrentDictionary<KnownFeature, IFeature>();
		private bool _initialized;

		private const string LOG_CATEGORY = "Feature Factory";

		#endregion

		public FeatureFactory(IServiceProvider serviceProvider, ILog log, IMessageService messageService)
		{
			_serviceProvider = serviceProvider;
			_log = log;
			_messageService = messageService;
		}

		#region IDisposable Pattern

		/// <summary>
		/// Tracks whether Dispose has been called.
		/// </summary>
		private bool _disposed = false;

		public void Dispose()
		{
			Dispose(true);

			// Prevent finalization code from executing a second time
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Disposes of managed and unmanaged resources.
		/// </summary>
		/// <param name="disposing">True - the method has been called (in)directly by code.
		/// False - the method has been called by the runtime from inside the finalizer -
		/// do not reference other objects.</param>
		private void Dispose(bool disposing)
		{
			// Check to see if Dispose() has already been called
			if (!_disposed)
			{
				// Dispose all managed and unmanaged resources
				// Called upon IDE shutdown
				if (disposing)
				{
					_features?.Clear();
				}
			}

			_disposed = true;
		}

		#endregion

		#region IFeatureFactory Members

		/// <summary>
		/// Returns feature friendly name.
		/// </summary>
		/// <param name="feature">Feature.</param>
		/// <returns>Feature name.</returns>
		public string GetFeatureName(KnownFeature feature)
		{
			return feature.GetDescription();
		}

		/// <summary>
		/// Returns feature instance.
		/// </summary>
		/// <param name="feature">Feature.</param>
		/// <returns>Feature instance.</returns>
		public IFeature GetFeature(KnownFeature feature)
		{
			Initialize();

			return _features[feature];
		}

		/// <summary>
		/// Returns all features.
		/// </summary>
		/// <returns>Feature collection.</returns>
		public ICollection<IFeature> GetAllFeatures()
		{
			Initialize();

			return _features.Values.Where(f => f != null).DefaultIfEmpty().ToList();
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Initializes all enabled feature instances.
		/// </summary>
		private void Initialize()
		{
			if (_initialized)
				return;

			_features.Clear();

			// Get all feature types
			var errors = new List<string>();
			foreach (Type type in this.GetType().Assembly.GetTypes())
			{
				if ((type != typeof(Feature)) && typeof(IFeature).IsAssignableFrom(type))
				{
					var attribs = type.GetCustomAttributes(typeof(KnownFeatureAttribute), false);
					if (attribs?.Length > 0)
					{
						var feature = ((KnownFeatureAttribute)attribs[0]).Feature;
						if (!_features.ContainsKey(feature))
						{
							_log.LogMessage($"Loading {feature} feature", LOG_CATEGORY);
							var featureInstance = (IFeature)_serviceProvider.GetService(type);
							try
							{
								if (featureInstance == null)
									throw new ApplicationException($"Feature {feature} is not available/registered");

								featureInstance.Initialize();
							}
							catch (Exception ex)
							{
								_log.LogMessage($"Failed to initialize feature {feature}", ex, LOG_CATEGORY);
								errors.Add(GetFeatureName(feature));
							}

							_features.TryAdd(feature, featureInstance);
							_log.LogMessage($"Loaded {feature} feature", LOG_CATEGORY);
						}
					}
				}
			}

			if (errors.Count > 0)
				_messageService.ShowError($"Failed to initialize the following features: {string.Join(", ", errors).Trim()}");

			_initialized = true;
		}

		#endregion
	}
}