using System.Windows.Media;

using DPackRx.CodeModel;

namespace DPackRx.Services
{
	/// <summary>
	/// Shell image service.
	/// </summary>
	public interface IShellImageService
	{
		/// <summary>
		/// Returns file image.
		/// </summary>
		/// <param name="fileName">File mame.</param>
		/// <returns>Image.</returns>
		ImageSource GetFileImage(string fileName);

		/// <summary>
		/// Returns member image.
		/// </summary>
		/// <param name="modifier">Member visibility modifier.</param>
		/// <param name="kind">Member kind.</param>
		/// <param name="isStatic">Whether member is static.</param>
		/// <returns>Image.</returns>
		ImageSource GetMemberImage(Modifier modifier, Kind kind, bool isStatic);

		/// <summary>
		/// Returns well known image.
		/// </summary>
		/// <param name="image">Image type.</param>
		/// <returns>Image.</returns>
		ImageSource GetWellKnownImage(WellKnownImage image);
	}

	#region WellKnownImage enum

	/// <summary>
	/// Well known built in images.
	/// </summary>
	public enum WellKnownImage
	{
		AllFiles,
		AllCode,
		Members,
		Classes,
		Methods,
		Properties
	}

	#endregion
}