using System;
using Microsoft.VisualStudio.Shell;

namespace DPackRx.Package.Registration
{
	/// <summary>
	/// Provides dynamic product version.
	/// </summary>
	public class InstalledProductRegistrationExAttribute : RegistrationAttribute
	{
		#region Fields

		private readonly string _productName;
		private readonly string _productDetails;
		private readonly int _iconResourceID;

		#endregion

		public InstalledProductRegistrationExAttribute(string productName, string productDetails, int iconResourceID)
		{
			if (string.IsNullOrEmpty(productName))
				throw new ArgumentNullException(nameof(productName));

			if (string.IsNullOrEmpty(productDetails))
				throw new ArgumentNullException(nameof(productDetails));

			_productName = productName;
			_productDetails = productDetails;
			_iconResourceID = iconResourceID;
		}

		#region RegistrationAttribute Overrides

		public override void Register(RegistrationContext context)
		{
			var attribute = GetAttribute();
			attribute.Register(context);
		}

		public override void Unregister(RegistrationContext context)
		{
			var attribute = GetAttribute();
			attribute.Unregister(context);
		}

		#endregion

		#region Private Methods

		private InstalledProductRegistrationAttribute GetAttribute()
		{
			var version = this.GetType().Assembly.GetName().Version;
			var productId = version.Revision == 0 ? version.ToString(3) : version.ToString(4);

#if BETA
			if (version.Major == 0)
				productId = $"Beta expires on {Beta.ExpirationDate:d}";
			else
				productId = $"{productId} - Beta expires on {Beta.ExpirationDate:d}";
#endif

			var attrib = new InstalledProductRegistrationAttribute(_productName, _productDetails, productId)
			{
				IconResourceID = _iconResourceID
			};
			return attrib;
		}

		#endregion
	}
}