using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

using DPackRx.Helpers;

namespace DPackRx.CodeModel
{
	/// <summary>
	/// File model.
	/// </summary>
	[DebuggerDisplay("{FileName}, {ItemSubType}, Member Count = {Members.Count}, Matched = {Matched}")]
	public class FileModel : INotifyPropertyChanged, IMatchItem, IExtensibilityItem
	{
		#region Fields

		private int _dataEndingIndex = NOT_SET;
		private string _pascalCasedData;

		private const int NOT_SET = -10;

		#endregion

		#region Properties

		/// <summary>
		/// File name without path.
		/// </summary>
		public string FileName { get; set; }

		/// <summary>
		/// File name with path.
		/// </summary>
		public string FileNameWithPath { get; set; }

		/// <summary>
		/// File's project.
		/// </summary>
		public ProjectModel Project { get; set; }

		/// <summary>
		/// Untyped extensibility link (name suffix matches the actual type).
		/// </summary>
		public object ParentProjectItem { get; set; }

		/// <summary>
		/// File parent name.
		/// </summary>
		public string ParentName { get; set; }

		/// <summary>
		/// Project name or comma separated project names file's linked to.
		/// </summary>
		public string ProjectName { get; set; }

		/// <summary>
		/// Optional file code members.
		/// </summary>
		public ICollection<MemberCodeModel> Members { get; private set; } = new List<MemberCodeModel>();

		#endregion

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged
		{
			add { }
			remove { }
		}

		#endregion

		#region IComparable Members - used for sorting

		public int CompareTo(object obj)
		{
			return CompareTo(obj as IMatchItem);
		}

		#endregion

		#region IComparable<IMatchItem>

		public int CompareTo(IMatchItem obj)
		{
			var item = obj as FileModel;
			if (item == null)
				return -1;

			if (((item.Rank == 0) && (this.Rank == 0)) || (item.Rank == this.Rank))
			{
				var result = string.Compare(this.ProjectName, item.ProjectName, StringComparison.OrdinalIgnoreCase);
				if (result == 0)
					result = string.Compare(this.FileName, item.FileName, StringComparison.OrdinalIgnoreCase);

				return result;
			}
			else
			{
				// Higher ranked item must goes first
				return item.Rank.CompareTo(this.Rank);
			}
		}

		#endregion

		#region IMatchItem Members

		/// <summary>
		/// Data used for matching.
		/// </summary>
		public string Data
		{
			get { return this.FileName; }
		}

		/// <summary>
		/// Data optional ending index.
		/// Matching uses that to treat partial match up to index as an exact one.
		/// It must be greater than 0.
		/// </summary>
		public int DataEndingIndex
		{
			get
			{
				if (_dataEndingIndex == NOT_SET)
					_dataEndingIndex = this.FileName.LastIndexOf('.'); // looking for extension here

				return _dataEndingIndex;
			}
		}

		/// <summary>
		/// Optional pascal cased version of Data used for matching.
		/// </summary>
		public string PascalCasedData
		{
			get
			{
				if (_pascalCasedData == null)
					_pascalCasedData = SearchHelper.GetPascalCasedString(Path.GetFileNameWithoutExtension(this.FileName));

				return _pascalCasedData;
			}
		}

		/// <summary>
		/// Item's match result.
		/// </summary>
		public bool Matched { get; set; } = true;

		/// <summary>
		/// Match rank 0 and up, the higher the better matched item it is.
		/// </summary>
		public int Rank { get; set; }

		#endregion

		#region IExtensibilityItem Members

		/// <summary>
		/// Item name.
		/// </summary>
		public string Name
		{
			get { return this.FileNameWithPath; }
		}

		/// <summary>
		/// Untyped extensibility link (name matches the actual type).
		/// </summary>
		public object ProjectItem { get; set; }

		/// <summary>
		/// Item code type, whenever's applicable.
		/// </summary>
		public FileSubType ItemSubType { get; set; }

		/// <summary>
		/// Item parent code type, whenever's applicable.
		/// </summary>
		public FileSubType ParentSubType { get; set; }

		#endregion
	}
}