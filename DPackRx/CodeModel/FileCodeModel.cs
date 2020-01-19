using System;
using System.ComponentModel;
using System.Diagnostics;

using DPackRx.Helpers;

namespace DPackRx.CodeModel
{
	/// <summary>
	/// File code model.
	/// </summary>
	[DebuggerDisplay("{Name}, {ElementModifier}, {ElementKind}, Matched = {Matched}")]
	public class FileCodeModel : INotifyPropertyChanged, IMatchItem, IExtensibilityItem
	{
		#region Fields

		private string _shortName;
		private int _dataEndingIndex = NOT_SET;
		private string _pascalCasedData;

		private const int NOT_SET = -10;

		#endregion

		#region Properties

		/// <summary>
		/// Item full name.
		/// </summary>
		public string FullName { get; set; }

		/// <summary>
		/// Item name with immediate parent name. Qualified parents are classes, interfaces and Structs.
		/// </summary>
		public string ParentFullName { get; set; }

		/// <summary>
		/// vsCMElement enum converted to int.
		/// </summary>
		public int CodeModelElementKind { get; set; }

		/// <summary>
		/// Element kind.
		/// </summary>
		public Kind ElementKind { get; set; }

		/// <summary>
		/// Element modifier.
		/// </summary>
		public Modifier ElementModifier { get; set; }

		/// <summary>
		/// Whether item is a constant.
		/// </summary>
		public bool IsConstant { get; set; }

		/// <summary>
		/// Whether item is static.
		/// </summary>
		public bool IsStatic { get; set; }

		/// <summary>
		/// Item line number.
		/// </summary>
		public int Line { get; set; }

		/// <summary>
		/// Item code snippet.
		/// </summary>
		public string Code { get; set; }

		/// <summary>
		/// Item return type name.
		/// </summary>
		public string ReturnTypeName { get; set; }

		/// <summary>
		/// Item Xml documentation.
		/// </summary>
		public string XmlDoc { get; set; }

		/// <summary>
		/// Whether item's language supports generics.
		/// </summary>
		public bool SupportsGenerics { get; set; }

		/// <summary>
		/// Item language generics suffix.
		/// </summary>
		public string GenericsSuffix { get; set; }

		/// <summary>
		/// Last part of item name separated by '.'.
		/// </summary>
		public string ShortName
		{
			get
			{
				if (_shortName == null)
				{
					var index = this.Name.LastIndexOf(".", StringComparison.OrdinalIgnoreCase);
					if (index >= 0)
						_shortName = index == this.Name.Length ? string.Empty : this.Name.Substring(index + 1);
					else
						_shortName = this.Name;
				}

				return _shortName;
			}
		}

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

		#region IComparable<IMatchItem> Members

		public int CompareTo(IMatchItem obj)
		{
			var item = obj as FileCodeModel;
			if (item == null)
				return -1;

			if (((item.Rank == 0) && (this.Rank == 0)) || (item.Rank == this.Rank))
			{
				return this.Line.CompareTo(item.Line);
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
			get { return this.ShortName; }
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
				{
					if (this.SupportsGenerics && !string.IsNullOrEmpty(this.GenericsSuffix))
					{
						var genericsStart = this.GenericsSuffix[0];
						_dataEndingIndex = this.ShortName.LastIndexOf(genericsStart); // looking for the start of generics definition here
					}
					else
					{
						_dataEndingIndex = this.ShortName.Length; // use the entire name
					}
				}

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
					_pascalCasedData = SearchHelper.GetPascalCasedString(this.ShortName);

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
		public string Name { get; set; }

		/// <summary>
		/// Untyped extensibility link (name matches the actual type).
		/// </summary>
		public object ProjectItem { get; set; }

		/// <summary>
		/// Item code type, whenever's applicable.
		/// </summary>
		public FileSubType ItemSubType { get; } = FileSubType.None;

		/// <summary>
		/// Item parent code type, whenever's applicable.
		/// </summary>
		public FileSubType ParentSubType { get; set; } = FileSubType.Code;

		#endregion
	}
}