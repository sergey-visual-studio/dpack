using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DPackRx.CodeModel
{
	/// <summary>
	/// Solution model.
	/// </summary>
	[DebuggerDisplay("Project Count = {Projects.Count}, File Count = {Files.Count}")]
	public class SolutionModel
	{
		#region Fields

		private List<FileModel> _files = new List<FileModel>(10);

		#endregion

		#region Properties

		/// <summary>
		/// Solution file name without extension.
		/// </summary>
		public string SolutionName { get; internal set; }

		/// <summary>
		/// Projects.
		/// </summary>
		public ICollection<ProjectModel> Projects { get; } = new List<ProjectModel>(4);

		/// <summary>
		/// Consolidated files from all projects.
		/// </summary>
		public ICollection<FileModel> Files
		{
			get { return _files; }
			set
			{
				if (value != null)
					_files = value.ToList();
				else
					_files.Clear();
			}
		}

		#endregion
	}
}