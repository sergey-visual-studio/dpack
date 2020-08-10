namespace DPackRx.Package
{
	/// <summary>
	/// Command IDs. MUST match Package.vsct command IDs.
	/// </summary>
	public static class CommandIDs
	{
		[CommandName("Tools.AllMembers", "Alt+G")] public const int CODE_BROWSER = 100;
		[CommandName("Tools.ClassesInterfacesOnly", "Alt+C")] public const int CODE_BROWSER_CI = 101;
		[CommandName("Tools.MethodsOnly", "Alt+M")] public const int CODE_BROWSER_M = 102;
		[CommandName("Tools.PropertiesOnly", "Shift+Alt+P")] public const int CODE_BROWSER_P = 103;

		[CommandName("Tools.FileBrowser", "Alt+U")] public const int FILE_BROWSER = 200;

		[CommandName("Edit.ToggleBookmark1", "Ctrl+Shift+1")] public const int BOOKMARK_SET_1 = 500;
		[CommandName("Edit.ToggleBookmark2", "Ctrl+Shift+2")] public const int BOOKMARK_SET_2 = 501;
		[CommandName("Edit.ToggleBookmark3", "Ctrl+Shift+3")] public const int BOOKMARK_SET_3 = 502;
		[CommandName("Edit.ToggleBookmark4", "Ctrl+Shift+4")] public const int BOOKMARK_SET_4 = 503;
		[CommandName("Edit.ToggleBookmark5", "Ctrl+Shift+5")] public const int BOOKMARK_SET_5 = 504;
		[CommandName("Edit.ToggleBookmark6", "Ctrl+Shift+6")] public const int BOOKMARK_SET_6 = 505;
		[CommandName("Edit.ToggleBookmark7", "Ctrl+Shift+7")] public const int BOOKMARK_SET_7 = 506;
		[CommandName("Edit.ToggleBookmark8", "Ctrl+Shift+8")] public const int BOOKMARK_SET_8 = 507;
		[CommandName("Edit.ToggleBookmark9", "Ctrl+Shift+9")] public const int BOOKMARK_SET_9 = 508;
		[CommandName("Edit.ToggleBookmark0", "Ctrl+Shift+0")] public const int BOOKMARK_SET_0 = 509;
		[CommandName("Edit.GoToBookmark1", "Ctrl+1")] public const int BOOKMARK_GET_1 = 510;
		[CommandName("Edit.GoToBookmark2", "Ctrl+2")] public const int BOOKMARK_GET_2 = 511;
		[CommandName("Edit.GoToBookmark3", "Ctrl+3")] public const int BOOKMARK_GET_3 = 512;
		[CommandName("Edit.GoToBookmark4", "Ctrl+4")] public const int BOOKMARK_GET_4 = 513;
		[CommandName("Edit.GoToBookmark5", "Ctrl+5")] public const int BOOKMARK_GET_5 = 514;
		[CommandName("Edit.GoToBookmark6", "Ctrl+6")] public const int BOOKMARK_GET_6 = 515;
		[CommandName("Edit.GoToBookmark7", "Ctrl+7")] public const int BOOKMARK_GET_7 = 516;
		[CommandName("Edit.GoToBookmark8", "Ctrl+8")] public const int BOOKMARK_GET_8 = 517;
		[CommandName("Edit.GoToBookmark9", "Ctrl+9")] public const int BOOKMARK_GET_9 = 518;
		[CommandName("Edit.GoToBookmark0", "Ctrl+0")] public const int BOOKMARK_GET_0 = 519;
		[CommandName("Edit.ClearAllFileBookmarks", "Ctrl+K, C")] public const int BOOKMARK_CLEAR_F = 520;
		[CommandName("Edit.ClearAllSolutionBookmarks", "Ctrl+K, A")] public const int BOOKMARK_CLEAR_S = 521;
		[CommandName("Edit.ToggleGlobalBookmark1", "Ctrl+Shift+Alt+1")] public const int BOOKMARK_SET_GLB_1 = 530;
		[CommandName("Edit.ToggleGlobalBookmark2", "Ctrl+Shift+Alt+2")] public const int BOOKMARK_SET_GLB_2 = 531;
		[CommandName("Edit.ToggleGlobalBookmark3", "Ctrl+Shift+Alt+3")] public const int BOOKMARK_SET_GLB_3 = 532;
		[CommandName("Edit.ToggleGlobalBookmark4", "Ctrl+Shift+Alt+4")] public const int BOOKMARK_SET_GLB_4 = 533;
		[CommandName("Edit.ToggleGlobalBookmark5", "Ctrl+Shift+Alt+5")] public const int BOOKMARK_SET_GLB_5 = 534;
		[CommandName("Edit.ToggleGlobalBookmark6", "Ctrl+Shift+Alt+6")] public const int BOOKMARK_SET_GLB_6 = 535;
		[CommandName("Edit.ToggleGlobalBookmark7", "Ctrl+Shift+Alt+7")] public const int BOOKMARK_SET_GLB_7 = 536;
		[CommandName("Edit.ToggleGlobalBookmark8", "Ctrl+Shift+Alt+8")] public const int BOOKMARK_SET_GLB_8 = 537;
		[CommandName("Edit.ToggleGlobalBookmark9", "Ctrl+Shift+Alt+9")] public const int BOOKMARK_SET_GLB_9 = 538;
		[CommandName("Edit.ToggleGlobalBookmark0", "Ctrl+Shift+Alt+0")] public const int BOOKMARK_SET_GLB_0 = 539;
		[CommandName("Edit.GoToGlobalBookmark1", "Ctrl+Alt+1")] public const int BOOKMARK_GET_GLB_1 = 540;
		[CommandName("Edit.GoToGlobalBookmark2", "Ctrl+Alt+2")] public const int BOOKMARK_GET_GLB_2 = 541;
		[CommandName("Edit.GoToGlobalBookmark3", "Ctrl+Alt+3")] public const int BOOKMARK_GET_GLB_3 = 542;
		[CommandName("Edit.GoToGlobalBookmark4", "Ctrl+Alt+4")] public const int BOOKMARK_GET_GLB_4 = 543;
		[CommandName("Edit.GoToGlobalBookmark5", "Ctrl+Alt+5")] public const int BOOKMARK_GET_GLB_5 = 544;
		[CommandName("Edit.GoToGlobalBookmark6", "Ctrl+Alt+6")] public const int BOOKMARK_GET_GLB_6 = 545;
		[CommandName("Edit.GoToGlobalBookmark7", "Ctrl+Alt+7")] public const int BOOKMARK_GET_GLB_7 = 546;
		[CommandName("Edit.GoToGlobalBookmark8", "Ctrl+Alt+8")] public const int BOOKMARK_GET_GLB_8 = 547;
		[CommandName("Edit.GoToGlobalBookmark9", "Ctrl+Alt+9")] public const int BOOKMARK_GET_GLB_9 = 548;
		[CommandName("Edit.GoToGlobalBookmark0", "Ctrl+Alt+0")] public const int BOOKMARK_GET_GLB_0 = 549;

		[CommandName("Edit.TryCatch", "Ctrl+K, X")] public const int SW_TRY_CATCH = 600;
		[CommandName("Edit.TryFinally", "Ctrl+K, T")] public const int SW_TRY_FINALLY = 601;
		[CommandName("Edit.For", "Ctrl+K, F")] public const int SW_FOR = 602;
		[CommandName("Edit.ForEach", "Ctrl+K, E")] public const int SW_FOR_EACH = 603;
		[CommandName("Edit.Region", "Ctrl+K, R")] public const int SW_REGION = 604;

		[CommandName("Tools.DPackWebSite")] public const int PROJECT_HOME = 700;
		[CommandName("Tools.DPackEmailUs")] public const int SUPPORT_EMAIL = 701;
		[CommandName("Tools.DPackOptions")] public const int OPTIONS = 710;

		[CommandName("Tools.SolutionStatistics")] public const int CODE_STATS = 800;

		[CommandName("Tools.SolutionBackup", "Shift+Alt+B")] public const int BACKUP = 900;

		[CommandName("ProjectandSolutionContextMenus.Solution.CollapseAllProjects", "Shift+Alt+C")] public const int COLLAPSE_SOLUTION_CONTEXT = 1101;
		[CommandName("ProjectandSolutionContextMenus.ReferenceRoot.Copy")] public const int COPY_REFERENCES_CONTEXT = 1102;
		[CommandName("ProjectandSolutionContextMenus.ReferenceRoot.Paste")] public const int PASTE_REFERENCES_CONTEXT = 1103;
		[CommandName("ProjectandSolutionContextMenus.ReferenceRoot.Copy")] public const int COPY_REFERENCE_CONTEXT = 1104;
		[CommandName("OtherContextMenus.EasyMDIDocumentWindow.LocateInSolutionExplorer", "Shift+Alt+L")] public const int LOCATE_IN_SOLUTION_EXPLORER_CONTEXT = 1105;
		[CommandName("ProjectandSolutionContextMenus.Project.OpenCommandPrompt")] public const int COMMAND_PROMPT = 1106;
		[CommandName("ProjectandSolutionContextMenus.Project.CopyFullPath")] public const int COPY_FULL_PATH = 1107;
	}
}