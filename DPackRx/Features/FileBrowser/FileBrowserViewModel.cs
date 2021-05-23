using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

using DPackRx.CodeModel;
using DPackRx.Extensions;
using DPackRx.Options;
using DPackRx.Services;
using DPackRx.UI;
using DPackRx.UI.Commands;

namespace DPackRx.Features.FileBrowser
{
	/// <summary>
	/// File Browser feature ViewModel.
	/// </summary>
	public class FileBrowserViewModel : FeatureViewModelBase
	{
		#region Fields

		private readonly ILog _log;
		private readonly IMessageService _messageService;
		private readonly IOptionsService _optionsService;
		private readonly ISolutionProcessor _solutionProcessor;
		private readonly IFileTypeResolver _fileTypeResolver;
		private readonly ISearchMatchService _searchMatchService;
		private readonly IShellHelperService _shellHelperService;
		private readonly IShellImageService _shellImageService;
		private readonly IUtilsService _utilsService;
		private readonly IFeatureFactory _featureFactory;
		private readonly ObservableCollection<FileModel> _sourceFiles;
		private readonly CollectionViewSource _files;
		private string _search = string.Empty;
		private bool _allFiles;
		private readonly List<string> _showFiles = new List<string>();

		#endregion

		// Design time constructor
		public FileBrowserViewModel() : base(KnownFeature.FileBrowser, new ServiceContainer())
		{
		}

		public FileBrowserViewModel(IServiceProvider serviceProvider, ILog log, IMessageService messageService,
			IOptionsService optionsService, ISolutionProcessor solutionProcessor, IFileTypeResolver fileTypeResolver,
			ISearchMatchService searchMatchService, IShellHelperService shellHelperService,
			IShellImageService shellImageService, IUtilsService utilsService, IFeatureFactory featureFactory)
			: base(KnownFeature.FileBrowser, serviceProvider)
		{
			_log = log;
			_messageService = messageService;
			_optionsService = optionsService;
			_solutionProcessor = solutionProcessor;
			_fileTypeResolver = fileTypeResolver;
			_searchMatchService = searchMatchService;
			_shellHelperService = shellHelperService;
			_shellImageService = shellImageService;
			_utilsService = utilsService;
			_featureFactory = featureFactory;

			// Source files must be setup in constructor or view won't show any binding data
			_sourceFiles = new ObservableCollection<FileModel>();
			_files = new CollectionViewSource { Source = _sourceFiles }; // must be ObservableCollection

			this.ShowFilesCommand = new RelayCommand(_messageService, OnShowAllFiles);
			this.OpenCodeBrowserAllCommand = new RelayCommand(_messageService, OnOpenCodeBrowserAll, OnCanOpenCodeBrowser);
			this.OpenCodeBrowserClassesCommand = new RelayCommand(_messageService, OnOpenCodeBrowserClasses, OnCanOpenCodeBrowser);
			this.OpenCodeBrowserMethodsCommand = new RelayCommand(_messageService, OnOpenCodeBrowserMethods, OnCanOpenCodeBrowser);
			this.OpenCodeBrowserPropertiesCommand = new RelayCommand(_messageService, OnOpenCodeBrowserProperties, OnCanOpenCodeBrowser);
			this.OpenFilesCommand = new RelayCommand(_messageService, OnOpenFiles, OnCanOpenFiles);
		}

		#region ViewModelBase Overrides

