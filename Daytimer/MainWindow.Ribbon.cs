using Daytimer.Controls;
using Daytimer.Controls.Panes.People;
using Daytimer.DatabaseHelpers;
using Daytimer.Functions;
using Microsoft.Windows.Controls.Ribbon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Daytimer
{
	public partial class MainWindow
	{
		#region Calendar

		private void today_Click(object sender, RoutedEventArgs e)
		{
			CurrentCalendarView().Today();
		}

		private void next7Days_Click(object sender, RoutedEventArgs e)
		{
			CalendarMode _d = calendarDisplayMode;

			calendarDisplayMode = CalendarMode.Week;
			weekButton.IsChecked = true;

			CreateWeekView();
			UpdateRibbon();

			DateTime nextWeek = DateTime.Now.AddDays(7);
			weekView.GoTo(nextWeek);

			if (_d == CalendarMode.Month)
			{
				monthView.SaveAndClose();

				if (Settings.AnimationsEnabled)
				{
					AnimationHelpers.ZoomDisplay anim = new AnimationHelpers.ZoomDisplay(monthView, weekView);
					anim.SwitchViews(AnimationHelpers.ZoomDirection.In);
				}
				else
				{
					weekView.Visibility = Visibility.Visible;
					monthView.Visibility = Visibility.Hidden;
				}
			}
			else if (_d == CalendarMode.Day)
			{
				dayView.EndEdit();

				if (Settings.AnimationsEnabled)
				{
					AnimationHelpers.ZoomDisplay anim = new AnimationHelpers.ZoomDisplay(dayView, weekView);
					anim.SwitchViews(AnimationHelpers.ZoomDirection.Out);
				}
				else
				{
					weekView.Visibility = Visibility.Visible;
					dayView.Visibility = Visibility.Hidden;
				}
			}
		}

		private void dayButton_Checked(object sender, RoutedEventArgs e)
		{
			if (calendarDisplayMode == CalendarMode.Month)
				DayGoToDate(monthView.Selected.Date);
			else if (calendarDisplayMode == CalendarMode.Week)
				DayGoToDate(weekView.CheckedDate);
		}

		private void DayGoToDate(DateTime date)
		{
			if (calendarDisplayMode != CalendarMode.Day)
			{
				CreateDayView();

				if (calendarDisplayMode == CalendarMode.Month)
				{
					calendarDisplayMode = CalendarMode.Day;
					UpdateRibbon();

					monthView.SaveAndClose();

					dayView.Month = date.Month;
					dayView.Year = date.Year;
					dayView.Day = date.Day;
					dayView.UpdateDisplay();

					if (Settings.AnimationsEnabled)
					{
						AnimationHelpers.ZoomDisplay anim = new AnimationHelpers.ZoomDisplay(monthView, dayView);
						anim.OnAnimationCompletedEvent += monthViewUnloadAnimation_Completed;
						anim.SwitchViews(AnimationHelpers.ZoomDirection.In);
					}
					else
					{
						dayView.Visibility = Visibility.Visible;
						monthView.Visibility = Visibility.Collapsed;
					}
				}
				else if (calendarDisplayMode == CalendarMode.Week)
				{
					calendarDisplayMode = CalendarMode.Day;
					UpdateRibbon();

					weekView.EndEdit();

					dayView.Month = date.Month;
					dayView.Year = date.Year;
					dayView.Day = date.Day;
					dayView.UpdateDisplay();

					if (Settings.AnimationsEnabled)
					{
						AnimationHelpers.ZoomDisplay anim = new AnimationHelpers.ZoomDisplay(weekView, dayView);
						anim.OnAnimationCompletedEvent += weekViewUnloadAnimation_Completed;
						anim.SwitchViews(AnimationHelpers.ZoomDirection.In);
					}
					else
					{
						dayView.Visibility = Visibility.Visible;
						weekView.Visibility = Visibility.Collapsed;
					}
				}
			}
			else
				dayView.GoTo(date);
		}

		private void weekButton_Checked(object sender, RoutedEventArgs e)
		{
			if (calendarDisplayMode != CalendarMode.Week)
			{
				CreateWeekView();

				if (calendarDisplayMode == CalendarMode.Month)
				{
					calendarDisplayMode = CalendarMode.Week;
					UpdateRibbon();

					monthView.SaveAndClose();

					weekView.Month = monthView.Selected.Date.Month;
					weekView.Day = monthView.Selected.Date.Day;
					weekView.Year = monthView.Selected.Date.Year;
					weekView.UpdateDisplay(false);

					weekView.HighlightDay(monthView.Selected.Date);

					if (Settings.AnimationsEnabled)
					{
						AnimationHelpers.ZoomDisplay anim = new AnimationHelpers.ZoomDisplay(monthView, weekView);
						anim.OnAnimationCompletedEvent += monthViewUnloadAnimation_Completed;
						anim.SwitchViews(AnimationHelpers.ZoomDirection.In);
					}
					else
					{
						weekView.Visibility = Visibility.Visible;
						monthView.Visibility = Visibility.Collapsed;
					}
				}
				else if (calendarDisplayMode == CalendarMode.Day)
				{
					calendarDisplayMode = CalendarMode.Week;
					UpdateRibbon();

					dayView.EndEdit();

					weekView.Month = dayView.Month;
					weekView.Day = dayView.Day;
					weekView.Year = dayView.Year;
					weekView.UpdateDisplay(false);

					weekView.HighlightDay(new DateTime(dayView.Year, dayView.Month, dayView.Day));

					if (Settings.AnimationsEnabled)
					{
						AnimationHelpers.ZoomDisplay anim = new AnimationHelpers.ZoomDisplay(dayView, weekView);
						anim.OnAnimationCompletedEvent += dayViewUnloadAnimation_Completed;
						anim.SwitchViews(AnimationHelpers.ZoomDirection.Out);
					}
					else
					{
						weekView.Visibility = Visibility.Visible;
						dayView.Visibility = Visibility.Collapsed;
					}
				}
			}
		}

		private void monthButton_Checked(object sender, RoutedEventArgs e)
		{
			if (calendarDisplayMode != CalendarMode.Month)
			{
				CreateMonthView();

				if (calendarDisplayMode == CalendarMode.Day)
				{
					calendarDisplayMode = CalendarMode.Month;
					UpdateRibbon();

					dayView.EndEdit();

					monthView.Month = dayView.Month;
					monthView.Year = dayView.Year;

					if (!monthView.IsLoaded)
					{
						monthView.Loaded += (obj, args) =>
						{
							monthView.UpdateDisplay(false);
							monthView.HighlightDay(dayView.Day);
						};
					}
					else
					{
						monthView.UpdateDisplay(false);
						monthView.HighlightDay(dayView.Day);
					}

					if (Settings.AnimationsEnabled)
					{
						AnimationHelpers.ZoomDisplay anim = new AnimationHelpers.ZoomDisplay(dayView, monthView);
						anim.OnAnimationCompletedEvent += dayViewUnloadAnimation_Completed;
						anim.SwitchViews(AnimationHelpers.ZoomDirection.Out);
					}
					else
					{
						monthView.Visibility = Visibility.Visible;
						dayView.Visibility = Visibility.Collapsed;
					}
				}
				else if (calendarDisplayMode == CalendarMode.Week)
				{
					calendarDisplayMode = CalendarMode.Month;
					UpdateRibbon();

					weekView.EndEdit();

					DateTime weekViewChecked = weekView.CheckedDate;

					monthView.Month = weekViewChecked.Month;
					monthView.Year = weekViewChecked.Year;

					if (!monthView.IsLoaded)
					{
						monthView.Loaded += (obj, args) =>
						{
							monthView.UpdateDisplay(false);
							monthView.HighlightDay(weekViewChecked.Day);
						};
					}
					else
					{
						monthView.UpdateDisplay(false);
						monthView.HighlightDay(weekViewChecked.Day);
					}

					if (Settings.AnimationsEnabled)
					{
						AnimationHelpers.ZoomDisplay anim = new AnimationHelpers.ZoomDisplay(weekView, monthView);
						anim.OnAnimationCompletedEvent += weekViewUnloadAnimation_Completed;
						anim.SwitchViews(AnimationHelpers.ZoomDirection.Out);
					}
					else
					{
						monthView.Visibility = Visibility.Visible;
						weekView.Visibility = Visibility.Collapsed;
					}
				}
			}
		}

		private void monthViewUnloadAnimation_Completed(object sender, EventArgs e)
		{
			monthView.Clear();
		}

		private void weekViewUnloadAnimation_Completed(object sender, EventArgs e)
		{
			weekView.Clear();
		}

		private void dayViewUnloadAnimation_Completed(object sender, EventArgs e)
		{
			dayView.Clear();
		}

		private void saveAndClose_Click(object sender, RoutedEventArgs e)
		{
			if (_isActiveMonthDetail)
				monthView.SaveAndClose();
			else if (_activeDetail != null)
			{
				if (calendarDisplayMode == CalendarMode.Day)
					dayView.EndEdit();
				else
					weekView.EndEdit();
			}
		}

		private void discardChanges_Click(object sender, RoutedEventArgs e)
		{
			if (_isActiveMonthDetail)
				monthView.CancelEdit();
			else if (_activeDetail != null)
			{
				if (calendarDisplayMode == CalendarMode.Day)
					dayView.CancelEdit();
				else
					weekView.CancelEdit();
			}
		}

		private void delete_Click(object sender, RoutedEventArgs e)
		{
			if (_isActiveMonthDetail)
				monthView.DeleteActive();
			else if (_activeDetail != null)
				(_activeDetail as DayDetail).Delete();
		}

		private void appointmentPrivate_Click(object sender, RoutedEventArgs e)
		{
			bool _private = appointmentPrivate.IsChecked == true;
			CurrentCalendarView().ChangePrivate(_private);
		}

		private void appointmentHighPriority_Checked(object sender, RoutedEventArgs e)
		{
			appointmentLowPriority.IsChecked = false;
			CurrentCalendarView().ChangePriority(Priority.High);
		}

		private void appointmentHighPriority_Unchecked(object sender, RoutedEventArgs e)
		{
			if (appointmentLowPriority.IsChecked == false)
				CurrentCalendarView().ChangePriority(Priority.Normal);
		}

		private void appointmentLowPriority_Checked(object sender, RoutedEventArgs e)
		{
			appointmentHighPriority.IsChecked = false;
			CurrentCalendarView().ChangePriority(Priority.Low);
		}

		private void appointmentLowPriority_Unchecked(object sender, RoutedEventArgs e)
		{
			if (appointmentHighPriority.IsChecked == false)
				CurrentCalendarView().ChangePriority(Priority.Normal);
		}

		private void specificDate_Checked(object sender, RoutedEventArgs e)
		{
			Point point = Mouse.GetPosition(this);
			Point sdPoint = Mouse.GetPosition(specificDate);
			point.Offset(specificDate.ActualWidth - sdPoint.X - specificDate.ActualWidth / 2, specificDate.ActualHeight - sdPoint.Y);
			point = PointToScreen(point);

			GoToDate date = new GoToDate(point);
			date.SelectedDate = SelectedDate;
			date.Closed += date_Closed;
			date.Owner = this;
			date.Show();
		}

		private void date_Closed(object sender, EventArgs e)
		{
			specificDate.IsChecked = false;

			DateTime? selected = (sender as GoToDate).SelectedDate;

			if (selected.HasValue)
				CurrentCalendarView().GoTo(selected.Value);

			Activate();
		}

		private void recurrence_Click(object sender, RoutedEventArgs e)
		{
			ShowRecurrenceDialog();
		}

		private void appointmentCategory_DropDownOpened(object sender, EventArgs e)
		{
			appointmentCategoryGallery.Items.Clear();

			MenuItem clearCategory = new MenuItem();
			clearCategory.Header = "_Clear Category";
			clearCategory.Click += clearCategory_Click;
			appointmentCategoryGallery.Items.Add(clearCategory);

			bool enabled;

			if (calendarDisplayMode == CalendarMode.Month)
				enabled = monthView.Selected.ActiveDetail.Appointment.CategoryID != "";
			else
				enabled = ((DayDetail)_activeDetail).Appointment.CategoryID != "";

			if (!enabled)
				clearCategory.Loaded += separator_Loaded;

			RibbonSeparator separator = new RibbonSeparator();
			separator.Margin = new Thickness(32, 1, 0, 0);
			separator.Loaded += separator_Loaded;
			appointmentCategoryGallery.Items.Add(separator);

			foreach (Category each in AppointmentDatabase.GetCategories())
			{
				if (!each.ReadOnly)
				{
					MenuItem m = new MenuItem();
					m.Click += Category_Click;
					m.Header = each.Name;
					m.Tag = each.ID;

					Border b = new Border();
					b.BorderThickness = new Thickness(1);
					b.Width = b.Height = 14;

					Color c = each.Color;
					b.Background = new SolidColorBrush(c);
					b.BorderBrush = new SolidColorBrush(Color.FromRgb((byte)(c.R > 75 ? c.R - 75 : 0), (byte)(c.G > 70 ? c.G - 70 : 0), (byte)(c.B > 80 ? c.B - 80 : 0)));

					m.Icon = b;

					appointmentCategoryGallery.Items.Add(m);
				}
			}

			RibbonSeparator separator2 = new RibbonSeparator();
			separator2.Margin = new Thickness(32, 1, 0, 0);
			separator2.Loaded += separator_Loaded;
			appointmentCategoryGallery.Items.Add(separator2);

			MenuItem modifyCategories = new MenuItem();
			modifyCategories.Header = "_Edit Categories";
			Image icon = new Image();
			icon.Stretch = Stretch.None;
			icon.Source = new BitmapImage(new Uri("pack://application:,,,/Daytimer.Images;component/Images/categorize_sml.png", UriKind.Absolute));
			modifyCategories.Icon = icon;
			modifyCategories.Click += modifyCategories_Click;
			appointmentCategoryGallery.Items.Add(modifyCategories);
		}

		private void clearCategory_Click(object sender, RoutedEventArgs e)
		{
			CurrentCalendarView().ChangeCategory("");
		}

		private void separator_Loaded(object sender, RoutedEventArgs e)
		{
			(appointmentCategoryGallery.ItemContainerGenerator.ContainerFromItem(sender) as UIElement).IsEnabled = false;
		}

		private void Category_Click(object sender, RoutedEventArgs e)
		{
			string categoryID = (sender as MenuItem).Tag as string;
			CurrentCalendarView().ChangeCategory(categoryID);
		}

		private void modifyCategories_Click(object sender, RoutedEventArgs e)
		{
			CategoryEditor editor = new CategoryEditor(() => { return AppointmentDatabase.GetCategories(); }, AppointmentDatabase.AddCategory, AppointmentDatabase.UpdateCategory, AppointmentDatabase.DeleteCategory);
			editor.Owner = this;
			editor.ShowDialog();

			// We need to refresh all items with a category.
			CurrentCalendarView().RefreshCategories();
		}

		private void showAs_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			ShowAs show = (ShowAs)(showAs.Items[0] as RibbonGalleryCategory).Items.IndexOf(showAs.SelectedItem);

			if ((int)show == -1)
				return;

			CurrentCalendarView().ChangeShowAs(show);
		}

		private void apptReminder_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			TimeSpan? reminder = MinutesDropDown.Parse(apptReminder.Text);
			CurrentCalendarView().ChangeReminder(reminder);
		}

		#endregion

		#region Tasks

		private void taskPrivate_Click(object sender, RoutedEventArgs e)
		{
			tasksView.ChangeActivePrivate(taskPrivate.IsChecked == true);
		}

		private void taskHighPriority_Checked(object sender, RoutedEventArgs e)
		{
			taskLowPriority.IsChecked = false;
			tasksView.ChangeActivePriority(Priority.High);
		}

		private void taskHighPriority_Unchecked(object sender, RoutedEventArgs e)
		{
			if (taskLowPriority.IsChecked == false)
				tasksView.ChangeActivePriority(Priority.Normal);
		}

		private void taskLowPriority_Checked(object sender, RoutedEventArgs e)
		{
			taskHighPriority.IsChecked = false;
			tasksView.ChangeActivePriority(Priority.Low);
		}

		private void taskLowPriority_Unchecked(object sender, RoutedEventArgs e)
		{
			if (taskHighPriority.IsChecked == false)
				tasksView.ChangeActivePriority(Priority.Normal);
		}

		private void showCompleted_Checked(object sender, RoutedEventArgs e)
		{
			if (tasksView != null)
				tasksView.ShowCompleted = true;
		}

		private void showCompleted_Unchecked(object sender, RoutedEventArgs e)
		{
			if (tasksView != null)
				tasksView.ShowCompleted = false;
		}

		private void saveAndCloseTask_Click(object sender, RoutedEventArgs e)
		{
			tasksView.SaveAndClose();
		}

		private void discardChangesTask_Click(object sender, RoutedEventArgs e)
		{
			tasksView.CancelEdit();
		}

		private void deleteTask_Click(object sender, RoutedEventArgs e)
		{
			tasksView.Delete();
		}

		private void markComplete_Click(object sender, RoutedEventArgs e)
		{
			tasksView.MarkComplete();
		}

		private void tasksView_OnShowCompletedChangedEvent(object sender, EventArgs e)
		{
			showCompleted.IsChecked = tasksView.ShowCompleted;
		}

		private void taskCategory_DropDownOpened(object sender, EventArgs e)
		{
			taskCategoryGallery.Items.Clear();

			MenuItem clearCategory = new MenuItem();
			clearCategory.Header = "_Clear Category";
			clearCategory.Click += taskClearCategory_Click;
			taskCategoryGallery.Items.Add(clearCategory);

			bool enabled = tasksView.ActiveTaskCategoryID != "";

			if (!enabled)
				clearCategory.Loaded += taskSeparator_Loaded;

			RibbonSeparator separator = new RibbonSeparator();
			separator.Margin = new Thickness(32, 1, 0, 0);
			separator.Loaded += taskSeparator_Loaded;
			taskCategoryGallery.Items.Add(separator);

			Category[] categories = TaskDatabase.GetCategories();

			foreach (Category each in categories)
			{
				if (!each.ReadOnly)
				{
					MenuItem m = new MenuItem();
					m.Click += TaskCategory_Click;
					m.Header = each.Name;
					m.Tag = each.ID;

					Border b = new Border();
					b.BorderThickness = new Thickness(1);
					b.Width = b.Height = 14;

					Color c = each.Color;
					b.Background = new SolidColorBrush(c);
					b.BorderBrush = new SolidColorBrush(Color.FromRgb((byte)(c.R > 75 ? c.R - 75 : 0), (byte)(c.G > 70 ? c.G - 70 : 0), (byte)(c.B > 80 ? c.B - 80 : 0)));

					m.Icon = b;

					taskCategoryGallery.Items.Add(m);
				}
			}

			RibbonSeparator separator2 = new RibbonSeparator();
			separator2.Margin = new Thickness(32, 1, 0, 0);
			separator2.Loaded += taskSeparator_Loaded;
			taskCategoryGallery.Items.Add(separator2);

			MenuItem modifyCategories = new MenuItem();
			modifyCategories.Header = "_Edit Categories";
			Image icon = new Image();
			icon.Stretch = Stretch.None;
			icon.Source = new BitmapImage(new Uri("pack://application:,,,/Daytimer.Images;component/Images/categorize_sml.png", UriKind.Absolute));
			modifyCategories.Icon = icon;
			modifyCategories.Click += taskModifyCategories_Click;
			taskCategoryGallery.Items.Add(modifyCategories);
		}

		private void taskClearCategory_Click(object sender, RoutedEventArgs e)
		{
			tasksView.ChangeActiveCategory("");
		}

		private void taskSeparator_Loaded(object sender, RoutedEventArgs e)
		{
			(taskCategoryGallery.ItemContainerGenerator.ContainerFromItem(sender) as UIElement).IsEnabled = false;
		}

		private void TaskCategory_Click(object sender, RoutedEventArgs e)
		{
			string categoryID = (sender as MenuItem).Tag as string;
			tasksView.ChangeActiveCategory(categoryID);
		}

		private void taskModifyCategories_Click(object sender, RoutedEventArgs e)
		{
			CategoryEditor editor = new CategoryEditor(() => { return TaskDatabase.GetCategories(); }, TaskDatabase.AddCategory, TaskDatabase.UpdateCategory, TaskDatabase.DeleteCategory);
			editor.Owner = this;
			editor.ShowDialog();

			tasksView.RefreshCategories();
		}

		#endregion

		#region General

		private void searchPaneButton_Checked(object sender, RoutedEventArgs e)
		{
			ShowSearchWindow(true);
			_wasSearchOpen = false;
			SwitchToNormalMode();
		}

		private void searchPaneButton_Unchecked(object sender, RoutedEventArgs e)
		{
			ShowSearchWindow(false);
		}

		#endregion

		#region People

		private void contactPicture_Click(object sender, RoutedEventArgs e)
		{
			peopleView.ChangeContactPicture();
		}

		private void contactPrivate_Click(object sender, RoutedEventArgs e)
		{
			peopleView.ChangeActivePrivate(contactPrivate.IsChecked == true);
		}

		private void saveAndCloseContact_Click(object sender, RoutedEventArgs e)
		{
			peopleView.SaveAndClose();
		}

		private void discardChangesContact_Click(object sender, RoutedEventArgs e)
		{
			peopleView.HideEdit();
		}

		private void deleteContact_Click(object sender, RoutedEventArgs e)
		{
			peopleView.Delete();
		}

		private void showAllContacts_Checked(object sender, RoutedEventArgs e)
		{
			if (IsLoaded && peopleView.CurrentView == PeopleView.ViewMode.Favorites)
				peopleView.ToggleShowFavorites();
		}

		private void showFavorites_Checked(object sender, RoutedEventArgs e)
		{
			if (IsLoaded && peopleView.CurrentView == PeopleView.ViewMode.All)
				peopleView.ToggleShowFavorites();
		}

		#endregion

		#region Weather

		private void goToHome_Click(object sender, RoutedEventArgs e)
		{
			weatherView.ChangeLocation(Settings.WeatherHome);
		}

		private void changeLocation_Click(object sender, RoutedEventArgs e)
		{
			weatherView.ChangeLocation();
		}

		private void refreshWeather_Click(object sender, RoutedEventArgs e)
		{
			weatherView.Refresh();
		}

		#endregion

		#region Text Formatting

		private void formatPainter_Click(object sender, RoutedEventArgs e)
		{
			throw new NotImplementedException("");
		}

		private async void fontFamilyBox_DropDownOpened(object sender, EventArgs e)
		{
			if (fontFamilyBox.Tag == null)
			{
				fontFamilyBox.Tag = "1";
				await fontFamilyBox.PopulateFonts();
			}
		}

		private void fontFamilyBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count > 0 && fontFamilyBox.SelectedItem != null)
				TextEditing.SetRTBValue(FontFamilyProperty, fontFamilyBox.SelectedItem.ToString(), true);
		}

		private void fontFamilyBox_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				e.Handled = true;
				UpdateFontFamily(true);
			}
		}

		private void fontFamilyBox_LostFocus(object sender, RoutedEventArgs e)
		{
			UpdateFontFamily(false);
		}

		private void UpdateFontFamily(bool focusRTB)
		{
			string text = fontFamilyBox.Text;

			TextEditing.SetRTBValue(FontFamilyProperty, text, focusRTB);

			bool found = false;

			for (int i = 0; i < fontFamilyBox.Items.Count; i++)
			{
				if (fontFamilyBox.Items[i].ToString() == text)
				{
					fontFamilyBox.SelectedIndex = i;
					found = true;
					break;
				}
			}

			if (!found)
			{
				fontFamilyBox.SelectedIndex = -1;
				fontFamilyBox.Text = text;
			}
		}

		private void fontSizeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count > 0)
			{
				string newSize = (fontSizeBox.SelectedItem as ComboBoxItem).Content as string;

				double value;

				if (double.TryParse(newSize, out value))
					TextEditing.SetRTBValue(FontSizeProperty, Converter.PixelToPoint(value), true);
			}
		}

		private void fontSizeBox_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				e.Handled = true;
				UpdateFontSize(true);
			}
		}

		private void fontSizeBox_LostFocus(object sender, RoutedEventArgs e)
		{
			UpdateFontSize(false);
		}

		//private void increaseSize_Click(object sender, RoutedEventArgs e)
		//{
		//	object fontSize = TextEditing.GetRTBValue(TextElement.FontSizeProperty);

		//	if (fontSize != DependencyProperty.UnsetValue)
		//	{
		//		fontSizeBox.IsTextSearchEnabled = true;
		//		fontSizeBox.Text = Converter.PointToPixel((double)(fontSize)).ToString();
		//		fontSizeBox.IsTextSearchEnabled = false;
		//	}
		//	else
		//	{
		//		fontSizeBox.Text = "";
		//		fontSizeBox.SelectedIndex = -1;
		//	}
		//}

		//private void decreaseSize_Click(object sender, RoutedEventArgs e)
		//{
		//	object fontSize = TextEditing.GetRTBValue(TextElement.FontSizeProperty);

		//	if (fontSize != DependencyProperty.UnsetValue)
		//	{
		//		fontSizeBox.IsTextSearchEnabled = true;
		//		fontSizeBox.Text = Converter.PointToPixel((double)(fontSize)).ToString();
		//		fontSizeBox.IsTextSearchEnabled = false;
		//	}
		//	else
		//	{
		//		fontSizeBox.Text = "";
		//		fontSizeBox.SelectedIndex = -1;
		//	}
		//}

		private void UpdateFontSize(bool focusRTB)
		{
			string text = fontSizeBox.Text;

			double value;

			if (double.TryParse(text, out value))
			{
				TextEditing.SetRTBValue(FontSizeProperty, Converter.PixelToPoint(value), focusRTB);

				bool found = false;

				foreach (ComboBoxItem each in fontSizeBox.Items)
				{
					if (each.Content.ToString() == text)
					{
						each.IsSelected = true;
						found = true;
						break;
					}
				}

				if (!found)
				{
					fontSizeBox.SelectedIndex = -1;
					fontSizeBox.Text = text;
				}
			}
		}

		private void LastAddedRTB_SelectionChanged(object sender, RoutedEventArgs e)
		{
			UpdateFormattingUI();
		}

		private void UpdateFormattingUI()
		{
			object font = TextEditing.GetRTBValue(TextElement.FontFamilyProperty);

			if (font != DependencyProperty.UnsetValue)
			{
				fontFamilyBox.IsTextSearchEnabled = true;
				fontFamilyBox.Text = font.ToString();
				fontFamilyBox.IsTextSearchEnabled = false;
			}
			else
			{
				fontFamilyBox.Text = "";
				fontFamilyBox.SelectedIndex = -1;
			}

			object fontSize = TextEditing.GetRTBValue(TextElement.FontSizeProperty);

			if (fontSize != DependencyProperty.UnsetValue)
			{
				fontSizeBox.IsTextSearchEnabled = true;
				fontSizeBox.Text = Converter.PointToPixel((double)(fontSize)).ToString();
				fontSizeBox.IsTextSearchEnabled = false;
			}
			else
			{
				fontSizeBox.Text = "";
				fontSizeBox.SelectedIndex = -1;
			}

			object fontWeight = TextEditing.GetRTBValue(FontWeightProperty);
			bold.IsChecked = fontWeight != DependencyProperty.UnsetValue ? (FontWeight)fontWeight == FontWeights.Bold : false;

			object fontStyle = TextEditing.GetRTBValue(FontStyleProperty);
			italic.IsChecked = fontStyle != DependencyProperty.UnsetValue ? (FontStyle)fontStyle == FontStyles.Italic : false;

			object textdecoration = TextEditing.GetRTBValue(TextBlock.TextDecorationsProperty);
			underline.IsChecked = TextEditing.ContainsTextDecoration(textdecoration, TextDecorations.Underline);
			strikethrough.IsChecked = TextEditing.ContainsTextDecoration(textdecoration, TextDecorations.Strikethrough);

			object variants = TextEditing.GetRTBValue(Typography.VariantsProperty);

			if (variants != DependencyProperty.UnsetValue)
			{
				switch ((FontVariants)variants)
				{
					case FontVariants.Subscript:
						subscript.IsChecked = true;
						break;

					case FontVariants.Superscript:
						superscript.IsChecked = true;
						break;

					default:
						subscript.IsChecked = false;
						superscript.IsChecked = false;
						break;
				}
			}
			else
			{
				subscript.IsChecked = false;
				superscript.IsChecked = false;
			}

			object color = TextEditing.GetRTBValue(ForegroundProperty);
			if (color != DependencyProperty.UnsetValue)
				fontColor.ActiveColor = (Brush)color;

			object highlight = TextEditing.GetRTBValue(TextElement.BackgroundProperty);
			if (highlight != DependencyProperty.UnsetValue)
				highlightColor.ActiveColor = (Brush)highlight;

			UpdateSelectionListType();

			object alignment = TextEditing.GetRTBValue(TextBlock.TextAlignmentProperty);

			if (alignment != DependencyProperty.UnsetValue)
			{
				switch ((TextAlignment)alignment)
				{
					case TextAlignment.Center:
						alignCenter.IsChecked = true;
						break;

					case TextAlignment.Justify:
						alignJustify.IsChecked = true;
						break;

					case TextAlignment.Left:
						alignLeft.IsChecked = true;
						break;

					case TextAlignment.Right:
						alignRight.IsChecked = true;
						break;
				}
			}
			else
			{
				alignLeft.IsChecked = false;
				alignCenter.IsChecked = false;
				alignRight.IsChecked = false;
				alignJustify.IsChecked = false;
			}

			object margin = TextEditing.GetRTBValue(Paragraph.MarginProperty);
			double pSpacing = margin != DependencyProperty.UnsetValue ? ((Thickness)margin).Bottom : -1;

			if (fontSize == DependencyProperty.UnsetValue)
				fontSize = SpellChecking.FocusedRTB.FontSize;

			bool foundIndex = false;

			foreach (ComboBoxItem each in paragraphSpacingBox.Items)
			{
				if ((double.Parse(each.Content.ToString()) - 1) * (double)fontSize == pSpacing)
				{
					each.IsSelected = true;
					foundIndex = true;
					break;
				}
			}

			if (!foundIndex)
				paragraphSpacingBox.SelectedIndex = -1;

			object border = TextEditing.GetRTBValue(Block.BorderThicknessProperty);

			if (border != DependencyProperty.UnsetValue)
				borderType.ActiveBorder = (Thickness)border;

			object background = TextEditing.GetSelectionParagraphPropertyValue(SpellChecking.FocusedRTB.Selection, TextElement.BackgroundProperty);

			if (background != DependencyProperty.UnsetValue)
				backgroundColor.ActiveColor = (Brush)background;
		}

		private void clear_Click(object sender, RoutedEventArgs e)
		{
			SpellChecking.FocusedRTB.BeginChange();

			SpellChecking.FocusedRTB.Selection.ClearAllProperties();
			SpellChecking.FocusedRTB.Selection.ApplyPropertyValue(Paragraph.MarginProperty, new Thickness(0));
			TextEditing.ChangeRTBParagraphBackground(SpellChecking.FocusedRTB, null);

			SpellChecking.FocusedRTB.EndChange();

			UpdateFormattingUI();
		}

		private void subscript_Checked(object sender, RoutedEventArgs e)
		{
			superscript.IsChecked = false;
		}

		private void superscript_Checked(object sender, RoutedEventArgs e)
		{
			subscript.IsChecked = false;
		}

		private void caseBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (caseBox.SelectedIndex == 0 || caseBox.SelectedIndex > 4)
				throw new NotImplementedException("");

			if (caseBox.SelectedIndex != -1)
			{
				SpellChecking.FocusedRTB.BeginChange();

				MemoryStream stream = new MemoryStream();
				SpellChecking.FocusedRTB.Selection.Save(stream, DataFormats.XamlPackage);

				FlowDocument selection = new FlowDocument();
				selection.Resources = SpellChecking.FocusedRTB.Resources;

				TextRange select = new TextRange(selection.ContentStart, selection.ContentEnd);
				select.Load(stream, DataFormats.XamlPackage);

				List<TextElement> elements = new List<TextElement>();

				foreach (TextElement element in selection.Blocks)
					elements.Add(element);

				// Lowercase
				switch (caseBox.SelectedIndex)
				{
					case 1:
						foreach (TextElement element in elements)
							ConvertTextElement(element, lowercase);

						//
						// The following code works, but also converts all font names to lowercase...
						// resulting in "calibri" and "segoe ui" showing up in editing UI.
						//
						//var textRange = new TextRange(SpellChecking.FocusedRTB.Selection.Start, SpellChecking.FocusedRTB.Selection.End);
						//string rtf;
						//using (var memoryStream = new MemoryStream())
						//{
						//	textRange.Save(memoryStream, DataFormats.Rtf);
						//	rtf = UTF32Encoding.Default.GetString(memoryStream.ToArray());
						//}

						//rtf = rtf.ToLower();

						//MemoryStream stream = new MemoryStream(UTF32Encoding.Default.GetBytes(rtf));
						//SpellChecking.FocusedRTB.Selection.Load(stream, DataFormats.Rtf);
						break;

					case 2:
						foreach (TextElement element in elements)
							ConvertTextElement(element, uppercase);
						break;

					case 3:
						foreach (TextElement element in elements)
							ConvertTextElement(element, capitalize);
						break;

					case 4:
						foreach (TextElement element in elements)
							ConvertTextElement(element, toggleCase);
						break;

					default:
						break;
				}

				select.Save(stream, DataFormats.XamlPackage);
				SpellChecking.FocusedRTB.Selection.Load(stream, DataFormats.XamlPackage);

				SpellChecking.FocusedRTB.EndChange();

				caseBox.SelectedIndex = -1;
			}
		}

		private void ConvertTextElement(TextElement element, Func<string, string> conversion)
		{
			Run run = element as Run;

			if (run == null)
			{
				List<object> children = new List<object>();

				foreach (object child in LogicalTreeHelper.GetChildren(element))
					children.Add(child);

				foreach (object child in children)
				{
					TextElement elem = child as TextElement;

					if (elem != null)
						ConvertTextElement(elem, conversion);
				}
			}
			else
				run.Text = conversion(run.Text);
		}

		private string lowercase(string s)
		{
			return s.ToLower();
		}

		private string uppercase(string s)
		{
			return s.ToUpper();
		}

		private string capitalize(string s)
		{
			string[] split = s.Split(' ');
			int length = split.Length;

			for (int i = 0; i < length; i++)
			{
				string single = split[i];

				if (single.Length > 0)
					split[i] = char.ToUpper(single[0]) + single.Substring(1).ToLower();
			}

			return string.Join(" ", split);
		}

		private string toggleCase(string s)
		{
			int length = s.Length;
			char[] chars = s.ToCharArray();

			for (int i = 0; i < length; i++)
			{
				char c = chars[i];
				chars[i] = char.IsLower(c) ? char.ToUpper(c) : char.ToLower(c);
			}

			return string.Join("", chars);
		}

		private void fontColor_OnSelectedChangedEvent(object sender, EventArgs e)
		{
			TextEditing.SetRTBValue(ForegroundProperty, fontColor.SelectedColor, true);
		}

		private void highlightColor_OnSelectedChangedEvent(object sender, EventArgs e)
		{
			TextEditing.SetRTBValue(TextElement.BackgroundProperty, highlightColor.SelectedColor, true);
		}

		private void UpdateSelectionListType()
		{
			Paragraph startParagraph = SpellChecking.FocusedRTB.Selection.Start.Paragraph;
			Paragraph endParagraph = SpellChecking.FocusedRTB.Selection.End.Paragraph;

			if (startParagraph != null && endParagraph != null && (startParagraph.Parent is ListItem) && (endParagraph.Parent is ListItem) && object.ReferenceEquals(((ListItem)startParagraph.Parent).List, ((ListItem)endParagraph.Parent).List))
			{
				TextMarkerStyle markerStyle = ((ListItem)startParagraph.Parent).List.MarkerStyle;
				if (markerStyle == TextMarkerStyle.Disc) //bullets
				{
					toggleBullets.IsChecked = true;
				}
				else if (markerStyle == TextMarkerStyle.Decimal) //numbers
				{
					toggleNumbers.IsChecked = true;
				}
			}
			else
			{
				toggleBullets.IsChecked = false;
				toggleNumbers.IsChecked = false;
			}
		}

		private void toggleBullets_Checked(object sender, RoutedEventArgs e)
		{
			toggleNumbers.IsChecked = false;
		}

		private void toggleNumbers_Checked(object sender, RoutedEventArgs e)
		{
			toggleBullets.IsChecked = false;
		}

		private void paragraphSpacingBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count > 0)
			{
				object fontSize = TextEditing.GetRTBValue(FontSizeProperty);

				if (fontSize == DependencyProperty.UnsetValue)
					fontSize = SpellChecking.FocusedRTB.FontSize;

				TextEditing.SetRTBValue(Paragraph.MarginProperty, new Thickness(0, 0, 0,
					(double.Parse((e.AddedItems[0] as ComboBoxItem).Content.ToString()) - 1)
					* (double)fontSize), true);
			}
		}

		private void backgroundColor_OnSelectedChangedEvent(object sender, EventArgs e)
		{
			SpellChecking.FocusedRTB.BeginChange();
			TextEditing.ChangeRTBParagraphBackground(SpellChecking.FocusedRTB, backgroundColor.SelectedColor);
			SpellChecking.FocusedRTB.EndChange();
		}

		private void borderType_OnSelectedChangedEvent(object sender, EventArgs e)
		{
			TextEditing.SetRTBValue(Block.BorderThicknessProperty, borderType.SelectedBorder, true);
		}

		private void insertSymbol_Insert(object sender, RoutedEventArgs e)
		{
			if (SpellChecking.FocusedRTB != null)
			{
				(new TextRange(SpellChecking.FocusedRTB.Selection.Start, SpellChecking.FocusedRTB.Selection.End)).Text = insertSymbol.Selected.ToString();
				SpellChecking.FocusedRTB.Focus();
			}
		}

		private void insertLine_Click(object sender, RoutedEventArgs e)
		{
			Image ruler = new Image { Source = new BitmapImage(new Uri("pack://application:,,,/Daytimer.Images;component/Images/1pixelA1A1A1.png")), Stretch = Stretch.Fill, HorizontalAlignment = HorizontalAlignment.Stretch, Height = 1 };
			new InlineUIContainer(ruler, SpellChecking.FocusedRTB.CaretPosition.GetInsertionPosition(LogicalDirection.Forward));
		}

		private void selectAll_Click(object sender, RoutedEventArgs e)
		{
			if (SpellChecking.FocusedRTB != null)
			{
				SpellChecking.FocusedRTB.SelectAll();
				SpellChecking.FocusedRTB.Focus();
			}
		}

		#endregion

		#region Sync

		private void workOfflineButton_Click(object sender, RoutedEventArgs e)
		{
			Settings.WorkOffline = (sender as ToggleButton).IsChecked == true;
			statusStrip.ShowConnectivity();
		}

		#endregion

		#region Update Ribbon

		private void UpdateRibbon()
		{
			switch (calendarDisplayMode)
			{
				case CalendarMode.Day:
				case CalendarMode.Week:
					statusStrip.EnableZoom(true);
					break;

				case CalendarMode.Month:
				default:
					statusStrip.EnableZoom(false);
					break;
			}
		}

		private void UpdateMarkCompleteButton()
		{
			if (tasksView.ActiveTaskStatus == UserTask.StatusPhase.Completed)
			{
				markComplete.Label = "Flag Incomplete";
				markComplete.ToolTipTitle = "Flag Incomplete";
				markComplete.ToolTipDescription = "Flag this task as incomplete.";
				markComplete.LargeImageSource = new BitmapImage(new Uri("pack://application:,,,/Daytimer.Images;component/Images/redflag_lg.png", UriKind.Absolute));
			}
			else
			{
				markComplete.Label = "Mark Complete";
				markComplete.ToolTipTitle = "Mark Complete";
				markComplete.ToolTipDescription = "Mark this task as complete when you are done with it.";
				markComplete.LargeImageSource = new BitmapImage(new Uri("pack://application:,,,/Daytimer.Images;component/Images/blackcheck.png", UriKind.Absolute));
			}
		}

		private void ShowAppointmentSpecificControls(RibbonTab focused, bool enableExport)
		{
			if (textTools.Visibility != Visibility.Visible
				|| calendarTools.Visibility != Visibility.Visible)
			{
				textTools.Visibility = Visibility.Visible;
				calendarTools.Visibility = Visibility.Visible;
				focused.IsSelected = true;
			}
		}

		private void HideAppointmentSpecificControls()
		{
			textTools.Visibility = Visibility.Collapsed;
			calendarTools.Visibility = Visibility.Collapsed;
			recurrence.IsChecked = false;
		}

		#endregion
	}
}
