using System;

using DPackRx.Features;
using DPackRx.Services;

namespace DPackRx.Extensions
{
	/// <summary>
	/// Log extensions.
	/// </summary>
	public static class LogExtensions
	{
		/// <summary>
		/// Custom: logs feature message.
		/// </summary>
		public static void LogMessage(this ILog log, KnownFeature feature, string message)
		{
			log.LogMessage(feature, message, null);
		}

		/// <summary>
		/// Custom: logs feature exception.
		/// </summary>
		public static void LogMessage(this ILog log, KnownFeature feature, Exception ex)
		{
			log.LogMessage(feature, null, ex);
		}

		/// <summary>
		/// Custom: logs feature message and exception.
		/// </summary>
		public static void LogMessage(this ILog log, KnownFeature feature, string message, Exception ex)
		{
			var featureMessage = $"{feature}:{Log.INDENT}";
			message = string.IsNullOrEmpty(message) ? featureMessage : featureMessage + message;

			log.LogMessage(message, ex);
		}
	}
}