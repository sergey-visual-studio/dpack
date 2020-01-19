using System.Collections.Generic;

namespace DPackRx.CodeModel
{
	/// <summary>
	/// Project file processor.
	/// </summary>
	public interface IProjectProcessor
	{
		/// <summary>
		/// Returns project files.
		/// </summary>
		/// <param name="project">Project. Untyped extensibility link (name matches the actual type).</param>
		/// <param name="flags">Processor flags.</param>
		/// <param name="filter">Code model filter.</param>
		/// <returns>Files.</returns>
		ICollection<FileModel> GetFiles(object project, ProcessorFlags flags, CodeModelFilterFlags filter = CodeModelFilterFlags.All);
	}
}