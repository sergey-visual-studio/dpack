using System.ComponentModel.Composition;

using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell;

namespace DPackRx.Extensions
{
	/// <summary>
	/// MEF extensions.
	/// </summary>
	public static class MEFExtensions
	{
		/// <summary>
		/// Custom: resolves/creates all MEF imports.
		/// </summary>
		public static void SatisfyImportsOnce(this object instance)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var compositionService = ServiceProvider.GlobalProvider.GetService<IComponentModel, SComponentModel>(false);
			compositionService?.DefaultCompositionService.SatisfyImportsOnce(instance);
		}
	}
}