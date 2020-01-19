namespace DPackRx.Services
{
	/// <summary>
	/// Code model events.
	/// </summary>
	public interface ICodeModelEvents
	{
		/// <summary>
		/// Code element added notification.
		/// </summary>
		/// <param name="element">Code element.</param>
		void ElementAdded(object element);

		/// <summary>
		/// Code element changed notification.
		/// </summary>
		/// <param name="element">Code element.</param>
		void ElementChanged(object element);

		/// <summary>
		/// Code element deleted notification.
		/// </summary>
		/// <param name="element">Code element.</param>
		/// <param name="parent">Code element's parent code element.</param>
		void ElementDeleted(object element, object parent);
	}
}