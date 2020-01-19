using System.Collections.Generic;
using System.ComponentModel;

namespace DPackRx.Features
{
	/// <summary>
	/// Feature.
	/// </summary>
	public interface IFeature
	{
		/// <summary>
		/// Feature name.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Feature.
		/// </summary>
		KnownFeature KnownFeature { get; }

		/// <summary>
		/// Initialization status.
		/// </summary>
		bool Initialized { get; }

		/// <summary>
		/// One time initialization.
		/// </summary>
		void Initialize();

		/// <summary>
		/// Returns all commands.
		/// </summary>
		/// <returns>Command Ids.</returns>
		ICollection<int> GetCommandIds();

		/// <summary>
		/// Checks if command is available or not.
		/// </summary>
		/// <param name="commandId">Command Id.</param>
		/// <returns>Command status.</returns>
		bool IsValidContext(int commandId);

		/// <summary>
		/// Executes a command.
		/// </summary>
		/// <param name="commandId">Command Id.</param>
		/// <returns>Execution status.</returns>
		bool Execute(int commandId);
	}

	#region KnownFeature enum

	/// <summary>
	/// All supported features with optional description.
	/// </summary>
	public enum KnownFeature // TODO: finish other features
	{
		/// <summary>
		/// Always available and enabled feature.
		/// </summary>
		[Description("Support Options")]
		SupportOptions,

		Miscellaneous,

		[Description("Code Browser")]
		CodeBrowser,

		[Description("File Browser")]
		FileBrowser,

		Bookmarks,

		//SurroundWith,

		//SolutionStats,

		//SolutionBackup,
	}

	#endregion
}