		/// <summary>
		/// Initializes data model before UI's shown.
		/// </summary>
		/// <param name="argument">Optional argument.</param>
		public override void OnInitialize(object argument)
		{
			_log.LogMessage(this.Feature, "Initializing...");

			var model = _solutionProcessor.GetProjects(ProcessorFlags.IncludeFiles | ProcessorFlags.GroupLinkedFiles);
			var files = model.Files;
			var solutionName = model.SolutionName;
			files = ApplyOptions(files);

			this.SolutionName = _optionsService.GetStringOption(this.Feature, "Solution", this.SolutionName);
			_search = _optionsService.GetStringOption(this.Feature, "Search", _search);
			_allFiles = _optionsService.GetBoolOption(this.Feature, "AllFiles", _allFiles);

			// Reset search on new solution
			if (!string.IsNullOrEmpty(this.SolutionName) && !string.IsNullOrEmpty(solutionName) && !solutionName.Equals(this.SolutionName, StringComparison.OrdinalIgnoreCase))
				_search = string.Empty;
			this.SolutionName = solutionName;

			_sourceFiles.Clear();
			_sourceFiles.AddRange(files); // causes filter to be evaluated

			using (_files.DeferRefresh()) // defers filter evaluation until end of using
			{
				_files.Filter += OnFilter;

				OnSearch();
			}

			_log.LogMessage(this.Feature, "Initialized");
		}

		/// <summary>
		/// Closes UI down.
		/// <paramref name="apply">Whether to apply or process the selection or not.</paramref>
		/// </summary>
		public override void OnClose(bool apply)
		{
			_optionsService.SetStringOption(this.Feature, "Solution", this.SolutionName);
			_optionsService.SetStringOption(this.Feature, "Search", _search);
			_optionsService.SetBoolOption(this.Feature, "AllFiles", _allFiles);

			if (!apply)
				return;

			if ((this.Selection == null) || (this.Selection.Count() == 0))
				return;

			if (this.SelectionCode)
			{
				_shellHelperService.OpenFiles(this.Selection);

				if (this.CodeBrowserCommandId > 0)
				{
					var feature = _featureFactory.GetFeature(KnownFeature.CodeBrowser);
					feature.Execute(this.CodeBrowserCommandId);
				}
			}
			else
			{
				_shellHelperService.OpenDesignerFiles(this.Selection);
			}
		}

		#endregion

		#region Commands

		/// <summary>
		/// Show Files command.
		/// </summary>
		public ICommand ShowFilesCommand { get; set; }

		/// <summary>
		/// Open Code Browser All command.
		/// </summary>
		public ICommand OpenCodeBrowserAllCommand { get; set; }

		/// <summary>
		/// Open Code Browser Classes command.
		/// </summary>
		public ICommand OpenCodeBrowserClassesCommand { get; set; }

		/// <summary>
		/// Open Code Browser Methods command.
		/// </summary>
		public ICommand OpenCodeBrowserMethodsCommand { get; set; }

		/// <summary>
		/// Open Code Browser Properties command.
		/// </summary>
		public ICommand OpenCodeBrowserPropertiesCommand { get; set; }

		/// <summary>
		/// Open Files command.
		/// </summary>
		public ICommand OpenFilesCommand { get; set; }

		#endregion

		#region Properties

		/// <summary>
		/// Search text.
		/// </summary>
		public string Search
		{
			get { return _search; }
			set
			{
				_search = value;
				RaisePropertyChanged(nameof(this.Search));

				OnSearch();
			}
		}

		/// <summary>
		/// All Files toggle.
		/// </summary>
		public bool AllFiles
		{
			get { return _allFiles; }
			set
			{
				_allFiles = value;
				RaisePropertyChanged(nameof(this.AllFiles));

				OnSearch();
			}
		}

		/// <summary>
		/// Filtered files view.
		/// </summary>
		public ICollectionView Files
		{
			get { return _files?.View; }
		}

		/// <summary>
		/// Filtered files.
		/// </summary>
		/// <remarks>Exposed for testing purposes only.</remarks>
		protected internal IList<FileModel> FilteredFiles
		{
			get { return ((IEnumerable)_files?.View).Cast<FileModel>().ToList(); }
		}

		/// <summary>
		/// Image for help button.
		/// </summary>
		public ImageSource ImageSearchHelp
		{
			get { return _shellImageService.GetWellKnownImage(WellKnownImage.Info); }
		}

		/// <summary>
		/// Image for Show All Files button state.
		/// </summary>
		public ImageSource ImageShowAllFiles
		{
			get { return _shellImageService.GetWellKnownImage(WellKnownImage.AllFiles); }
		}

