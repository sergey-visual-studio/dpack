using System.ComponentModel.Composition;

using DPackRx.Options;
using DPackRx.Services;
using DPackRx.Extensions;

using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace DPackRx.Features.Miscellaneous
{
	[Export(typeof (IWpfTextViewCreationListener))]
  [ContentType("text")]
  [TextViewRole(PredefinedTextViewRoles.Zoomable)]
	internal class MouseWheelZoomListener : IWpfTextViewCreationListener
  {
#pragma warning disable CS0414
		[Import]
		internal ISharedServiceProvider _serviceProvider = null;
#pragma warning restore CS0414

		#region IWpfTextViewCreationListener Members

		public void TextViewCreated(IWpfTextView textView)
		{
			var optionsService = _serviceProvider?.GetService<IOptionsService>(false);
			var mouseWheelZoom = optionsService != null && optionsService.GetBoolOption(KnownFeature.Miscellaneous, "MouseWheelZoom", false);

			textView.Options.SetOptionValue<bool>(DefaultWpfViewOptions.EnableMouseWheelZoomId, mouseWheelZoom);
		}

		#endregion
	}
}