using System.Collections.Generic;

using DPackRx.CodeModel;

namespace DPackRx.Services
{
	/// <summary>
	/// References shell service.
	/// </summary>
	public interface IShellReferenceService
	{
		/// <summary>
		/// Returns current project references.
		/// </summary>
		ICollection<ProjectReference> GetProjectReferences(bool selectedOnly);

		/// <summary>
		/// Adds project reference to current project.
		/// </summary>
		bool AddProjectReference(string projectName);

		/// <summary>
		/// Adds assembly references to current project.
		/// </summary>
		bool AddAssemblyReference(string fileName);
	}
}