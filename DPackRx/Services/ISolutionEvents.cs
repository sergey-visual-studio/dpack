namespace DPackRx.Services
{
	/// <summary>
	/// Solution, project and file based events.
	/// </summary>
	public interface ISolutionEvents
	{
		#region Solution notifications

		/// <summary>
		/// Solution opened event notification.
		/// </summary>
		/// <param name="allProjectsLoaded">Whether all projects have finished loading.</param>
		void SolutionOpened(bool allProjectsLoaded);

		/// <summary>
		/// Solution closing event notification.
		/// </summary>
		void SolutionClosing();

		/// <summary>
		/// Solution closed event notification.
		/// </summary>
		void SolutionClosed();

		/// <summary>
		/// Solution saved event notification.
		/// </summary>
		void SolutionSaved();

		/// <summary>
		/// Solution renamed event notification.
		/// </summary>
		/// <param name="oldName">Old solution name.</param>
		/// <param name="newName">New solution name.</param>
		void SolutionRenamed(string oldName, string newName);

		#endregion

		#region Project notifications

		/// <summary>
		/// New project added event notification.
		/// </summary>
		/// <param name="project">Project reference.</param>
		void ProjectAdded(object project);

		/// <summary>
		/// Project deleted event notification.
		/// </summary>
		/// <param name="project">Project reference.</param>
		void ProjectDeleted(object project);

		/// <summary>
		/// Project renamed event notification.
		/// </summary>
		/// <param name="project">PRoject reference.</param>
		void ProjectRenamed(object project);

		/// <summary>
		/// Project unloaded event notification.
		/// </summary>
		/// <param name="project">Project reference.</param>
		void ProjectUnloaded(object project);

		#endregion

		#region File notifications

		/// <summary>
		/// New file added event notification.
		/// </summary>
		/// <param name="fileNames">New file names.</param>
		/// <param name="project">Project reference.</param>
		void FileAdded(string[] fileNames, object project);

		/// <summary>
		/// File deleted event notification.
		/// </summary>
		/// <param name="fileNames">File names.</param>
		/// <param name="project">Project reference.</param>
		void FileDeleted(string[] fileNames, object project);

		/// <summary>
		/// File renamed event notification.
		/// </summary>
		/// <param name="oldFileNames">Old file names.</param>
		/// <param name="newFileNames">New file names.</param>
		/// <param name="project">Project reference.</param>
		void FileRenamed(string[] oldFileNames, string[] newFileNames, object project);

		/// <summary>
		/// File changed event notification.
		/// </summary>
		/// <param name="fileName">File name.</param>
		/// <param name="projectItem">Project reference.</param>
		void FileChanged(string fileName, object projectItem);

		/// <summary>
		/// File opened event notification.
		/// </summary>
		/// <param name="fileName">File name.</param>
		/// <param name="projectItem">Project reference.</param>
		void FileOpened(string fileName, object projectItem);

		/// <summary>
		/// File closed event notification.
		/// </summary>
		/// <param name="fileName">File name.</param>
		/// <param name="projectItem">Project reference.</param>
		void FileClosed(string fileName, object projectItem);

		/// <summary>
		/// File saved event notification.
		/// </summary>
		/// <param name="fileName">File name.</param>
		/// <param name="projectItem">Project reference.</param>
		void FileSaved(string fileName, object projectItem);

		#endregion
	}
}