		/// <summary>
		/// Image for Show Code Files button state.
		/// </summary>
		public ImageSource ImageShowCodeFiles
		{
			get { return _shellImageService.GetWellKnownImage(WellKnownImage.AllCode); }
		}

		/// <summary>
		/// Image for Open Code Browser All button state.
		/// </summary>
		public ImageSource ImageOpenCodeBrowserAll
		{
			get { return _shellImageService.GetWellKnownImage(WellKnownImage.Members); }
		}

		/// <summary>
		/// Image for Open Code Browser Classes button state.
		/// </summary>
		public ImageSource ImageOpenCodeBrowserClasses
		{
			get { return _shellImageService.GetWellKnownImage(WellKnownImage.Classes); }
		}

		/// <summary>
		/// Image for Open Code Browser Methods button state.
		/// </summary>
		public ImageSource ImageOpenCodeBrowserMethods
		{
			get { return _shellImageService.GetWellKnownImage(WellKnownImage.Methods); }
		}

		/// <summary>
		/// Image for Open Code Browser Properties button state.
		/// </summary>
		public ImageSource ImageOpenCodeBrowserProperties
		{
			get { return _shellImageService.GetWellKnownImage(WellKnownImage.Properties); }
		}

		/// <summary>
		/// Whether selection is for code or designer file.
		/// </summary>
		/// <remarks>Exposed for testing purposes only.</remarks>
		protected internal bool SelectionCode { get; set; } = true;

		/// <summary>
		/// Current selection.
		/// </summary>
		/// <remarks>Exposed for testing purposes only.</remarks>
		protected internal IEnumerable<IExtensibilityItem> Selection { get; set; }

		/// <summary>
		/// Whether Code Browser should be opened with command Id.
		/// </summary>
		/// <remarks>Exposed for testing purposes only.</remarks>
		protected internal int CodeBrowserCommandId { get; set; }

		/// <summary>
		/// Solution name without extension.
		/// </summary>
		/// <remarks>Exposed for testing purposes only.</remarks>
		protected internal string SolutionName { get; private set; } = string.Empty;

		#endregion

		#region Private Methods

		/// <summary>
		/// Returns filtered out files based on feature options.
		/// </summary>
		private ICollection<FileModel> ApplyOptions(ICollection<FileModel> files)
		{
			var ignoreFiles = _optionsService.GetStringOption(this.Feature, "IgnoreFiles");
			if (!string.IsNullOrEmpty(ignoreFiles))
			{
				foreach (var ignoreFile in ignoreFiles.Split(','))
				{
					if (!string.IsNullOrEmpty(ignoreFile))
					{
						var deleteFileList = files.Where(f => f.FileName.EndsWith(ignoreFile, StringComparison.OrdinalIgnoreCase)).ToList();
						deleteFileList.ForEach(f => files.Remove(f));
					}
				}
			}

			var showFiles = _optionsService.GetStringOption(this.Feature, "ShowFiles") ?? string.Empty;
			_showFiles.Clear();
			if (!string.IsNullOrEmpty(showFiles))
				_showFiles.AddRange(showFiles.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));

