using System.Collections.Generic;

namespace DPackRx.Language
{
	/// <summary>
	/// Language registration service.
	/// </summary>
	public interface ILanguageRegistrationService
	{
		/// <summary>
		/// Returns all registered languages.
		/// </summary>
		/// <returns>Languages.</returns>
		ICollection<LanguageSettings> GetLanguages();
	}
}