using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace Daytimer.Controls.Panes.People
{
	/// <summary>
	/// Interaction logic for ContactDetailBlockHyperlink.xaml
	/// </summary>
	public partial class ContactDetailBlockHyperlink : Grid
	{
		public ContactDetailBlockHyperlink()
		{
			InitializeComponent();
			InitializeRichTextBox();
		}

		public ContactDetailBlockHyperlink(string title, string detail, Panel owner)
		{
			InitializeComponent();
			InitializeRichTextBox();

			Title = title;
			Hyperlink = detail;
			owner.Children.Add(this);
		}

		public ContactDetailBlockHyperlink(string title, string detail, string uri, Panel owner)
		{
			InitializeComponent();
			InitializeRichTextBox();

			Title = title;

			new TextRange(textBox.Document.ContentStart, textBox.Document.ContentEnd).Text = detail;
			Hyperlink link = new Hyperlink(textBox.Document.ContentStart, textBox.Document.ContentEnd);
			link.NavigateUri = new Uri(uri);

			owner.Children.Add(this);
		}

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
			"Title", typeof(string), typeof(ContactDetailBlockHyperlink), new PropertyMetadata("Title"));

		/// <summary>
		/// Gets or sets the font size to use for the title when in edit mode.
		/// </summary>
		public double TitleFontSize
		{
			get { return (double)GetValue(TitleFontSizeProperty); }
			set { SetValue(TitleFontSizeProperty, value); }
		}

		public static readonly DependencyProperty TitleFontSizeProperty = DependencyProperty.Register(
			"TitleFontSize", typeof(double), typeof(ContactDetailBlockHyperlink),
			new PropertyMetadata(16d));

		#endregion

		public string Hyperlink
		{
			get { return new TextRange(textBox.Document.ContentStart, textBox.Document.ContentEnd).Text; }
			set
			{
				new TextRange(textBox.Document.ContentStart, textBox.Document.ContentEnd).Text = value;
				Hyperlink link = new Hyperlink(textBox.Document.ContentStart, textBox.Document.ContentEnd);

				try { link.NavigateUri = new Uri(value); }
				catch
				{
					try { link.NavigateUri = new Uri("http://" + value); }
					catch { }
				}
			}
		}

		/// <summary>
		/// Virtually turn off wrapping in the RichTextBox.
		/// </summary>
		private void InitializeRichTextBox()
		{
			textBox.Document.PageWidth = 4096;
		}

		private void Hyperlink_Click(object sender, RoutedEventArgs e)
		{
			TextEditing.Hyperlink_Click(sender, e);
		}

		private void Hyperlink_MouseEnter(object sender, MouseEventArgs e)
		{
			TextEditing.Hyperlink_MouseEnter(sender, e);
		}

		private void Hyperlink_MouseLeave(object sender, MouseEventArgs e)
		{
			TextEditing.Hyperlink_MouseLeave(sender, e);
		}
	}
}
