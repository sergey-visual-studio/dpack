using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using System.Windows.Input;

namespace DPackRx.Helpers
{
	/// <summary>
	/// Keyboard auto-entry helper.
	/// </summary>
	/// <remarks>Source: Node.js tools for Visual Studio.</remarks>
	public static class KeyboardHelper
	{
		#region Fields

		private const int VKEY_SHIFT_MASK = 0x0100;
		private const int VKEY_CHAR_MASK = 0x00FF;

		private const int INPUT_KEYBOARD = 1; // INPUT_MOUSE = 0;

		private const int KEY_EVENT_EXTENDED_KEY = 0x0001;
		private const int KEY_EVENT_KEY_UP = 0x0002;
		private const int KEY_EVENT_SCAN_CODE = 0x0008;

		private const char LEFT = '\u2190';
		private const char RIGHT = '\u2192';
		private const char UP = '\u2191';
		private const char DOWN = '\u2193';
		private const char CTRL_SPACE = '\u266B';
		private const char DELAY = '\u2026';

		private static readonly Key[] _extendedKeys = new Key[] {
			Key.RightAlt, Key.RightCtrl,
			Key.NumLock, Key.Insert, Key.Delete,
			Key.Home, Key.End,
			Key.Prior, Key.Next,
			Key.Up, Key.Down, Key.Left, Key.Right,
			Key.Apps, Key.RWin, Key.LWin };

		#endregion

		#region Exports

		[StructLayout(LayoutKind.Sequential)]
		private struct INPUT
		{
			public int type;
			public INPUTUNION union;
		};

		[StructLayout(LayoutKind.Explicit)]
		private struct INPUTUNION
		{
			[FieldOffset(0)]
			public MOUSEINPUT mouseInput;
			[FieldOffset(0)]
			public KEYBDINPUT keyboardInput;
		};

		[StructLayout(LayoutKind.Sequential)]
		private struct MOUSEINPUT
		{
			public int dx;
			public int dy;
			public int mouseData;
			public int dwFlags;
			public int time;
			public IntPtr dwExtraInfo;
		};

		[StructLayout(LayoutKind.Sequential)]
		private struct KEYBDINPUT
		{
			public short wVk;
			public short wScan;
			public int dwFlags;
			public int time;
			public IntPtr dwExtraInfo;
		};

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern int MapVirtualKey(int nVirtKey, int nMapType);

		[DllImport("user32.dll", SetLastError = true)]
		private static extern int SendInput(int nInputs, ref INPUT mi, int cbSize);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern short VkKeyScan(char ch);

		#endregion

		#region Public Methods

		/// <summary>
		/// Types the specified text.
		/// </summary>
		public static void Type(string text)
		{
			if (string.IsNullOrEmpty(text))
				return;

			foreach (var ch in text)
			{
				switch (ch)
				{
					case LEFT:
						Type(Key.Left);
						break;
					case RIGHT:
						Type(Key.Right);
						break;
					case UP:
						Type(Key.Up);
						break;
					case DOWN:
						Type(Key.Down);
						break;
					case CTRL_SPACE:
						PressAndRelease(Key.Space, Key.LeftCtrl);
						break;
					case DELAY:
						Thread.Sleep(1000);
						break;
					default:
						var keyValue = VkKeyScan(ch);
						bool keyIsShifted = (keyValue & VKEY_SHIFT_MASK) == VKEY_SHIFT_MASK;
						var key = KeyInterop.KeyFromVirtualKey(keyValue & VKEY_CHAR_MASK);

						if (keyIsShifted)
							Type(key, new Key[] { Key.LeftShift });
						else
							Type(key);

						break;
				}
			}

			Thread.Sleep(200);
		}

		/// <summary>
		/// Presses and releases the specified key.
		/// </summary>
		public static void Type(Key key)
		{
			PressKey(key);
			ReleaseKey(key);
		}

		#endregion

		#region Private Methods

		private static void PressAndRelease(Key key, params Key[] modifiers)
		{
			for (var index = 0; index < modifiers.Length; index++)
			{
				PressKey(modifiers[index]);
			}

			PressKey(key);
			ReleaseKey(key);

			for (var index = modifiers.Length - 1; index >= 0; index--)
			{
				ReleaseKey(modifiers[index]);
			}
		}

		private static void PressKey(Key key)
		{
			SendKeyboardInput(key, true);
		}

		private static void ReleaseKey(Key key)
		{
			SendKeyboardInput(key, false);
		}

		private static void SendKeyboardInput(Key key, bool press)
		{
			PermissionSet permissions = new PermissionSet(PermissionState.Unrestricted);
			permissions.Demand();

			var input = new INPUT { type = INPUT_KEYBOARD };
			input.union.keyboardInput.wVk = (short)KeyInterop.VirtualKeyFromKey(key);
			input.union.keyboardInput.wScan = (short)MapVirtualKey(input.union.keyboardInput.wVk, 0);
			int dwFlags = 0;
			if (input.union.keyboardInput.wScan > 0)
				dwFlags |= KEY_EVENT_SCAN_CODE;
			if (!press)
				dwFlags |= KEY_EVENT_KEY_UP;
			input.union.keyboardInput.dwFlags = dwFlags;
			if (_extendedKeys.Contains(key))
				input.union.keyboardInput.dwFlags |= KEY_EVENT_EXTENDED_KEY;
			input.union.keyboardInput.time = 0;
			input.union.keyboardInput.dwExtraInfo = new IntPtr(0);

			if (SendInput(1, ref input, Marshal.SizeOf(input)) == 0)
				throw new Win32Exception(Marshal.GetLastWin32Error());

			Thread.Sleep(10);
		}

		private static void Type(Key key, Key[] modifiers)
		{
			foreach (var modifier in modifiers)
			{
				PressKey(modifier);
			}

			Type(key);

			foreach (var modifier in modifiers.Reverse())
			{
				ReleaseKey(modifier);
			}
		}

		#endregion
	}
}