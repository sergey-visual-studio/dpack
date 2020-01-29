using System.Collections.Generic;

using DPackRx.CodeModel;
using DPackRx.Options;

namespace DPackRx.Services
{
	/// <summary>
	/// Miscellaneous helpers shell service.
	/// </summary>
	public interface IShellHelperService
	{
		/// <summary>
		/// Returns untyped DTE instance.
		/// </summary>
		object GetDTE();

		/// <summary>
		/// Returns selected Solution Explorer item path.
		/// </summary>
		string GetSelectedItemPath();

		/// <summary>
		/// Returns selected Solution Explorer project path.
		/// </summary>
		string GetCurrentProjectPath();

		/// <summary>
		/// Selects currently open document in Solution Explorer.
		/// </summary>
		bool SelectSolutionExplorerDocument(out string documentName);

		/// <summary>
		/// Collapses all projects in Solution Explorer.
		/// </summary>
		void CollapseAllProjects();

		/// <summary>
		/// Opens a given file name editor navigating to line and column.
		/// </summary>
		bool OpenFileAt(string fileName, int line, int column);

		/// <summary>
		/// Opens given file editors.
		/// </summary>
		void OpenFiles(IEnumerable<IExtensibilityItem> files);

		/// <summary>
		/// Opens given file designers.
		/// </summary>
		void OpenDesignerFiles(IEnumerable<IExtensibilityItem> files);

		/// <summary>
		/// Assigns package shortcuts.
		/// </summary>
		bool AssignShortcuts();

		/// <summary>
		/// Shows options setting page.
		/// </summary>
		void ShowOptions<T>() where T : OptionsBase;

		/// <summary>
		/// Executes built-in command.
		/// </summary>
		/// <param name="command">Internal command name.</param>
		/// <param name="arguments">Optional command arguments.</param>
		void ExecuteCommand(string command, string arguments = null);
	}
}