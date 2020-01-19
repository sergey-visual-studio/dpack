namespace DPackRx.CodeModel
{
	/// <summary>
	/// Scaled down minimum code item definition.
	/// </summary>
	public interface IExtensibilityItem
	{
		/// <summary>
		/// Item name.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Untyped extensibility link (name matches the actual type).
		/// </summary>
		object ProjectItem { get; }

		/// <summary>
		/// Item code type, whenever's applicable.
		/// </summary>
		FileSubType ItemSubType { get; }

		/// <summary>
		/// Item parent code type, whenever's applicable.
		/// </summary>
		FileSubType ParentSubType { get; }
	}
}