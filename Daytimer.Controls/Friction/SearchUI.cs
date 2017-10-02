using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace Daytimer.Controls.Friction
{
	[ComVisible(false)]
	[TemplatePart(Name = SearchUI.SearchBoxName, Type = typeof(TextBox))]
	[TemplatePart(Name = SearchUI.ReplaceBoxName, Type = typeof(TextBox))]
	public class SearchUI : Control
	{
		#region Constructors

		static SearchUI()
		{
			Type ownerType = typeof(SearchUI);
			DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));

			InputGestureCollection findGestures = new InputGestureCollection();
			findGestures.Add(new KeyGesture(Key.F3));

			InputGestureCollection replaceNextGestures = new InputGestureCollection();
			replaceNextGestures.Add(new KeyGesture(Key.R, ModifierKeys.Alt));

			InputGestureCollection replaceAllGestures = new InputGestureCollection();
			replaceAllGestures.Add(new KeyGesture(Key.A, ModifierKeys.Alt));

			InputGestureCollection closeGestures = new InputGestureCollection();
			closeGestures.Add(new KeyGesture(Key.Escape));

			FindCommand = new RoutedCommand("FindCommand", ownerType, findGestures);
			ReplaceNextCommand = new RoutedCommand("ReplaceNextCommand", ownerType, replaceNextGestures);
			ReplaceAllCommand = new RoutedCommand("ReplaceAllCommand", ownerType, replaceAllGestures);
			ToggleModeCommand = new RoutedCommand("ToggleModeCommand", ownerType);
			CloseCommand = new RoutedCommand("CloseCommand", ownerType, closeGestures);

			CommandBinding find = new CommandBinding(FindCommand, FindExecuted);
			CommandBinding replaceNext = new CommandBinding(ReplaceNextCommand, ReplaceNextExecuted);
			CommandBinding replaceAll = new CommandBinding(ReplaceAllCommand, ReplaceAllExecuted);
			CommandBinding toggle = new CommandBinding(ToggleModeCommand, ToggleModeExecuted);
			CommandBinding close = new CommandBinding(CloseCommand, CloseExecuted);

			CommandManager.RegisterClassCommandBinding(ownerType, find);
			CommandManager.RegisterClassCommandBinding(ownerType, replaceNext);
			CommandManager.RegisterClassCommandBinding(ownerType, replaceAll);
			CommandManager.RegisterClassCommandBinding(ownerType, toggle);
			CommandManager.RegisterClassCommandBinding(ownerType, close);
		}

		public SearchUI()
		{

		}

		#endregion

		#region Fields

		private const string SearchBoxName = "PART_SearchBox";
		private const string ReplaceBoxName = "PART_ReplaceBox";

		private TextBox PART_SearchBox;
		private TextBox PART_ReplaceBox;

		#endregion

		#region Dependency Properties

		public static readonly DependencyProperty SearchModeProperty = DependencyProperty.Register(
			"SearchMode", typeof(SearchMode), typeof(SearchUI), new PropertyMetadata(SearchMode.Find));

		/// <summary>
		/// Gets or sets the search mode.
		/// </summary>
		public SearchMode SearchMode
		{
			get { return (SearchMode)GetValue(SearchModeProperty); }
			set { SetValue(SearchModeProperty, value); }
		}

		public static readonly DependencyProperty SearchTextProperty = DependencyProperty.Register(
			"SearchText", typeof(string), typeof(SearchUI));

		/// <summary>
		/// Gets or sets the string to search for.
		/// </summary>
		public string SearchText
		{
			get { return (string)GetValue(SearchTextProperty); }
			set { SetValue(SearchTextProperty, value); }
		}

		public static readonly DependencyProperty ReplaceTextProperty = DependencyProperty.Register(
			"ReplaceText", typeof(string), typeof(SearchUI));

		/// <summary>
		/// Gets or sets the replacement string.
		/// </summary>
		public string ReplaceText
		{
			get { return (string)GetValue(ReplaceTextProperty); }
			set { SetValue(ReplaceTextProperty, value); }
		}

		public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register(
			"IsOpen", typeof(bool), typeof(SearchUI), new PropertyMetadata(false));

		/// <summary>
		/// Gets or sets if the UI is visible.
		/// </summary>
		public bool IsOpen
		{
			get { return (bool)GetValue(IsOpenProperty); }
			set
			{
				SetValue(IsOpenProperty, value);

				if (value)
				{
					if (PART_SearchBox == null)
						return;

					Dispatcher.CurrentDispatcher.BeginInvoke(() =>
					{
						PART_SearchBox.SelectAll();
						Keyboard.Focus(PART_SearchBox);
					});
				}
			}
		}

		public static readonly DependencyProperty MatchCaseProperty = DependencyProperty.Register(
			"MatchCase", typeof(bool), typeof(SearchUI), new PropertyMetadata(false));

		/// <summary>
		/// Gets or sets if a query should be case-sensitive.
		/// </summary>
		public bool MatchCase
		{
			get { return (bool)GetValue(MatchCaseProperty); }
			set { SetValue(MatchCaseProperty, value); }
		}

		public static readonly DependencyProperty WholeWordProperty = DependencyProperty.Register(
			"WholeWord", typeof(bool), typeof(SearchUI), new PropertyMetadata(false));

		/// <summary>
		/// Gets or sets if a query should only search for whole words.
		/// </summary>
		public bool WholeWord
		{
			get { return (bool)GetValue(WholeWordProperty); }
			set { SetValue(WholeWordProperty, value); }
		}

		public static readonly DependencyProperty RegexProperty = DependencyProperty.Register(
			"Regex", typeof(bool), typeof(SearchUI), new PropertyMetadata(false));

		/// <summary>
		/// Gets or sets if a query should use regular expressions.
		/// </summary>
		public bool Regex
		{
			get { return (bool)(GetValue(RegexProperty)); }
			set { SetValue(RegexProperty, value); }
		}

		#endregion

		#region Commands

		public static RoutedCommand FindCommand;
		public static RoutedCommand ReplaceNextCommand;
		public static RoutedCommand ReplaceAllCommand;
		public static RoutedCommand ToggleModeCommand;
		public static RoutedCommand CloseCommand;

		#endregion

		#region Private Methods

		private SearchOptions GetSelectedOptions()
		{
			SearchOptions options = new SearchOptions();

			if (MatchCase)
				options |= SearchOptions.MatchCase;

			if (WholeWord)
				options |= SearchOptions.WholeWord;

			if (Regex)
				options |= SearchOptions.Regex;

			return options;
		}

		private static void FindExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			SearchUI searchUI = (SearchUI)sender;
			searchUI.RaiseSearchExecutedEvent(searchUI.SearchText, searchUI.GetSelectedOptions());
		}

		private static void ReplaceNextExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			SearchUI searchUI = (SearchUI)sender;
			searchUI.RaiseReplaceExecutedEvent(searchUI.SearchText, searchUI.ReplaceText, searchUI.GetSelectedOptions(), ReplaceMode.Next);
		}

		private static void ReplaceAllExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			SearchUI searchUI = (SearchUI)sender;
			searchUI.RaiseReplaceExecutedEvent(searchUI.SearchText, searchUI.ReplaceText, searchUI.GetSelectedOptions(), ReplaceMode.All);
		}

		private static void ToggleModeExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			SearchUI search = (SearchUI)sender;
			search.SearchMode = search.SearchMode == SearchMode.Find ? SearchMode.Replace : SearchMode.Find;
		}

		private static void CloseExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			((SearchUI)sender).IsOpen = false;
		}

		private void PART_SearchBox_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				e.Handled = true;

				// Since we are watching a preview event, the binding will not
				// have been updated yet.
				SearchText = (sender as TextBox).Text;

				FindCommand.Execute(null, this);
			}
		}

		private void PART_ReplaceBox_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				e.Handled = true;

				// Since we are watching a preview event, the binding will not
				// have been updated yet.
				ReplaceText = (sender as TextBox).Text;

				ReplaceNextCommand.Execute(null, this);
			}
		}

		#endregion

		#region Protected Methods

		protected override void OnPreviewMouseRightButtonUp(MouseButtonEventArgs e)
		{
			base.OnPreviewMouseRightButtonUp(e);

			// Prevent the context menu for the RTB owner from showing.
			if (!(Keyboard.FocusedElement is TextBox))
				e.Handled = true;
		}

		protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			base.OnPreviewMouseLeftButtonDown(e);

			//
			// Prevent the context menu for the RTB owner from showing.
			//
			if (!(((FrameworkElement)Mouse.DirectlyOver).TemplatedParent is ButtonBase)

				// Using strings to compare this is bad programming, but I'm not sure how else to do this...
				&& Mouse.DirectlyOver.GetType().FullName != "System.Windows.Controls.TextBoxView")
				e.Handled = true;
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			PART_SearchBox = (TextBox)Template.FindName(SearchBoxName, this);
			PART_ReplaceBox = (TextBox)Template.FindName(ReplaceBoxName, this);

			PART_SearchBox.PreviewKeyDown += PART_SearchBox_PreviewKeyDown;
			PART_ReplaceBox.PreviewKeyDown += PART_ReplaceBox_PreviewKeyDown;
		}

		#endregion

		#region Routed Events

		public static readonly RoutedEvent SearchExecutedEvent = EventManager.RegisterRoutedEvent(
			"SearchExecuted", RoutingStrategy.Bubble, typeof(SearchExecutedEventHandler), typeof(SearchUI));

		public event SearchExecutedEventHandler SearchExecuted
		{
			add { AddHandler(SearchExecutedEvent, value); }
			remove { RemoveHandler(SearchExecutedEvent, value); }
		}

		private void RaiseSearchExecutedEvent(string query, SearchOptions options)
		{
			RaiseEvent(new SearchExecutedEventArgs(SearchExecutedEvent, query, options));
		}

		public static readonly RoutedEvent ReplaceExecutedEvent = EventManager.RegisterRoutedEvent(
			"ReplaceExecuted", RoutingStrategy.Bubble, typeof(ReplaceExecutedEventHandler), typeof(SearchUI));

		public event ReplaceExecutedEventHandler ReplaceExecuted
		{
			add { AddHandler(ReplaceExecutedEvent, value); }
			remove { RemoveHandler(ReplaceExecutedEvent, value); }
		}

		private void RaiseReplaceExecutedEvent(string query, string replace, SearchOptions options, ReplaceMode mode)
		{
			RaiseEvent(new ReplaceExecutedEventArgs(ReplaceExecutedEvent, query, replace, options, mode));
		}

		#endregion
	}

	public enum SearchMode : byte { Find, Replace };
	public enum SearchOptions : byte { Regex, MatchCase, WholeWord };
	public enum ReplaceMode : byte { Next, All };

	public delegate void SearchExecutedEventHandler(object sender, SearchExecutedEventArgs e);
	public delegate void ReplaceExecutedEventHandler(object sender, ReplaceExecutedEventArgs e);

	[ComVisible(false)]
	public class SearchExecutedEventArgs : RoutedEventArgs
	{
		public SearchExecutedEventArgs(RoutedEvent routedEvent, string query, SearchOptions options)
			: base(routedEvent)
		{
			Query = query;
			Options = options;
		}

		/// <summary>
		/// Gets the query string.
		/// </summary>
		public string Query
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets all options.
		/// </summary>
		public SearchOptions Options
		{
			get;
			private set;
		}
	}

	[ComVisible(false)]
	public class ReplaceExecutedEventArgs : SearchExecutedEventArgs
	{
		public ReplaceExecutedEventArgs(RoutedEvent routedEvent, string query, string replace, SearchOptions options, ReplaceMode mode)
			: base(routedEvent, query, options)
		{
			Replace = replace;
			Mode = mode;
		}

		/// <summary>
		/// Gets the replacement string.
		/// </summary>
		public string Replace
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets how replacement should be executed.
		/// </summary>
		public ReplaceMode Mode
		{
			get;
			private set;
		}
	}
}
