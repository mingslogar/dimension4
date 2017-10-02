using System.Windows;
using System.Windows.Media;

namespace Daytimer.Controls
{
	public static class DependencyObjectExtensionMethods
	{
		public static T FindChild<T>(this DependencyObject that, string elementName)
			where T : FrameworkElement
		{
			if (that == null)
				return null;

			var childrenCount = VisualTreeHelper.GetChildrenCount(that);

			for (var i = 0; i < childrenCount; i++)
			{
				var child = VisualTreeHelper.GetChild(that, i);
				var frameworkElement = child as FrameworkElement;

				if (frameworkElement != null && elementName == frameworkElement.Name)
					return (T)frameworkElement;

				if ((frameworkElement = frameworkElement.FindChild<T>(elementName)) != null)
					return (T)frameworkElement;
			}

			return null;
		}
	}
}
