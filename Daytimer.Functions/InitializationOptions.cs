using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Daytimer.Functions
{
	/// <summary>
	/// This class is taken from http://www.codeproject.com/Articles/217022/Delaying-Element-Initialization-for-Collapsed-Cont.
	/// </summary>
	public static class InitializationOptions
	{
		private static Type _skipType = null;

		public static readonly DependencyProperty SkipTypeProperty = DependencyProperty.RegisterAttached(
			"SkipType",
			typeof(Type),
			typeof(InitializationOptions),
			new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnSkipTypeChanged)));

		/// <summary>
		/// When (or rather, if) we ever actually initialize the control - when its visibility state has changed to non-collapsed - 
		/// we need to call the handlers for Initialized and Loaded that would normally have been called during control initialization.
		/// Since we can't raise these events ourselves, we need to store the handlers so that we can directly invoke them after we
		/// perform initialization.
		/// </summary>
		public static readonly DependencyProperty TargetInitializedProperty = DependencyProperty.RegisterAttached(
			"TargetInitialized",
			typeof(EventHandler),
			typeof(InitializationOptions));

		public static readonly DependencyProperty TargetLoadedProperty = DependencyProperty.RegisterAttached(
			"TargetLoaded",
			typeof(RoutedEventHandler),
			typeof(InitializationOptions));

		private static void OnSkipTypeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
		{
			// If the skip type is already set when this is called, that means we're nesting the skipping of collapsed children. 
			// If we allowed this, then when the innermost element that sets this type finished loading, it would clear the skip type, 
			// thereby fouling up the intended behavior for the other children of the outer element(s) that set the skip type.
			//
			// To prevent this from happening, we prohibit this nesting entirely by forbidding the setting of the skip type when 
			// the skip type is already set.
			// (An alternative would be a stack of skip types that get pushed and popped, but since I doubt this "nesting" 
			// situation will ever actually occur, why cut into our time savings with more architecture?)
			//           
			if (_skipType != null)
			{
				throw new NotSupportedException("Nesting the SkipType setting is not supported.");
			}
			_skipType = args.NewValue as Type;
			(obj as FrameworkElement).Initialized += new EventHandler(InitializationOptions_Initialized);
		}

		public static void InitializationOptions_Initialized(object sender, EventArgs args)
		{
			_skipType = null;
			// Once the skip type is cleared, there's no more need for this handler.
			(sender as FrameworkElement).Initialized -= new EventHandler(InitializationOptions_Initialized);
		}

		/// <summary>
		/// Enables support for delayed initialization. Call this instead of InitializeComponent() at the top of a control 
		/// constructor to enable delayed initialization for that control class.
		/// </summary>
		/// <param name="element">this - the control for which delayed initialization is being enabled</param>
		/// <param name="initHandler">Handler for the control's Initialized event</param>
		/// <param name="loadHandler">Handler for the control's Loaded event</param>
		/// 
		// (Convenience overload for specifying the loaded handler with no initialized handler.)
		//
		public static void Initialize(FrameworkElement element, RoutedEventHandler loadedHandler)
		{
			Initialize(element, null, loadedHandler);
		}

		/// <summary>
		/// Enables support for delayed initialization. Call this instead of InitializeComponent() at the top of a control 
		/// constructor to enable delayed initialization for that control class.
		/// </summary>
		/// <param name="element">this - the control for which delayed initialization is being enabled</param>
		/// <param name="initHandler">Handler for the control's Initialized event</param>
		/// <param name="loadHandler">Handler for the control's Loaded event</param>
		public static void Initialize(FrameworkElement element, EventHandler initializedHandler = null, RoutedEventHandler loadedHandler = null)
		{
			if ((_skipType != null) && _skipType.IsInstanceOfType(element))
			{
				if (initializedHandler != null)
					element.SetValue(TargetInitializedProperty, initializedHandler);
				if (loadedHandler != null)
					element.SetValue(TargetLoadedProperty, loadedHandler);
				element.IsVisibleChanged += IsVisibleChangedHandler;
			}
			else
			{
				((IComponentConnector)element).InitializeComponent();
			}
		}

		/// <summary>
		/// This is the heart of delayed initialization: This method is not called until the element becomes
		/// non-collapsed, at which point it is initialized and the handlers for Initialized and Loaded are called.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public static void IsVisibleChangedHandler(Object sender, DependencyPropertyChangedEventArgs e)
		{
			Control element = sender as Control;
			// We check Template==null instead of the usual suspects (IsInitialized, IsLoaded) because the framework will have set
			// them to true by now even if we haven't called InitializeComponent().
			//
			if ((element != null) && (element.Visibility != Visibility.Collapsed) && (element.Template == null))
			{
				((IComponentConnector)element).InitializeComponent();
				EventHandler initHandler = (EventHandler)element.GetValue(TargetInitializedProperty);
				if (initHandler != null)
				{
					initHandler(element, new EventArgs());
				}
				RoutedEventHandler loadHandler = (RoutedEventHandler)element.GetValue(TargetLoadedProperty);
				if (loadHandler != null)
				{
					loadHandler(element, new RoutedEventArgs(FrameworkElement.LoadedEvent, element));
				}
				// The element only needs to get initialized once.
				element.IsVisibleChanged -= IsVisibleChangedHandler;
			}
		}

		/// <summary>
		/// We never want to actually call the set or get methods for the SkipType attached property - all we care about is the
		/// static _skipType field. Nonetheless, this method needs to *exist* in order for the XAML parser to recognize that the 
		/// attached property is legally settable from xaml. We care about being able to set the property from XAML because we need for 
		/// OnSkipTypeChanged to be called.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="value"></param>
		/// <exception cref="System.NotSupportedException">Always thrown when this function is called.</exception>
		public static void SetSkipType(DependencyObject obj, Type value)
		{
			// If anybody ever actually calls this method, it's a mistake.
			throw new NotSupportedException();
		}
	}
}
