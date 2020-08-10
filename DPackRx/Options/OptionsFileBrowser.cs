using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;

using DPackRx.Features;

namespace DPackRx.Options
{
	/// <summary>
	/// File Browser Tools|Options page.
	/// </summary>
	[Guid(Package.GUIDs.OPTIONS_FILE_BROWSER)]
	[ComVisible(true)]
	[ClassInterface(ClassInterfaceType.AutoDual)]
	[ToolboxItem(false)]
	public class OptionsFileBrowser : OptionsBase
	{
		#region Fields

		private string _ignoreFiles;
		private string _showFiles;
		private string _ignoreFolders;

		#endregion

		public OptionsFileBrowser() : base(KnownFeature.FileBrowser)
		{
		}

		#region Properties

		public string IgnoreFiles
		{
			get { return _ignoreFiles; }
			set
			{
				_ignoreFiles = value;
				RaisePropertyChanged(nameof(this.IgnoreFiles));
			}
		}

		public string ShowFiles
		{
			get { return _showFiles; }
			set
			{
				_showFiles = value;
				RaisePropertyChanged(nameof(this.ShowFiles));
			}
		}

		public string IgnoreFolders
		{
			get { return _ignoreFolders; }
			set
			{
				_ignoreFolders = value;
				RaisePropertyChanged(nameof(this.IgnoreFolders));
			}
		}

		#endregion

		#region OptionsBase Overrides

		/// <summary>
		/// Returns page view.
		/// </summary>
		/// <returns>View.</returns>
		protected override UIElement GetView()
		{
			return new OptionsFileBrowserControl { DataContext = this };
		}

		/// <summary>
		/// Called on page load.
		/// </summary>
		protected override void OnLoad()
		{
			_ignoreFiles = this.OptionsService.GetStringOption(this.Feature, "IgnoreFiles");
			_showFiles = this.OptionsService.GetStringOption(this.Feature, "ShowFiles");
			_ignoreFolders = this.OptionsService.GetStringOption(this.Feature, "IgnoreFolders");

			Refresh();
		}

		/// <summary>
		/// Called on page save.
		/// </summary>
		protected override void OnSave()
		{
			this.OptionsService.SetStringOption(this.Feature, "IgnoreFiles", _ignoreFiles);
			this.OptionsService.SetStringOption(this.Feature, "ShowFiles", _showFiles);
			this.OptionsService.SetStringOption(this.Feature, "IgnoreFolders", _ignoreFolders);
		}

		#endregion

		#region Private Methods

		private void Refresh()
		{
			RaisePropertyChanged(nameof(this.IgnoreFiles));
			RaisePropertyChanged(nameof(this.ShowFiles));
			RaisePropertyChanged(nameof(this.IgnoreFolders));
		}

		#endregion
	}
}