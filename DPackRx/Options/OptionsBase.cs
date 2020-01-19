using System;
using System.ComponentModel;
using System.Windows;
using Microsoft.VisualStudio.Shell;

using DPackRx.Extensions;
using DPackRx.Features;
using DPackRx.Package;
using DPackRx.Services;

namespace DPackRx.Options
{
	/// <summary>
	/// Base Tools|Options page.
	/// </summary>
	/// <remarks>
	/// Visual Studio options pages are created once and re-used thereafter.
	/// Descendants of this type aren't testable due to DialogPage base type constructor restrictions.
	/// </remarks>
	[DesignerCategory("Code")]
	public abstract class OptionsBase : UIElementDialogPage, INotifyPropertyChanged
	{
		#region Fields

		private UIElement _view;
		private bool _loaded;

		#endregion

		public OptionsBase(KnownFeature feature)
		{
			this.Feature = feature;
		}

		#region Properties

		/// <summary>
		/// Feature.
		/// </summary>
		public KnownFeature Feature { get; private set; }

		/// <summary>
		/// Service provider, available once page is sited.
		/// </summary>
		public IServiceProvider ServiceProvider { get; protected internal set; }

		/// <summary>
		/// Options service, available once page is sited.
		/// </summary>
		public IOptionsService OptionsService { get; protected internal set; }

		/// <summary>
		/// Message service, available once page is sited.
		/// </summary>
		public IMessageService MessageService { get; protected internal set; }

		/// <summary>
		/// Shell helper service, available once page is sited.
		/// </summary>
		public IShellHelperService ShellHelperService { get; protected internal set; }

		#endregion

		#region Abstract Methods

		/// <summary>
		/// Returns page view.
		/// </summary>
		/// <returns>View.</returns>
		protected abstract UIElement GetView();

		/// <summary>
		/// Called on page load.
		/// </summary>
		protected abstract void OnLoad();

		/// <summary>
		/// Called on page save.
		/// </summary>
		protected abstract void OnSave();

		#endregion

		#region UIElementDialogPage Overrides

		protected override UIElement Child
		{
			get
			{
				if (_view == null)
				{
					this.ServiceProvider = this.Site.GetService<IPackageService>();
					this.OptionsService = this.ServiceProvider.GetService<IOptionsService>();
					this.OptionsService.Reset += Options_Reset;
					this.MessageService = this.ServiceProvider.GetService<IMessageService>();
					this.ShellHelperService = this.ServiceProvider.GetService<IShellHelperService>();

					_view = GetView();
				}

				return _view;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (this.OptionsService != null)
				{
					this.OptionsService.Reset -= Options_Reset;
					this.OptionsService = null;
				}

				this.ServiceProvider = null;
				this.MessageService = null;
				this.ShellHelperService = null;
				_view = null;
			}
			base.Dispose(disposing);
		}

		protected override void OnActivate(CancelEventArgs e)
		{
			base.OnActivate(e);

			if (!_loaded)
			{
				_loaded = true;

				OnLoad();
			}
		}

		protected override void OnApply(PageApplyEventArgs e)
		{
			base.OnApply(e);

			if (e.ApplyBehavior == ApplyKind.Apply)
			{
				OnSave();

				this.OptionsService.Flush(this.Feature);
			}
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);

			_loaded = false;
		}

		#endregion

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		#region Protected Methods

		protected void RaisePropertyChanged(string propertyName)
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion

		#region Private Methods

		private void Options_Reset(object sender, FeatureEventArgs e)
		{
			if (e.Feature == this.Feature)
				OnLoad();
		}

		#endregion
	}
}