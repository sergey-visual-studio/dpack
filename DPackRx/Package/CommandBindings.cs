using System.Collections.Generic;

using EnvDTE80;

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
					_commands = new List<CommandNameAttribute>();
					_commands.Add(new CommandNameAttribute("View.ViewCode", "F7", ContextGuids.vsContextGuidWindowsFormsDesigner));
					_commands.Add(new CommandNameAttribute("View.ViewCode", "F7", ContextGuids.vsContextGuidHTMLSourceView));
					_commands.Add(new CommandNameAttribute("View.ViewCode", "F7", ContextGuids.vsContextGuidTextEditor));
					_commands.Add(new CommandNameAttribute("View.ViewDesigner", "F7", ContextGuids.vsContextGuidWindowsFormsDesigner));
					_commands.Add(new CommandNameAttribute("View.ViewDesigner", "F7", ContextGuids.vsContextGuidHTMLSourceView));
					_commands.Add(new CommandNameAttribute("View.ViewDesigner", "F7", ContextGuids.vsContextGuidTextEditor));
					_commands.Add(new CommandNameAttribute("EditorContextMenus.CodeWindow.RemoveAndSort", "Global::Ctrl+Shift+Alt+U"));
					_commands.Add(new CommandNameAttribute("View.FindResults1", "Global::Ctrl+K, Ctrl+1"));
					_commands.Add(new CommandNameAttribute("View.FindResults2", "Global::Ctrl+K, Ctrl+2"));

					// Fix bookmark shortcut collisions
					_commands.Add(new CommandNameAttribute("Edit.GoToMember", "Global::Ctrl+Shift+M"));
					_commands.Add(new CommandNameAttribute("Edit.GoToType", "Global::Ctrl+Shift+T"));
				}

				return _commands;
			}
		}

		#endregion
	}
}