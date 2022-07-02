using System.ComponentModel.Composition;

using DPackRx.Services;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace DPackRx.Features.Bookmarks
{
	/// <summary>
	/// Creates bookmark tagger.
	/// </summary>
	[Export(typeof(ITaggerProvider))]
	[ContentType("text")]
	[Order(Before = Priority.Default)]
	[TextViewRole(PredefinedTextViewRoles.Document)]
	[TagType(typeof(BookmarkTag))]
	public class BookmarksTaggerProvider : ITaggerProvider
	{
#pragma warning disable CS0414
		[Import]
		internal ISharedServiceProvider _serviceProvider = null;
#pragma warning restore CS0414

		#region ITaggerProvider Members

		public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
		{
			if (buffer == null)
				return null;

			return buffer.Properties.GetOrCreateSingletonProperty(() => new BookmarksSimpleTagger(_serviceProvider, buffer)) as ITagger<T>;
		}

		#endregion
	}
}