using DPackRx.Features;

namespace DPackRx.Package
{
	public interface IFeatureCommand
	{
		/// <summary>
		/// Command Id.
		/// </summary>
		int CommandId { get; }

		/// <summary>
		/// Feature.
		/// </summary>
		KnownFeature Feature { get; }

		/// <summary>
		/// Indicated whether command's been initialized.
		/// </summary>
		bool Initialized { get; }

		/// <summary>
		/// Initializes the command.
		/// </summary>
		/// <param name="feature">Command feature.</param>
		/// <param name="commandId">Command id.</param>
		void Initialize(IFeature feature, int commandId);

		/// <summary>
		/// Checks command availability in the current context.
		/// </summary>
		/// <returns>Whether command is valid in the current context.</returns>
		bool IsValidContext();

		/// <summary>
		/// Executes command.
		/// </summary>
		/// <returns>Whether command's been execute successfully.</returns>
		bool Execute();
	}
}