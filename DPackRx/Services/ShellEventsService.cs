using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using DPackRx.Extensions;
using DPackRx.Package;

using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace DPackRx.Services
{
	/// <summary>
	/// Shell events subscription service.
	/// </summary>
	public class ShellEventsService : IShellEventsService, IDisposable,
		IVsSolutionEvents3, IVsSolutionEvents4, IVsSolutionLoadEvents, IVsRunningDocTableEvents3, IVsTrackProjectDocumentsEvents2
	{
		#region Fields

		private readonly IServiceProvider _serviceProvider;
		private readonly ILog _log;
		private readonly IPackageService _packageService;
		private readonly IShellProjectService _shellProjectService;
		private readonly IShellStatusBarService _shellStatusBarService;
		private readonly List<ISolutionEvents> _solutionEventSubscribers = new List<ISolutionEvents>(4);
		private readonly List<ICodeModelEvents> _codeModelEventSubscribers = new List<ICodeModelEvents>(4);
		private bool _initialized;
		private SolutionEvents _solutionEvents;
		private DocumentEvents _documentEvents;
		private CodeModelEvents _codeModelEvents;
		private uint _solutionCookie;
		private uint _docEventsCookie;
		private uint _windowEventsCookie;

		#endregion

		public ShellEventsService(IServiceProvider serviceProvider, ILog log, IPackageService packageService,
			IShellProjectService shellProjectService, IShellStatusBarService shellStatusBarService)
		{
			_serviceProvider = serviceProvider;
			_log = log;
			_packageService = packageService;
			_shellProjectService = shellProjectService;
			_shellStatusBarService = shellStatusBarService;
		}

		#region IDisposable Members

		public void Dispose()
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			if (_solutionEvents != null)
			{
				_solutionEvents.Renamed -= SolutionEvents_Renamed;
				_solutionEvents = null;
			}

			if (_documentEvents != null)
			{
				_documentEvents.DocumentOpened -= DocumentEvents_DocumentOpened;
				_documentEvents.DocumentClosing -= DocumentEvents_DocumentClosing;
				_documentEvents.DocumentSaved -= DocumentEvents_DocumentSaved;
				_documentEvents = null;
			}

			if (_codeModelEvents != null)
			{
				_codeModelEvents.ElementAdded -= CodeModelEvents_ElementAdded;
				_codeModelEvents.ElementChanged -= CodeModelEvents_ElementChanged;
				_codeModelEvents.ElementDeleted -= CodeModelEvents_ElementDeleted;
				_codeModelEvents = null;
			}

			if (_solutionCookie != 0)
			{
				var solution = _serviceProvider.GetService<IVsSolution, SVsSolution>(false);
				if (solution != null)
					solution.UnadviseSolutionEvents(_solutionCookie);
			}

			if (_docEventsCookie != 0)
			{
				var docEvents = _serviceProvider.GetService<IVsTrackProjectDocuments2, SVsTrackProjectDocuments>(false);
				if (docEvents != null)
					docEvents.UnadviseTrackProjectDocumentsEvents(_docEventsCookie);
			}

			if (_windowEventsCookie != 0)
			{
				var windowEvents = _serviceProvider.GetService<IVsRunningDocumentTable, SVsRunningDocumentTable>(false);
				if (windowEvents != null)
					windowEvents.UnadviseRunningDocTableEvents(_windowEventsCookie);
			}
		}

		#endregion

		#region IShellEventsService Members

		/// <summary>
		/// One time subscriber notification that solution's been opened.
		/// </summary>
		/// <remarks>Solution may or may not be actually open.</remarks>
		public void NotifySolutionOpened()
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			_shellStatusBarService.SetStatusBarText($"{_packageService.ProductName} {_packageService.Version} initialized");

			var dte = _shellProjectService.GetDTE() as DTE;
			var solution = dte.Solution;
			if ((solution == null) || !solution.IsOpen)
				return;

			OnAfterOpenSolution(null, Convert.ToInt32(false));
		}

		/// <summary>
		/// Subscribes to the solution events.
		/// </summary>
		/// <param name="subscriber">Subscriber.</param>
		public void SubscribeSolutionEvents(ISolutionEvents subscriber)
		{
			if (subscriber == null)
				throw new ArgumentNullException(nameof(subscriber));

			ThreadHelper.ThrowIfNotOnUIThread();

			if (!_initialized)
				Initialize();

			if (!_solutionEventSubscribers.Contains(subscriber))
				_solutionEventSubscribers.Add(subscriber);
		}

		/// <summary>
		/// Unsubscribes from the solution events.
		/// </summary>
		/// <param name="subscriber"></param>
		public void UnsubscribeSolutionEvents(ISolutionEvents subscriber)
		{
			if (subscriber == null)
				throw new ArgumentNullException(nameof(subscriber));

			if (_solutionEventSubscribers.Contains(subscriber))
				_solutionEventSubscribers.Remove(subscriber);
		}

		/// <summary>
		/// Subscribes to the code model events.
		/// </summary>
		/// <param name="subscriber">Subscriber.</param>
		public void SubscribeCodeModelEvents(ICodeModelEvents subscriber)
		{
			if (subscriber == null)
				throw new ArgumentNullException(nameof(subscriber));

			ThreadHelper.ThrowIfNotOnUIThread();

			if (!_initialized)
				Initialize();

			if (!_codeModelEventSubscribers.Contains(subscriber))
				_codeModelEventSubscribers.Add(subscriber);
		}

		/// <summary>
		/// Unsubscribes from the code model events.
		/// </summary>
		/// <param name="subscriber">Subscriber.</param>
		public void UnsubscribeCodeModelEvents(ICodeModelEvents subscriber)
		{
			if (subscriber == null)
				throw new ArgumentNullException(nameof(subscriber));

			if (_codeModelEventSubscribers.Contains(subscriber))
				_codeModelEventSubscribers.Remove(subscriber);
		}

		#endregion

		#region IVsSolutionEvents3 - solution open/close events

		public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
		{
			if (fAdded == Convert.ToInt32(true))
			{
				var project = _shellProjectService.GetHierarchyProject(pHierarchy);
				if (project != null)
					_solutionEventSubscribers.For(s => s.ProjectAdded(project));
			}

			return VSConstants.S_OK;
		}

		public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
		{
			// We don't care about this event
			return VSConstants.S_OK;
		}

		public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
		{
			if (fRemoved == Convert.ToInt32(true))
			{
				var project = _shellProjectService.GetHierarchyProject(pHierarchy);
				if (project != null)
					_solutionEventSubscribers.For(s => s.ProjectDeleted(project));
			}

			return VSConstants.S_OK;
		}

		public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
		{
			return VSConstants.S_OK;
		}

		public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
		{
			return VSConstants.S_OK;
		}

		public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
		{
			var project = _shellProjectService.GetHierarchyProject(pRealHierarchy);
			if (project != null)
				_solutionEventSubscribers.For(s => s.ProjectUnloaded(project));

			return VSConstants.S_OK;
		}

		public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
		{
			_solutionEventSubscribers.For(s => s.SolutionOpened(Convert.ToBoolean(fNewSolution)));
			return VSConstants.S_OK;
		}

		public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
		{
			// We don't care about this event
			pfCancel = Convert.ToInt32(false);
			return VSConstants.S_OK;
		}

		public int OnBeforeCloseSolution(object pUnkReserved)
		{
			_solutionEventSubscribers.For(s => s.SolutionClosing());
			return VSConstants.S_OK;
		}

		public int OnAfterCloseSolution(object pUnkReserved)
		{
			_solutionEventSubscribers.For(s => s.SolutionClosed());
			return VSConstants.S_OK;
		}

		public int OnAfterMergeSolution(object pUnkReserved)
		{
			return VSConstants.S_OK;
		}

		public int OnAfterClosingChildren(IVsHierarchy pHierarchy)
		{
			return VSConstants.S_OK;
		}

		public int OnAfterOpeningChildren(IVsHierarchy pHierarchy)
		{
			return VSConstants.S_OK;
		}

		public int OnBeforeClosingChildren(IVsHierarchy pHierarchy)
		{
			return VSConstants.S_OK;
		}

		public int OnBeforeOpeningChildren(IVsHierarchy pHierarchy)
		{
			return VSConstants.S_OK;
		}

		#endregion

		#region IVsSolutionEvents4 - solution rename events

		public int OnAfterAsynchOpenProject(IVsHierarchy pHierarchy, int fAdded)
		{
			return VSConstants.S_OK;
		}

		public int OnQueryChangeProjectParent(IVsHierarchy pHierarchy, IVsHierarchy pNewParentHier, 
			ref int pfCancel)
		{
			pfCancel = Convert.ToInt32(false);
			return VSConstants.S_OK;
		}

		public int OnAfterChangeProjectParent(IVsHierarchy pHierarchy)
		{
			return VSConstants.S_OK;
		}

		public int OnAfterRenameProject(IVsHierarchy pHierarchy)
		{
			var project = _shellProjectService.GetHierarchyProject(pHierarchy);
			if (project != null)
				_solutionEventSubscribers.For(s => s.ProjectRenamed(project));

			return VSConstants.S_OK;
		}

		#endregion

		#region IVsSolutionLoadEvents Members

		public int OnBeforeOpenSolution(string pszSolutionFilename)
		{
			return VSConstants.S_OK;
		}

		public int OnBeforeBackgroundSolutionLoadBegins()
		{
			return VSConstants.S_OK;
		}

		public int OnAfterBackgroundSolutionLoadComplete()
		{
			_solutionEventSubscribers.For(s => s.SolutionOpened(true));
			return VSConstants.S_OK;
		}

		public int OnQueryBackgroundLoadProjectBatch(out bool pfShouldDelayLoadToNextIdle)
		{
			pfShouldDelayLoadToNextIdle = false;
			return VSConstants.S_OK;
		}

		public int OnBeforeLoadProjectBatch(bool fIsBackgroundIdleBatch)
		{
			return VSConstants.S_OK;
		}
		public int OnAfterLoadProjectBatch(bool fIsBackgroundIdleBatch)
		{
			return VSConstants.S_OK;
		}

		#endregion

		#region IVsRunningDocTableEvents3 - documents selection changes

		public int OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, 
			uint dwReadLocksRemaining, uint dwEditLocksRemaining)
		{
			return VSConstants.S_OK;
		}

		public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, 
			uint dwReadLocksRemaining, uint dwEditLocksRemaining)
		{
			return VSConstants.S_OK;
		}

		public int OnAfterSave(uint docCookie)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			if (!IsSolutionOpen())
				return VSConstants.S_OK;

			var dte = _shellProjectService.GetDTE() as DTE;
			var fileName = dte.Solution.FileName;
			if (string.IsNullOrEmpty(fileName))
				return VSConstants.S_OK;

			var rdt = _serviceProvider.GetService<IVsRunningDocumentTable, SVsRunningDocumentTable>(false);
			if (rdt == null)
				return VSConstants.S_OK;

			var itemid = VSConstants.VSITEMID_NIL;
			var result = rdt.GetDocumentInfo(docCookie, out _, out _, out _, out string docFileName, out _, out itemid, out IntPtr ppunkDocData);

			if (ppunkDocData != IntPtr.Zero)
				Marshal.Release(ppunkDocData);

			if (result != VSConstants.S_OK)
				return VSConstants.S_OK;

			// Check if it's a solution's item
			if (string.Compare(fileName, docFileName, StringComparison.OrdinalIgnoreCase) == 0)
				_solutionEventSubscribers.For(s => s.SolutionSaved());

			return VSConstants.S_OK;
		}

		public int OnBeforeSave(uint docCookie)
		{
			return VSConstants.S_OK;
		}

		public int OnAfterAttributeChange(uint docCookie, uint grfAttribs)
		{
			return VSConstants.S_OK;
		}

		public int OnAfterAttributeChangeEx(uint docCookie, uint grfAttribs, 
			IVsHierarchy pHierOld, uint itemidOld, string pszMkDocumentOld, 
			IVsHierarchy pHierNew, uint itemidNew, string pszMkDocumentNew)
		{
			return VSConstants.S_OK;
		}

		public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
		{
			return VSConstants.S_OK;
		}

		public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
		{
			return VSConstants.S_OK;
		}

		#endregion

		#region IVsTrackProjectDocumentsEvents2 - files/folders add, remove and rename events

		#region Files

		public int OnQueryAddFiles(IVsProject pProject, int cFiles, string[] rgpszMkDocuments,
			VSQUERYADDFILEFLAGS[] rgFlags, VSQUERYADDFILERESULTS[] pSummaryResult, 
			VSQUERYADDFILERESULTS[] rgResults)
		{
			return VSConstants.S_OK;
		}

		public int OnAfterAddFilesEx(int cProjects, int cFiles, IVsProject[] rgpProjects,
			int[] rgFirstIndices, string[] rgpszMkDocuments, VSADDFILEFLAGS[] rgFlags)
		{
			if ((rgpszMkDocuments != null) && (rgpszMkDocuments.Length > 0) && (rgpProjects != null) && (rgpProjects.Length > 0))
			{
				var project = _shellProjectService.GetHierarchyProject(rgpProjects[0]);
				if (project != null)
					_solutionEventSubscribers.For(s => s.FileAdded(rgpszMkDocuments, project));
			}

			return VSConstants.S_OK;
		}

		public int OnQueryRemoveFiles(IVsProject pProject, int cFiles, string[] rgpszMkDocuments,
			VSQUERYREMOVEFILEFLAGS[] rgFlags, VSQUERYREMOVEFILERESULTS[] pSummaryResult,
			VSQUERYREMOVEFILERESULTS[] rgResults)
		{
			return VSConstants.S_OK;
		}

		public int OnAfterRemoveFiles(int cProjects, int cFiles, IVsProject[] rgpProjects,
			int[] rgFirstIndices, string[] rgpszMkDocuments, VSREMOVEFILEFLAGS[] rgFlags)
		{
			if ((rgpszMkDocuments != null) &&
					(rgpszMkDocuments.Length > 0) &&
					(rgpProjects != null) &&
					(rgpProjects.Length > 0))
			{
				var project = _shellProjectService.GetHierarchyProject(rgpProjects[0]);
				if (project != null)
					_solutionEventSubscribers.For(s => s.FileDeleted(rgpszMkDocuments, project));
			}

			return VSConstants.S_OK;
		}

		public int OnQueryRenameFiles(IVsProject pProject, int cFiles,
			string[] rgszMkOldNames, string[] rgszMkNewNames, VSQUERYRENAMEFILEFLAGS[] rgFlags,
			VSQUERYRENAMEFILERESULTS[] pSummaryResult, VSQUERYRENAMEFILERESULTS[] rgResults)
		{
			return VSConstants.S_OK;
		}

		public int OnAfterRenameFiles(int cProjects, int cFiles, IVsProject[] rgpProjects,
			int[] rgFirstIndices, string[] rgszMkOldNames, string[] rgszMkNewNames,
			VSRENAMEFILEFLAGS[] rgFlags)
		{
			// We don't care about directory events
			if ((rgFlags != null) && (rgFlags.Length > 0) &&
					(rgFlags[0] != VSRENAMEFILEFLAGS.VSRENAMEFILEFLAGS_Directory) &&
					(rgszMkOldNames != null) && (rgszMkNewNames != null) &&
					(rgszMkOldNames.Length > 0) && (rgszMkOldNames.Length == rgszMkNewNames.Length) &&
					(rgpProjects != null) && (rgpProjects.Length > 0))
			{
				var project = _shellProjectService.GetHierarchyProject(rgpProjects[0]);
				if (project != null)
					_solutionEventSubscribers.For(s => s.FileRenamed(rgszMkOldNames, rgszMkNewNames, project));
			}

			return VSConstants.S_OK;
		}

		#endregion

		#region Directories

		public int OnQueryAddDirectories(IVsProject pProject, int cDirectories,
			string[] rgpszMkDocuments, VSQUERYADDDIRECTORYFLAGS[] rgFlags,
			VSQUERYADDDIRECTORYRESULTS[] pSummaryResult, VSQUERYADDDIRECTORYRESULTS[] rgResults)
		{
			return VSConstants.S_OK;
		}

		public int OnAfterAddDirectoriesEx(int cProjects, int cDirectories,
			IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments,
			VSADDDIRECTORYFLAGS[] rgFlags)
		{
			return VSConstants.S_OK;
		}

		public int OnQueryRemoveDirectories(IVsProject pProject, int cDirectories,
			string[] rgpszMkDocuments, VSQUERYREMOVEDIRECTORYFLAGS[] rgFlags,
			VSQUERYREMOVEDIRECTORYRESULTS[] pSummaryResult, VSQUERYREMOVEDIRECTORYRESULTS[] rgResults)
		{
			return VSConstants.S_OK;
		}

		public int OnAfterRemoveDirectories(int cProjects, int cDirectories,
			IVsProject[] rgpProjects, int[] rgFirstIndices, string[] rgpszMkDocuments,
			VSREMOVEDIRECTORYFLAGS[] rgFlags)
		{
			return VSConstants.S_OK;
		}

		public int OnQueryRenameDirectories(IVsProject pProject, int cDirs,
			string[] rgszMkOldNames, string[] rgszMkNewNames, VSQUERYRENAMEDIRECTORYFLAGS[] rgFlags,
			VSQUERYRENAMEDIRECTORYRESULTS[] pSummaryResult, VSQUERYRENAMEDIRECTORYRESULTS[] rgResults)
		{
			return VSConstants.S_OK;
		}

		public int OnAfterRenameDirectories(int cProjects, int cDirs, IVsProject[] rgpProjects,
			int[] rgFirstIndices, string[] rgszMkOldNames, string[] rgszMkNewNames,
			VSRENAMEDIRECTORYFLAGS[] rgFlags)
		{
			return VSConstants.S_OK;
		}

		#endregion

		#region Misc

		public int OnAfterSccStatusChanged(int cProjects, int cFiles, IVsProject[] rgpProjects,
			int[] rgFirstIndices, string[] rgpszMkDocuments, uint[] rgdwSccStatus)
		{
			return VSConstants.S_OK;
		}

		#endregion

		#endregion

		#region Private Methods

		private void Initialize()
		{
			if (_initialized)
				return;

			ThreadHelper.ThrowIfNotOnUIThread();

			try
			{
				var dte = _shellProjectService.GetDTE() as DTE2;
				if (dte != null)
				{
					var events = dte.Events as Events2;

					_solutionEvents = events?.SolutionEvents;
					if (_solutionEvents != null)
						_solutionEvents.Renamed += SolutionEvents_Renamed;

					_documentEvents = events?.get_DocumentEvents(null);
					if (_documentEvents != null)
					{
						_documentEvents.DocumentOpened += DocumentEvents_DocumentOpened;
						_documentEvents.DocumentClosing += DocumentEvents_DocumentClosing;
						_documentEvents.DocumentSaved += DocumentEvents_DocumentSaved;
					}

					_codeModelEvents = events?.get_CodeModelEvents(null);
					if (_codeModelEvents != null)
					{
						_codeModelEvents.ElementAdded += CodeModelEvents_ElementAdded;
						_codeModelEvents.ElementChanged += CodeModelEvents_ElementChanged;
						_codeModelEvents.ElementDeleted += CodeModelEvents_ElementDeleted;
					}
				}

				var solution = _serviceProvider.GetService<IVsSolution, SVsSolution>(false);
				if (solution != null)
					solution.AdviseSolutionEvents(this, out _solutionCookie);

				var docEvents = _serviceProvider.GetService<IVsTrackProjectDocuments2, SVsTrackProjectDocuments>(false);
				if (docEvents != null)
					docEvents.AdviseTrackProjectDocumentsEvents(this, out _docEventsCookie);

				var windowEvents = _serviceProvider.GetService<IVsRunningDocumentTable, SVsRunningDocumentTable>(false);
				if (windowEvents != null)
					windowEvents.AdviseRunningDocTableEvents(this, out _windowEventsCookie);
			}
			finally
			{
				_initialized = true;
			}
		}

		private bool IsSolutionOpen()
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var dte = _shellProjectService.GetDTE() as DTE;
			return (dte != null) && (dte.Solution != null) && dte.Solution.IsOpen;
		}

		private void SolutionEvents_Renamed(string oldName)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			var dte = _shellProjectService.GetDTE() as DTE;
			var newName = (dte != null) && (dte.Solution != null) ? dte.Solution.FileName : null;

			if (!string.IsNullOrEmpty(oldName) && !string.IsNullOrEmpty(newName))
				_solutionEventSubscribers.For(s => s.SolutionRenamed(oldName, newName));
		}

		private void DocumentEvents_DocumentOpened(Document document)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			if ((document != null) && IsSolutionOpen())
			{
				var fileName = document.FullName;
				if (!string.IsNullOrEmpty(fileName))
				{
					var projectItem = _shellProjectService.GetDocumentProjectItem(document);
					if (projectItem != null)
						_solutionEventSubscribers.For(s => s.FileOpened(fileName, projectItem));
				}
			}
		}

		private void DocumentEvents_DocumentClosing(Document document)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			// Work around for file's code model changed event not being raised for web project
			// when file is closed w/o saving changes. Thus, FileChanged change type needs to be 
			// processed for web projects only
			if ((document != null) && IsSolutionOpen())
			{
				var fileName = document.FullName;
				if (!string.IsNullOrEmpty(fileName))
				{
					if (!document.Saved)
					{
						var projectItem = _shellProjectService.GetDocumentProjectItem(document);
						if (projectItem != null)
							_solutionEventSubscribers.For(s => s.FileChanged(fileName, projectItem));
					}

					var project = _shellProjectService.GetDocumentProject(document);
					if (project != null)
						_solutionEventSubscribers.For(s => s.FileClosed(fileName, project));
				}
			}
		}

		private void DocumentEvents_DocumentSaved(Document document)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			// Work around for file's code model changed event not being raised when 
			// code member line # is changed. Thus, FileChanged change type needs to be 
			// processed to force the code model refresh
			if ((document != null) && IsSolutionOpen())
			{
				var fileName = document.FullName;
				if (!string.IsNullOrEmpty(fileName))
				{
					var projectItem = _shellProjectService.GetDocumentProjectItem(document);
					if (projectItem != null)
						_solutionEventSubscribers.For(s => s.FileSaved(fileName, projectItem));
				}
			}
		}

		private void CodeModelEvents_ElementAdded(CodeElement element)
		{
			if (element != null)
				_codeModelEventSubscribers.For(s => s.ElementAdded(element));
		}

		private void CodeModelEvents_ElementChanged(CodeElement element, vsCMChangeKind change)
		{
			if ((element != null) && (
					(change == vsCMChangeKind.vsCMChangeKindUnknown) ||
					(change == vsCMChangeKind.vsCMChangeKindRename) ||
					(change == vsCMChangeKind.vsCMChangeKindSignatureChange)))
				_codeModelEventSubscribers.For(s => s.ElementChanged(element));
		}

		private void CodeModelEvents_ElementDeleted(object parent, CodeElement element)
		{
			if (element != null)
				_codeModelEventSubscribers.For(s => s.ElementDeleted(element, parent));
		}

		#endregion
	}
}