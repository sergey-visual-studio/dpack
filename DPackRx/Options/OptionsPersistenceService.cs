using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;

using DPackRx.Extensions;
using DPackRx.Features;
using DPackRx.Package;
using DPackRx.Services;

namespace DPackRx.Options
{
	/// <summary>
	/// Options dialog persistence service.
	/// </summary>
	public class OptionsPersistenceService : IOptionsPersistenceService
	{
		#region Fields

		private readonly ILog _log;
		private readonly IPackageService _packageService;

		private const string LOG_CATEGORY = "Options Persister";

		#endregion

		public OptionsPersistenceService(ILog log, IPackageService packageService)
		{
			_log = log;
			_packageService = packageService;
		}

		#region IOptionsPersistenceService Members

		/// <summary>
		/// Loads feature options from the storage.
		/// </summary>
		/// <param name="feature">Feature.</param>
		/// <returns>Options.</returns>
		public IDictionary<string, object> LoadOptions(KnownFeature feature)
		{
			RegistryKey featureKey;
			try
			{
				featureKey = _packageService.GetUserRegistryRootKey(feature.ToString());
			}
			catch
			{
				featureKey = null;
			}

			var options = new Dictionary<string, object>(4, StringComparer.OrdinalIgnoreCase);

			if (featureKey != null)
			{
				using (featureKey)
				{
					var names = featureKey.GetValueNames();
					for (int index = 0; index < names.Length; index++)
					{
						var name = names[index];
						var customValue = featureKey.GetValue(name);
						if (customValue != null)
							options.Add(name, customValue);
					}
				}
			}
			else
			{
				_log.LogMessage($"No {feature} options to load", LOG_CATEGORY);
			}

			return options;
		}

		/// <summary>
		/// Saves feature options to the storage.
		/// </summary>
		/// <param name="feature">Feature.</param>
		/// <param name="options">Options.</param>
		public void SaveOptions(KnownFeature feature, IDictionary<string, object> options)
		{
			if (options == null)
				return;

			RegistryKey featureKey;
			try
			{
				featureKey = _packageService.GetUserRegistryRootKey(feature.ToString());
			}
			catch
			{
				featureKey = null;
			}
			if (featureKey == null)
			{
				_log.LogMessage($"Failed to saved {feature} options", LOG_CATEGORY);
				return;
			}

			using (featureKey)
			{
				// Delete existing values first
				foreach (var value in featureKey.GetValueNames())
				{
					featureKey.DeleteValue(value);
				}

				foreach (var option in options.Keys)
				{
					var customValue = options[option];
					if (customValue != null)
						featureKey.SetValue(option, customValue);
				}
			}
		}

		/// <summary>
		/// Loads feature default options.
		/// </summary>
		/// <param name="feature">Feature.</param>
		/// <returns>Default options.</returns>
		public IDictionary<string, object> LoadDefaultOptions(KnownFeature feature)
		{
			Type type = null;
			var featureTypes = this.GetType().Assembly.GetTypes().Where(t => typeof(Feature).IsAssignableFrom(t));
			foreach (var featureType in featureTypes)
			{
				if (featureType.IsClass && !featureType.IsAbstract)
				{
					var attribs = featureType.GetCustomAttributes(typeof(KnownFeatureAttribute), false);
					if ((attribs != null) && (attribs.Length > 0) && (((KnownFeatureAttribute)attribs[0]).Feature == feature))
					{
						type = featureType;
						break;
					};
				}
			}

			var defaultAttribs = type?.GetCustomAttributes(typeof(OptionsDefaultsAttribute), false);
			if ((defaultAttribs == null) || (defaultAttribs.Length == 0))
				return null;

			var options = new Dictionary<string, object>(defaultAttribs.Length, StringComparer.OrdinalIgnoreCase);
			defaultAttribs.Cast<OptionsDefaultsAttribute>().ForEach(a => options.Add(a.OptionName, a.DefaultValue));
			return options;
		}

		/// <summary>
		/// Deletes all feature options from the storage.
		/// </summary>
		/// <param name="feature">Feature.</param>
		public void DeleteOptions(KnownFeature feature)
		{
			RegistryKey featureKey;
			try
			{
				featureKey = _packageService.GetUserRegistryRootKey(feature.ToString(), false);
			}
			catch
			{
				featureKey = null;
			}

			if (featureKey != null)
			{
				using (featureKey)
				{
					foreach (var value in featureKey.GetValueNames())
					{
						featureKey.DeleteValue(value);
					}
				}
			}
		}

		#endregion
	}
}