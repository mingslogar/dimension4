using Daytimer.Dialogs;
using Daytimer.Functions;
using Daytimer.Fundamentals;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Daytimer.Controls
{
	/// <summary>
	/// Interaction logic for GoToDate.xaml
	/// </summary>
	public partial class GoToDate : Window
	{
		public GoToDate(Point screenMousePos)
		{
			InitializeComponent();
			Left = screenMousePos.X - Width / 2;
			Top = screenMousePos.Y - 5;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			if (Settings.AnimationsEnabled)
			{
				DoubleAnimation showAnim = new DoubleAnimation(1, AnimationHelpers.AnimationDuration, FillBehavior.HoldEnd);
				showAnim.EasingFunction = AnimationHelpers.EasingFunction;
				BeginAnimation(OpacityProperty, showAnim);

				DoubleAnimation slideAnim = new DoubleAnimation(Top - 30, Top, AnimationHelpers.AnimationDuration, FillBehavior.HoldEnd);
				slideAnim.EasingFunction = AnimationHelpers.EasingFunction;
				BeginAnimation(TopProperty, slideAnim);
			}
			else
			{
				Opacity = 1;
			}

			dateText.Focus();
			dateText.SelectAll();
		}

		protected override void OnDeactivated(EventArgs e)
		{
			base.OnDeactivated(e);

			if (!_showingDialog)
			{
				try { Close(); }
				catch { }
			}
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);

			if (!_closedByFunction)
			{
				e.Cancel = true;

				if (Settings.AnimationsEnabled)
				{
					DoubleAnimation hideAnim = new DoubleAnimation(0, AnimationHelpers.AnimationDuration, FillBehavior.Stop);
					hideAnim.EasingFunction = AnimationHelpers.EasingFunction;
					hideAnim.Completed += hideAnim_Completed;
					BeginAnimation(OpacityProperty, hideAnim);
				}
				else
				{
					e.Cancel = false;
				}
			}
		}

		protected override void OnPreviewKeyDown(KeyEventArgs e)
		{
			base.OnPreviewKeyDown(e);

			if (e.Key == Key.Escape)
			{
				_closedByButton = false;
				Close();
			}
		}

		private bool _closedByFunction = false;
		private bool _closedByButton = false;

		private void hideAnim_Completed(object sender, EventArgs e)
		{
			_closedByFunction = true;
			Close();
		}

		private bool _showingDialog = false;

		private void goButton_Click(object sender, RoutedEventArgs e)
		{
			_closedByButton = true;

			if (SelectedDate == null)
			{
				_closedByButton = false;
				_showingDialog = true;

				TaskDialog dialog = new TaskDialog(Application.Current.MainWindow, "Invalid Date", "The date you entered does not match a known format.", MessageType.Error);
				dialog.ShowDialog();
				dateText.SelectAll();

				_showingDialog = false;
			}
			else
				Close();
		}

		public DateTime? SelectedDate
		{
			get
			{
				if (_closedByButton)
				{
					DateTime output;

					if (DateTime.TryParse(dateText.Text, out output))
						return output;
					else
						return null;
				}
				else
					return null;
			}
			set
			{
				if (value == null)
					dateText.Text = null;
				else
					dateText.Text = value.Value.ToString("MMMM d, yyyy");
			}
		}
	}
}
