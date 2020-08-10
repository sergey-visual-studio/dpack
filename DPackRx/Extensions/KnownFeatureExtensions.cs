using System.ComponentModel;

using DPackRx.Features;

namespace DPackRx.Extensions
{
	/// <summary>
	/// <see cref="KnownFeature"/> extensions.
	/// </summary>
	public static class KnownFeatureExtensions
	{
		/// <summary>
		/// Custom: returns feature's description.
		/// </summary>
		public static string GetDescription(this KnownFeature feature)
		{
			var attribs = typeof(KnownFeature).GetField(feature.ToString())?.GetCustomAttributes(typeof(DescriptionAttribute), false);

			return (attribs == null) || (attribs.Length == 0) ? feature.ToString() : ((DescriptionAttribute)attribs[0]).Description;
		}
	}
}