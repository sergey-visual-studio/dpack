using Microsoft.VisualStudio.PlatformUI;

namespace DPackRx.Features.FileBrowser
{
	/// <summary>
	/// Interaction logic for FileBrowserWindow.xaml
	/// </summary>
	public partial class FileBrowserWindow : DialogWindow // must use this window ancestor for VS modal dialogs.
	{
		public FileBrowserWindow()
		{
			InitializeComponent();
		}
	}
}