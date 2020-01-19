using System;
using System.Collections.Generic;

using DPackRx.Options;
using DPackRx.Package;
using DPackRx.Services;

namespace DPackRx.Features.Bookmarks
{
	/// <summary>
	/// Bookmarks feature.
	/// </summary>
	[KnownFeature(KnownFeature.Bookmarks)]
	public class BookmarksFeature : Feature
	{
		#region Fields

		private readonly IBookmarksService _bookmarksService;
		private readonly IShellSelectionService _shellSelectionService;

		#endregion

		public BookmarksFeature(IServiceProvider serviceProvider, ILog log, IOptionsService optionsService,
			IBookmarksService bookmarksService, IShellSelectionService shellSelectionService) : base(serviceProvider, log, optionsService)
		{
			_bookmarksService = bookmarksService;
			_shellSelectionService = shellSelectionService;
		}

		// Test constructor
		protected internal BookmarksFeature() : base()
		{
		}

		#region Feature Overrides

		/// <summary>
		/// Returns all commands.
		/// </summary>
		/// <returns>Command Ids.</returns>
		public override ICollection<int> GetCommandIds()
		{
			return new List<int>(new[] {
				CommandIDs.BOOKMARK_GET_1, CommandIDs.BOOKMARK_GET_2, CommandIDs.BOOKMARK_GET_3, CommandIDs.BOOKMARK_GET_4, CommandIDs.BOOKMARK_GET_5,
				CommandIDs.BOOKMARK_GET_6, CommandIDs.BOOKMARK_GET_7, CommandIDs.BOOKMARK_GET_8, CommandIDs.BOOKMARK_GET_9, CommandIDs.BOOKMARK_GET_0,
				CommandIDs.BOOKMARK_SET_1, CommandIDs.BOOKMARK_SET_2, CommandIDs.BOOKMARK_SET_3, CommandIDs.BOOKMARK_SET_4, CommandIDs.BOOKMARK_SET_5,
				CommandIDs.BOOKMARK_SET_6, CommandIDs.BOOKMARK_SET_7, CommandIDs.BOOKMARK_SET_8, CommandIDs.BOOKMARK_SET_9, CommandIDs.BOOKMARK_SET_0,
				CommandIDs.BOOKMARK_GET_GLB_1, CommandIDs.BOOKMARK_GET_GLB_2, CommandIDs.BOOKMARK_GET_GLB_3, CommandIDs.BOOKMARK_GET_GLB_4, CommandIDs.BOOKMARK_GET_GLB_5,
				CommandIDs.BOOKMARK_GET_GLB_6, CommandIDs.BOOKMARK_GET_GLB_7, CommandIDs.BOOKMARK_GET_GLB_8, CommandIDs.BOOKMARK_GET_GLB_9, CommandIDs.BOOKMARK_GET_GLB_0,
				CommandIDs.BOOKMARK_SET_GLB_1, CommandIDs.BOOKMARK_SET_GLB_2, CommandIDs.BOOKMARK_SET_GLB_3, CommandIDs.BOOKMARK_SET_GLB_4, CommandIDs.BOOKMARK_SET_GLB_5,
				CommandIDs.BOOKMARK_SET_GLB_6, CommandIDs.BOOKMARK_SET_GLB_7, CommandIDs.BOOKMARK_SET_GLB_8, CommandIDs.BOOKMARK_SET_GLB_9, CommandIDs.BOOKMARK_SET_GLB_0,
				CommandIDs.BOOKMARK_CLEAR_F, CommandIDs.BOOKMARK_CLEAR_S
			});
		}

		/// <summary>
		/// Checks if command is available or not.
		/// </summary>
		/// <param name="commandId">Command Id.</param>
		/// <returns>Command status.</returns>
		public override bool IsValidContext(int commandId)
		{
			switch (commandId)
			{
				case CommandIDs.BOOKMARK_SET_1:
				case CommandIDs.BOOKMARK_SET_2:
				case CommandIDs.BOOKMARK_SET_3:
				case CommandIDs.BOOKMARK_SET_4:
				case CommandIDs.BOOKMARK_SET_5:
				case CommandIDs.BOOKMARK_SET_6:
				case CommandIDs.BOOKMARK_SET_7:
				case CommandIDs.BOOKMARK_SET_8:
				case CommandIDs.BOOKMARK_SET_9:
				case CommandIDs.BOOKMARK_SET_0:
				case CommandIDs.BOOKMARK_GET_1:
				case CommandIDs.BOOKMARK_GET_2:
				case CommandIDs.BOOKMARK_GET_3:
				case CommandIDs.BOOKMARK_GET_4:
				case CommandIDs.BOOKMARK_GET_5:
				case CommandIDs.BOOKMARK_GET_6:
				case CommandIDs.BOOKMARK_GET_7:
				case CommandIDs.BOOKMARK_GET_8:
				case CommandIDs.BOOKMARK_GET_9:
				case CommandIDs.BOOKMARK_GET_0:
				case CommandIDs.BOOKMARK_SET_GLB_1:
				case CommandIDs.BOOKMARK_SET_GLB_2:
				case CommandIDs.BOOKMARK_SET_GLB_3:
				case CommandIDs.BOOKMARK_SET_GLB_4:
				case CommandIDs.BOOKMARK_SET_GLB_5:
				case CommandIDs.BOOKMARK_SET_GLB_6:
				case CommandIDs.BOOKMARK_SET_GLB_7:
				case CommandIDs.BOOKMARK_SET_GLB_8:
				case CommandIDs.BOOKMARK_SET_GLB_9:
				case CommandIDs.BOOKMARK_SET_GLB_0:
				case CommandIDs.BOOKMARK_GET_GLB_1:
				case CommandIDs.BOOKMARK_GET_GLB_2:
				case CommandIDs.BOOKMARK_GET_GLB_3:
				case CommandIDs.BOOKMARK_GET_GLB_4:
				case CommandIDs.BOOKMARK_GET_GLB_5:
				case CommandIDs.BOOKMARK_GET_GLB_6:
				case CommandIDs.BOOKMARK_GET_GLB_7:
				case CommandIDs.BOOKMARK_GET_GLB_8:
				case CommandIDs.BOOKMARK_GET_GLB_9:
				case CommandIDs.BOOKMARK_GET_GLB_0:
				case CommandIDs.BOOKMARK_CLEAR_F:
				case CommandIDs.BOOKMARK_CLEAR_S:
					return
						_shellSelectionService.IsContextActive(ContextType.SolutionExists) && (
						_shellSelectionService.IsContextActive(ContextType.TextEditor) ||
						_shellSelectionService.IsContextActive(ContextType.XMLTextEditor) ||
						_shellSelectionService.IsContextActive(ContextType.XamlEditor) ||
						_shellSelectionService.IsContextActive(ContextType.NewXamlEditor) ||
						_shellSelectionService.IsContextActive(ContextType.HTMLSourceEditor) ||
						_shellSelectionService.IsContextActive(ContextType.CSSTextEditor));
				default:
					return false;
			}
		}

