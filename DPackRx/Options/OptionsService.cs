using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using DPackRx.Extensions;
using DPackRx.Features;
using DPackRx.Services;

namespace DPackRx.Options
{
	public class OptionsService : IOptionsService, IDisposable
	{
		#region Fields

		private readonly ILog _log;
		private readonly IOptionsPersistenceService _optionsPersistenceService;
		private readonly ConcurrentDictionary<KnownFeature, IDictionary<string, object>> _options = new ConcurrentDictionary<KnownFeature, IDictionary<string, object>>();
		private readonly ConcurrentDictionary<KnownFeature, IList<string>> _deletedOptions = new ConcurrentDictionary<KnownFeature, IList<string>>();

		private const string LOG_CATEGORY = "Options";

		#endregion

		public OptionsService(ILog log, IOptionsPersistenceService optionsPersistenceService)
		{
			_log = log;
			_optionsPersistenceService = optionsPersistenceService;
		}

		#region IDisposable Members

		public void Dispose()
		{
			if (_options != null)
			{
				_options.Keys.ForEach(f => Save(f));

				_options.Clear();
				_deletedOptions.Clear();
			}
		}

		#endregion

		#region IOptionsService Members

		/// <summary>
		/// Raised when options change.
		/// </summary>
		public event FeatureEventHandler Changed;

		/// <summary>
		/// Raised when options are reset.
		/// </summary>
		public event FeatureEventHandler Reset;

		/// <summary>
		/// Returns options count.
		/// </summary>
		/// <param name="feature">Feature.</param>
		/// <returns>Count.</returns>
		/// <remarks>Used for testing only.</remarks>
		public int GetOptionCount(KnownFeature feature)
		{
			if (_options.ContainsKey(feature))
				return _options[feature].Count;
			else
				return 0;
		}

