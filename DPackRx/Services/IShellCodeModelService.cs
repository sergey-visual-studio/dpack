using DPackRx.CodeModel;

namespace DPackRx.Services
{
	/// <summary>
	/// File code model shell service. 
	/// </summary>
	public interface IShellCodeModelService
	{
		/// <summary>
		/// Returns whether untyped CodeElement instance is constant member.
		/// </summary>
		bool IsConstant(object element);

		/// <summary>
		/// Returns whether untyped CodeElement instance is static member.
		/// </summary>
		bool IsStatic(object element);

		/// <summary>
		/// Returns untyped CodeElement generic member suffix.
		/// </summary>
		string GetGenericsSuffix(object element);

		/// <summary>
		/// Returns untyped CodeElement instance member modifier.
		/// </summary>
		Modifier GetElementModifier(object element);

		/// <summary>
		/// Returns untyped CodeElement instance member kind.
		/// </summary>
		Kind GetElementKind(object element);

		/// <summary>
		/// Returns untyped CodeElement instance member name.
		/// </summary>
		string GetElementKindName(object element);
	}
}