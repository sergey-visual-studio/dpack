using System.Collections.Generic;
using System.Diagnostics;

namespace DPackRx.CodeModel
{
	[DebuggerDisplay("Member Count = {Members.Count}")]
	public class FileCodeModel
	{
		#region Properties

		/// <summary>
		/// Full file name.
		/// </summary>
		public string FileName { get; internal set; } = string.Empty;

		/// <summary>
		/// Projects.
		/// </summary>
		public ICollection<MemberCodeModel> Members { get; internal set; } = new List<MemberCodeModel>(10);

		#endregion
	}
}