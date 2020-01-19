using System.Diagnostics;

namespace DPackRx.CodeModel
{
	/// <summary>
	/// Project reference.
	/// </summary>
	[DebuggerDisplay("{Name}")]
	public class ProjectReference
	{
		#region Properties

		/// <summary>
		/// Reference name.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Reference path.
		/// </summary>
		public string Path { get; set; }

		/// <summary>
		/// Referencing project name.
		/// </summary>
		public string ReferencingProjectName { get; set; }


		#endregion
	}
}