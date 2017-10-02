using Daytimer.Dialogs;
using Daytimer.Functions;
using Daytimer.Fundamentals;
using Setup.Install;
using Setup.InstallHelpers;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace Setup
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
			//ShadowController c = new ShadowController(this);

			if (!InstallerData.UninstallMode)
				_firstSlide = new Welcome();
			else
				_firstSlide = new Uninstall.Welcome();

			content.Content = _firstSlide;
			counter++;

			Loaded += MainWindow_Loaded;
		}

		private void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			((App)Application.Current).SplashWindow.LoadComplete();
			Activate();
		}

		private object _firstSlide;
		private bool _isSetupComplete = false;

		protected override void OnContentRendered(EventArgs e)
		{
			base.OnContentRendered(e);

			if (!InstallerData.UninstallMode)
			{
				if (!Activation.IsActivated(false))
					Slides = new object[] { 
						_firstSlide,
						new CEIP(), 
						new ProductKey(),
						new License(),
						new Personalize(),
						//new InstallLocation(),
						new InstallProgress() 
					};
				else
					Slides = new object[] { 
						_firstSlide,
						new CEIP(), 
						new License(),
						new Personalize(),
						//new InstallLocation(),
						new InstallProgress() 
					};
			}
			else
			{
				Slides = new object[] {
					_firstSlide,
					new Uninstall.UninstallOptions(), 
					new Uninstall.UninstallProgress()
				};
			}

			Thread load = new Thread(loadDlls);
			load.IsBackground = true;
			load.Priority = ThreadPriority.Lowest;
			load.Start();

			Activate();
		}

		#region Pre-load DLLs

		private void loadDlls()
		{
			//Thread.Sleep(Extensions.AnimationDuration.TimeSpan);

			new QuarticEase();
			new DoubleAnimation();
			new ThicknessAnimation();
			new BitmapCache();
			new RenderTargetBitmap(1, 1, 96, 96, PixelFormats.Pbgra32);
			new VisualBrush();
			DrawingVisual drawingVisual = new DrawingVisual();
			DrawingContext dC = drawingVisual.RenderOpen();

			if (InstallerData.UninstallMode)
			{
				Process[] procs = Process.GetProcessesByName("Daytimer");

				foreach (Process each in procs)
					try { each.Kill(); }
					catch { }
			}

			Dispatcher.BeginInvoke(new VoidDelegate(complete));
		}

		private delegate void VoidDelegate();

		private void complete()
		{
			nextButton.Content = "_Next";
			nextButton.IsEnabled = true;
		}

		#endregion

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			base.OnClosing(e);

			if (!_isSetupComplete)
			{
				TaskDialog td = new TaskDialog(this, "Cancel Setup", "Setup is not complete. Are you sure you want to exit?", MessageType.Question, true);
				if (td.ShowDialog() == false)
					e.Cancel = true;
				else
				{
					if (InstallerData.UninstallMode)
						InstallerData.UninstallMode = false;
				}
			}
		}

		//#region Custom window

		//protected override void OnActivated(EventArgs e)
		//{
		//	base.OnActivated(e);
		//	minimizeButton.Foreground = new SolidColorBrush(Color.FromArgb(255, 102, 102, 102));
		//	windowBorder.SetResourceReference(Border.BorderBrushProperty, "WindowBorderFocused");
		//}

		//protected override void OnDeactivated(EventArgs e)
		//{
		//	base.OnDeactivated(e);
		//	minimizeButton.Foreground = new SolidColorBrush(Color.FromArgb(255, 178, 178, 178));
		//	windowBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 130, 130, 130));
		//}

		//private void minimizeButton_Click(object sender, RoutedEventArgs e)
		//{
		//	WindowState = WindowState.Minimized;
		//}

		//private void closeButton_Click(object sender, RoutedEventArgs e)
		//{
		//	Close();
		//}

		//#endregion

		#region Slides

		private int counter = 0;
		private object[] Slides;

		private void NextSlide()
		{
			if (counter > 0)
				floatingLogo.FadeIn();

			if (counter < Slides.Length)
			{
				object next = Slides[counter++];

				image.Source = ImageProc.GetImage(content);

				//
				// Install
				//
				if (next is CEIP)
					nextButton.IsEnabled = true;
				else if (next is ProductKey)
				{
					InstallerData.JoinedCEIP = (content.Content as CEIP).joinCEIP.IsChecked == true;

					ProductKey _next = next as ProductKey;

					_next.OptionSelected -= ProductKey_OptionSelected;
					_next.OptionDeselected -= ProductKey_OptionDeselected;
					_next.OptionSelected += ProductKey_OptionSelected;
					_next.OptionDeselected += ProductKey_OptionDeselected;

					if (_next.freeTrial.IsChecked == true || InstallerData.ProductKey != null)
						nextButton.IsEnabled = true;
				}
				else if (next is License)
				{
					// In case of upgrade, the product key slide will never be shown.
					if (content.Content is CEIP)
						InstallerData.JoinedCEIP = (content.Content as CEIP).joinCEIP.IsChecked == true;

					License _next = next as License;

					_next.Agreed -= License_Agreed;
					_next.Disagreed -= License_Disagreed;
					_next.Agreed += License_Agreed;
					_next.Disagreed += License_Disagreed;

					if (InstallerData.AcceptedAgreement)
						nextButton.IsEnabled = true;

					//	nextButton.Content = "_Install";
				}
				else if (next is Personalize)
				{
					nextButton.IsEnabled = true;
					nextButton.Content = "_Install";
				}
				//else if (next is InstallLocation)
				//{
				//	nextButton.IsEnabled = true;
				//	nextButton.Content = "_Install";
				//}
				else if (next is InstallProgress)
				{
					prevButton.FadeOut();
					prevButton.IsEnabled = false;

					nextButton.Content = "_Working...";

					//InstallerData.InstallLocation = (content.Content as InstallLocation).installLocation.Text;

					if (!InstallerData.InstallLocation.EndsWith("\\" + InstallerData.DisplayName))
						InstallerData.InstallLocation += "\\" + InstallerData.DisplayName;

					(next as InstallProgress).Error += Install_Error;
					(next as InstallProgress).Completed += Install_Completed;
				}

				//
				// Uninstall
				//
				if (next is Uninstall.UninstallOptions)
				{
					nextButton.IsEnabled = true;
					nextButton.Content = "_Uninstall";
				}
				else if (next is Uninstall.UninstallProgress)
				{
					prevButton.FadeOut();
					prevButton.IsEnabled = false;

					nextButton.Content = "_Working...";

					Uninstall.UninstallOptions optns = content.Content as Uninstall.UninstallOptions;

					InstallerData.DeleteDatabase = (bool)optns.deleteDatabase.IsChecked;
					InstallerData.DeleteAccounts = (bool)optns.deleteAccounts.IsChecked;
					InstallerData.DeleteSettings = (bool)optns.deleteSettings.IsChecked;
					InstallerData.DeleteDictionary = (bool)optns.deleteDictionary.IsChecked;

					(next as Uninstall.UninstallProgress).Error += Uninstall_Error;
					(next as Uninstall.UninstallProgress).Completed += Uninstall_Completed;
				}

				content.Content = next;
				content.UpdateLayout();
				content.SlideDisplay(image, Extensions.SlideDirection.Right);
			}
			else
			{
				Close();
			}
		}

		private void PreviousSlide()
		{
			counter -= 2;
			object prev = Slides[counter++];
			image.Source = ImageProc.GetImage(content);
			content.Content = prev;
			content.SlideDisplay(image, Extensions.SlideDirection.Left);

			if (counter > 1)
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

		#region Install

		private void ProductKey_OptionSelected(object sender, EventArgs e)
		{
			nextButton.IsEnabled = true;
		}

		private void ProductKey_OptionDeselected(object sender, EventArgs e)
		{
			nextButton.IsEnabled = false;
		}

		private void License_Agreed(object sender, EventArgs e)
		{
			nextButton.IsEnabled = true;
		}

		private void License_Disagreed(object sender, EventArgs e)
		{
			nextButton.IsEnabled = false;
		}

		private void Install_Completed(object sender, EventArgs e)
		{
			nextButton.IsEnabled = true;
			nextButton.Content = "_All Done!";
			nextButton.IsDefault = true;
			_isSetupComplete = true;

			floatingLogo.FadeOut();
		}

		private void Install_Error(object sender, EventArgs e)
		{
			nextButton.IsEnabled = true;
			nextButton.Content = "_Close";
			nextButton.IsDefault = true;
			_isSetupComplete = true;
		}

		#endregion

		#region Uninstall

		private void Uninstall_Completed(object sender, EventArgs e)
		{
			nextButton.IsEnabled = true;
			nextButton.Content = "_Finish";
			nextButton.IsDefault = true;
			_isSetupComplete = true;

			floatingLogo.FadeOut();
		}

		private void Uninstall_Error(object sender, EventArgs e)
		{
			nextButton.IsEnabled = true;
			nextButton.Content = "_Close";
			nextButton.IsDefault = true;
			_isSetupComplete = true;
		}

		#endregion

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
