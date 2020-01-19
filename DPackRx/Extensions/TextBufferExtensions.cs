using Microsoft.VisualStudio.Text;

namespace DPackRx.Extensions
{
	/// <summary>
	/// Text buffer extensions.
	/// </summary>
	public static class TextBufferExtensions
	{
		/// <summary>
		/// Custom: returns <typeparamref name="T"/> service from the text buffer.
		/// </summary>
		public static T GetService<T>(this ITextBuffer textBuffer)
		{
			T service = default;
			var result = textBuffer.Properties?.TryGetProperty(typeof(T), out service);
			if (result == true)
				return service;
			else
				return default;
		}
	}
}