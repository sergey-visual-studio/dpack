using System;

namespace DPackRx.Package
{
#if BETA
	/// <summary>
	/// Beta information.
	/// </summary>
	internal static class Beta
	{
		public static readonly DateTime ExpirationDate = new DateTime(2021, 12, 7).AddHours(12);
	}
#endif
}