using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Microsoft.Win32;

namespace DPackRx.Helpers
{
	/// <summary>
	/// Access to Shell icons for the file types.
	/// </summary>
	public static class ShellImageHelper
	{
		#region Imports

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		private struct SHFILEINFO
		{
			public IntPtr hIcon;
			public int iIcon;
			public uint dwAttributes;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			public string szDisplayName;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
			public string szTypeName;
		}

		[Flags]
		private enum SHGFI : int
		{
			Icon = 0x000000100,
			DisplayName = 0x000000200,
			TypeName = 0x000000400,
			Attributes = 0x000000800,
			IconLocation = 0x000001000,
			ExeType = 0x000002000,
			SysIconIndex = 0x000004000,
			LinkOverlay = 0x000008000,
			Selected = 0x000010000,
			Attr_Specified = 0x000020000,
			LargeIcon = 0x000000000,
			SmallIcon = 0x000000001,
			OpenIcon = 0x000000002,
			ShellIconSize = 0x000000004,
			PIDL = 0x000000008,
			UseFileAttributes = 0x000000010,
			AddOverlays = 0x000000020,
			OverlayIndex = 0x000000040,
		}

		[DllImport("shell32.dll")]
		private static extern int SHGetFileInfo(string pszPath, uint dwFileAttributes, out SHFILEINFO psfi, uint cbSizeFileInfo, SHGFI uFlags);

		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		private static extern uint ExtractIconEx(string szFileName, int nIconIndex,
			 IntPtr[] phiconLarge, IntPtr[] phiconSmall, uint nIcons);

		[DllImport("User32.dll")]
		private static extern int DestroyIcon(IntPtr hIcon);

		[DllImport("gdi32.dll", SetLastError = true)]
		private static extern bool DeleteObject(IntPtr hObject);

		#endregion

		#region Public Methods

		/// <summary>
		/// Converts icon to WPF image source.
		/// </summary>
		/// <param name="icon">Icon.</param>
		/// <returns>Image.</returns>
		public static ImageSource IconToImage(Icon icon)
		{
			if (icon == null)
				return null;

			var bitmap = icon.ToBitmap();
			var hBitmap = bitmap.GetHbitmap();

			var image = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
			DeleteObject(hBitmap);

			return image;
		}

		/// <summary>
		/// Returns an icon associated with the shell extension.
		/// </summary>
		/// <param name="extension">Extension.</param>
		/// <returns>Icon.</returns>
		public static Icon GetShellAssociatedIcon(string extension)
		{
			if (string.IsNullOrEmpty(extension))
				throw new ArgumentNullException(nameof(extension));

			var info = new SHFILEINFO();
			var cbFileInfo = Marshal.SizeOf(info);
			var flags = SHGFI.Icon | SHGFI.SmallIcon | SHGFI.UseFileAttributes;

			var result = SHGetFileInfo(extension, 0, out info, (uint)cbFileInfo, flags);
			if ((result != 1) || (info.hIcon == IntPtr.Zero))
				return null;

			var icon = (Icon)Icon.FromHandle(info.hIcon).Clone();
			DestroyIcon(info.hIcon);

			return icon;
		}

		/// <summary>
		/// Returns extension assigned system icon.
		/// </summary>
		/// <param name="extension">Extension.</param>
		/// <param name="defaultIconOnly">Whether to use default icon only.</param>
		/// <returns>Icon.</returns>
		public static Icon GetClassRootIcon(string extension, bool defaultIconOnly)
		{
			if (string.IsNullOrEmpty(extension))
				return null;

			var shellExtension = extension;

			if (!defaultIconOnly)
			{
				try
				{
					var regKeyShellExt = Registry.ClassesRoot.OpenSubKey(extension);

					if (regKeyShellExt != null)
					{
						using (regKeyShellExt)
						{
							shellExtension = regKeyShellExt.GetValue(string.Empty, null) as string;
						}
					}
				}
				catch
				{
					shellExtension = null;
				}
			}

			if (string.IsNullOrEmpty(shellExtension))
				return null;

			// Get extended file extension associated application icon information
			RegistryKey regKeyIcon;
			try
			{
				regKeyIcon = Registry.ClassesRoot.OpenSubKey(shellExtension + "\\DefaultIcon");
			}
			catch
			{
				regKeyIcon = null;
			}
			if (regKeyIcon == null)
				return null;

			string iconPath;
			using (regKeyIcon)
			{
				iconPath = regKeyIcon.GetValue(string.Empty, null) as string;
			}
			if (string.IsNullOrEmpty(iconPath))
				return null;

			var iconSetup = iconPath.Split(',');
			var iconFileName = iconSetup[0].Trim();
			var iconFileIndex = 0;
			if (iconSetup.Length > 1)
				iconFileIndex = Convert.ToInt32(iconSetup[1].Trim());
			if (iconFileName.StartsWith("\""))
				iconFileName = iconFileName.Substring(1);
			if (iconFileName.EndsWith("\""))
				iconFileName = iconFileName.Substring(0, iconFileName.Length - 1);

			var hLargeIcons = new IntPtr[] { IntPtr.Zero };
			var hSmallIcons = new IntPtr[] { IntPtr.Zero };

			Icon icon = null;
			var iconCount = ExtractIconEx(iconFileName, iconFileIndex, hLargeIcons, hSmallIcons, 1);
			if ((iconCount >= 1) && (hSmallIcons[0] != IntPtr.Zero))
				icon = (Icon)Icon.FromHandle(hSmallIcons[0]).Clone();

			if (hSmallIcons[0] != IntPtr.Zero)
				DestroyIcon(hSmallIcons[0]);
			if (hLargeIcons[0] != IntPtr.Zero)
				DestroyIcon(hLargeIcons[0]);

			return icon;
		}

		#endregion
	}
}