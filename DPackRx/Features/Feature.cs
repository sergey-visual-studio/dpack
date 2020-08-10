using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using DPackRx.Extensions;
using DPackRx.Options;
using DPackRx.Services;

namespace DPackRx.Features
{
	/// <summary>
	/// Feature.
	/// </summary>
	[DebuggerDisplay("{Feature} - {GetType()}")]
	public abstract class Feature : IFeature
	{
		public Feature(IServiceProvider serviceProvider, ILog log, IOptionsService optionsService) : this()
		{
			this.ServiceProvider = serviceProvider;
			this.Log = log;
			this.OptionsService = optionsService;
		}

		// Test constructor
		protected Feature()
		{
			var feature = this.GetType().GetCustomAttributes(typeof(KnownFeatureAttribute), false).FirstOrDefault() as KnownFeatureAttribute;
			if (feature == null)
				throw new ApplicationException($"Invalid feature {this.GetType()}");

			this.KnownFeature = feature.Feature;
			this.Name = this.KnownFeature.GetDescription();
		}

		#region Properties

		/// <summary>
		/// Service provider instance.
		/// </summary>
		protected internal IServiceProvider ServiceProvider { get; private set; }

		/// <summary>
		/// Log instance.
		/// </summary>
		protected internal ILog Log { get; private set; }

		/// <summary>
		/// Options service instance.
		/// </summary>
		protected internal IOptionsService OptionsService { get; private set; }

		#endregion

		#region IFeature Members

		/// <summary>
		/// Feature name.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Feature.
		/// </summary>
		public KnownFeature KnownFeature { [DebuggerStepThrough] get; private set; }

		/// <summary>
		/// Initialization status.
		/// </summary>
		public bool Initialized { get; private set; }

		/// <summary>
		/// One time initialization.
		/// </summary>
		public virtual void Initialize()
		{
			this.Initialized = true;
		}

		/// <summary>
		/// Returns all commands.
		/// </summary>
		/// <returns>Command Ids.</returns>
		public abstract ICollection<int> GetCommandIds();

		/// <summary>
		/// Checks if command is available or not.
		/// </summary>
		/// <param name="commandId">Command Id.</param>
		/// <returns>Command status.</returns>
		public virtual bool IsValidContext(int commandId)
		{
			this.Log.LogMessage($"Feature {this.Name} - unknown command Id {commandId}", this.KnownFeature.GetDescription());
			return false;
		}

		/// <summary>
		/// Executes a command.
		/// </summary>
		/// <param name="commandId">Command Id.</param>
		/// <returns>Execution status.</returns>
		public virtual bool Execute(int commandId)
		{
			this.Log.LogMessage($"Feature {this.Name} - unknown command Id {commandId}", this.KnownFeature.GetDescription());
			return false;
		}

		#endregion
	}
}