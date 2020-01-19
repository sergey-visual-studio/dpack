using System.Runtime.InteropServices;

namespace DPackRx.Helpers
{
	/// <summary>
	/// Win32 helper class.
	/// </summary>
	public static class Win32Helper
	{
		#region Constants

		// GetKeyState
		private const int VK_SHIFT = 0x0010;
		private const int VK_CONTROL = 0x0011;
		private const int VK_ALT = 0x0012;

		/// <summary>
		/// Message beep enum.
		/// </summary>
		public enum MessageBeepType : uint
		{
			IconAsterisk = 0x00000040,
			IconExclamation = 0x00000030,
			IconHand = 0x00000010,
			IconQuestion = 0x00000020,
			OK = 0x00000000,
			Simple = 0xFFFFFFFF
		}

		#endregion

		#region Imports

		[DllImport("user32.dll")]
		private static extern short GetKeyState(int nVirtKey);

		[DllImport("user32.dll")]
		private static extern bool MessageBeep(uint uType);

		#endregion

		#region Public Methods

		/// <summary>
		/// Checks if Control key's down.
		/// </summary>
		public static bool ControlKeyDown()
		{
			if (GetKeyState(VK_CONTROL) < 0)
				return true;
			else
				return false;
		}

		/// <summary>
		/// Checks if Alt key's down.
		/// </summary>
		public static bool AltKeyDown()
		{
			if (GetKeyState(VK_ALT) < 0)
				return true;
			else
				return false;
		}

		/// <summary>
		/// Checks if Shift key's down.
		/// </summary>
		public static bool ShiftKeyDown()
		{
			if (GetKeyState(VK_SHIFT) < 0)
				return true;
			else
				return false;
		}

		/// <summary>
		/// Sounds system message beep.
		/// </summary>
		public static void MessageBeep(MessageBeepType type)
		{
			MessageBeep((uint)type);
		}

		#endregion
	}
}