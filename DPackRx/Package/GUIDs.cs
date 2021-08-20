using System;

namespace DPackRx.Package
{
	/// <summary>
	/// Must match VSCT resources.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "All Context Guids Copied from EnvDTE80")]
	public static class GUIDs
	{
		/// <summary>
		/// Must match VSIX manifest Product ID.
		/// </summary>
		public const string PRODUCT_ID = "EE57E48A-8422-4CEC-B415-3E3BA31F1199";

		public static readonly Guid CommandSet = new Guid("{BD0B0377-DC68-4C27-A9A3-D240F33367D4}");

		public const string GUID_XAML_EDITOR_FACTORY = "4C87B692-1202-46AA-B64C-EF01FAEC53DA";
		public const string GUID_XAML_LANGUAGE_SERVICE = "C9164055-039B-4669-832D-F257BD5554D4";
		public const string GUID_NEW_XAML_LANGUAGE_SERVICE = "CD53C9A1-6BC2-412B-BE36-CC715ED8DD41";
		public const string GUID_XAML_DESIGNER = "2E06F766-183C-46F9-B8AC-BC4FB78480A7";

		public const string OPTIONS_GENERAL = "F26A055B-47EC-4642-839F-3240A2594742";
		public const string OPTIONS_FILE_BROWSER = "B998A0CD-69D6-4980-9240-5EC65F3B6E02";

