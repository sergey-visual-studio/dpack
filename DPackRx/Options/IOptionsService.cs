using System;

using DPackRx.Features;

namespace DPackRx.Options
{
	/// <summary>
	/// Options dialog service.
	/// </summary>
	public interface IOptionsService
	{
		/// <summary>
		/// Raised when options change.
		/// </summary>
		event FeatureEventHandler Changed;

		/// <summary>
		/// Raised when options are reset.
		/// </summary>
		event FeatureEventHandler Reset;

		/// <summary>
		/// Returns options count.
		/// </summary>
		/// <param name="feature">Feature.</param>
		/// <returns>Count.</returns>
		/// <remarks>Used for testing only.</remarks>
		int GetOptionCount(KnownFeature feature);

		/// <summary>
		/// Checks whether options exists.
		/// </summary>
		/// <param name="feature">Feature.</param>
		/// <param name="name">Option name.</param>
		/// <returns>Option status.</returns>
		bool OptionExists(KnownFeature feature, string name);

		/// <summary>
		/// Deletes option.
		/// </summary>
		/// <param name="feature">Feature.</param>
		/// <param name="name">Option name.</param>
		void DeleteOption(KnownFeature feature, string name);

		/// <summary>
		/// Returns option value.
		/// </summary>
		/// <param name="feature">Feature.</param>
		/// <param name="name">Option name.</param>
		/// <returns>Option value.</returns>
		object GetOption(KnownFeature feature, string name);

		/// <summary>
		/// Sets option value.
		/// </summary>
		/// <param name="feature">Feature.</param>
		/// <param name="name">Option name.</param>
		/// <param name="value">Option value.</param>
		void SetOption(KnownFeature feature, string name, object value);

		/// <summary>
		/// Returns string option value.
		/// </summary>
		/// <param name="feature">Feature.</param>
		/// <param name="name">Option name.</param>
		/// <param name="defaultValue">Default option value.</param>
		/// <returns>Option value.</returns>
		string GetStringOption(KnownFeature feature, string name, string defaultValue = null);

		/// <summary>
		/// Sets string option value.
		/// </summary>
		/// <param name="feature">Feature.</param>
		/// <param name="name">Option name.</param>
		/// <param name="value">Option value.</param>
		void SetStringOption(KnownFeature feature, string name, string value);

		/// <summary>
		/// Returns integer option value.
		/// </summary>
		/// <param name="feature">Feature.</param>
		/// <param name="name">Option name.</param>
		/// <param name="defaultValue">Default option value.</param>
		/// <returns>Option value.</returns>
		int GetIntOption(KnownFeature feature, string name, int defaultValue = 0);

		/// <summary>
		/// Sets integer option value.
		/// </summary>
		/// <param name="feature">Feature.</param>
		/// <param name="name">Option name.</param>
		/// <param name="value">Option value.</param>
		void SetIntOption(KnownFeature feature, string name, int value);

		/// <summary>
		/// Returns boolean option value.
		/// </summary>
		/// <param name="feature">Feature.</param>
		/// <param name="name">Option name.</param>
		/// <param name="defaultValue">Default option value.</param>
		/// <returns>Option value.</returns>
		bool GetBoolOption(KnownFeature feature, string name, bool defaultValue = false);

		/// <summary>
		/// Sets boolean option value.
		/// </summary>
		/// <param name="feature">Feature.</param>
		/// <param name="name">Option name.</param>
		/// <param name="value">Option value.</param>
		void SetBoolOption(KnownFeature feature, string name, bool value);

		/// <summary>
		/// Flushes option values to the storage.
		/// </summary>
		/// <param name="feature">Feature.</param>
		void Flush(KnownFeature feature);

		/// <summary>
		/// Reloads option values from the storage.
		/// </summary>
		/// <param name="feature">Feature.</param>
		void Reload(KnownFeature feature);

		/// <summary>
		/// Resets all features all options to defaults.
		/// </summary>
		void ResetAll();
	}

	public delegate void FeatureEventHandler(object sender, FeatureEventArgs e);

	#region FeatureEventArgs class

	/// <summary>
	/// Feature event arguments.
	/// </summary>
	public class FeatureEventArgs : EventArgs
	{
		public FeatureEventArgs(KnownFeature feature)
		{
			this.Feature = feature;
		}

		/// <summary>
		/// Feature.
		/// </summary>
		public KnownFeature Feature { get; private set; }
	}

	#endregion
}