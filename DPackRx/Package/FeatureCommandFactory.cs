using System;

using DPackRx.Extensions;
using DPackRx.Features;

namespace DPackRx.Package
{
	/// <summary>
	/// Feature command factory.
	/// </summary>
	public class FeatureCommandFactory : IFeatureCommandFactory
	{
		#region Fields

		private readonly IServiceProvider _serviceProvider;

		#endregion

		public FeatureCommandFactory(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		#region ICommandFactory Members

		/// <summary>
		/// Create feature command.
		/// </summary>
		public IFeatureCommand CreateCommand(IFeature feature, int commandId)
		{
			var command = _serviceProvider.GetService<IFeatureCommand>();
			command.Initialize(feature, commandId);
			return command;
		}

		#endregion
	}
}