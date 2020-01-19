using System;

namespace DPackRx.Package
{
	/// <summary>
	/// Used to define command canonical name and key binding.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class CommandNameAttribute : Attribute
	{
		public CommandNameAttribute(string name)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name));

			this.Name = name;
		}

		public CommandNameAttribute(string name, string binding, string scope = null) : this(name)
		{
			if (string.IsNullOrEmpty(binding))
				throw new ArgumentNullException(nameof(binding));

			this.Binding = binding;
			this.Scope = scope;
		}

		#region Properties

		/// <summary>
		/// Command canonical name.
		/// </summary>
		public string Name
		{
			get; private set;
		}

		/// <summary>
		/// Command key binding.
		/// </summary>
		public string Binding
		{
			get; private set;
		}

		/// <summary>
		/// Optional binding scope Guid.
		/// </summary>
		public string Scope
		{
			get; private set;
		}

		#endregion
	}
}