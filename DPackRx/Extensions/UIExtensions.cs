using System.Windows.Media;

namespace DPackRx.Extensions
{
	public static class UIExtensions
	{
		/// <summary>
		/// Custom: returns child element of a given type.
		/// </summary>
		public static T GetChild<T>(this Visual element) where T : Visual
		{
			if (element == null)
				return default;

			if (element.GetType() == typeof(T))
				return element as T;

			T result = null;

			for (var index = 0; index < VisualTreeHelper.GetChildrenCount(element); index++)
			{
				var visualElement = VisualTreeHelper.GetChild(element, index) as Visual;
				result = visualElement.GetChild<T>();
				if (result != null)
					break;
			}

			return result;
		}
	}
}