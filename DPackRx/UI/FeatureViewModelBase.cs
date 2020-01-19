using System;
using System.Diagnostics;

using DPackRx.Features;

namespace DPackRx.UI
{
	/// <summary>
	/// Base feature ViewModel.
	/// </summary>
	public abstract class FeatureViewModelBase : ViewModelBase
	{
		public FeatureViewModelBase(KnownFeature feature, IServiceProvider serviceProvider) : base(serviceProvider)
		{
			this.Feature = feature;
		}

		#region Properties

		/// <summary>
		/// Feature.
		/// </summary>
		public KnownFeature Feature { [DebuggerStepThrough] get; private set; }

		#endregion
	}
}