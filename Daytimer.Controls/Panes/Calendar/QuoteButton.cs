using Daytimer.DatabaseHelpers.Quotes;
using Daytimer.Fundamentals;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace Daytimer.Controls.Panes.Calendar
{
	[ComVisible(false)]
	public class QuoteButton : ToggleButton
	{
		#region Constructors

		static QuoteButton()
		{
			Type ownerType = typeof(QuoteButton);
			DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));
		}

		public QuoteButton()
		{

		}

		#endregion

		#region Dependency Properties

		public static readonly DependencyProperty QuoteProperty = DependencyProperty.Register(
			"Quote", typeof(Quote), typeof(QuoteButton));

		/// <summary>
		/// Gets or sets the quote that is displayed in this popup.
		/// </summary>
		public Quote Quote
		{
			get { return (Quote)GetValue(QuoteProperty); }
			set { SetValue(QuoteProperty, value); }
		}

		public static readonly DependencyProperty BalloonTipStyleProperty = DependencyProperty.Register(
			"BalloonTipStyle", typeof(Style), typeof(QuoteButton));

		/// <summary>
		/// Gets or sets the style of the balloon tip which is
		/// displayed when this button is checked.
		/// </summary>
		public Style BalloonTipStyle
		{
			get { return (Style)GetValue(BalloonTipStyleProperty); }
			set { SetValue(BalloonTipStyleProperty, value); }
		}

		#endregion

		#region Protected Methods

		protected override void OnChecked(RoutedEventArgs e)
		{
			base.OnChecked(e);
			e.Handled = true;

			ShowPopup();
		}

		protected override void OnUnchecked(RoutedEventArgs e)
		{
			base.OnUnchecked(e);
			e.Handled = true;

			if (tip != null && tip.IsOpen)
				tip.FastClose();
		}

		protected override void OnMouseEnter(MouseEventArgs e)
		{
			base.OnMouseEnter(e);

			if (tip != null && tip.IsOpen)
				tip.ResetTimer();
		}

		protected override void OnMouseLeave(MouseEventArgs e)
		{
			base.OnMouseLeave(e);

			if (tip != null && tip.IsOpen)
				tip.Close();
		}

		#endregion

		#region Private Methods

		private BalloonTip tip;

		private void ShowPopup()
		{
			if (tip != null && tip.IsOpen)
				tip.FastClose();

			tip = new BalloonTip(this);
			tip.Style = BalloonTipStyle;
			tip.Owner = Window.GetWindow(this);

			tip.SetBinding(Window.ContentProperty, new Binding()
			{
				Source = this,
				Path = new PropertyPath(QuoteProperty)
			});

			tip.Closed += (sender, e) =>
			{
				IsChecked = false;
				tip = null;
			};

			tip.FastShow();
		}

		#endregion
	}
}
