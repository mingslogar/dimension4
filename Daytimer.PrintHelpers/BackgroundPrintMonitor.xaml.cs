using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents.Serialization;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Xps;

namespace Daytimer.PrintHelpers
{
	/// <summary>
	/// Interaction logic for BackgroundSyncMonitor.xaml
	/// </summary>
	[ComVisible(false)]
	public partial class BackgroundPrintMonitor : Button
	{
		public BackgroundPrintMonitor(XpsDocumentWriter documentWriter)
		{
			InitializeComponent();

			xpsDocumentWriter = documentWriter;

			xpsDocumentWriter.WritingCancelled += xpsDocumentWriter_WritingCancelled;
			xpsDocumentWriter.WritingCompleted += xpsDocumentWriter_WritingCompleted;
			xpsDocumentWriter.WritingProgressChanged += xpsDocumentWriter_WritingProgressChanged;

			Unloaded += BackgroundPrintMonitor_Unloaded;
		}

		private void xpsDocumentWriter_WritingCancelled(object sender, WritingCancelledEventArgs e)
		{
			(Parent as ItemsControl).Items.Remove(this);
		}

		private void xpsDocumentWriter_WritingCompleted(object sender, WritingCompletedEventArgs e)
		{
			(Parent as ItemsControl).Items.Remove(this);
		}

		private void xpsDocumentWriter_WritingProgressChanged(object sender, WritingProgressChangedEventArgs e)
		{
			if (e.WritingLevel == WritingProgressChangeLevel.FixedDocumentWritingProgress)
			{
				// Doesn't seem like we are getting a progress value from the print job. Word
				// doesn't show progress, so I'm just showing indeterminate.

				//	progress.IsIndeterminate = false;
				//	progress.Value = e.ProgressPercentage;
				status.Text = "PRINTING DOCUMENT (" + e.Number.ToString() + " PAGE" + (e.Number != 1 ? "S" : "") + " COMPLETED)";

				//	KillAnimation();
			}
		}

		private void BackgroundPrintMonitor_Unloaded(object sender, RoutedEventArgs e)
		{
			xpsDocumentWriter.WritingCancelled -= xpsDocumentWriter_WritingCancelled;
			xpsDocumentWriter.WritingCompleted -= xpsDocumentWriter_WritingCompleted;
			xpsDocumentWriter.WritingProgressChanged -= xpsDocumentWriter_WritingProgressChanged;
			xpsDocumentWriter = null;
			KillAnimation();
		}

		public XpsDocumentWriter XpsDocumentWriter
		{
			get { return xpsDocumentWriter; }
		}

		private XpsDocumentWriter xpsDocumentWriter;

		private bool _isFirstTime = true;

		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);

			if (_isFirstTime)
			{
				_isFirstTime = false;

				try
				{
					ThicknessAnimation anim = new ThicknessAnimation(new Thickness(-13, 0, 0, 0),
						new Thickness(0, 0, -13, 0), new Duration(TimeSpan.FromMilliseconds(300)));
					anim.RepeatBehavior = RepeatBehavior.Forever;
					(progress.Template.FindName("IndeterminateFill", progress) as Rectangle).BeginAnimation(MarginProperty, anim);
				}
				catch { }
			}
		}

		private void KillAnimation()
		{
			try { (progress.Template.FindName("IndeterminateFill", progress) as Rectangle).ApplyAnimationClock(MarginProperty, null); }
			catch { }
		}

		private void cancelButton_Click(object sender, RoutedEventArgs e)
		{
			xpsDocumentWriter.CancelAsync();
		}
	}
}
