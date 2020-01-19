using DPackRx.Features;

namespace DPackRx.Package
{
	/// <summary>
	/// Feature command factory.
	/// </summary>
	public interface IFeatureCommandFactory
	{
		/// <summary>
		/// Create feature command.
		/// </summary>
		IFeatureCommand CreateCommand(IFeature feature, int commandId);
	}
}