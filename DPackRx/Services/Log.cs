using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;

using DPackRx.Package;

namespace DPackRx.Services
{
	/// <summary>
	/// Application log.
	/// </summary>
	[Serializable]
	public class Log : ILog
	{
		#region Fields

		private readonly IPackageService _packageService;
		private FileStream _fileStream;
		private StreamWriter _file;
		private readonly object _threadSafeLock = new object();
		private bool _enabled;

		private const string LOG_CATEGORY = "Initialization";
		private const int MAX_FILE_NAME_TRY = 20;
		private const long MAX_FILE_SIZE = 1024 * 1024 * 10; // in Mb
		private const int CATEGORY_LENGTH = 18;
		protected internal const string INDENT = "  ";
		protected internal const string CATEGORY_INDENT = "  ";

		#endregion

		public Log(IPackageService packageService)
		{
			_packageService = packageService;
		}

		#region IDisposable Pattern

		/// <summary>
		/// Tracks whether Dispose has been called.
		/// </summary>
		private bool _disposed = false;

		/// <summary>
		/// Called upon IDE shutdown.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);

			// Prevent finalization code from executing a second time
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Disposes of managed and unmanaged resources.
		/// </summary>
		/// <param name="disposing">True - the method has been called (in)directly by code.
		/// False - the method has been called by the runtime from inside the finalizer -
		/// do not reference other objects.</param>
		private void Dispose(bool disposing)
		{
			// Check to see if Dispose() has already been called
			if (!_disposed)
			{
				// Dispose all managed and unmanaged resources
				if (disposing)
				{
					Close();
				}
				else
				{
					// Release unmanaged resources only
				}
			}

			_disposed = true;
		}

		~Log()
		{
			Dispose(false);
		}

		#endregion

		#region ILog Members

		/// <summary>
		/// Log file name with path.
		/// </summary>
		public string FileName { get; private set; }

		/// <summary>
		/// Whether log file's enabled or not.
		/// </summary>
		public bool Enabled
		{
			get { return _enabled; }
			set
			{
				_enabled = value;

				if (_enabled)
					Open();
				else
					Close();
			}
		}

		/// <summary>
		/// Logs a message.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="category">Category.</param>
		public void LogMessage(string message, string category = null)
		{
			LogMessage(message, null, category);
		}

		/// <summary>
		/// Logs an exception.
		/// </summary>
		/// <param name="ex">Exception.</param>
		/// <param name="category">Category.</param>
		public void LogMessage(Exception ex, string category = null)
		{
			LogMessage(null, ex, category);
		}

		/// <summary>
		/// Logs message and exception.
		/// </summary>
		/// <param name="message">Message.</param>
		/// <param name="ex">Exception.</param>
		/// <param name="category">Category.</param>
		public void LogMessage(string message, Exception ex, string category = null)
		{
			InternalLogMessage(message, ex, category);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Opens log file.
		/// </summary>
		private void Open()
		{
			if (_file != null)
				return;

			var attrib = (AssemblyProductAttribute)Attribute.GetCustomAttribute(
				this.GetType().Assembly, typeof(AssemblyProductAttribute), false);
			var product = attrib.Product;

			var logFullName = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				product,
				$"VS {_packageService.VSKnownVersion}",
				this.GetType().Assembly.GetName().Name + ".log");

			var dirName = Path.GetDirectoryName(logFullName);
			var logName = Path.Combine(dirName, Path.GetFileNameWithoutExtension(logFullName));
			var shortLogName = Path.GetFileNameWithoutExtension(logFullName);

			if (!Directory.Exists(dirName))
				Directory.CreateDirectory(dirName);

			var maxLogByteSize = MAX_FILE_SIZE;

			// Attempt to open the log file in case of multiple VS instances
			for (int index = 1; index <= MAX_FILE_NAME_TRY; index++)
			{
				try
				{
					_fileStream = new FileStream(logFullName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);

					if ((maxLogByteSize > 0) && (_fileStream.Length >= maxLogByteSize))
					{
						_fileStream.Close();
						_fileStream = new FileStream(logFullName, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
					}
					else
					{
						_fileStream.Position = _fileStream.Length;
					}

					_file = new StreamWriter(_fileStream);
				}
				catch
				{
					_file = null;
				}

				if (_file != null)
				{
					this.FileName = logFullName;
					break;
				}

				var newExt = " " + index.ToString() + ".log";
				logFullName = logName + newExt;
			}

			if (_file != null)
			{
				// This is slower but data is safely flushed
				_file.AutoFlush = true;

				InternalWriteMessage("*** Start logging ***");
				InternalWriteMessage($"Date: {DateTime.Now:M/d/yy}", null, LOG_CATEGORY);
				InternalWriteMessage($"Version: {Assembly.GetExecutingAssembly().GetName().Version}", null, LOG_CATEGORY);
#if BETA
				InternalWriteMessage($"Beta expires on: {Beta.ExpirationDate.ToShortDateString()}", null, LOG_CATEGORY);
#endif
				InternalWriteMessage($"Visual Studio: {_packageService.VSVersion}", null, LOG_CATEGORY);
				InternalWriteMessage($"OS: {Environment.OSVersion.Version}", null, LOG_CATEGORY);
				InternalWriteMessage($"Process Id: {Process.GetCurrentProcess().Id}", null, LOG_CATEGORY);
			}
		}

		/// <summary>
		/// Closes log file.
		/// </summary>
		private void Close()
		{
			if (_file != null)
			{
				InternalWriteMessage("*** Stop logging ***");
				InternalWriteMessage(string.Empty, null, null, false);
			}

			if (_file != null)
			{
				_file.Close();
				_file = null;
			}

			if (_fileStream != null)
			{
				_fileStream.Close();
				_fileStream = null;
			}
		}

		/// <summary>
		/// Logs message to the log file.
		/// </summary>
		private void InternalLogMessage(string message, Exception exception, string category)
		{
			if (!_enabled)
				return;

			if (string.IsNullOrEmpty(message) && (exception == null))
				return;

			if (_file == null)
				Open();

			if (_file != null)
			{
				if (_threadSafeLock != null)
				{
					bool locked = false;
					Monitor.Enter(_threadSafeLock, ref locked);
				}

				try
				{
					if (_file != null)
					{
						InternalWriteMessage(message, exception, category);

						_file.Flush();
					}
				}
				catch (ObjectDisposedException)
				{
					// Do nothing
				}
				finally
				{
					if (_threadSafeLock != null)
						Monitor.Exit(_threadSafeLock);
				}
			}
		}

		/// <summary>
		/// Writes message to the log file.
		/// </summary>
		private void InternalWriteMessage(string message, Exception exception = null, string category = null, bool timeStamp = true)
		{
			if (timeStamp)
			{
				_file.Write($"{DateTime.Now:HH:mm:ss.fff}");
				_file.Write(INDENT);
			}

			if (!string.IsNullOrEmpty(category))
			{
				_file.Write($"{category,-CATEGORY_LENGTH}");
				_file.Write(CATEGORY_INDENT);
			}

			if (message != null) // allow an empty string message
				_file.WriteLine(message);

			if (exception != null)
			{
				while (exception != null)
				{
					_file.WriteLine(exception);
					exception = exception.InnerException;
				}
			}
		}

		#endregion
	}
}