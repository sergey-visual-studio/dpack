using System.Collections.Generic;

using DPackRx.Features;

namespace DPackRx.Options
{
	/// <summary>
	/// Options dialog persistence service.
	/// </summary>
	public interface IOptionsPersistenceService
	{
		/// <summary>
		/// Loads feature options from the storage.
		/// </summary>
		/// <param name="feature">Feature.</param>
		/// <returns>Options.</returns>
		IDictionary<string, object> LoadOptions(KnownFeature feature);

		/// <summary>
		/// Saves feature options to the storage.
		/// </summary>
		/// <param name="feature">Feature.</param>
		/// <param name="options">Options.</param>
		void SaveOptions(KnownFeature feature, IDictionary<string, object> options);

		/// <summary>
		/// Loads feature default options.
		/// </summary>
		/// <param name="feature">Feature.</param>
		/// <returns>Default options.</returns>
		IDictionary<string, object> LoadDefaultOptions(KnownFeature feature);

		/// <summary>
		/// Deletes all feature options from the storage.
		/// </summary>
		/// <param name="feature">Feature.</param>
		void DeleteOptions(KnownFeature feature);
	}
}