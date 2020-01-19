using System;
using System.Diagnostics;
using System.Reflection;
using System.Resources;

using DPackRx.Extensions;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using VSPackage = Microsoft.VisualStudio.Shell.Package;

namespace DPackRx.Package
{
	/// <summary>
	/// Package service.
	/// </summary>
	/// <remarks>Purposely don't inject ILog here to avoid circular dependency.</remarks>
	public class PackageService : IPackageService
	{
		#region Fields

		private readonly IServiceProvider _serviceProvider;
		private ResourceManager _resourceManager;
		private string _vsVersion;
		private string _vsKnownVersion;

		#endregion

		public PackageService(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		#region IServiceProvider Members

		public object GetService(Type serviceType)
		{
			return _serviceProvider.GetService(serviceType);
		}

		#endregion

		#region IPackageService Members

		/// <summary>
		/// Help|About type VS version.
		/// </summary>
		public string VSVersion
		{
			get
			{
				if (_vsVersion != null)
					return _vsVersion;

				ThreadHelper.ThrowIfNotOnUIThread();

				var version = string.Empty;
				try
				{
					var shell = _serviceProvider.GetService<IVsShell, SVsShell>();
					var result = shell.GetProperty((int)__VSSPROPID5.VSSPROPID_ReleaseVersion, out object var);
					if ((result == VSConstants.S_OK) && (var != null) && (var is string))
						version = ((string)var).Trim();
				}
				catch (Exception ex)
				{
					Trace.TraceError($"Error retrieving VS version\r\n{ex}");
				}

				_vsVersion = version;
				return _vsVersion;
			}
		}

		/// <summary>
		/// VS known 20XX format version.
		/// </summary>
		public string VSKnownVersion
		{
			get
			{
				if (_vsKnownVersion != null)
					return _vsKnownVersion;

				ThreadHelper.ThrowIfNotOnUIThread();

				var version = this.VSVersion;
				if (string.IsNullOrEmpty(version))
					return version;

				int index = version.IndexOf('.');
				if (index > 0)
					version = version.Substring(0, index);

				switch (version)
				{
					case "15":
						version = "2017";
						break;
					case "16":
						version = "2019";
						break;
					default:
						Debug.WriteLine($"Unknown or unsupported Visual Studio version {version}");
						break;
				}

				_vsKnownVersion = version;
				return _vsKnownVersion;
			}
		}

		/// <summary>
		/// VS installation directory.
		/// </summary>
		public string VSInstallDir
		{
			get
			{
				ThreadHelper.ThrowIfNotOnUIThread();

				try
				{
					var shell = _serviceProvider.GetService<IVsShell, SVsShell>();
					var result = shell.GetProperty((int)__VSSPROPID2.VSSPROPID_InstallRootDir, out object var);
					if ((result == VSConstants.S_OK) && (var is string))
						return (string)var;
				}
				catch (Exception ex)
				{
					Trace.TraceError($"Error retrieving VS installation directory\r\n{ex}");
				}

				return string.Empty;
			}
		}

		/// <summary>
		/// Product name.
		/// </summary>
		public string ProductName
		{
			get { return GetResourceString(IDs.PRODUCT); }
		}

		/// <summary>
		/// Product company name.
		/// </summary>
		public string CompanyName
		{
			get { return GetResourceString(IDs.COMPANY); }
		}

		/// <summary>
		/// Product version.
		/// </summary>
		public string Version
		{
			get
			{
				var version = Assembly.GetExecutingAssembly().GetName().Version;
				if (version.Revision == 0)
					return version.ToString(3); // w/o revision
				else
					return version.ToString(); // full version
			}
		}

		/// <summary>
		/// Returns package resource.
		/// </summary>
		/// <param name="id">Resource id.</param>
		/// <returns>Resource value.</returns>
		public string GetResourceString(int id)
		{
			if (id <= 0)
				throw new ArgumentNullException(nameof(id));

			if (_resourceManager == null)
				_resourceManager = new ResourceManager("VSPackage", this.GetType().Assembly);

			var value = _resourceManager.GetString(id.ToString());
			return value;
		}

		/// <summary>
		/// Read-only key intended for static data.
		/// </summary>
		public RegistryKey GetSystemRegistryRootKey(string subKey)
		{
			if (string.IsNullOrEmpty(subKey))
				return null;

			var package = _serviceProvider.GetService<VSPackage>();
			return package.ApplicationRegistryRoot.OpenSubKey(subKey, false);
		}

		/// <summary>
		/// Read-only key intended for static data.
		/// </summary>
		public RegistryKey GetAppRegistryRootKey(string subKey = null)
		{
			var package = _serviceProvider.GetService<VSPackage>();

			if (string.IsNullOrEmpty(subKey))
				subKey = this.ProductName;
			else
				subKey = $"{this.ProductName}\\{subKey}";

			return package.ApplicationRegistryRoot.OpenSubKey(subKey, false);
		}

		/// <summary>
		/// Writable key intended for customizable data.
		/// </summary>
		public RegistryKey GetUserRegistryRootKey(string subKey = null, bool createMissingSubKey = true)
		{
			var package = _serviceProvider.GetService<VSPackage>();

			var productKey = package.UserRegistryRoot.OpenSubKey(this.ProductName, true);
			if (productKey == null)
				productKey = package.UserRegistryRoot.CreateSubKey(this.ProductName, true);
			if (string.IsNullOrEmpty(subKey))
				return productKey;

			try
			{
				var key = productKey.OpenSubKey(subKey, true);
				if ((key == null) && createMissingSubKey)
					key = productKey.CreateSubKey(subKey, true);

				return key;
			}
			finally
			{
				productKey.Dispose();
			}
		}

		#endregion
	}
}