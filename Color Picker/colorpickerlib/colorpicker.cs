using Gma.UserActivityMonitor;
using Microsoft.Windows.Shell;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Microsoft.Samples.CustomControls
{
	[ComVisible(false)]
	public class colorpickerlib : Control
	{
		static colorpickerlib()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(colorpickerlib), new FrameworkPropertyMetadata(typeof(colorpickerlib)));
		}

		public colorpickerlib()
		{
			templateApplied = false;
			m_color = Colors.White;
			shouldFindPoint = true;
			SetValue(AProperty, m_color.A);
			SetValue(RProperty, m_color.R);
			SetValue(GProperty, m_color.G);
			SetValue(BProperty, m_color.B);
			SetValue(SelectedColorProperty, m_color);
		}

		#region Public Methods

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			m_ColorDetail = GetTemplateChild(ColorDetailName) as FrameworkElement;
			m_ColorMarker = GetTemplateChild(ColorMarkerName) as Path;
			m_ColorSlider = GetTemplateChild(ColorSliderName) as SpectrumSlider;
			ColorPickerButton = GetTemplateChild(ColorPickerButtonName) as ToggleButton;

			m_ColorSlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(BaseColorChanged);

			m_ColorMarker.RenderTransform = markerTransform;
			m_ColorMarker.RenderTransformOrigin = new Point(0.5, 0.5);
			m_ColorDetail.MouseLeftButtonDown += new MouseButtonEventHandler(OnMouseLeftButtonDown);
			m_ColorDetail.PreviewMouseMove += new MouseEventHandler(OnMouseMove);
			m_ColorDetail.MouseLeftButtonUp += OnMouseLeftButtonUp;
			m_ColorDetail.SizeChanged += new SizeChangedEventHandler(ColorDetailSizeChanged);

			ColorPickerButton.Checked += ColorPickerButton_Checked;

			templateApplied = true;
			shouldFindPoint = true;
			isAlphaChange = false;

			SelectedColor = m_color;
		}

		#endregion

		#region Public Properties

		// Gets or sets the selected color.
		public System.Windows.Media.Color SelectedColor
		{
			get { return (System.Windows.Media.Color)GetValue(SelectedColorProperty); }
			set { SetValue(SelectedColorProperty, m_color); setColor((Color)value); }
		}

		#region ARGB Properties

		// Gets or sets the ARGB alpha value of the selected color.
		public byte A
		{
			get { return (byte)GetValue(AProperty); }
			set { SetValue(AProperty, value); }
		}

		// Gets or sets the ARGB red value of the selected color.
		public byte R
		{
			get { return (byte)GetValue(RProperty); }
			set { SetValue(RProperty, value); }
		}

		// Gets or sets the ARGB green value of the selected color.
		public byte G
		{
			get { return (byte)GetValue(GProperty); }
			set { SetValue(GProperty, value); }
		}

		// Gets or sets the ARGB blue value of the selected color.
		public byte B
		{
			get { return (byte)GetValue(BProperty); }
			set { SetValue(BProperty, value); }
		}

		#endregion RGB Properties

		#region ScRGB Properties

		// Gets or sets the ScRGB alpha value of the selected color.
		public double ScA
		{
			get { return (double)GetValue(ScAProperty); }
			set { SetValue(ScAProperty, value); }
		}

		// Gets or sets the ScRGB red value of the selected color.
		public double ScR
		{
			get { return (double)GetValue(ScRProperty); }
			set { SetValue(RProperty, value); }
		}

		// Gets or sets the ScRGB green value of the selected color.
		public double ScG
		{
			get { return (double)GetValue(ScGProperty); }
			set { SetValue(GProperty, value); }
		}

		// Gets or sets the ScRGB blue value of the selected color.
		public double ScB
		{
			get { return (double)GetValue(BProperty); }
			set { SetValue(BProperty, value); }
		}

		#endregion ScRGB Properties

		// Gets or sets the the selected color in hexadecimal notation.
		public string HexadecimalString
		{
			get { return (string)GetValue(HexadecimalStringProperty); }
			set { SetValue(HexadecimalStringProperty, value); }
		}

		#endregion

		#region Public Events

		public event RoutedPropertyChangedEventHandler<Color> SelectedColorChanged
		{
			add { AddHandler(SelectedColorChangedEvent, value); }
			remove { RemoveHandler(SelectedColorChangedEvent, value); }
		}

		#endregion

		#region Dependency Property Fields
		public static readonly DependencyProperty SelectedColorProperty =
			DependencyProperty.Register
			("SelectedColor", typeof(System.Windows.Media.Color), typeof(colorpickerlib),
			new PropertyMetadata(System.Windows.Media.Colors.Transparent,
				new PropertyChangedCallback(selectedColor_changed)));

		public static readonly DependencyProperty ScAProperty =
			   DependencyProperty.Register
			   ("ScA", typeof(float), typeof(colorpickerlib),
			   new PropertyMetadata((float)1,
			   new PropertyChangedCallback(ScAChanged)));

		public static readonly DependencyProperty ScRProperty =
			  DependencyProperty.Register
			  ("ScR", typeof(float), typeof(colorpickerlib),
			  new PropertyMetadata((float)1,
			  new PropertyChangedCallback(ScRChanged)));

		public static readonly DependencyProperty ScGProperty =
			  DependencyProperty.Register
			  ("ScG", typeof(float), typeof(colorpickerlib),
			  new PropertyMetadata((float)1,
			  new PropertyChangedCallback(ScGChanged)));

		public static readonly DependencyProperty ScBProperty =
			  DependencyProperty.Register
			  ("ScB", typeof(float), typeof(colorpickerlib),
			  new PropertyMetadata((float)1,
			  new PropertyChangedCallback(ScBChanged)));

		public static readonly DependencyProperty AProperty =
			  DependencyProperty.Register
			  ("A", typeof(byte), typeof(colorpickerlib),
			  new PropertyMetadata((byte)255,
			  new PropertyChangedCallback(AChanged)));

		public static readonly DependencyProperty RProperty =
			DependencyProperty.Register
			("R", typeof(byte), typeof(colorpickerlib),
			new PropertyMetadata((byte)255,
			new PropertyChangedCallback(RChanged)));

		public static readonly DependencyProperty GProperty =
			DependencyProperty.Register
			("G", typeof(byte), typeof(colorpickerlib),
			new PropertyMetadata((byte)255,
			new PropertyChangedCallback(GChanged)));

		public static readonly DependencyProperty BProperty =
			DependencyProperty.Register
			("B", typeof(byte), typeof(colorpickerlib),
			new PropertyMetadata((byte)255,
			new PropertyChangedCallback(BChanged)));

		public static readonly DependencyProperty HexadecimalStringProperty =
			DependencyProperty.Register
			("HexadecimalString", typeof(string), typeof(colorpickerlib),
			new PropertyMetadata("#FFFFFFFF",
			new PropertyChangedCallback(HexadecimalStringChanged)));

		#endregion

		#region RoutedEvent Fields

		public static readonly RoutedEvent SelectedColorChangedEvent = EventManager.RegisterRoutedEvent(
			"SelectedColorChanged",
			RoutingStrategy.Bubble,
			typeof(RoutedPropertyChangedEventHandler<Color>),
			typeof(colorpickerlib));
		#endregion

		#region Property Changed Callbacks

		private static void AChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			colorpickerlib c = (colorpickerlib)d;
			c.OnAChanged((byte)e.NewValue);
		}

		protected virtual void OnAChanged(byte newValue)
		{
			m_color.A = newValue;
			SetValue(ScAProperty, m_color.ScA);
			SetValue(SelectedColorProperty, m_color);
		}

		private static void RChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			colorpickerlib c = (colorpickerlib)d;
			c.OnRChanged((byte)e.NewValue);
		}

		protected virtual void OnRChanged(byte newValue)
		{
			m_color.R = newValue;
			SetValue(ScRProperty, m_color.ScR);
			SetValue(SelectedColorProperty, m_color);
		}

		private static void GChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			colorpickerlib c = (colorpickerlib)d;
			c.OnGChanged((byte)e.NewValue);
		}

		protected virtual void OnGChanged(byte newValue)
		{
			m_color.G = newValue;
			SetValue(ScGProperty, m_color.ScG);
			SetValue(SelectedColorProperty, m_color);
		}

		private static void BChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			colorpickerlib c = (colorpickerlib)d;
			c.OnBChanged((byte)e.NewValue);
		}

		protected virtual void OnBChanged(byte newValue)
		{
			m_color.B = newValue;
			SetValue(ScBProperty, m_color.ScB);
			SetValue(SelectedColorProperty, m_color);
		}

		private static void ScAChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			colorpickerlib c = (colorpickerlib)d;
			c.OnScAChanged((float)e.NewValue);
		}

		protected virtual void OnScAChanged(float newValue)
		{
			isAlphaChange = true;
			if (shouldFindPoint)
			{
				m_color.ScA = newValue;
				SetValue(AProperty, m_color.A);
				SetValue(SelectedColorProperty, m_color);
				SetValue(HexadecimalStringProperty, m_color.ToString());
			}
			isAlphaChange = false;
		}

		private static void ScRChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			colorpickerlib c = (colorpickerlib)d;
			c.OnScRChanged((float)e.NewValue);
		}

		protected virtual void OnScRChanged(float newValue)
		{
			if (shouldFindPoint)
			{
				m_color.ScR = newValue;
				SetValue(RProperty, m_color.R);
				SetValue(SelectedColorProperty, m_color);
				SetValue(HexadecimalStringProperty, m_color.ToString());
			}
		}

		private static void ScGChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			colorpickerlib c = (colorpickerlib)d;
			c.OnScGChanged((float)e.NewValue);
		}

		protected virtual void OnScGChanged(float newValue)
		{
			if (shouldFindPoint)
			{
				m_color.ScG = newValue;
				SetValue(GProperty, m_color.G);
				SetValue(SelectedColorProperty, m_color);
				SetValue(HexadecimalStringProperty, m_color.ToString());
			}
		}

		private static void ScBChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			colorpickerlib c = (colorpickerlib)d;
			c.OnScBChanged((float)e.NewValue);
		}

		protected virtual void OnScBChanged(float newValue)
		{
			if (shouldFindPoint)
			{
				m_color.ScB = newValue;
				SetValue(BProperty, m_color.B);
				SetValue(SelectedColorProperty, m_color);
				SetValue(HexadecimalStringProperty, m_color.ToString());
			}
		}

		private static void HexadecimalStringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			colorpickerlib c = (colorpickerlib)d;
			c.OnHexadecimalStringChanged((string)e.OldValue, (string)e.NewValue);
		}

		protected virtual void OnHexadecimalStringChanged(string oldValue, string newValue)
		{
			try
			{
				if (shouldFindPoint)
				{
					m_color = (Color)ColorConverter.ConvertFromString(newValue);
				}

				SetValue(AProperty, m_color.A);
				SetValue(RProperty, m_color.R);
				SetValue(GProperty, m_color.G);
				SetValue(BProperty, m_color.B);

				if (shouldFindPoint && !isAlphaChange && templateApplied)
				{
					updateMarkerPosition(m_color);
				}
			}
			catch (FormatException)
			{
				SetValue(HexadecimalStringProperty, oldValue);
			}
		}

		private static void selectedColor_changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			colorpickerlib cPicker = (colorpickerlib)d;
			Color newValue = (Color)e.NewValue;

			cPicker.OnSelectedColorChanged((Color)e.OldValue, newValue);

			if (cPicker.m_ColorMarker != null)
				if (newValue.A > 150)
					cPicker.m_ColorMarker.Stroke = new SolidColorBrush(Color.FromArgb(255, (byte)(255 - newValue.R), (byte)(255 - newValue.G), (byte)(255 - newValue.B)));
				else
					cPicker.m_ColorMarker.Stroke = Brushes.Black;
		}

		protected virtual void OnSelectedColorChanged(Color oldColor, Color newColor)
		{
			RoutedPropertyChangedEventArgs<Color> newEventArgs =
				new RoutedPropertyChangedEventArgs<Color>(oldColor, newColor);
			newEventArgs.RoutedEvent = colorpickerlib.SelectedColorChangedEvent;
			RaiseEvent(newEventArgs);
		}

		#endregion

		#region Template Part Event Handlers

		protected override void OnTemplateChanged(ControlTemplate oldTemplate, ControlTemplate newTemplate)
		{
			templateApplied = false;
			if (oldTemplate != null)
			{
				m_ColorSlider.ValueChanged -= new RoutedPropertyChangedEventHandler<double>(BaseColorChanged);
				m_ColorDetail.MouseLeftButtonDown -= new MouseButtonEventHandler(OnMouseLeftButtonDown);
				m_ColorDetail.PreviewMouseMove -= new MouseEventHandler(OnMouseMove);
				m_ColorDetail.MouseLeftButtonUp -= OnMouseLeftButtonUp;
				m_ColorDetail.SizeChanged -= new SizeChangedEventHandler(ColorDetailSizeChanged);
				m_ColorDetail = null;
				m_ColorMarker = null;
				m_ColorSlider = null;
			}
			base.OnTemplateChanged(oldTemplate, newTemplate);
		}

		private void BaseColorChanged(object sender, RoutedPropertyChangedEventArgs<Double> e)
		{
			if (m_ColorPosition != null)
			{
				determineColor((Point)m_ColorPosition);
			}
		}

		private Point SanitizeInput(Point p)
		{
			p.X = p.X >= 0 ? p.X : 0;
			p.Y = p.Y >= 0 ? p.Y : 0;
			p.X = p.X <= m_ColorDetail.ActualWidth ? p.X : m_ColorDetail.ActualWidth;
			p.Y = p.Y < m_ColorDetail.ActualHeight ? p.Y : m_ColorDetail.ActualHeight;
			return p;
		}

		private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			Mouse.Capture(m_ColorDetail);
			Point p = e.GetPosition(m_ColorDetail);
			p = SanitizeInput(p);
			updateMarkerPosition(p);
		}

		private void OnMouseMove(object sender, MouseEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed)
			{
				Point p = e.GetPosition(m_ColorDetail);
				p = SanitizeInput(p);
				updateMarkerPosition(p);
			}
		}

		private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			Mouse.Capture(null);
		}

		private void ColorDetailSizeChanged(object sender, SizeChangedEventArgs args)
		{
			if (args.PreviousSize != Size.Empty &&
				args.PreviousSize.Width != 0 && args.PreviousSize.Height != 0)
			{
				double widthDifference = args.NewSize.Width / args.PreviousSize.Width;
				double heightDifference = args.NewSize.Height / args.PreviousSize.Height;
				markerTransform.X = markerTransform.X * widthDifference;
				markerTransform.Y = markerTransform.Y * heightDifference;
			}
			else if (m_ColorPosition != null)
			{
				markerTransform.X = ((Point)m_ColorPosition).X * args.NewSize.Width;
				markerTransform.Y = ((Point)m_ColorPosition).Y * args.NewSize.Height;
			}
		}

		private void ColorPickerButton_Checked(object sender, RoutedEventArgs e)
		{
			HookManager.MouseClick += HookManager_MouseClick;
		}

		private void HookManager_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			HookManager.MouseClick -= HookManager_MouseClick;

			System.Drawing.Bitmap bmpScreenShot = new System.Drawing.Bitmap(1, 1);
			System.Drawing.Graphics.FromImage(bmpScreenShot).CopyFromScreen(e.X, e.Y, 0, 0, new System.Drawing.Size(1, 1));
			System.Drawing.Color picked = bmpScreenShot.GetPixel(0, 0);

			setColor(Color.FromArgb(picked.A, picked.R, picked.G, picked.B));

			ColorPickerButton.IsChecked = false;

			Window window = Window.GetWindow(this);

			if (window.WindowState == WindowState.Minimized)
				SystemCommands.RestoreWindow(window);

			window.Activate();
		}

		#endregion

		#region Color Resolution Helpers

		private void setColor(Color theColor)
		{
			m_color = theColor;

			if (templateApplied)
			{
				SetValue(AProperty, m_color.A);
				SetValue(RProperty, m_color.R);
				SetValue(GProperty, m_color.G);
				SetValue(BProperty, m_color.B);
				updateMarkerPosition(theColor);
			}
		}

		private void updateMarkerPosition(Point p)
		{
			markerTransform.X = p.X;
			markerTransform.Y = p.Y;
			p.X = p.X / m_ColorDetail.ActualWidth;
			p.Y = p.Y / m_ColorDetail.ActualHeight;
			m_ColorPosition = p;
			determineColor(p);
		}

		private void updateMarkerPosition(Color theColor)
		{
			m_ColorPosition = null;

			HsvColor hsv = ColorUtilities.ConvertRgbToHsv(theColor.R, theColor.G, theColor.B);

			m_ColorSlider.Value = hsv.H;

			Point p = new Point(hsv.S, 1 - hsv.V);

			m_ColorPosition = p;
			p.X = p.X * m_ColorDetail.ActualWidth;
			p.Y = p.Y * m_ColorDetail.ActualHeight;
			markerTransform.X = p.X;
			markerTransform.Y = p.Y;
		}

		private void determineColor(Point p)
		{
			HsvColor hsv = new HsvColor(360 - m_ColorSlider.Value, 1, 1);
			hsv.S = p.X;
			hsv.V = 1 - p.Y;
			m_color = ColorUtilities.ConvertHsvToRgb(hsv.H, hsv.S, hsv.V);
			shouldFindPoint = false;
			m_color.ScA = (float)GetValue(ScAProperty);
			SetValue(HexadecimalStringProperty, m_color.ToString());
			shouldFindPoint = true;
		}

		#endregion

		#region Private Fields

		private SpectrumSlider m_ColorSlider;
		private const string ColorSliderName = "PART_ColorSlider";
		private FrameworkElement m_ColorDetail;
		private const string ColorDetailName = "PART_ColorDetail";
		private TranslateTransform markerTransform = new TranslateTransform();
		private Path m_ColorMarker;
		private const string ColorMarkerName = "PART_ColorMarker";
		private Point? m_ColorPosition;
		private Color m_color;
		private bool shouldFindPoint;
		private bool templateApplied;
		private bool isAlphaChange;

		private const string ColorPickerButtonName = "colorPickerButton";
		private ToggleButton ColorPickerButton;

		#endregion
	}
}