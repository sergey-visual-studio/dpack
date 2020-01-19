using System;
using System.Diagnostics;

namespace DPackRx.Options
{
	/// <summary>
	/// Default option value attribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
	[DebuggerDisplay("{OptionName} - {DefaultValue}")]
	public class OptionsDefaultsAttribute : Attribute
	{
		public OptionsDefaultsAttribute(string optionName, string defaultValue)
		{
			if (string.IsNullOrEmpty(optionName))
				throw new ArgumentNullException(nameof(optionName));

			this.OptionName = optionName;
			this.DefaultValue = defaultValue;
		}

		public OptionsDefaultsAttribute(string optionName, int defaultValue)
		{
			if (string.IsNullOrEmpty(optionName))
				throw new ArgumentNullException(nameof(optionName));

			this.OptionName = optionName;
			this.DefaultValue = defaultValue;
		}

		public OptionsDefaultsAttribute(string optionName, bool defaultValue)
		{
			if (string.IsNullOrEmpty(optionName))
				throw new ArgumentNullException(nameof(optionName));

			this.OptionName = optionName;
			this.DefaultValue = defaultValue;
		}

		public OptionsDefaultsAttribute(string optionName, Enum defaultValue)
		{
			if (string.IsNullOrEmpty(optionName))
				throw new ArgumentNullException(nameof(optionName));

			this.OptionName = optionName;
			this.DefaultValue = Convert.ToInt32(defaultValue);
		}

		#region Properties

		/// <summary>
		/// Option name.
		/// </summary>
		public string OptionName { [DebuggerStepThrough] get; private set; }

		/// <summary>
		/// Default option value.
		/// </summary>
		public object DefaultValue { [DebuggerStepThrough] get; private set; }

		#endregion
	}
}