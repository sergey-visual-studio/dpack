namespace DPackRx.Services
{
	/// <summary>
	/// Miscellaneous utilities service.
	/// </summary>
	public interface IUtilsService
	{
		/// <summary>
		/// System beep.
		/// </summary>
		void Beep();

		/// <summary>
		/// Checks if Control key's down.
		/// </summary>
		bool ControlKeyDown();

		/// <summary>
		/// Sets clipboard data.
		/// </summary>
		void SetClipboardData(string data);

		/// <summary>
		/// Retrieves clipboard data.
		/// </summary>
		bool GetClipboardData(out string data);
	}
}