using DPackRx.Features.SurroundWith;

namespace DPackRx.Services
{
	/// <summary>
	/// Surround with formatter.
	/// </summary>
	public interface ISurroundWithFormatterService
	{
		/// <summary>
		/// Formats surround with according to provided model.
		/// </summary>
		void Format(SurroundWithLanguageModel model);
	}
}