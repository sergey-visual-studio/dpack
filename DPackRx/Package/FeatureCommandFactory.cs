using System.ComponentModel.Design;

using DPackRx.Features;
using DPackRx.Services;

namespace DPackRx.Package
{
	/// <summary>
	/// Feature command factory.
	/// </summary>
	public class FeatureCommandFactory : IFeatureCommandFactory
	{
		#region Fields

		private readonly ILog _log;
		private readonly IMenuCommandService _menuCommandService;
		private readonly IMessageService _messageService;
		private readonly IUtilsService _utilsService;

		#endregion

		public FeatureCommandFactory(ILog log, IMenuCommandService menuCommandService, IMessageService messageService, IUtilsService utilsService)
		{
			_log = log;
			_menuCommandService = menuCommandService;
			_messageService = messageService;
			_utilsService = utilsService;
		}

		#region ICommandFactory Members

		/// <summary>
		/// Create feature command.
		/// </summary>
		public IFeatureCommand CreateCommand(IFeature feature, int commandId)
		{
			var command = new FeatureCommand(_log, _menuCommandService, _messageService, _utilsService);
			command.Initialize(feature, commandId);
			return command;
		}

		#endregion
	}
}