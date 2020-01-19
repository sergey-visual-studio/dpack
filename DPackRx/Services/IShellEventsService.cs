namespace DPackRx.Services
{
	/// <summary>
	/// Shell events subscription service.
	/// </summary>
	public interface IShellEventsService
	{
		/// <summary>
		/// One time subscriber notification that solution's been opened.
		/// </summary>
		void NotifySolutionOpened();

		/// <summary>
		/// Subscribes to the solution events.
		/// </summary>
		/// <param name="subscriber">Subscriber.</param>
		void SubscribeSolutionEvents(ISolutionEvents subscriber);

		/// <summary>
		/// Unsubscribes from the solution events.
		/// </summary>
		/// <param name="subscriber"></param>
		void UnsubscribeSolutionEvents(ISolutionEvents subscriber);

		/// <summary>
		/// Subscribes to the code model events.
		/// </summary>
		/// <param name="subscriber">Subscriber.</param>
		void SubscribeCodeModelEvents(ICodeModelEvents subscriber);

		/// <summary>
		/// Unsubscribes from the code model events.
		/// </summary>
		/// <param name="subscriber">Subscriber.</param>
		void UnsubscribeCodeModelEvents(ICodeModelEvents subscriber);
	}
}