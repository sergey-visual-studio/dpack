using System.Collections.Generic;

using DPackRx.Features;

namespace DPackRx.Services
{
	/// <summary>
	/// Feature factory.
	/// </summary>
	public interface IFeatureFactory
	{
		/// <summary>
		/// Returns feature friendly name.
		/// </summary>
		/// <param name="feature">Feature.</param>
		/// <returns>Feature name.</returns>
		string GetFeatureName(KnownFeature feature);

		/// <summary>
		/// Returns feature instance.
		/// </summary>
		/// <param name="feature">Feature.</param>
		/// <returns>Feature instance.</returns>
		IFeature GetFeature(KnownFeature feature);

		/// <summary>
		/// Returns all features.
		/// </summary>
		/// <returns>Feature collection.</returns>
		ICollection<IFeature> GetAllFeatures();
	}
}