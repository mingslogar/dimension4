using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Daytimer.Controls.Panes.People
{
	/// <summary>
	/// Interaction logic for ContactDetailBlock.xaml
	/// </summary>
	public partial class ContactDetailBlock : Grid
	{
		public ContactDetailBlock()
		{
			InitializeComponent();

			Binding binding = new Binding();
			binding.Source = this;
			binding.Path = new PropertyPath(ReadOnlyProperty);
			binding.Mode = BindingMode.OneWay;

			SetBinding(TitleReadOnlyProperty, binding);
		}

		public ContactDetailBlock(string title, string detail)
			: this()
		{
			Title = title;
			Detail = detail;
		}

		public ContactDetailBlock(string title, string detail, Panel owner)
			: this(title, detail)
		{
			owner.Children.Add(this);
		}

		public ContactDetailBlock(string title, string detail, Panel owner, object tag)
			: this(title, detail, owner)
		{
			Tag = tag;
		}

		public object OriginalData = null;

		#region DependencyProperties

		/// <summary>
		/// Gets or sets the title.
		/// </summary>
		public string Title
		{
			get { return (string)GetValue(TitleProperty); }
			set { SetValue(TitleProperty, value); }
		}

		public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
			"Title", typeof(string), typeof(ContactDetailBlock),
			new PropertyMetadata("Title"));

		/// <summary>
		/// Gets or sets the detail.
		/// </summary>
		public string Detail
		{
			get { return (string)GetValue(DetailProperty); }
			set { SetValue(DetailProperty, value); }
		}

		public static readonly DependencyProperty DetailProperty = DependencyProperty.Register(
			"Detail", typeof(string), typeof(ContactDetailBlock),
			new PropertyMetadata("Detail"));

		/// <summary>
		/// Gets or sets if the detail is read-only.
		/// </summary>
		public bool ReadOnly
		{
			get { return (bool)GetValue(ReadOnlyProperty); }
			set { SetValue(ReadOnlyProperty, value); }
		}

		public static readonly DependencyProperty ReadOnlyProperty = DependencyProperty.Register(
			"ReadOnly", typeof(bool), typeof(ContactDetailBlock),
			new PropertyMetadata(true, UpdateControlCallback));

		/// <summary>
		/// Gets or sets if the title field is read-only.
		/// </summary>
		public bool TitleReadOnly
		{
			get { return (bool)GetValue(TitleReadOnlyProperty); }
			set { SetValue(TitleReadOnlyProperty, value); }
		}

		public static readonly DependencyProperty TitleReadOnlyProperty = DependencyProperty.Register(
			"TitleReadOnly", typeof(bool), typeof(ContactDetailBlock),
			new PropertyMetadata(true, UpdateControlCallback));

		/// <summary>
		/// Gets or sets the font size to use for the title when in read-only mode.
		/// </summary>
		public double TitleFontSizeNormal
		{
			get { return (double)GetValue(TitleFontSizeNormalProperty); }
			set { SetValue(TitleFontSizeNormalProperty, value); }
		}

		public static readonly DependencyProperty TitleFontSizeNormalProperty = DependencyProperty.Register(
			"TitleFontSizeNormal", typeof(double), typeof(ContactDetailBlock),
			new PropertyMetadata(16d));

		/// <summary>
		/// Gets or sets the font size to use for the title when in edit mode.
		/// </summary>
		public double TitleFontSizeEdit
		{
			get { return (double)GetValue(TitleFontSizeEditProperty); }
			set { SetValue(TitleFontSizeEditProperty, value); }
		}

		public static readonly DependencyProperty TitleFontSizeEditProperty = DependencyProperty.Register(
			"TitleFontSizeEdit", typeof(double), typeof(ContactDetailBlock),
			new PropertyMetadata(12d));

		/// <summary>
		/// Gets or sets if the editing control is multiline.
		/// </summary>
		public bool IsMultiLine
		{
			get { return (bool)GetValue(IsMultiLineProperty); }
			set { SetValue(IsMultiLineProperty, value); }
		}

		public static readonly DependencyProperty IsMultiLineProperty = DependencyProperty.Register(
			"IsMultiLine", typeof(bool), typeof(ContactDetailBlock),
			new PropertyMetadata(false, UpdateControlCallback));

		/// <summary>
		/// Gets or sets if an ellipsis is shown next to the text box.
		/// </summary>
		public bool CanClarify
		{
			get { return (bool)GetValue(CanClarifyProperty); }
			set { SetValue(CanClarifyProperty, value); }
		}

		public static readonly DependencyProperty CanClarifyProperty = DependencyProperty.Register(
			"CanClarify", typeof(bool), typeof(ContactDetailBlock),
			new PropertyMetadata(false, UpdateControlCallback));

		private static void UpdateControlCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ContactDetailBlock block = d as ContactDetailBlock;

			if (e.Property == IsMultiLineProperty)
			{
				if ((bool)e.NewValue)
				{
					block.textBox.MaxLines = int.MaxValue;
					block.textBox.Height = block.ReadOnly ? double.NaN : 66;
					block.textBox.Padding = block.ReadOnly ? new Thickness(0, 3, 0, 3) : new Thickness(2, 4, 2, 4);
					block.textBox.VerticalContentAlignment = VerticalAlignment.Top;
					block.textBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
					block.textBox.TextWrapping = TextWrapping.Wrap;
					block.textBox.AcceptsReturn = true;
				}
				else
				{
					block.textBox.MaxLines = 1;
					block.textBox.Height = block.ReadOnly ? double.NaN : 23;
					block.textBox.Padding = block.ReadOnly ? new Thickness(0) : new Thickness(2, 0, 2, 0);
					block.textBox.VerticalContentAlignment = VerticalAlignment.Center;
					block.textBox.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
					block.textBox.TextWrapping = TextWrapping.NoWrap;
					block.textBox.AcceptsReturn = false;
				}
			}
			else if (e.Property == ReadOnlyProperty)
			{
				Binding b = new Binding();
				b.ElementName = "userControl";

				if ((bool)e.NewValue)
				{
					b.Path = new PropertyPath(TitleFontSizeNormalProperty);
					block.textBox.Padding = block.IsMultiLine ? new Thickness(0, 3, 0, 3) : new Thickness(0);
					block.textBox.Height = double.NaN;
					block.textBox.BorderThickness = new Thickness(0);
					block.textBox.HorizontalAlignment = HorizontalAlignment.Left;
					block.header.Margin = new Thickness(0);
				}
				else
				{
					b.Path = new PropertyPath(TitleFontSizeEditProperty);

					if (block.IsMultiLine)
					{
						block.textBox.Padding = new Thickness(2, 4, 2, 4);
						block.textBox.Height = 66;
					}
					else
					{
						block.textBox.Padding = new Thickness(0, 0, 2, 0);
						block.textBox.Height = 23;
						block.header.Margin = new Thickness(0, 0, 0, 3);
					}

					block.textBox.BorderThickness = new Thickness(1);
					block.textBox.HorizontalAlignment = HorizontalAlignment.Stretch;
				}

				block.header.SetBinding(Control.FontSizeProperty, b);
			}
			else if (e.Property == TitleReadOnlyProperty)
			{
				if ((bool)e.NewValue)
					block.header.IsHitTestVisible = false;
				else
					block.header.IsHitTestVisible = true;
			}
			else if (e.Property == CanClarifyProperty)
			{
				block.clarifyButton.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
			}
		}

		#endregion

		public void FocusTextBox()
		{
			textBox.Activate();
		}

		private void textBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			Detail = textBox.Text;
		}

		private void clarifyButton_Click(object sender, RoutedEventArgs e)
		{
			RaiseClarifyEvent();
		}

		#region RoutedEvents

		public static readonly RoutedEvent ClarifyEvent = EventManager.RegisterRoutedEvent(
			"Clarify", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ContactDetailBlock));

		public event RoutedEventHandler Clarify
		{
			add { AddHandler(ClarifyEvent, value); }
			remove { RemoveHandler(ClarifyEvent, value); }
		}

		private void RaiseClarifyEvent()
		{
			RaiseEvent(new RoutedEventArgs(ContactDetailBlock.ClarifyEvent));
		}

		#endregion
	}
}
