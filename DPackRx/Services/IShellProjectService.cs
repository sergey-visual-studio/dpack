namespace DPackRx.Services
{
	/// <summary>
	/// Projects and files shell service.
	/// </summary>
	public interface IShellProjectService
	{
		/// <summary>
		/// Returns untyped DTE instance.
		/// </summary>
		object GetDTE();

		/// <summary>
		/// Checks whether untyped Project instance is a qualified project.
		/// </summary>
		bool IsProject(object project);

		/// <summary>
		/// Checks whether untyped Project instance is a qualified classic ASP.NET web project.
		/// </summary>
		bool IsWebProject(object project);

		/// <summary>
		/// Checks whether untyped ProjectItem instance is a qualified classic ASP.NET web project item.
		/// </summary>
		bool IsWebProjectItem(object projectItem);

		/// <summary>
		/// Checks whether untyped Project instance project load's deferred, ie project hasn't been fully loaded yet.
		/// </summary>
		bool IsProjectLoadDeferred(object project, out bool loaded);

		/// <summary>
		/// Checks whether untyped ProjectItem instance is a qualified C++ project item.
		/// </summary>
		bool IsCppFile(object projectItem);

		/// <summary>
		/// Returns untyped Project instance project name.
		/// </summary>
		string GetFriendlyProjectName(object project);

		/// <summary>
		/// Returns untyped Project instance project path and full name.
		/// </summary>
		void GetProjectPath(object project, out string projectPath, out string projectFullName);

		/// <summary>
		/// Checks whether untyped Project instance has valid code model.
		/// </summary>
		bool IsCodeModelProject(object project);

		/// <summary>
		/// Returns untyped Project instance project code model language.
		/// </summary>
		string GetProjectLanguage(object project);

		/// <summary>
		/// Returns untyped Project instance project language Guid for projects with no code model.
		/// </summary>
		string GetNoCodeModelProjectLanguage(object project);

		/// <summary>
		/// Returns untyped Project instance from untyped IVsHierarchy instance.
		/// </summary>
		object GetHierarchyProject(object hierarchy);

		/// <summary>
		/// Returns untyped FileCodeModel instance for untyped ProjectItem instance.
		/// </summary>
		object GetFileCodeModel(object projectItem);

		/// <summary>
		/// Returns untyped Project or ProjectItem instance BuildAction property value.
		/// </summary>
		bool CompileBuildAction(object projectItemOrProject);

		/// <summary>
		/// Returns untyped Project instance for untyped Document instance.
		/// </summary>
		object GetDocumentProject(object document);

		/// <summary>
		/// Returns untyped ProjectItem instance for untyped Document instance.
		/// </summary>
		object GetDocumentProjectItem(object document);

		/// <summary>
		/// Returns untyped Project file name.
		/// </summary>
		string GetItemFileName(object projectItem);

		/// <summary>
		/// Returns item's SubType.
		/// </summary>
		/// <param name="item">Project item.</param>
		/// <returns>SubType or null.</returns>
		string GetProjectItemSubType(object projectItem);
	}
}