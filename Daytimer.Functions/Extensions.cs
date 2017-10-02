using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media.Animation;

namespace Daytimer.Functions
{
	public static class Extensions
	{
		#region Fade Out

		public static void FadeOut(this FrameworkElement element, Duration fadeDuration, FrameworkElement crossFade = null, bool delayCrossFade = true)
		{
			DoubleAnimation fadeOutAnim = new DoubleAnimation(1, 0, fadeDuration);

			fadeOutAnim.Completed += (sender, e) =>
			{
				if (element != null)
				{
					element.Opacity = 0;
					element.ApplyAnimationClock(FrameworkElement.OpacityProperty, null);
				}

				if (delayCrossFade && crossFade != null)
					crossFade.FadeIn(fadeDuration);
			};

			element.BeginAnimation(FrameworkElement.OpacityProperty, fadeOutAnim);

			if (!delayCrossFade && crossFade != null)
				crossFade.FadeIn(fadeDuration);
		}

		#endregion

		#region Fade In

		public static void FadeIn(this FrameworkElement element, Duration fadeDuration)
		{
			DoubleAnimation fadeInAnim = new DoubleAnimation(0, 1, fadeDuration);

			fadeInAnim.Completed += (sender, e) =>
			{
				if (element != null)
				{
					element.Opacity = 1;
					element.ApplyAnimationClock(FrameworkElement.OpacityProperty, null);
				}
			};

			element.BeginAnimation(FrameworkElement.OpacityProperty, fadeInAnim);
		}

		#endregion

		#region String Manipulation

		public static string Capitalize(this string data)
		{
			return char.ToUpper(data[0]) + data.Substring(1);
		}

		#endregion

		/// <summary>
		/// Creates a deep copy of a FlowDocument.
		/// </summary>
		/// <param name="doc"></param>
		/// <returns></returns>
		public static FlowDocument Copy(this FlowDocument doc)
		{
			if (doc == null)
				return null;

			TextRange range = new TextRange(doc.ContentStart, doc.ContentEnd);
			MemoryStream mStream = new MemoryStream();
			range.Save(mStream, DataFormats.XamlPackage);

			FlowDocument copy = new FlowDocument();
			TextRange copyRange = new TextRange(copy.ContentStart, copy.ContentEnd);
			copyRange.Load(mStream, DataFormats.XamlPackage);

			mStream.Close();

			copy.Resources = doc.Resources;

			return copy;
		}
	}
}
