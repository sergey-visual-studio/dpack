using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Resources;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using DPackRx.CodeModel;
using DPackRx.Extensions;
using DPackRx.Features.Bookmarks;
using DPackRx.Language;
using DPackRx.Options;
using DPackRx.Package;
using DPackRx.Package.Registration;
using DPackRx.Services;
using LightInject;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using IAsyncServiceProvider = Microsoft.VisualStudio.Shell.IAsyncServiceProvider;
using SystemIMenuCommandService = System.ComponentModel.Design.IMenuCommandService;
using Task = System.Threading.Tasks.Task;

namespace DPackRx
{
	/// <summary>
	/// Main package.
	/// </summary>
	#region Package Attributes
	[PackageRegistration(UseManagedResourcesOnly = true, RegisterUsing = RegistrationMethod.CodeBase, AllowsBackgroundLoading = true)]
	[InstalledProductRegistrationEx("#101", "#102", 400)] // Help|About information
	[Guid(GUIDs.ProductID)]
	[ProvideMenuResource("Menus.ctmenu", 1)]
	// Auto-load context
	[ProvideAutoLoad(UIContextGuids.NoSolution, PackageAutoLoadFlags.BackgroundLoad)]
	[ProvideAutoLoad(UIContextGuids.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
	[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string, PackageAutoLoadFlags.BackgroundLoad)]
	// Options
	[ProvideOptionPage(typeof(OptionsGeneral), "DPack Rx", "General", 0, 0, false)]
	[ProvideOptionPage(typeof(OptionsFileBrowser), "DPack Rx", "Browsers\\File Browser", 0, 0, false)]
	// Languages
	[ProvideLanguage(
		"#101", EnvDTE.CodeModelLanguageConstants.vsCMLanguageCSharp, "C#", new[] { "cs" },
		WebName = "Visual C#", WebLanguage = "CSharpCodeProvider", Comments = new[] { "//", "/*" }, XmlDoc = "/doc/summary",
		DesignerFiles = LanguageDesignerFiles.FullySupported, Imports = LanguageImports.Supported)]
	[ProvideLanguage(
		"#101", EnvDTE.CodeModelLanguageConstants.vsCMLanguageVB, "VB", new[] { "bas", "vb", "frm" },
		WebName = "Visual Basic", WebLanguage = "VBCodeProvider", Comments = new[] { "'" }, XmlDoc = "/summary", XmlDocSurround = true,
		DesignerFiles = LanguageDesignerFiles.FullySupported, Imports = LanguageImports.Supported)]
	[ProvideLanguage(
		"#101", EnvDTE.CodeModelLanguageConstants.vsCMLanguageVC, "C++", new[] { "c", "cpp", "h", "hpp", "inl", "cc", "hxx", "hh" },
		Comments = new[] { "//", "/*" }, XmlDoc = "/summary,", XmlDocSurround = true,
		CheckDuplicateNames = true, IgnoreCodeType = true, ParentlessFullName = true)]
	[ProvideLanguage(
		"#101", LanguageConsts.vsLanguageJavaScript, "JavaScript", new[] { "js", "aspx", "ascx", "html", "htm", "asp", "master", "cshtml", "vbhtml" },
		Comments = new[] { "//", "/*" }, SmartFormat = false)]
	[ProvideLanguage("#101", LanguageConsts.vsLanguageXml, "Xml", new[] { "xml", "config", "targets", "vsct" }, Comments = new[] { "!--" })]
	[ProvideLanguage("#101", LanguageConsts.vsLanguageSolutionItems, "Solution Items", new string[0])]
	#endregion
	public sealed class DPackRx : AsyncPackage
	{
		#region Fields

		private IServiceContainer _container;

		[Import]
		private ISharedServiceProvider _sharedServiceProvider = null;

		#endregion

		#region Package Members

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_container?.Dispose();
				_container = null;
			}

			base.Dispose(disposing);
		}

