using System;
using System.Runtime.InteropServices;

using DPackRx.Features.SurroundWith;

using EnvDTE;

using Microsoft.VisualStudio.Shell;

namespace DPackRx.Services
{
	/// <summary>
	/// Surround with formatter.
	/// </summary>
	public class SurroundWithFormatterService : ISurroundWithFormatterService
	{
		#region Fields

		private readonly IShellSelectionService _shellSelectionService;
		private readonly IShellHelperService _shellHelperService;

		private const int E_ABORT = -2147467260;
		private const string COMMAND_NEXT_WORD = "Edit.WordNext";
		private const string COMMAND_SELECT_WORD = "Edit.SelectCurrentWord";

		#endregion

		public SurroundWithFormatterService(IShellSelectionService shellSelectionService, IShellHelperService shellHelperService)
		{
			_shellSelectionService = shellSelectionService;
			_shellHelperService = shellHelperService;
		}

		#region ISurroundWithFormatterService Members

		/// <summary>
		/// Formats surround with according to provided model.
		/// </summary>
		public void Format(SurroundWithLanguageModel model)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			if (model == null)
				throw new ArgumentNullException(nameof(model));

			Document activeDocument = _shellSelectionService.GetActiveDocument() as Document;
			if (activeDocument == null)
				return;

			TextSelection selection = activeDocument.Selection as TextSelection;
			if (selection == null)
				return;

			TextRanges colRanges = selection.TextRanges;
			TextRange textRangeStart = colRanges.Item(1);
			TextRange textRangeEnd = colRanges.Item(colRanges.Count);
			EditPoint ptSelectionStart;
			EditPoint ptSelectionEnd;
			bool addLastLineBreak;
			string firstLinePrefix;

			try
			{
				if ((selection.TopPoint.Line == selection.BottomPoint.Line) &&
						(selection.TopPoint.LineCharOffset == selection.BottomPoint.LineCharOffset))
				{
					// No selection case
					firstLinePrefix = string.Empty;

					EditPoint ptWork = textRangeStart.StartPoint.CreateEditPoint();
					ptWork.EndOfLine();
					if (ptWork.LineLength != 0)
						ptWork.Insert(Environment.NewLine);
					ptSelectionStart = ptWork.CreateEditPoint();

					ptWork.Insert(model.StartingCode);
					if (!model.StartingCode.EndsWith(Environment.NewLine))
						ptWork.Insert(Environment.NewLine);
					ptWork.Insert(model.EndingCode);
					ptSelectionEnd = ptWork.CreateEditPoint();
					addLastLineBreak = (!ptSelectionEnd.AtEndOfLine);
				}
				else
				{
					ptSelectionStart = textRangeStart.StartPoint.CreateEditPoint();
					ptSelectionEnd = textRangeEnd.EndPoint;
					if ((textRangeEnd.StartPoint.Line == ptSelectionEnd.Line) &&
							(textRangeEnd.StartPoint.LineCharOffset == ptSelectionEnd.LineCharOffset))
						addLastLineBreak = true;
					else
						addLastLineBreak = false;

					textRangeStart.StartPoint.Insert(model.StartingCode);
					if (!model.StartingCode.EndsWith(Environment.NewLine))
						textRangeStart.StartPoint.Insert(Environment.NewLine);
					if ((ptSelectionEnd.AtEndOfLine) && (!ptSelectionEnd.AtStartOfLine))
						ptSelectionEnd.Insert(Environment.NewLine);
					ptSelectionEnd.Insert(model.EndingCode);
				}

				if (addLastLineBreak)
					ptSelectionEnd.Insert(Environment.NewLine);
			}
			catch (Exception ex)
			{
				if ((ex is COMException comEx) && (comEx.ErrorCode == E_ABORT))
					return;
				else
					throw;
			}

			selection.MoveToPoint(ptSelectionStart, false);
			selection.MoveToPoint(ptSelectionEnd, true);
			selection.SmartFormat();

			// Jump to the element first line/column
			selection.MoveToPoint(ptSelectionStart, false);
			selection.StartOfLine(vsStartOfLineOptions.vsStartOfLineOptionsFirstText, false);

			// Select a word to be filled out
			if (model.WordOffset > 0)
			{
				for (int i = 1; i <= model.WordOffset; i++)
				{
					_shellHelperService.ExecuteCommand(COMMAND_NEXT_WORD, string.Empty);
				}

				_shellHelperService.ExecuteCommand(COMMAND_SELECT_WORD, string.Empty);
			}
		}

		#endregion
	}
}