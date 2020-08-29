using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

using DPackRx.CodeModel;
using DPackRx.Extensions;
using DPackRx.Options;
using DPackRx.Services;
using DPackRx.UI;
using DPackRx.UI.Commands;

namespace DPackRx.Features.CodeBrowser
{
	/// <summary>
	/// Code Browser feature ViewModel.
	/// </summary>
	public class CodeBrowserViewModel : FeatureViewModelBase
	{
		#region Fields

		private readonly ILog _log;
		private readonly IMessageService _messageService;
		private readonly IOptionsService _optionsService;
		private readonly IFileProcessor _fileProcessor;
		private readonly ISearchMatchService _searchMatchService;
		private readonly IShellSelectionService _shellSelectionService;
		private readonly IShellImageService _shellImageService;
		private readonly ObservableCollection<MemberCodeModel> _sourceMembers;
		private readonly CollectionViewSource _members;
		private string _title;
		private string _search = string.Empty;
		private bool _sameType;

		#endregion

		// Design time constructor
		public CodeBrowserViewModel() : base(KnownFeature.CodeBrowser, new ServiceContainer())
		{
		}

		public CodeBrowserViewModel(IServiceProvider serviceProvider, ILog log, IMessageService messageService,
			IOptionsService optionsService, IFileProcessor fileProcessor, ISearchMatchService searchMatchService,
			IShellSelectionService shellSelectionService, IShellImageService shellImageService)
			: base(KnownFeature.CodeBrowser, serviceProvider)
		{
			_log = log;
			_messageService = messageService;
			_optionsService = optionsService;
			_fileProcessor = fileProcessor;
			_searchMatchService = searchMatchService;
			_shellSelectionService = shellSelectionService;
			_shellImageService = shellImageService;

			// Source members must be setup in constructor or view won't show any binding data
			_sourceMembers = new ObservableCollection<MemberCodeModel>();
			_members = new CollectionViewSource { Source = _sourceMembers }; // must be ObservableCollection

			this.ShowAllMembersCommand = new RelayCommand(_messageService, OnShowAllMembers, OnCanShowAllMembers);
			this.SelectMemberCommand = new RelayCommand(_messageService, OnSelectMember, OnCanSelectMember);
		}

		#region ViewModelBase Overrides