		/// <summary>
		/// Initialization of the package after it's been sited.
		/// </summary>
		protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
		{
			await base.InitializeAsync(cancellationToken, progress);
			await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

			try
			{
				if (await ConfigureServicesAsync(cancellationToken))
				{
					// Enable logging if need be as soon as possible
					var log = _container.GetInstance<ILog>();
#if BETA
					log.Enabled = true;
#else
					var optionsService = _container.GetInstance<IOptionsService>();

					log.Enabled = optionsService.GetBoolOption(Features.KnownFeature.SupportOptions, "Logging");
#endif

					var featureFactory = _container.GetInstance<IFeatureFactory>();
					var commandFactory = _container.GetInstance<IFeatureCommandFactory>();
					foreach (var feature in featureFactory.GetAllFeatures())
					{
						foreach (var commandId in feature.GetCommandIds())
						{
							var command = commandFactory.CreateCommand(feature, commandId);
							if (command == null)
								throw new ApplicationException($"Failed to create {feature.Name} command {commandId}");
						}
					}

					PreloadReferences();

					Initialized();
				}
			}
			catch (Exception ex)
			{
				var log = _container.TryGetInstance<ILog>();
				log?.LogMessage($"{nameof(InitializeAsync)} error", ex);
			}
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Preloads some of our required references.
		/// </summary>
		private void PreloadReferences()
		{
			try
			{
				// Simply referencing type here causes its assembly to be preloaded
				// Otherwise WPF cannot resolve assembly in question on its own
				var type = typeof(System.Windows.Interactivity.Interaction);
				if (type == null)
					return;
			}
			finally
			{
				var log = _container.TryGetInstance<ILog>();
				log?.LogMessage("Preloaded references");
			}
		}

		/// <summary>
		/// Post initialization.
		/// </summary>
		private void Initialized()
		{
			var shellEventsService = _container.GetInstance<IShellEventsService>();
			shellEventsService.NotifySolutionOpened();
		}

		/// <summary>
		/// Configures global service container.
		/// </summary>
		private async Task<bool> ConfigureServicesAsync(CancellationToken cancellationToken)
		{
			await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

			_container = new ServiceContainer();
			try
			{
				// Here's where MEF and 3rd party DI are linked
				if (_sharedServiceProvider == null)
					this.SatisfyImportsOnce();
				if (_sharedServiceProvider == null)
				{
					Trace.TraceError("Shared service provider could not be resolved");
					_sharedServiceProvider = new SharedServiceProvider(); // DI setup is gonna fail without it
				}

				var menuCommandService = await this.GetServiceAsync<SystemIMenuCommandService>();

				this.AddService(typeof(IServiceProvider), async (container, token, serviceType) =>
				{
					await this.JoinableTaskFactory.SwitchToMainThreadAsync(token);
					return _container;
				}); // expose DI factory itself
				this.AddService(typeof(IPackageService), async (container, token, serviceType) =>
				{
					await this.JoinableTaskFactory.SwitchToMainThreadAsync(token);
					return _container.TryGetInstance<IPackageService>();
				}, true); // promote our own service
									// Global services
				_container.Register<IAsyncServiceProvider>((factory) => this, new PerContainerLifetime()); // used for shell services resolution
				_container.Register<IServiceProvider>((factory) => _sharedServiceProvider, new PerContainerLifetime()); // used for shell services resolution
				_container.Register<IPackageService, PackageService>(new PerContainerLifetime());
				_container.Register<ILog, Log>(new PerContainerLifetime());
				_container.Register<SystemIMenuCommandService>((factory) => menuCommandService, new PerContainerLifetime());
				_container.Register<ILanguageRegistrationService, LanguageRegistrationService>(new PerContainerLifetime());
				_container.Register<ILanguageService, LanguageService>(new PerContainerLifetime());
				_container.Register<ShellService>(new PerContainerLifetime());
				_container.Register<IShellHelperService>((factory) => factory.GetInstance<ShellService>(), new PerContainerLifetime());
				_container.Register<IShellStatusBarService>((factory) => factory.GetInstance<ShellService>(), new PerContainerLifetime());
				_container.Register<IShellReferenceService>((factory) => factory.GetInstance<ShellService>(), new PerContainerLifetime());
				_container.Register<IShellSelectionService>((factory) => factory.GetInstance<ShellService>(), new PerContainerLifetime());
				_container.Register<IShellProjectService>((factory) => factory.GetInstance<ShellService>(), new PerContainerLifetime());
				_container.Register<IShellCodeModelService>((factory) => factory.GetInstance<ShellService>(), new PerContainerLifetime());
				_container.Register<IShellEventsService, ShellEventsService>(new PerContainerLifetime());
				_container.Register<IShellImageService, ShellImageService>(new PerContainerLifetime());
				_container.Register<IImageService, ImageService>(new PerContainerLifetime());
				_container.Register<IMessageService, MessageService>(new PerContainerLifetime());
				_container.Register<JoinableTaskFactory>((factory) => this.JoinableTaskFactory, new PerContainerLifetime());
				_container.Register<IAsyncTaskService, AsyncTaskService>(new PerContainerLifetime());
				_container.Register<IOptionsService, OptionsService>(new PerContainerLifetime());
				_container.Register<IOptionsPersistenceService, OptionsPersistenceService>(new PerContainerLifetime());
				// Per request services
				_container.Register<IFeatureCommandFactory, FeatureCommandFactory>();
				_container.Register<IFeatureFactory, FeatureFactory>();
				_container.Register<IUtilsService, UtilsService>();
				_container.Register<IFileTypeResolver, FileTypeResolver>();
				_container.Register<ISolutionProcessor, SolutionProcessor>();
				_container.Register<IProjectProcessor, ProjectProcessor>();
				_container.Register<IFileProcessor, FileProcessor>();
				_container.Register<IModalDialogService, ModalDialogService>();
				_container.Register<ISearchMatchService, SearchMatchService>();
				_container.Register<IWildcardMatch, WildcardMatch>();
				// Features
				_container.Register<Features.SupportOptions.SupportOptionsFeature>(new PerContainerLifetime());
				_container.Register<Features.Miscellaneous.MiscellaneousFeature>(new PerContainerLifetime());
				// File Browser feature
				_container.Register<Features.FileBrowser.FileBrowserFeature>(new PerContainerLifetime());
				_container.Register<Features.FileBrowser.FileBrowserViewModel>();
				// Code Browser feature
				_container.Register<Features.CodeBrowser.CodeBrowserFeature>(new PerContainerLifetime());
				_container.Register<Features.CodeBrowser.CodeBrowserViewModel>();
				// Bookmarks feature
				_container.Register<BookmarksFeature>(new PerContainerLifetime());
				_container.Register<IBookmarksService, BookmarksService>(new PerContainerLifetime());

				_container.Compile();
				_sharedServiceProvider?.Initialize(this, _container);
				return true;
			}
			catch (Exception ex)
			{
				// Failing this early we can't even use existing services
				Trace.TraceError($"Failed to initialize:\r\n{ex}");

				// Can't rely on any of our own services here

				var resourceManager = new ResourceManager("VSPackage", this.GetType().Assembly);
				var product = resourceManager?.GetString(IDs.PRODUCT.ToString());

				VsShellUtilities.ShowMessageBox(this, $"Failed to initialize: {ex.Message}", product,
					OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

				return false;
			}
		}

		#endregion
	}
}