			var ignoreFolders = _optionsService.GetStringOption(this.Feature, "IgnoreFolders");
			if (!string.IsNullOrEmpty(ignoreFolders))
			{
				foreach (var ignoreFolder in ignoreFolders.Split(';'))
				{
					if (!string.IsNullOrEmpty(ignoreFolder))
					{
						var folder = ignoreFolder;
						var isRooted = Path.IsPathRooted(folder);
						if (isRooted)
						{
							if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
								folder = folder + Path.DirectorySeparatorChar;
						}
						else // surround non-rooted path with '\' for exact folder match
						{
							if (!folder.StartsWith(Path.DirectorySeparatorChar.ToString()))
								folder = Path.DirectorySeparatorChar + folder;
							if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
								folder = folder + Path.DirectorySeparatorChar;
						}

						var deleteFileList = files.Where(f => isRooted ?
							f.FileNameWithPath.StartsWith(folder, StringComparison.OrdinalIgnoreCase) :
							f.FileNameWithPath.IndexOf(folder, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
						deleteFileList.ForEach(f => files.Remove(f));
					}
				}
			}

			return files;
		}

		/// <summary>
		/// Triggers search matching and file list filter application.
		/// </summary>
		private void OnSearch()
		{
			_searchMatchService.MatchItems(_search, _sourceFiles);

			_files?.View.Refresh(); // causes filter to be evaluated
			_files?.View.MoveCurrentToFirst(); // auto-select first item - must be done after refresh
		}

		/// <summary>
		/// Filters individual file list items.
		/// </summary>
		private void OnFilter(object sender, FilterEventArgs e)
		{
			if (string.IsNullOrEmpty(_search) && _allFiles)
			{
				e.Accepted = true;
				return;
			}

			if (!(e.Item is FileModel))
			{
				e.Accepted = false;
				return;
			}

			var fileModel = (FileModel)e.Item;
			e.Accepted = fileModel.Matched;

			if (!_allFiles && !_fileTypeResolver.IsCodeSubType(fileModel.ItemSubType))
			{
				if ((_showFiles.Count == 0) || !_showFiles.Contains(Path.GetExtension(fileModel.FileName), StringComparer.OrdinalIgnoreCase))
					e.Accepted = false;
			}
		}

		/// <summary>
		/// Executes Show All Files command.
		/// </summary>
		/// <param name="obj">Optional Xaml defined parameter.</param>
		private void OnShowAllFiles(object obj)
		{
			this.AllFiles = !this.AllFiles;
		}

		/// <summary>
		/// Checks whether Open Code Browser command is available.
		/// </summary>
		private bool OnCanOpenCodeBrowser(object argument)
		{
			var item = _files?.View.CurrentItem as IExtensibilityItem;
			return (item != null) && _fileTypeResolver.IsCodeSubType(item.ItemSubType);
		}

		/// <summary>
		/// Executes Open Code Browser command.
		/// </summary>
		/// <param name="obj">Optional Xaml defined parameter.</param>
		private void OnOpenCodeBrowserAll(object obj)
		{
			this.CodeBrowserCommandId = Package.CommandIDs.CODE_BROWSER;
			OnOpenFiles(obj);
		}

		/// <summary>
		/// Executes Open Code Browser Classes command.
		/// </summary>
		/// <param name="obj">Optional Xaml defined parameter.</param>
		private void OnOpenCodeBrowserClasses(object obj)
		{
			this.CodeBrowserCommandId = Package.CommandIDs.CODE_BROWSER_CI;
			OnOpenFiles(obj);
		}

		/// <summary>
		/// Executes Open Code Browser Methods command.
		/// </summary>
		/// <param name="obj">Optional Xaml defined parameter.</param>
		private void OnOpenCodeBrowserMethods(object obj)
		{
			this.CodeBrowserCommandId = Package.CommandIDs.CODE_BROWSER_M;
			OnOpenFiles(obj);
		}

		/// <summary>
		/// Executes Open Code Browser Properties command.
		/// </summary>
		/// <param name="obj">Optional Xaml defined parameter.</param>
		private void OnOpenCodeBrowserProperties(object obj)
		{
			this.CodeBrowserCommandId = Package.CommandIDs.CODE_BROWSER_P;
			OnOpenFiles(obj);
		}

		/// <summary>
		/// Returns current selection.
		/// </summary>
		private IEnumerable<IExtensibilityItem> GetSelectedFiles(object obj)
		{
			if (!(obj is IList))
				return null;

			var selection = ((IList)obj).Cast<FileModel>();
			return selection;
		}

		/// <summary>
		/// Checks whether Open Files command is available.
		/// </summary>
		private bool OnCanOpenFiles(object argument)
		{
			return (argument is IList) && (((IList)argument).Count > 0);
		}

		/// <summary>
		/// Executes Open Files command.
		/// </summary>
		/// <param name="obj">Optional Xaml defined parameter.</param>
		private void OnOpenFiles(object obj)
		{
			this.SelectionCode = !_utilsService.ControlKeyDown();
			this.Selection = GetSelectedFiles(obj);
			this.CloseWindow = true;
		}

		#endregion
	}
}