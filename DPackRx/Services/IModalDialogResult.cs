namespace DPackRx.Services
{
	/// <summary>
	/// Modal dialog result.
	/// </summary>
	/// <typeparam name="T">Result type.</typeparam>
	public interface IModalDialogResult<T>
	{
		/// <summary>
		/// Result.
		/// </summary>
		T Result { get; }
	}
}