		/// <summary>
		/// Checks whether options exists.
		/// </summary>
		/// <param name="feature">Feature.</param>
		/// <param name="name">Option name.</param>
		/// <returns>Option status.</returns>
		public bool OptionExists(KnownFeature feature, string name)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name));

			var options = GetOptions(feature);
			if (options == null)
				return false;

			return !string.IsNullOrEmpty(name) && options.ContainsKey(name);
		}

		/// <summary>
		/// Deletes option.
		/// </summary>
		/// <param name="feature">Feature.</param>
		/// <param name="name">Option name.</param>
		public void DeleteOption(KnownFeature feature, string name)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name));

			var options = GetOptions(feature, false);
			if (options == null)
				return;

			if (options.ContainsKey(name))
				options.Remove(name);

			if (!_deletedOptions.ContainsKey(feature))
				_deletedOptions.TryAdd(feature, new List<string>(2));

			var deletedOptions = _deletedOptions[feature];
			if (!deletedOptions.Contains(name, StringComparer.OrdinalIgnoreCase))
				deletedOptions.Add(name);
		}

		/// <summary>
		/// Returns option value.
		/// </summary>
		/// <param name="feature">Feature.</param>
		/// <param name="name">Option name.</param>
		/// <returns>Option value.</returns>
		public object GetOption(KnownFeature feature, string name)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name));

			var options = GetOptions(feature);
			if (options == null)
				return null;

			if (options.ContainsKey(name))
				return options[name];
			else
				return null;
		}

		/// <summary>
		/// Sets option value.
		/// </summary>
		/// <param name="feature">Feature.</param>
		/// <param name="name">Option name.</param>
		/// <param name="value">Option value.</param>
		public void SetOption(KnownFeature feature, string name, object value)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name));

			if (value is string)
				SetStringOption(feature, name, (string)value);
			else
			if (value is int)
				SetIntOption(feature, name, (int)value);
			else
			if (value is bool)
				SetBoolOption(feature, name, (bool)value);
		}

		/// <summary>
		/// Returns string option value.
		/// </summary>
		/// <param name="feature">Feature.</param>
		/// <param name="name">Option name.</param>
		/// <param name="defaultValue">Default option value.</param>
		/// <returns>Option value.</returns>
		public string GetStringOption(KnownFeature feature, string name, string defaultValue = null)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name));

			var options = GetOptions(feature);
			if ((options != null) && options.ContainsKey(name))
			{
				var option = options[name];
				if (option is string)
					return (string)option;
			}

			return defaultValue != null ? defaultValue : string.Empty;
		}

		/// <summary>
		/// Sets string option value.
		/// </summary>
		/// <param name="feature">Feature.</param>
		/// <param name="name">Option name.</param>
		/// <param name="value">Option value.</param>
		public void SetStringOption(KnownFeature feature, string name, string value)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name));

			var options = GetOptions(feature);
			options[name] = value;
		}

		/// <summary>
		/// Returns integer option value.
		/// </summary>
		/// <param name="feature">Feature.</param>
		/// <param name="name">Option name.</param>
		/// <param name="defaultValue">Default option value.</param>
		/// <returns>Option value.</returns>
		public int GetIntOption(KnownFeature feature, string name, int defaultValue = 0)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name));

			var options = GetOptions(feature);
			if ((options != null) && options.ContainsKey(name))
			{
				var option = options[name];
				if (option is int)
					return (int)option;
			}

			return defaultValue;
		}

		/// <summary>
		/// Sets integer option value.
		/// </summary>
		/// <param name="feature">Feature.</param>
		/// <param name="name">Option name.</param>
		/// <param name="value">Option value.</param>
		public void SetIntOption(KnownFeature feature, string name, int value)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name));

			var options = GetOptions(feature);
			options[name] = value;
		}

		/// <summary>
		/// Returns boolean option value.
		/// </summary>
		/// <param name="feature">Feature.</param>
		/// <param name="name">Option name.</param>
		/// <param name="defaultValue">Default option value.</param>
		/// <returns>Option value.</returns>
		public bool GetBoolOption(KnownFeature feature, string name, bool defaultValue = false)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name));

			var options = GetOptions(feature);
			if ((options != null) && options.ContainsKey(name))
			{
				var option = options[name];
				if (option is int)
					return ((int)option) == 1;
			}

			return defaultValue;
		}

		/// <summary>
		/// Sets boolean option value.
		/// </summary>
		/// <param name="feature">Feature.</param>
		/// <param name="name">Option name.</param>
		/// <param name="value">Option value.</param>
		public void SetBoolOption(KnownFeature feature, string name, bool value)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name));

			var options = GetOptions(feature);
			options[name] = Convert.ToInt32(value);
		}

		/// <summary>
		/// Flushes option values to the storage.
		/// </summary>
		/// <param name="feature">Feature.</param>
		public void Flush(KnownFeature feature)
		{
			if (Save(feature))
				RaiseChanged(feature);
		}

		/// <summary>
		/// Reloads option values from the storage.
		/// </summary>
		/// <param name="feature">Feature.</param>
		public void Reload(KnownFeature feature)
		{
			Load(feature);

			RaiseChanged(feature);
		}

		/// <summary>
		/// Resets all features all options to defaults.
		/// </summary>
		public void ResetAll()
		{
			foreach (var feature in _options.Keys)
			{
				DeleteAll(feature);

				RaiseChanged(feature);
				RaiseReset(feature);
			}

			_log.LogMessage("Reset all options", LOG_CATEGORY);
		}

		#endregion

		#region Private Methods

		private IDictionary<string, object> GetOptions(KnownFeature feature, bool createIfMissing = true)
		{
			if (!_options.ContainsKey(feature))
			{
				if (createIfMissing)
					Load(feature);
				else
					return null;
			}

			var options = _options[feature];
			return options;
		}

		/// <summary>
		/// Loads all properties from the storage.
		/// </summary>
		private void Load(KnownFeature feature)
		{
			Clear(feature);

			var options = _optionsPersistenceService.LoadOptions(feature);
			if (_options.ContainsKey(feature))
				_options.TryRemove(feature, out _);

			_options.TryAdd(feature, options);
			if (options.Count == 0)
				ApplyDefaults(feature);
		}

		/// <summary>
		/// Saves all feature properties to the storage.
		/// </summary>
		private bool Save(KnownFeature feature)
		{
			if (!_options.ContainsKey(feature))
				return false;

			if (_deletedOptions.ContainsKey(feature))
			{
				var options = _options[feature];

				foreach (var deleted in _deletedOptions[feature])
				{
					if (options.ContainsKey(deleted))
						options.Remove(deleted);
				}
			}

			_optionsPersistenceService.SaveOptions(feature, _options[feature]);
			_log.LogMessage($"Options saved: {feature.GetDescription()}", LOG_CATEGORY);

			return true;
		}

		private void Clear(KnownFeature feature)
		{
			if (_options.ContainsKey(feature))
				_options[feature].Clear();

			if (_deletedOptions.ContainsKey(feature))
				_deletedOptions[feature].Clear();
		}

		/// <summary>
		/// Deletes all properties from the storage.
		/// </summary>
		private void DeleteAll(KnownFeature feature)
		{
			if (_options.ContainsKey(feature))
				_optionsPersistenceService.DeleteOptions(feature);

			Clear(feature);
			ApplyDefaults(feature);
		}

		private void ApplyDefaults(KnownFeature feature)
		{
			if (!_options.ContainsKey(feature))
				return;

			var defaultOptions = _optionsPersistenceService.LoadDefaultOptions(feature);
			defaultOptions?.Keys.ForEach(o => SetOption(feature, o, defaultOptions[o]));
		}

		private void RaiseChanged(KnownFeature feature)
		{
			if (_options.ContainsKey(feature))
				Changed?.Invoke(this, new FeatureEventArgs(feature));
		}

		private void RaiseReset(KnownFeature feature)
		{
			if (_options.ContainsKey(feature))
				Reset?.Invoke(this, new FeatureEventArgs(feature));
		}

		#endregion
	}
}