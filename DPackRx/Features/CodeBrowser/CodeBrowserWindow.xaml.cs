using Microsoft.VisualStudio.PlatformUI;

namespace DPackRx.Features.CodeBrowser
{
	/// <summary>
	/// Interaction logic for CodeBrowserWindow.xaml
	/// </summary>
	public partial class CodeBrowserWindow : DialogWindow // must use this window ancestor for VS modal dialogs.
	{
		public CodeBrowserWindow()
		{
			InitializeComponent();
		}
	}
}