		#region envDTE ContextGuids

		
		public const string vsContextGuidSolutionBuilding = "{ADFC4E60-0397-11D1-9F4E-00A0C911004F}";
		//
		// Summary:
		//     Represents whether the Search pane of the Help window is displayed.
		public const string vsContextGuidHelpSearch = "{46C87F81-5A06-43A8-9E25-85D33BAC49F8}";
		//
		// Summary:
		//     Represents whether the Index tab of the Help window is displayed.
		public const string vsContextGuidHelpIndex = "{73F6DD58-437E-11D3-B88E-00C04F79F802}";
		//
		// Summary:
		//     Represents whether the Table of Contents tab of the Help window is displayed.
		public const string vsContextGuidHelpContents = "{4A791147-19E4-11D3-B86B-00C04F79F802}";
		//
		// Summary:
		//     Represents whether the Call Browser window is displayed. (Visual C++ only.)
		public const string vsContextGuidCallBrowser = "{5415EA3A-D813-4948-B51E-562082CE0887}";
		//
		// Summary:
		//     Represents whether the Code Definition Window is displayed.
		public const string vsContextGuidCodeDefinition = "{588470CC-84F8-4A57-9AC4-86BCA0625FF4}";
		//
		// Summary:
		//     Represents whether the Task List (Visual Studio) is displayed.
		public const string vsContextGuidTaskList = "{4A9B7E51-AA16-11D0-A8C5-00A0C921A4D2}";
		//
		// Summary:
		//     Represents whether the Toolbox is displayed.
		public const string vsContextGuidToolbox = "{B1E99781-AB81-11D0-B683-00AA00A3EE26}";
		//
		// Summary:
		//     Represents whether the Call Stack window is displayed.
		public const string vsContextGuidCallStack = "{0504FF91-9D61-11D0-A794-00A0C9110051}";
		//
		// Summary:
		//     Represents the Threads window.
		public const string vsContextGuidThread = "{E62CE6A0-B439-11D0-A79D-00A0C9110051}";
		//
		// Summary:
		//     Represents whether the Locals window is displayed.
		public const string vsContextGuidLocals = "{4A18F9D0-B838-11D0-93EB-00A0C90F2734}";
		//
		// Summary:
		//     Represents whether the AutoLocals window is displayed.
		public const string vsContextGuidAutoLocals = "{F2E84780-2AF1-11D1-A7FA-00A0C9110051}";
		//
		// Summary:
		//     Represents the Watch window.
		public const string vsContextGuidWatch = "{90243340-BD7A-11D0-93EF-00A0C90F2734}";
		//
		// Summary:
		//     Represents whether the Properties Window is displayed.
		public const string vsContextGuidProperties = "{EEFA5220-E298-11D0-8F78-00A0C9110057}";
		//
		// Summary:
		//     Represents whether Solution Explorer is displayed.
		public const string vsContextGuidSolutionExplorer = "{3AE79031-E1BC-11D0-8F78-00A0C9110057}";
		//
		// Summary:
		//     Represents whether the Output Window is displayed.
		public const string vsContextGuidOutput = "{34E76E81-EE4A-11D0-AE2E-00A0C90FFFC3}";
		//
		// Summary:
		//     Represents whether Object Browser is displayed.
		public const string vsContextGuidObjectBrowser = "{269A02DC-6AF8-11D3-BDC4-00C04F688E50}";
		//
		// Summary:
		//     Represents whether the Macro Explorer Window is displayed.
		public const string vsContextGuidMacroExplorer = "{07CD18B4-3BA1-11D2-890A-0060083196C6}";
		//
		// Summary:
		//     Represents whether the Dynamic Help window is displayed.
		public const string vsContextGuidDynamicHelp = "{66DBA47C-61DF-11D2-AA79-00C04F990343}";
		//
		// Summary:
		//     Represents whether Class View is displayed.
		public const string vsContextGuidClassView = "{C9C0AE26-AA77-11D2-B3F0-0000F87570EE}";
		//
		// Summary:
		//     Represents whether the Resource View Window is displayed.
		public const string vsContextGuidResourceView = "{2D7728C2-DE0A-45B5-99AA-89B609DFDE73}";
		//
		// Summary:
		//     Represents whether the Document Outline window is displayed.
		public const string vsContextGuidDocumentOutline = "{25F7E850-FFA1-11D0-B63F-00A0C922E851}";
		//
		// Summary:
		//     Represents whether Server Explorer/Database Explorer is displayed.
		public const string vsContextGuidServerExplorer = "{74946827-37A0-11D2-A273-00C04F8EF4FF}";
		//
		// Summary:
		//     Represents whether the Command Window is displayed.
		public const string vsContextGuidCommandWindow = "{28836128-FC2C-11D2-A433-00C04F72D18A}";
		//
		// Summary:
		//     Represents whether the Find Symbol window is displayed.
		public const string vsContextGuidFindSymbol = "{53024D34-0EF5-11D3-87E0-00C04F7971A5}";
		//
		// Summary:
		//     Represents whether the Find Symbol Results Window is displayed.
		public const string vsContextGuidFindSymbolResults = "{68487888-204A-11D3-87EB-00C04F7971A5}";
		//
		// Summary:
		//     Represents whether the Find and Replace Window is displayed.
		public const string vsContextGuidFindReplace = "{CF2DDC32-8CAD-11D2-9302-005345000000}";
		//
		// Summary:
		//     Represents whether the Find Results Windows 1 is displayed.
		public const string vsContextGuidFindResults1 = "{0F887920-C2B6-11D2-9375-0080C747D9A0}";
		//
		// Summary:
		//     Represents whether the Find Results Windows 2 is displayed.
		public const string vsContextGuidFindResults2 = "{0F887921-C2B6-11D2-9375-0080C747D9A0}";
		//
		// Summary:
		//     Represents the main Visual Studio window.
		public const string vsContextGuidMainWindow = "{9DDABE98-1D02-11D3-89A1-00C04F688DDE}";
		//
		// Summary:
		//     Represents whether the Error List Window is displayed.
		public const string vsContextGuidErrorList = "{D78612C7-9962-4B83-95D9-268046DAD23A}";
		public const string vsContextGuidFavorites = "{57DC5D59-11C2-4955-A7B4-D7699D677E93}";
		//
		// Summary:
		//     Represents whether the Application Browser is displayed.
		public const string vsContextGuidApplicationBrowser = "{399832EA-70A8-4AE7-9B99-3C0850DAD152}";
		//
		// Summary:
		//     Represents whether the Bookmark Window is displayed.
		public const string vsContextGuidBookmarks = "{A0C5197D-0AC7-4B63-97CD-8872A789D233}";
		//
		// Summary:
		//     Represents whether the integrated development environment (IDE) is in debugging
		//     mode.
		public const string vsContextGuidDebugging = "{ADFC4E61-0397-11D1-9F4E-00A0C911004F}";
		//
		// Summary:
		//     Represents an item that is being dragged to or from a TreeView Control (Windows
		//     Forms) or other hierarchical control.
		public const string vsContextGuidUIHierarchyDragging = "{B706F393-2E5B-49E7-9E2E-B1825F639B63}";
		//
		// Summary:
		//     Represents whether the integrated development environment (IDE) is in full-screen
		//     view, rather than windowed view.
		public const string vsContextGuidFullScreenMode = "{ADFC4E62-0397-11D1-9F4E-00A0C911004F}";
		//
		// Summary:
		//     Represents whether the integrated development environment (IDE) is in Design
		//     view.
		public const string vsContextGuidDesignMode = "{ADFC4E63-0397-11D1-9F4E-00A0C911004F}";
		//
		// Summary:
		//     Represents whether the integrated development environment (IDE) is open but with
		//     no solution loaded.
		public const string vsContextGuidNoSolution = "{ADFC4E64-0397-11D1-9F4E-00A0C911004F}";
		//
		// Summary:
		//     Represents whether a solution is loaded in the integrated development environment
		//     (IDE).
		public const string vsContextGuidSolutionExists = "{F1536EF8-92EC-443C-9ED7-FDADF150DA82}";
		//
		// Summary:
		//     Represents whether an empty solution (one without projects) is open in the integrated
		//     development environment (IDE).
		public const string vsContextGuidEmptySolution = "{ADFC4E65-0397-11D1-9F4E-00A0C911004F}";
		//
		// Summary:
		//     Represents whether the current solution contains only one project.
		public const string vsContextGuidSolutionHasSingleProject = "{ADFC4E66-0397-11D1-9F4E-00A0C911004F}";
		//
		// Summary:
		//     Represents whether the current solution contains more than one project.
		public const string vsContextGuidSolutionHasMultipleProjects = "{93694FA0-0397-11D1-9F4E-00A0C911004F}";
		//
		// Summary:
		//     Represents whether the Code and Text Editor is visible.
		public const string vsContextGuidCodeWindow = "{8FE2DF1D-E0DA-4EBE-9D5C-415D40E487B5}";
		//
		// Summary:
		//     Represents whether the integrated development environment (IDE) is not building
		//     or debugging code.
		public const string vsContextGuidNotBuildingAndNotDebugging = "{48EA4A80-F14E-4107-88FA-8D0016F30B9C}";
		//
		// Summary:
		//     Represents whether the current solution, or project in the solution, is being
		//     upgraded.
		public const string vsContextGuidSolutionOrProjectUpgrading = "{EF4F870B-7B85-4F29-9D15-CE1ABFBE733B}";
		//
		// Summary:
		//     Represents whether the Data Sources Window is supported in the current context.
		public const string vsContextGuidDataSourceWindowSupported = "{95C314C4-660B-4627-9F82-1BAF1C764BBF}";
		//
		// Summary:
		//     Represents whether the Data Sources Window is visible.
		public const string vsContextGuidDataSourceWindowAutoVisible = "{2E78870D-AC7C-4460-A4A1-3FE37D00EF81}";
		//
		// Summary:
		//     Represents whether the current window is a linked window.
		public const string vsContextGuidLinkedWindowFrame = "{9DDABE99-1D02-11D3-89A1-00C04F688DDE}";
		//
		// Summary:
		//     Represents whether the Windows Forms Designer is displayed.
		public const string vsContextGuidWindowsFormsDesigner = "{BA09E2AF-9DF2-4068-B2F0-4C7E5CC19E2F}";
		//
		// Summary:
		//     Represents whether a solution is loaded but not being built or debugged.
		public const string vsContextGuidSolutionExistsAndNotBuildingAndNotDebugging = "{D0E4DEEC-1B53-4CDA-8559-D454583AD23B}";
		//
		// Summary:
		//     Represents whether the Code and Text Editor is displayed.
		public const string vsContextGuidTextEditor = "{8B382828-6202-11D1-8870-0000F87579D2}";
		//
		// Summary:
		//     Represents whether the XML Editor window is displayed.
		public const string vsContextGuidXMLTextEditor = "{F6819A78-A205-47B5-BE1C-675B3C7F0B8E}";
		//
		// Summary:
		//     Represents whether the CSS Editor is displayed.
		public const string vsContextGuidCSSTextEditor = "{A764E898-518D-11D2-9A89-00C04F79EFC3}";
		//
		// Summary:
		//     Represents whether the Editor pane of the HTML Source editor is displayed.
		public const string vsContextGuidHTMLSourceEditor = "{58E975A0-F8FE-11D2-A6AE-00104BCC7269}";
		//
		// Summary:
		//     Represents whether the Code and Text Editor is in Design view.
		public const string vsContextGuidHTMLDesignView = "{CB3FCFEA-03DF-11D1-81D2-00A0C91BBEE3}";
		//
		// Summary:
		//     Represents whether the View pane of the HTML Source editor is displayed.
		public const string vsContextGuidHTMLSourceView = "{CB3FCFEB-03DF-11D1-81D2-00A0C91BBEE3}";
		//
		// Summary:
		//     Represents whether the Code View of the editor is displayed.
		public const string vsContextGuidHTMLCodeView = "{4C01CBEE-FB8C-4ED0-8EC0-68348C52822E}";
		//
		// Summary:
		//     Represents whether the current context contains a window frame.
		public const string vsContextGuidFrames = "{CB3FCFEC-03DF-11D1-81D2-00A0C91BBEE3}";
		//
		// Summary:
		//     Represents whether the Schema view is displayed.
		public const string vsContextGuidSchema = "{E6631B5B-2EAB-41E8-82FD-6469645C76C9}";
		public const string vsContextGuidData = "{F482F8AF-1E66-4760-919E-964707265994}";
		//
		// Summary:
		//     Represents whether the Start Page is displayed.
		public const string vsContextGuidKindStartPage = "{387CB18D-6153-4156-9257-9AC3F9207BBE}";
		//
		// Summary:
		//     Represents whether the CodeZone Community window is displayed.
		public const string vsContextGuidCommunityWindow = "{96DB1F3B-0E7A-4406-B73E-C6F0A2C67B97}";
		public const string vsContextGuidDeviceExplorer = "{B65E9355-A4C7-4855-96BB-1D3EC8514E8F}";
		//
		// Summary:
		//     Represents whether the Toolbox is being started and intialized.
		public const string vsContextGuidToolboxInitialized = "{DC5DB425-F0FD-4403-96A1-F475CDBA9EE0}";
		//
		// Summary:
		//     Represents whether the internal Visual Studio web browser is displayed.
		public const string vsContextGuidWebBrowser = "{E8B06F52-6D01-11D2-AA7D-00C04F990343}";

		#endregion
	}
}