using System;
using System.Collections.Generic;

namespace DPackRx.CodeModel
{
	/// <summary>
	/// File code model processor.
	/// </summary>
	public interface IFileProcessor
	{
		/// <summary>
		/// Returns whether document is valid for code model collection.
		/// </summary>
		/// <param name="document">Open document or null for active document. Untyped extensibility link (name matches the actual type).</param>
		/// <param name="projectItem">Item associated with the document. Untyped extensibility link (name matches the actual type).</param>
		/// <returns>Code model collection status.</returns>
		bool IsDocumentValid(object document, out object projectItem);

		/// <summary>
		/// Returns current document code members.
		/// </summary>
		/// <param name="flags">Processor flags.</param>
		/// <param name="filter">Code model filter.</param>
		/// <returns>Code members.</returns>
		FileCodeModel GetMembers(ProcessorFlags flags, CodeModelFilterFlags filter = CodeModelFilterFlags.All);

		/// <summary>
		/// Returns item code members.
		/// </summary>
		/// <param name="projectItem">Item. Untyped extensibility link (name matches the actual type).</param>
		/// <param name="flags">Processor flags.</param>
		/// <param name="filter">Code model filter.</param>
		/// <returns>Code members.</returns>
		FileCodeModel GetMembers(object projectItem, ProcessorFlags flags, CodeModelFilterFlags filter = CodeModelFilterFlags.All);
	}

	#region CodeModelFilterFlags enum

	/// <summary>
	/// Code model member filter.
	/// </summary>
	[Flags]
	public enum CodeModelFilterFlags
	{
		Classes = 1,
		Interfaces = 2,
		Structs = 4,
		Enums = 8,
		Methods = 16,
		Constructors = 32,
		Properties = 64,
		Fields = 128,
		Events = 256,
		Delegates = 512,
		All = Classes | Interfaces | Structs | Enums | Methods | Constructors | Properties | Fields | Events | Delegates,
		ClassesInterfaces = Classes | Interfaces,
		MethodsConstructors = Methods | Constructors
	}

	#endregion
}