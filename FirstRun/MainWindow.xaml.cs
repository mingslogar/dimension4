using Daytimer.Functions;
using Daytimer.Fundamentals;
using Setup;
using Setup.Install;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace FirstRun
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	[ComVisible(false)]
	public partial class MainWindow : OfficeWindow
	{
		public MainWindow()
		{
			InitializeComponent();

			content.Content = new Welcome();
			CurrentSlidePosition++;
		}

		#region Protected Methods

		protected override void OnContentRendered(EventArgs e)
		{
			base.OnContentRendered(e);

			Slides = new object[]
			{
				content.Content,
				new CEIP(),
				new Personalize(),
				new Complete()
			};

			Thread load = new Thread(loadDlls);
			load.IsBackground = true;
			load.Priority = ThreadPriority.Lowest;
			load.Start();

			Activate();
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);

			if (Complete)
			{
				Settings.FirstRun = false;
				//Environment.Exit(0);
				Application.Current.Shutdown(0);
			}
			else
				//Environment.Exit(-1);
				Application.Current.Shutdown(-1);
		}

		#endregion

		#region Pre-load DLLs

		private void loadDlls()
		{
			new QuarticEase();
			new DoubleAnimation();
			new ThicknessAnimation();
			new BitmapCache();
			new RenderTargetBitmap(1, 1, 96, 96, PixelFormats.Pbgra32);
			new VisualBrush();
			DrawingVisual drawingVisual = new DrawingVisual();
			DrawingContext dC = drawingVisual.RenderOpen();

			Dispatcher.BeginInvoke(() =>
			{
				nextButton.Content = "_Next";
				nextButton.IsEnabled = true;
			});
		}

		#endregion

		#region Private Fields

		private object[] Slides = null;
		private int CurrentSlidePosition = 0;
		private bool Complete = false;

		#endregion

		#region Private Methods

		private void NextSlide()
		{
			if (CurrentSlidePosition < Slides.Length)
			{
				object next = Slides[CurrentSlidePosition++];

				image.Source = ImageProc.GetImage(content);

				if (next is CEIP)
				{
					nextButton.IsEnabled = true;
					floatingLogo.FadeIn();
				}
				else if (next is Personalize)
				{
					Settings.JoinedCEIP = (content.Content as CEIP).joinCEIP.IsChecked == true;
					nextButton.IsEnabled = true;
				}
				else if (next is Complete)
				{
					Complete = true;
					nextButton.IsEnabled = true;
					nextButton.Content = "_All Done!";
					nextButton.IsDefault = true;

					prevButton.FadeOut();
					floatingLogo.FadeOut();
				}

				content.Content = next;
				content.UpdateLayout();
				content.SlideDisplay(image, Setup.Extensions.SlideDirection.Right);
			}
			else
			{
				Close();
			}
		}

		private void PreviousSlide()
		{
			CurrentSlidePosition -= 2;
			object prev = Slides[CurrentSlidePosition++];
			image.Source = ImageProc.GetImage(content);
			content.Content = prev;
			content.SlideDisplay(image, Setup.Extensions.SlideDirection.Left);

			if (CurrentSlidePosition > 1)
			{
				prevButton.FadeIn();
				prevButton.IsEnabled = true;
			}
			else
			{
				prevButton.FadeOut();
				floatingLogo.FadeOut();
			}
		}

		#endregion

		private void prevButton_Click(object sender, RoutedEventArgs e)
		{
			prevButton.IsEnabled = false;
			nextButton.IsEnabled = true;
			nextButton.Content = "_Next";
			PreviousSlide();
		}

		private void nextButton_Click(object sender, RoutedEventArgs e)
		{
			nextButton.IsEnabled = false;
			prevButton.IsEnabled = true;
			prevButton.FadeIn();
			NextSlide();
		}
	}
}
