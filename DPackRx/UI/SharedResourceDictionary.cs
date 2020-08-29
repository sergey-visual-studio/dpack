using System;
using System.Collections.Generic;
using System.Windows;

namespace DPackRx.UI
{
	/// <summary>
	/// Provides cached hash table / dictionary implementation that contains WPF resources used by components and other elements of a WPF application.
	/// </summary>
	public class SharedResourceDictionary : ResourceDictionary
	{
		#region Fields

		private Uri _sourceUri;
		private static readonly Dictionary<Uri, ResourceDictionary> _sharedDictionaries = new Dictionary<Uri, ResourceDictionary>(10);

		#endregion

		#region Properties

		/// <summary>
		/// Uniform resource identifier (URI) to load resources from.
		/// </summary>
		public new Uri Source
		{
			get { return _sourceUri; }
			set
			{
				_sourceUri = value;

				if (value != null)
				{
					if (_sharedDictionaries.ContainsKey(value))
					{
						this.MergedDictionaries.Add(_sharedDictionaries[value]);
					}
					else
					{
						// Load the dictionary by setting the source of the base class
						base.Source = value;

						_sharedDictionaries.Add(value, this);
					}
				}
			}
		}

		#endregion
	}
}