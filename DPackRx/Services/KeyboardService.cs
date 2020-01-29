using System;
using System.Windows.Input;

using DPackRx.Helpers;

namespace DPackRx.Services
{
	/// <summary>
	/// Keyboard auto-entry service.
	/// </summary>
	public class KeyboardService : IKeyboardService
	{
		#region IKeyboardService Members

		/// <summary>
		/// Types the specified text.
		/// </summary>
		public void Type(string text)
		{
			if (string.IsNullOrEmpty(text))
				throw new ArgumentNullException(nameof(text));

			KeyboardHelper.Type(text);
		}

		/// <summary>
		/// Presses and releases the specified key.
		/// </summary>
		public void Type(Key key)
		{
			KeyboardHelper.Type(key);
		}

		#endregion
	}
}