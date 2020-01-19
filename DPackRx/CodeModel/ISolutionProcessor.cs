using System;

namespace DPackRx.CodeModel
{
	/// <summary>
	/// Solution project processor.
	/// </summary>
	public interface ISolutionProcessor
	{
		/// <summary>
		/// Returns solution projects.
		/// </summary>
		/// <param name="flags">Processor flags.</param>
		/// <param name="filter">Code model filter.</param>
		/// <returns>Projects.</returns>
		SolutionModel GetProjects(ProcessorFlags flags, CodeModelFilterFlags filter = CodeModelFilterFlags.All);
	}

	#region ProcessorFlags enum

	/// <summary>
	/// Processor flags.
	/// </summary>
	[Flags]
	public enum ProcessorFlags
	{
		/// <summary>
		/// Whether to process known projects or all projects.
		/// </summary>
		KnownProjectsOnly = 1,
		/// <summary>
		/// Whether to include files into project processing.
		/// </summary>
		IncludeFiles = 2,
		/// <summary>
		/// Whether to process just code files instead of all files.
		/// </summary>
		IncludeCodeFilesOnly = 4,
		/// <summary>
		/// Whether to process .design files.
		/// </summary>
		IncludeDesignerFiles = 8,
		/// <summary>
		/// Whether to process solution folder files.
		/// </summary>
		IncludeSolutionFolderFiles = 16,
		/// <summary>
		/// Whether to group linked project files.
		/// </summary>
		GroupLinkedFiles = 32,
		/// <summary>
		/// Whether to process file's code model.
		/// </summary>
		IncludeFileCodeModel = 64,
		/// <summary>
		/// Whether to include file code model full member name.
		/// </summary>
		IncludeMemeberFullName = 128,
		/// <summary>
		/// Whether to gather file code model Xml doc information.
		/// </summary>
		IncludeMemberXmlDoc = 256
	}

	#endregion
}