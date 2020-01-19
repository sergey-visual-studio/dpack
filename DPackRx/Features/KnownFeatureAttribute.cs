using System;
using System.Diagnostics;

namespace DPackRx.Features
{
	/// <summary>
	/// Feature attribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	[DebuggerDisplay("{Feature}")]
	public class KnownFeatureAttribute : Attribute
	{
		public KnownFeatureAttribute(KnownFeature feature)
		{
			this.Feature = feature;
		}

		public KnownFeature Feature { get; private set; }
	}
}