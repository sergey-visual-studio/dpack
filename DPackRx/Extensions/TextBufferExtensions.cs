using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Projection;

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
			T service = TryGetService<T>(textBuffer);
			if (service != null)
				return service;

			if (textBuffer is IProjectionBufferBase projectionBuffer)
			{
				foreach (ITextBuffer sourceTextBuffer in projectionBuffer.SourceBuffers)
				{
					service = TryGetService<T>(sourceTextBuffer);
					if (service != null)
						return service;
				}
			}

			return default;
		}

		#region Private Methods

		private static T TryGetService<T>(this ITextBuffer textBuffer)
		{
			T service = default;
			var result = textBuffer.Properties?.TryGetProperty(typeof(T), out service);
			if (result == true)
				return service;
			else
				return default;
		}

		#endregion
	}
}