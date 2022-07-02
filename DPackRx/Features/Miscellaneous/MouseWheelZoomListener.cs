using System.ComponentModel.Composition;

using DPackRx.Services;

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
			textView.Options.SetOptionValue<bool>(DefaultWpfViewOptions.EnableMouseWheelZoomId, false);
		}

		#endregion
	}
}