		/// <summary>
		/// Initializes data model before UI's shown.
		/// </summary>
		/// <param name="argument">Optional argument.</param>
		public override void OnInitialize(object argument)
		{
			_log.LogMessage(this.Feature, "Initializing...");

			if (!(argument is CodeModelFilterFlags))
				throw new ArgumentException("Invalid initialization argument", nameof(argument));

			this.FileName = _optionsService.GetStringOption(this.Feature, "File", this.FileName);
			this.Title = $"USysWare Code Browser - {Path.GetFileName(this.FileName)}";
			_search = _optionsService.GetStringOption(this.Feature, "Search", _search);
			this.Filter = (CodeModelFilterFlags)argument;
			var filter = (CodeModelFilterFlags)_optionsService.GetIntOption(this.Feature, "Filter", (int)this.Filter);
			if (filter != this.Filter)
				_search = string.Empty;

			ApplyMembers();

			using (_members.DeferRefresh()) // defers filter evaluation until end of using
			{
				_members.Filter += OnFilter;

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
			_optionsService.SetStringOption(this.Feature, "File", this.FileName);
			_optionsService.SetStringOption(this.Feature, "Search", _search);
			_optionsService.SetIntOption(this.Feature, "Filter", (int)this.Filter);

			if (!apply)
				return;

			if (this.Selection == null)
				return;

			var member = this.Selection as MemberCodeModel;
			if (member == null)
				return;

			_shellSelectionService.SetActiveFilePosition(member.Line, 1);
		}

		#endregion

		#region Commands

		/// <summary>
		/// Show All Members command.
		/// </summary>
		public ICommand ShowAllMembersCommand { get; set; }

		/// <summary>
		/// Select Member command.
		/// </summary>
		public ICommand SelectMemberCommand { get; set; }

		#endregion

		#region Properties

		public string Title
		{
			get { return _title; }
			set
			{
				_title = value;
				RaisePropertyChanged(nameof(this.Title));
			}
		}

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
		/// Member filter.
		/// </summary>
		/// <remarks>Exposed for testing purposes only.</remarks>
		protected internal CodeModelFilterFlags Filter { get; set; }

		/// <summary>
		/// Filtered members view.
		/// </summary>
		public ICollectionView Members
		{
			get { return _members?.View; }
		}

		/// <summary>
		/// Whether all code model members are from the same type declaration.
		/// </summary>
		public bool SameType
		{
			get { return _sameType; }
			set
			{
				_sameType = value;
				RaisePropertyChanged(nameof(this.SameType));
			}
		}

		/// <summary>
		/// Filtered members.
		/// </summary>
		/// <remarks>Exposed for testing purposes only.</remarks>
		protected internal IList<MemberCodeModel> FilteredMembers
		{
			get { return ((IEnumerable)_members?.View).Cast<MemberCodeModel>().ToList(); }
		}

		/// <summary>
		/// Show All Members visibility.
		/// </summary>
		public Visibility ShowAllMembers
		{
			get
			{
				// Available just once when other than all members are shown
				if (this.Filter == CodeModelFilterFlags.All)
					return Visibility.Collapsed;
				else
					return Visibility.Visible;
			}
		}

		/// <summary>
		/// Image for Show All Members button state.
		/// </summary>
		public ImageSource ImageShowAllMembers
		{
			get { return _shellImageService.GetWellKnownImage(WellKnownImage.AllCode); }
		}

		/// <summary>
		/// Current selection.
		/// </summary>
		/// <remarks>Exposed for testing purposes only.</remarks>
		protected internal IExtensibilityItem Selection { get; set; }

		/// <summary>
		/// Full file name.
		/// </summary>
		/// <remarks>Exposed for testing purposes only.</remarks>
		protected internal string FileName { get; private set; } = string.Empty;

		#endregion

		#region Private Methods

		/// <summary>
		/// Rescans all members.
		/// </summary>
		private void ApplyMembers()
		{
			var flags = ProcessorFlags.IncludeFileCodeModel;
			if (_optionsService.GetBoolOption(this.Feature, "XmlDoc"))
				flags = flags | ProcessorFlags.IncludeMemberXmlDoc;

			var model = _fileProcessor.GetMembers(flags, this.Filter);
			var members = model.Members;
			var fileName = model.FileName;
			this.SameType = model.Members.Count(
				m => (m.ElementKind == Kind.Class) || (m.ElementKind == Kind.Interface) || (m.ElementKind == Kind.Struct) || (m.ElementKind == Kind.Enum)) <= 1;

			// Reset search on new file
			if (!string.IsNullOrEmpty(this.FileName) && !string.IsNullOrEmpty(fileName) && !fileName.Equals(this.FileName, StringComparison.OrdinalIgnoreCase))
				_search = string.Empty;
			this.FileName = fileName;

			_sourceMembers.Clear();
			_sourceMembers.AddRange(members); // causes filter to be evaluated
		}

		/// <summary>
		/// Triggers search matching and member list filter application.
		/// </summary>
		private void OnSearch()
		{
			_searchMatchService.MatchItems(_search, _sourceMembers);

			_members?.View.Refresh(); // causes filter to be evaluated
			_members?.View.MoveCurrentToFirst(); // auto-select first item - must be done after refresh
		}

		/// <summary>
		/// Filters individual member list items.
		/// </summary>
		private void OnFilter(object sender, FilterEventArgs e)
		{
			if (string.IsNullOrEmpty(_search) && (this.Filter == CodeModelFilterFlags.All))
			{
				e.Accepted = true;
				return;
			}

			if (!(e.Item is MemberCodeModel))
			{
				e.Accepted = false;
				return;
			}

			var member = (MemberCodeModel)e.Item;
			e.Accepted = member.Matched;
		}

		/// <summary>
		/// Checks whether Show All Members command is available.
		/// </summary>
		private bool OnCanShowAllMembers(object argument)
		{
			return this.ShowAllMembers == Visibility.Visible;
		}

		/// <summary>
		/// Executes Show All Members command.
		/// </summary>
		/// <param name="obj">Optional Xaml defined parameter.</param>
		private void OnShowAllMembers(object obj)
		{
			if (obj is CodeModelFilterFlags)
				this.Filter = (CodeModelFilterFlags)obj;
			else
				this.Filter = CodeModelFilterFlags.All;
			RaisePropertyChanged(nameof(this.ShowAllMembers));

			ApplyMembers();

			OnSearch();
		}

		/// <summary>
		/// Checks whether Select Member command is available.
		/// </summary>
		private bool OnCanSelectMember(object argument)
		{
			return argument != null;
		}

		/// <summary>
		/// Executes Select Member command.
		/// </summary>
		/// <param name="obj">Optional Xaml defined parameter.</param>
		private void OnSelectMember(object obj)
		{
			this.Selection = obj as MemberCodeModel;
			this.CloseWindow = true;
		}

		#endregion
	}
}