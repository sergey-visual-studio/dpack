using System;

namespace DPackRx.Services
{
	public interface ILog : IDisposable
	{
		/// <summary>
		/// Log file name with path.
		/// </summary>
		string FileName { get; }

		/// <summary>
		/// Whether log file's enabled or not.
		/// </summary>
		bool Enabled { get; set; }

		/// <summary>
		/// Logs a message.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="catgory">Category.</param>
		void LogMessage(string message, string catgory = null);

		/// <summary>
		/// Logs an exception.
		/// </summary>
		/// <param name="ex">Exception.</param>
		/// <param name="catgory">Category.</param>
		void LogMessage(Exception ex, string catgory = null);

		/// <summary>
		/// Logs message and exception.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="ex">Exception.</param>
		/// <param name="catgory">Category.</param>
		void LogMessage(string message, Exception ex, string catgory = null);
	}
}