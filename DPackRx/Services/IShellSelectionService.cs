using System.Diagnostics;

namespace DPackRx.Services
{
	/// <summary>
	/// Selection shell service.
	/// </summary>
	public interface IShellSelectionService
	{
		/// <summary>
		/// Checks whether selection context is active.
		/// </summary>
		bool IsContextActive(ContextType context);

		/// <summary>
		/// Returns active document's untyped Project instance.
		/// </summary>
		object GetActiveProject();

		/// <summary>
		/// Returns active document's untyped ProjectItem instance.
		/// </summary>
		object GetActiveItem();

		/// <summary>
		/// Returns active document's untyped Document instance.
		/// </summary>
		object GetActiveDocument();

		/// <summary>
		/// Returns active document's untyped Window instance.
		/// </summary>
		object GetActiveWindow();

		/// <summary>
		/// Returns active document's file name.
		/// </summary>
		string GetActiveFileName();

		/// <summary>
		/// Returns active document's cursor position.
		/// </summary>
		Position GetActiveFilePosition();

		/// <summary>
		/// Sets active document's cursor position.
		/// </summary>
		bool SetActiveFilePosition(int row, int column);
	}

	#region ContextType enum

	/// <summary>
	/// Selection contexts.
	/// </summary>
	public enum ContextType
	{
		SolutionExistsAndFullyLoaded,
		SolutionHasProjects,
		SolutionExists,
		CodeWindow,
		TextEditor,
		XMLTextEditor,
		CSSTextEditor,
		HTMLCodeView,
		HTMLSourceView,
		HTMLSourceEditor,
		XamlEditor,
		NewXamlEditor,
		XamlDesigner
	}

	#endregion

	#region Position struct

	[DebuggerDisplay("{Line}:{Column}")]
	public struct Position
	{
		public Position(int line, int column)
		{
			this.Line = line;
			this.Column = column;
		}

		public int Line;
		public int Column;
		public static Position Empty = new Position(0, 0);

		public bool IsEmpty()
		{
			return (this.Line <= 0) && (this.Column <= 0);
		}
	}

	#endregion
}