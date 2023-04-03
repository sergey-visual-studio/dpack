using System.Collections.Generic;
using System.Diagnostics;

using DPackRx.Language;

namespace DPackRx.Features.SurroundWith
{
	/// <summary>
	/// Surround with model.
	/// </summary>
	[DebuggerDisplay("Count = {Models.Count)")]
	public class SurroundWithModel
	{
		public List<SurroundWithLanguageModel> Models { get; set; } = new List<SurroundWithLanguageModel>();
	}

	/// <summary>
	/// Surround with language model.
	/// </summary>
	[DebuggerDisplay("{Language}, {Type}")]
	public class SurroundWithLanguageModel
	{
		public SurroundWithLanguage Language { get; set; }

		public SurroundWithType Type { get; set; }

		public string StartingCode { get; set; }

		public string EndingCode { get; set; }

		public int WordOffset { get; set; }
	}

	/// <summary>
	/// All surround with supporting languages.
	/// </summary>
	public enum SurroundWithLanguage
	{
		None,
		CSharp = LanguageType.CSharp,
		VB = LanguageType.VB,
		CPP = LanguageType.CPP
	}

	/// <summary>
	/// Supported surround with types.
	/// </summary>
	public enum SurroundWithType
	{
		TryCatch,
		TryFinally,
		For,
		ForEach,
		Region
	}
}