using Daytimer.DatabaseHelpers;
using Daytimer.DatabaseHelpers.Contacts;
using Modern.FileBrowser;
using Daytimer.Functions;
using Daytimer.Fundamentals;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Daytimer.Controls.Panes.People
{
	/// <summary>
	/// Interaction logic for TilePicker.xaml
	/// </summary>
	public partial class TilePicker : OfficeWindow
	{
		public TilePicker(Name name)
		{
			InitializeComponent();

			if (name != null)
				header.Text = "Choose an image to show on " + name.FirstName + "'" + (!name.FirstName.EndsWith("s", StringComparison.InvariantCultureIgnoreCase) ? "s" : "") + " tile.";
			else
				header.Text = "Choose an image to show on this person's tile.";
		}

		/// <summary>
		/// Gets or sets the image.
		/// </summary>
		public BitmapSource ImageSource
		{
			get { return (BitmapSource)GetValue(ImageSourceProperty); }
			set
			{
				SetValue(ImageSourceProperty, value);

				if (value is BitmapImage && (value as BitmapImage).UriSource == Contact.DefaultTile)
				{
					_isDefault = true;
					clearButton.Visibility = Visibility.Hidden;
				}
				else
				{
					_isDefault = false;
					clearButton.Visibility = Visibility.Visible;
				}
			}
		}

		public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register(
			"ImageSource", typeof(BitmapSource), typeof(TilePicker),
			new PropertyMetadata(null, UpdateControlCallback));

		private static void UpdateControlCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			TilePicker tp = d as TilePicker;

			if (e.Property == ImageSourceProperty)
			{
				BitmapSource src = e.NewValue as BitmapSource;

				if (src != null && src.PixelHeight > 0 && src.PixelWidth > 0)
				{
					if (src.PixelWidth > src.PixelHeight)
					{
						double value = (((double)src.PixelWidth / ((double)src.PixelHeight / 96d)) - 96d) / 2d;
						tp.image.Margin = new Thickness(-value, 0, -value, 0);
					}
					else if (src.PixelHeight > src.PixelWidth)
					{
						double value = (((double)src.PixelHeight / ((double)src.PixelWidth / 96d)) - 96d) / 2d;
						tp.image.Margin = new Thickness(0, -value, 0, -value);
					}
				}
			}
		}

		private bool _imageChanged = false;
		//private Uri _imageSource = null;
		private bool _isDefault = true;

		private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			double addend = 96 * (e.NewValue - e.OldValue);

			double horizontalAddend = addend;

			if (image.ActualWidth > image.ActualHeight)
				horizontalAddend *= image.ActualWidth / image.ActualHeight;

			double verticalAddend = addend;

			if (image.ActualHeight > image.ActualWidth)
				verticalAddend *= image.ActualHeight / image.ActualWidth;

			double left = image.Margin.Left - horizontalAddend;
			double top = image.Margin.Top - verticalAddend;
			double right = image.Margin.Right - horizontalAddend;
			double bottom = image.Margin.Bottom - verticalAddend;

			if (left > 0)
			{
				right += left;
				left = 0;
			}
			else if (right > 0)
			{
				left += right;
				right = 0;
			}

			if (top > 0)
			{
				bottom += top;
				top = 0;
			}
			else if (bottom > 0)
			{
				top += bottom;
				bottom = 0;
			}

			image.Margin = new Thickness(left, top, right, bottom);
			_imageChanged = true;
			_isDefault = false;
			clearButton.Visibility = Visibility.Visible;
		}

		private bool _isDragging = false;
		private Point _prevPoint;

		private void image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			_isDragging = true;
			Mouse.Capture(image);
			_prevPoint = Mouse.GetPosition(this);
		}

		private void image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			_isDragging = false;
			Mouse.Capture(null);
		}

		private void image_MouseMove(object sender, MouseEventArgs e)
		{
			if (_isDragging)
			{
				Point _newPoint = e.GetPosition(this);
				double changeX = _newPoint.X - _prevPoint.X;
				double changeY = _newPoint.Y - _prevPoint.Y;

				double left = image.Margin.Left + changeX;
				double top = image.Margin.Top + changeY;
				double right = image.Margin.Right - changeX;
				double bottom = image.Margin.Bottom - changeY;

				if (left > 0)
				{
					right += left;
					left = 0;
				}
				else if (right > 0)
				{
					left += right;
					right = 0;
				}

				if (top > 0)
				{
					bottom += top;
					top = 0;
				}
				else if (bottom > 0)
				{
					top += bottom;
					bottom = 0;
				}

				_prevPoint = _newPoint;
				image.Margin = new Thickness(left, top, right, bottom);
			}
		}

		private void okButton_Click(object sender, RoutedEventArgs e)
		{
			if (_imageChanged)
			{
				//double scale = image.Source.Width / image.ActualWidth;
				//int left = -(int)(image.Margin.Left / scale);
				//int top = -(int)(image.Margin.Top / scale);
				//int size = (int)(96 / scale);

				//BitmapImage img = new BitmapImage(_imageSource);
				//CroppedBitmap cropped = new CroppedBitmap(img, new Int32Rect(left, top, size, size));
				//TransformedBitmap scaled = new TransformedBitmap(cropped, new ScaleTransform(1 / scale, 1 / scale));
				//ImageSource = scaled;

				if (_isDefault)
					ImageSource = null;
				else
					ImageSource = ImageProc.GetImage(border);

				DialogResult = true;
			}
			else
				DialogResult = false;
		}

		private void browseButton_Click(object sender, RoutedEventArgs e)
		{
			FileDialog d = new FileDialog(this,
				Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
				FileDialogType.Open, ListViewMode.LargeIcon);
			d.Filter = "Pictures (*.bmp;*.emf;*.gif;*.ico;*.icon;*.jpg;*.jpeg;*.jpe;*.jfif;*.png;*.tif;*.tiff;*.wmf)|.bmp;.emf;.gif;.ico;.icon;.jpg;.jpeg;.jpe;.jfif;.png;.tif;.tiff;.wmf";
			d.FilterIndex = 0;
			//d.RootFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
			d.Title = "Choose Image";
			//d.IconSize = IconSize.Large;
			if (d.ShowDialog() == true)
			{
				Thread load = new Thread(LoadImage);
				load.IsBackground = true;
				load.Priority = ThreadPriority.Lowest;
				load.Start(d.SelectedFile);

				//BitmapImage img = new BitmapImage();
				//img.BeginInit();
				//img.UriSource = new Uri(d.SelectedFile);
				//img.EndInit();
				//img.Freeze();

				//image.Margin = new Thickness(0);
				//ImageSource = img;
				//img = null;

				//slider.ValueChanged -= slider_ValueChanged;
				//slider.Value = 0;
				//slider.ValueChanged += slider_ValueChanged;

				//_imageChanged = true;
				//_isDefault = false;
			}
			d = null;
		}

		private void LoadImage(object source)
		{
			BitmapImage img = new BitmapImage();
			img.BeginInit();
			img.UriSource = new Uri(source.ToString());
			img.EndInit();
			img.Freeze();

			Dispatcher.BeginInvoke(new SetImageDelegate(SetImage), DispatcherPriority.Send, new object[] { img });
		}

		private delegate void SetImageDelegate(BitmapSource source);

		private void SetImage(BitmapSource source)
		{
			image.Margin = new Thickness(0);
			ImageSource = source;

			source = null;

			slider.ValueChanged -= slider_ValueChanged;
			slider.Value = 0;
			slider.ValueChanged += slider_ValueChanged;

			_imageChanged = true;
			_isDefault = false;

			GC.Collect();
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);
			image.Source = null;
		}

		private void clearButton_Click(object sender, RoutedEventArgs e)
		{
			image.Margin = new Thickness(0);
			ImageSource = new BitmapImage(Contact.DefaultTile);
			_imageChanged = true;

			slider.ValueChanged -= slider_ValueChanged;
			slider.Value = 0;
			slider.ValueChanged += slider_ValueChanged;
		}
	}
}
