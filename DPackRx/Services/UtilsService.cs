using System.Windows;

using DPackRx.Helpers;

namespace DPackRx.Services
{
	/// <summary>
	/// Miscellaneous utilities service.
	/// </summary>
	public class UtilsService : IUtilsService
	{
		#region IUtilsService Members

		/// <summary>
		/// System beep.
		/// </summary>
		public void Beep()
		{
			Win32Helper.MessageBeep(Win32Helper.MessageBeepType.IconExclamation);
		}

		/// <summary>
		/// Checks if Control key's down.
		/// </summary>
		public bool ControlKeyDown()
		{
			return Win32Helper.ControlKeyDown();
		}

		/// <summary>
		/// Sets clipboard data.
		/// </summary>
		public void SetClipboardData(string data)
		{
			if (data == null)
				data = string.Empty;

			try
			{
				Clipboard.SetDataObject(data, true);
			}
			catch
			{
			}
		}

		/// <summary>
		/// Retrieves clipboard data.
		/// </summary>
		public bool GetClipboardData(out string data)
		{
			data = null;
			try
			{
				if (Clipboard.ContainsText())
				{
					var text = Clipboard.GetDataObject();
					if (text != null)
						data = text.GetData(typeof(string)) as string;
				}

				return data != null;
			}
			catch
			{
				return false;
			}
		}

		#endregion
	}
}