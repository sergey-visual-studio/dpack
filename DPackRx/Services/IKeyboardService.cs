using System.Windows.Input;

namespace DPackRx.Services
{
	/// <summary>
	/// Keyboard auto-entry service.
	/// </summary>
	public interface IKeyboardService
	{
		/// <summary>
		/// Types the specified text.
		/// </summary>
		void Type(string text);

		/// <summary>
		/// Presses and releases the specified key.
		/// </summary>
		void Type(Key key);
	}
}