using System;

namespace DPackRx.Package
{
#if BETA
	/// <summary>
	/// Beta information.
	/// </summary>
	internal static class Beta
	{
		public static readonly DateTime ExpirationDate = new DateTime(2023, 1, 16).AddHours(12);
	}
#endif
}