using System.Collections.Generic;
using System.Diagnostics;

using DPackRx.Language;

namespace DPackRx.CodeModel
{
	/// <summary>
	/// Project model.
	/// </summary>
	[DebuggerDisplay("{Name}, {Language.FriendlyName}, File Count = {Files.Count}")]
	public class ProjectModel
	{
		#region Properties

		/// <summary>
		/// Untyped extensibility link (name matches the actual type).
		/// </summary>
		public object Project { get; set; }

		/// <summary>
		/// Project name.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Project name that takes web project into account.
		/// </summary>
		public string FriendlyName { get; set; }

		/// <summary>
		/// Project file name with path.
		/// </summary>
		public string FileName { get; set; }

		/// <summary>
		/// Project language.
		/// </summary>
		public LanguageSettings Language { get; set; }

		/// <summary>
		/// Whether project is web one.
		/// </summary>
		public bool IsWebProject { get; set; }

		/// <summary>
		/// Optional project files.
		/// </summary>
		public ICollection<FileModel> Files { get; private set; } = new List<FileModel>(10);

		#endregion
	}
}