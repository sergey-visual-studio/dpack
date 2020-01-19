using System.Windows.Media;

using DPackRx.CodeModel;

namespace DPackRx.Services
{
	/// <summary>
	/// Image access service.
	/// </summary>
	public interface IImageService
	{
		/// <summary>
		/// Returns associated file image.
		/// </summary>
		/// <param name="fileName">File name.</param>
		/// <returns>Image.</returns>
		ImageSource GetImage(string fileName);

		/// <summary>
		/// Returns member image.
		/// </summary>
		/// <param name="modifier">Member visibility modifier.</param>
		/// <param name="kind">Member kind.</param>
		/// <param name="isStatic">Whether member is static.</param>
		/// <returns>Image.</returns>
		ImageSource GetMemberImage(Modifier modifier, Kind kind, bool isStatic);
	}
}