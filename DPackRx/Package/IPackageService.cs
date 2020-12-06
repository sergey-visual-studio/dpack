using System;

using Microsoft.Win32;

namespace DPackRx.Package
{
	/// <summary>
	/// Package service.
	/// </summary>
	public interface IPackageService : IServiceProvider
	{
		/// <summary>
		/// Help|About type VS version.
		/// </summary>
		string VSVersion { get; }

		/// <summary>
		/// VS known 20XX format version.
		/// </summary>
		string VSKnownVersion { get; }

		/// <summary>
		/// VS installation directory.
		/// </summary>
		string VSInstallDir { get; }

		/// <summary>
		/// Product name.
		/// </summary>
		string ProductName { get; }

		/// <summary>
		/// Product version.
		/// </summary>
		string Version { get; }

		/// <summary>
		/// Returns package resource.
		/// </summary>
		/// <param name="id">Resource id.</param>
		/// <returns>Resource value.</returns>
		string GetResourceString(int id);

		/// <summary>
		/// Read-only key intended for static data.
		/// </summary>
		RegistryKey GetSystemRegistryRootKey(string subKey);

		/// <summary>
		/// Read-only key intended for static data.
		/// </summary>
		RegistryKey GetAppRegistryRootKey(string subKey = null);

		/// <summary>
		/// Writable key intended for customizable data.
		/// </summary>
		RegistryKey GetUserRegistryRootKey(string subKey = null, bool createMissingSubKey = true);
	}
}