		/// <summary>
		/// Executes a command.
		/// </summary>
		/// <param name="commandId">Command Id.</param>
		/// <returns>Execution status.</returns>
		public override bool Execute(int commandId)
		{
			switch (commandId)
			{
				case CommandIDs.BOOKMARK_SET_1:
				case CommandIDs.BOOKMARK_SET_2:
				case CommandIDs.BOOKMARK_SET_3:
				case CommandIDs.BOOKMARK_SET_4:
				case CommandIDs.BOOKMARK_SET_5:
				case CommandIDs.BOOKMARK_SET_6:
				case CommandIDs.BOOKMARK_SET_7:
				case CommandIDs.BOOKMARK_SET_8:
				case CommandIDs.BOOKMARK_SET_9:
				case CommandIDs.BOOKMARK_SET_0:
					var number = commandId - CommandIDs.BOOKMARK_SET_1 + 1;
					return _bookmarksService.SetBookmark(number);

				case CommandIDs.BOOKMARK_GET_1:
				case CommandIDs.BOOKMARK_GET_2:
				case CommandIDs.BOOKMARK_GET_3:
				case CommandIDs.BOOKMARK_GET_4:
				case CommandIDs.BOOKMARK_GET_5:
				case CommandIDs.BOOKMARK_GET_6:
				case CommandIDs.BOOKMARK_GET_7:
				case CommandIDs.BOOKMARK_GET_8:
				case CommandIDs.BOOKMARK_GET_9:
				case CommandIDs.BOOKMARK_GET_0:
					number = commandId - CommandIDs.BOOKMARK_GET_1 + 1;
					return _bookmarksService.GoToBookmark(number);

				case CommandIDs.BOOKMARK_SET_GLB_1:
				case CommandIDs.BOOKMARK_SET_GLB_2:
				case CommandIDs.BOOKMARK_SET_GLB_3:
				case CommandIDs.BOOKMARK_SET_GLB_4:
				case CommandIDs.BOOKMARK_SET_GLB_5:
				case CommandIDs.BOOKMARK_SET_GLB_6:
				case CommandIDs.BOOKMARK_SET_GLB_7:
				case CommandIDs.BOOKMARK_SET_GLB_8:
				case CommandIDs.BOOKMARK_SET_GLB_9:
				case CommandIDs.BOOKMARK_SET_GLB_0:
					number = commandId - CommandIDs.BOOKMARK_SET_GLB_1 + 1;
					return _bookmarksService.SetGlobalBookmark(number);

				case CommandIDs.BOOKMARK_GET_GLB_1:
				case CommandIDs.BOOKMARK_GET_GLB_2:
				case CommandIDs.BOOKMARK_GET_GLB_3:
				case CommandIDs.BOOKMARK_GET_GLB_4:
				case CommandIDs.BOOKMARK_GET_GLB_5:
				case CommandIDs.BOOKMARK_GET_GLB_6:
				case CommandIDs.BOOKMARK_GET_GLB_7:
				case CommandIDs.BOOKMARK_GET_GLB_8:
				case CommandIDs.BOOKMARK_GET_GLB_9:
				case CommandIDs.BOOKMARK_GET_GLB_0:
					number = commandId - CommandIDs.BOOKMARK_GET_GLB_1 + 1;
					return _bookmarksService.GoToGlobalBookmark(number);

				case CommandIDs.BOOKMARK_CLEAR_F:
					return _bookmarksService.ClearFileBookmarks();

				case CommandIDs.BOOKMARK_CLEAR_S:
					return _bookmarksService.ClearAllBookmarks();

				default:
					return false;
			}
		}

		#endregion
	}
}