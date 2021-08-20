using System.Collections.Generic;

namespace DPackRx.Package
{
	/// <summary>
	/// Small subset of commands for which custom key bindings are applied.
	/// </summary>
	public static class CommandBindings
	{
		#region Fields

		private static List<CommandNameAttribute> _commands;

		#endregion

		#region Public Methods

		public static ICollection<CommandNameAttribute> Commands
		{
			get
			{
				if (_commands == null)
				{
					_commands = new List<CommandNameAttribute>
					{
						new CommandNameAttribute("View.ViewCode", "F7", GUIDs.vsContextGuidHTMLSourceView),
						new CommandNameAttribute("View.ViewCode", "F7", GUIDs.vsContextGuidTextEditor),
						new CommandNameAttribute("View.ViewDesigner", "F7", GUIDs.vsContextGuidHTMLSourceView),
						new CommandNameAttribute("View.ViewDesigner", "F7", GUIDs.vsContextGuidTextEditor),
						new CommandNameAttribute("EditorContextMenus.CodeWindow.RemoveAndSort", "Ctrl+Shift+Alt+U"),
						new CommandNameAttribute("View.FindResults1", "Ctrl+K, Ctrl+1"),
						new CommandNameAttribute("View.FindResults2", "Ctrl+K, Ctrl+2"),

						// Fix bookmark shortcut collisions
						new CommandNameAttribute("Edit.GoToMember", "Ctrl+Shift+M"),
						new CommandNameAttribute("Edit.GoToType", "Ctrl+Shift+T")
					};
				}

				return _commands;
			}
		}

		#endregion
	}
}