using System;
using System.IO;
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace Daytimer.Help
{
	/// <summary>
	/// <para>
	/// Class wraps a WebBrowser (which itself is a badly designed WPF control) and presents itself as
	/// a better designed WPF control. Provides, for example, bindable source properties and commands.</para>
	/// <para>
	/// Based on http://www.codeproject.com/Articles/555302/A-better-WPF-Browser-Control-IE-Wrapper.
	/// </para>
	/// </summary>
	[ComVisible(false)]
	public class WpfWebBrowserWrapper : ContentControl
	{
		public WpfWebBrowserWrapper()
		{
			m_innerBrowser = new WebBrowser()
			{
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch
			};

			Content = m_innerBrowser;
			m_innerBrowser.Navigated += m_innerBrowser_Navigated_;
			m_innerBrowser.Navigating += m_innerBrowser_Navigating_;
			m_innerBrowser.LoadCompleted += m_innerBrowser_LoadCompleted_;
			m_innerBrowser.Loaded += m_innerBrowser_Loaded_;
			m_innerBrowser.SizeChanged += m_innerBrowser_SizeChanged_;

			CommandBindings.Add(new CommandBinding(NavigationCommands.BrowseBack, BrowseBackExecuted_, CanBrowseBack_));
			CommandBindings.Add(new CommandBinding(NavigationCommands.BrowseForward, BrowseForwardExecuted_, CanBrowseForward_));
			CommandBindings.Add(new CommandBinding(NavigationCommands.Refresh, BrowseRefreshExecuted_));
			CommandBindings.Add(new CommandBinding(NavigationCommands.BrowseStop, BrowseStopExecuted_));
			CommandBindings.Add(new CommandBinding(NavigationCommands.GoToPage, BrowseGoToPageExecuted_));
		}

		private void m_innerBrowser_SizeChanged_(object sender, SizeChangedEventArgs e)
		{
			ApplyZoom(this);
		}

		private void m_innerBrowser_Loaded_(object sender, EventArgs e)
		{
			// Make browser control not silent: allow HTTP-Auth-dialogs. Requery command availability.
			SHDocVw.InternetExplorer ie = ActiveXControl;
			ie.Silent = false;
			CommandManager.InvalidateRequerySuggested();
		}

		// Called when the loading of a web page is done.
		private void m_innerBrowser_LoadCompleted_(object sender, NavigationEventArgs e)
		{
			ApplyZoom(this);  // Apply later and not only at changed event, since only works if browser is rendered.
			SetCurrentValue(IsNavigatingProperty, false);
			CommandManager.InvalidateRequerySuggested();
		}

		// Called when the browser started to load and retrieve data.
		private void m_innerBrowser_Navigating_(object sender, NavigatingCancelEventArgs e)
		{
			SetCurrentValue(IsNavigatingProperty, true);
		}

		// Re-query the commands once done navigating.
		private void m_innerBrowser_Navigated_(object sender, NavigationEventArgs e)
		{
			//RegisterWindowErrorHanlder_();

			CommandManager.InvalidateRequerySuggested();
			// Publish the just navigated (maybe redirected) URL.
			if (e.Uri != null)
			{
				SetCurrentValue(CurrentUrlProperty, e.Uri.ToString());
			}
		}

		/// <summary>
		/// Bindable source property to make the browser navigate to the given url. Assign this from your URL bar.
		/// </summary>
		public string BindableSource
		{
			get
			{
				return (string)GetValue(BindableSourceProperty);
			}
			set
			{
				SetValue(BindableSourceProperty, value);
			}
		}

		/// <summary>
		/// Bindable property depicting the current url. Use this to read out and present in your url bar.
		/// </summary>
		public string CurrentUrl
		{
			get
			{
				return (string)GetValue(CurrentUrlProperty);
			}
			set
			{
				SetValue(CurrentUrlProperty, value);
			}
		}

		/// <summary>
		/// Percentage value: 20-800 change to let control zoom in out.
		/// </summary>
		public int Zoom
		{
			get
			{
				return (int)GetValue(ZoomProperty);
			}
			set
			{
				SetValue(ZoomProperty, value);
			}
		}

		/// <summary>
		/// The last error encountered.
		/// </summary>
		public string LastError
		{
			get
			{
				return (string)GetValue(LastErrorProperty);
			}
		}

		/// <summary>
		/// Gets if the browser is currently loading a page.
		/// </summary>
		public int IsNavigating
		{
			get
			{
				return (int)GetValue(IsNavigatingProperty);
			}
			private set
			{
				SetValue(IsNavigatingProperty, value);
			}
		}

		/// <summary>
		/// Gets the browser control's underlying ActiveXControl.
		/// </summary>
		public SHDocVw.InternetExplorer ActiveXControl
		{
			get
			{
				// this is a brilliant way to access the WebBrowserObject prior to displaying the actual document (eg. Document property)
				FieldInfo fiComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
				if (fiComWebBrowser == null) return null;
				object objComWebBrowser = fiComWebBrowser.GetValue(m_innerBrowser);
				if (objComWebBrowser == null) return null;
				return objComWebBrowser as SHDocVw.InternetExplorer;
			}
		}

		private void BrowseForwardExecuted_(object sender, ExecutedRoutedEventArgs e)
		{
			m_innerBrowser.GoForward();
		}

		private void CanBrowseForward_(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = m_innerBrowser.IsLoaded && m_innerBrowser.CanGoForward;
		}

		private void BrowseBackExecuted_(object sender, ExecutedRoutedEventArgs e)
		{
			m_innerBrowser.GoBack();
		}

		private void CanBrowseBack_(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = m_innerBrowser.IsLoaded && m_innerBrowser.CanGoBack;
		}

		private void BrowseRefreshExecuted_(object sender, ExecutedRoutedEventArgs e)
		{
			try
			{
				//ActiveXControl.Refresh2(false);
				m_innerBrowser.Refresh(noCache: true);
				//RegisterWindowErrorHanlder_();
			}
			catch (COMException)
			{

			}
			// No setting of navigating=true, since the reset event never triggers on Refresh!
			//SetCurrentValue(IsNavigatingProperty, true);
		}

		private void BrowseStopExecuted_(object sender, ExecutedRoutedEventArgs executedRoutedEventArgs)
		{
			var control = this.ActiveXControl;
			if (control != null)
				control.Stop();
		}

		/// <summary>
		/// Navigates to the page specified in the parameter.
		/// </summary>
		private void BrowseGoToPageExecuted_(object sender, ExecutedRoutedEventArgs executedRoutedEventArgs)
		{
			string urlstr = executedRoutedEventArgs.Parameter as string;
			if (urlstr != null)
				m_innerBrowser.Navigate(urlstr);
		}

		private static void BindableSourcePropertyChanged_(DependencyObject o, DependencyPropertyChangedEventArgs e)
		{
			var wrapper = (WpfWebBrowserWrapper)o;
			var browser = wrapper.m_innerBrowser as System.Windows.Controls.WebBrowser;
			if (browser != null)
			{

				string uri = e.NewValue as string;
				if (!string.IsNullOrWhiteSpace(uri) && Uri.IsWellFormedUriString(uri, UriKind.Absolute))
				{
					Uri uriObj;
					try
					{
						uriObj = new Uri(uri);
						browser.Source = uriObj;
					}
					catch (UriFormatException)
					{
						// just don't crash because of a malformed url
					}

				}
				else
				{
					browser.Source = null;
				}
			}
		}

		#region Browser Function Wrappers

		/// <summary>
		/// Navigate back to the previous document, if there is one.
		/// </summary>
		/// <exception cref="System.ObjectDisposedException">The System.Windows.Controls.WebBrowser instance is no longer valid.</exception>
		/// <exception cref="System.InvalidOperationException">A reference to the underlying native WebBrowser could not be retrieved.</exception>
		/// <exception cref="System.Runtime.InteropServices.COMException">There is no document to navigate back to.</exception>
		[SecurityCritical]
		public void GoBack()
		{
			m_innerBrowser.GoBack();
		}

		/// <summary>
		/// Navigate forward to the next HTML document, if there is one.
		/// </summary>
		/// <exception cref="System.ObjectDisposedException">The System.Windows.Controls.WebBrowser instance is no longer valid.</exception>
		/// <exception cref="System.InvalidOperationException">A reference to the underlying native WebBrowser could not be retrieved.</exception>
		/// <exception cref="System.Runtime.InteropServices.COMException">There is no document to navigate forward to.</exception>
		[SecurityCritical]
		public void GoForward()
		{
			m_innerBrowser.GoForward();
		}

		/// <summary>
		/// Executes a script function that is implemented by the currently loaded document.
		/// </summary>
		/// <param name="scriptName">The name of the script function to execute.</param>
		/// <returns>The object returned by the Active Scripting call.</returns>
		/// <exception cref="System.ObjectDisposedException">The System.Windows.Controls.WebBrowser instance is no longer valid.</exception>
		/// <exception cref="System.InvalidOperationException">A reference to the underlying native WebBrowser could not be retrieved.</exception>
		/// <exception cref="System.Runtime.InteropServices.COMException">The script function does not exist.</exception>
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public object InvokeScript(string scriptName)
		{
			return m_innerBrowser.InvokeScript(scriptName);
		}

		/// <summary>
		/// Executes a script function that is defined in the currently loaded document.
		/// </summary>
		/// <param name="scriptName">The name of the script function to execute.</param>
		/// <param name="args">The parameters to pass to the script function.</param>
		/// <returns>The object returned by the Active Scripting call.</returns>
		/// <exception cref="System.ObjectDisposedException">The System.Windows.Controls.WebBrowser instance is no longer valid.</exception>
		/// <exception cref="System.InvalidOperationException">A reference to the underlying native WebBrowser could not be retrieved.</exception>
		/// <exception cref="System.Runtime.InteropServices.COMException">The script function does not exist.</exception>
		[SecurityCritical]
		public object InvokeScript(string scriptName, params object[] args)
		{
			return m_innerBrowser.InvokeScript(scriptName, args);
		}

		/// <summary>
		/// Navigates asynchronously to the document at the specified URL.
		/// </summary>
		/// <param name="source">The URL to navigate to.</param>
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public void Navigate(string source)
		{
			m_innerBrowser.Navigate(source);
		}

		/// <summary>
		/// Navigate asynchronously to the document at the specified System.Uri.
		/// </summary>
		/// <param name="source">The System.Uri to navigate to.</param>
		/// <exception cref="System.ObjectDisposedException">The System.Windows.Controls.WebBrowser instance is no longer valid.</exception>
		/// <exception cref="System.InvalidOperationException">A reference to the underlying native WebBrowser could not be retrieved.</exception>
		/// <exception cref="System.Security.SecurityException">Navigation from an application that is running in partial trust to a System.Uri that is not located at the site of origin.</exception>
		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public void Navigate(Uri source)
		{
			m_innerBrowser.Navigate(source);
		}

		/// <summary>
		/// Navigates asynchronously to the document at the specified URL and specify
		/// the target frame to load the document's content into. Additional HTTP POST
		/// data and HTTP headers can be sent to the server as part of the navigation
		/// request.
		/// </summary>
		/// <param name="source">The URL to navigate to.</param>
		/// <param name="targetFrameName">The name of the frame to display the document's content in.</param>
		/// <param name="postData">HTTP POST data to send to the server when the source is requested.</param>
		/// <param name="additionalHeaders">HTTP headers to send to the server when the source is requested.</param>
		public void Navigate(string source, string targetFrameName, byte[] postData, string additionalHeaders)
		{
			m_innerBrowser.Navigate(source, targetFrameName, postData, additionalHeaders);
		}

		/// <summary>
		/// Navigate asynchronously to the document at the specified System.Uri and specify
		/// the target frame to load the document's content into. Additional HTTP POST
		/// data and HTTP headers can be sent to the server as part of the navigation
		/// request.
		/// </summary>
		/// <param name="source">The System.Uri to navigate to.</param>
		/// <param name="targetFrameName">The name of the frame to display the document's content in.</param>
		/// <param name="postData">HTTP POST data to send to the server when the source is requested.</param>
		/// <param name="additionalHeaders">HTTP headers to send to the server when the source is requested.</param>
		/// <exception cref="System.ObjectDisposedException">The System.Windows.Controls.WebBrowser instance is no longer valid.</exception>
		/// <exception cref="System.InvalidOperationException">A reference to the underlying native WebBrowser could not be retrieved.</exception>
		/// <exception cref="System.Security.SecurityException">Navigation from an application that is running in partial trust:To a System.Uri that is not located at the site of origin, or targetFrameName name is not null or empty.</exception>
		public void Navigate(Uri source, string targetFrameName, byte[] postData, string additionalHeaders)
		{
			m_innerBrowser.Navigate(source, targetFrameName, postData, additionalHeaders);
		}

		/// <summary>
		/// Navigate asynchronously to a System.IO.Stream that contains the content for
		/// a document.
		/// </summary>
		/// <param name="stream">The System.IO.Stream that contains the content for a document.</param>
		/// <exception cref="System.ObjectDisposedException">The System.Windows.Controls.WebBrowser instance is no longer valid.</exception>
		/// <exception cref="System.InvalidOperationException">A reference to the underlying native WebBrowser could not be retrieved.</exception>
		public void NavigateToStream(Stream stream)
		{
			m_innerBrowser.NavigateToStream(stream);
		}

		/// <summary>
		/// Navigate asynchronously to a System.String that contains the content for
		/// a document.
		/// </summary>
		/// <param name="text">The System.String that contains the content for a document.</param>
		/// <exception cref="System.ObjectDisposedException">The System.Windows.Controls.WebBrowser instance is no longer valid.</exception>
		/// <exception cref="System.InvalidOperationException">A reference to the underlying native WebBrowser could not be retrieved.</exception>
		public void NavigateToString(string text)
		{
			m_innerBrowser.NavigateToString(text);
		}

		/// <summary>
		/// Reloads the current page.
		/// </summary>
		/// <exception cref="System.ObjectDisposedException">The System.Windows.Controls.WebBrowser instance is no longer valid.</exception>
		/// <exception cref="System.InvalidOperationException">A reference to the underlying native WebBrowser could not be retrieved.</exception>
		[SecurityCritical]
		public void Refresh()
		{
			m_innerBrowser.Refresh();
		}

		/// <summary>
		/// Reloads the current page with optional cache validation.
		/// </summary>
		/// <param name="noCache">Specifies whether to refresh without cache validation.</param>
		/// <exception cref="System.ObjectDisposedException">The System.Windows.Controls.WebBrowser instance is no longer valid.</exception>
		/// <exception cref="System.InvalidOperationException">A reference to the underlying native WebBrowser could not be retrieved.</exception>
		[SecurityCritical]
		public void Refresh(bool noCache)
		{
			m_innerBrowser.Refresh(noCache);
		}

		#endregion

		//// register script errors handler on DOM - document.window
		//private void RegisterWindowErrorHanlder_()
		//{
		//	object parwin = ((dynamic)m_innerBrowser.Document).parentWindow;
		//	var cookie = new System.Windows.Forms.AxHost.ConnectionPointCookie(parwin, new HtmlWindowEvents2Impl(this), typeof(IIntHTMLWindowEvents2));
		//	// MemoryLEAK? No: cookie has a Finalize() to Disconnect istelf. We'll rely on that. If disconnected too early, 
		//	// though (eg. in LoadCompleted-event) scripts continue to run and can cause error messages to appear again.
		//	// --> forget cookie and be happy.
		//}

		// needed to implement the Event for script errors
		[Guid("3050f625-98b5-11cf-bb82-00aa00bdce0b"), InterfaceType(ComInterfaceType.InterfaceIsIDispatch), TypeLibType(TypeLibTypeFlags.FHidden)]
		[ComImport]
		private interface IIntHTMLWindowEvents2
		{
			[DispId(1002)]
			bool onerror(string description, string url, int line);
		}

		// needed to implement the Event for script errors
		private class HtmlWindowEvents2Impl : IIntHTMLWindowEvents2
		{
			private WpfWebBrowserWrapper m_control;

			public HtmlWindowEvents2Impl(WpfWebBrowserWrapper control)
			{ m_control = control; }

			// implementation of the onerror Javascript error. Return true to indicate a "Handled" state.
			public bool onerror(string description, string urlString, int line)
			{
				this.m_control.SetCurrentValue(LastErrorProperty, description + "@" + urlString + ": " + line);
				// Handled:
				return true;
			}
		}

		// Needed to expose the WebBrowser's underlying ActiveX control for zoom functionality
		[ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		[Guid("6d5140c1-7436-11ce-8034-00aa006009fa")]
		internal interface IServiceProvider
		{
			[return: MarshalAs(UnmanagedType.IUnknown)]
			object QueryService(ref Guid guidService, ref Guid riid);
		}
		static readonly Guid SID_SWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");

		public static void ZoomPropertyChanged_(DependencyObject src, DependencyPropertyChangedEventArgs e)
		{
			ApplyZoom(src);
		}

		private static void ApplyZoom(DependencyObject src)
		{
			const int k_minZoom = 10;
			const int k_maxZoom = 1000;
			const float k_zoomInReference = 800.0f;


			var browser = src as WpfWebBrowserWrapper;
			if (browser != null && browser.IsLoaded)
			{
				WebBrowser webbr = browser.m_innerBrowser;
				int zoomPercent = browser.Zoom;

				// Determine auto-zoom
				if (browser.Zoom == -1)
				{
					if (browser.ActualWidth < k_zoomInReference)
						zoomPercent = (int)(browser.ActualWidth / k_zoomInReference * 100);
					else
						zoomPercent = 100;
				}

				// rescue too high or too low values
				zoomPercent = Math.Min(zoomPercent, k_maxZoom);
				zoomPercent = Math.Max(zoomPercent, k_minZoom);

				// grab a handle to the underlying ActiveX object
				IServiceProvider serviceProvider = null;
				if (webbr.Document != null)
				{
					serviceProvider = (IServiceProvider)webbr.Document;
				}
				if (serviceProvider == null)
					return;

				Guid serviceGuid = SID_SWebBrowserApp;
				Guid iid = typeof(SHDocVw.IWebBrowser2).GUID;
				SHDocVw.IWebBrowser2 browserInst = (SHDocVw.IWebBrowser2)serviceProvider.QueryService(ref serviceGuid, ref iid);

				try
				{
					object zoomPercObj = zoomPercent;
					// send the zoom command to the ActiveX object
					browserInst.ExecWB(SHDocVw.OLECMDID.OLECMDID_OPTICAL_ZOOM,
									   SHDocVw.OLECMDEXECOPT.OLECMDEXECOPT_DONTPROMPTUSER,
									   ref zoomPercObj,
									   IntPtr.Zero);
				}
				catch (Exception)
				{
					// ignore this dynamic call if it fails.
				}
			}
		}

		public static readonly DependencyProperty BindableSourceProperty =
			DependencyProperty.Register("BindableSource", typeof(string), typeof(WpfWebBrowserWrapper),
			new UIPropertyMetadata("about:blank", BindableSourcePropertyChanged_));

		public static readonly DependencyProperty CurrentUrlProperty =
			DependencyProperty.Register("CurrentUrl", typeof(string), typeof(WpfWebBrowserWrapper),
			new UIPropertyMetadata(string.Empty));

		public static readonly DependencyProperty ZoomProperty =
			DependencyProperty.Register("Zoom", typeof(int), typeof(WpfWebBrowserWrapper),
			new UIPropertyMetadata(100, ZoomPropertyChanged_));

		public static readonly DependencyProperty IsNavigatingProperty =
			DependencyProperty.Register("IsNavigating", typeof(bool), typeof(WpfWebBrowserWrapper),
			new UIPropertyMetadata(false));

		public static readonly DependencyProperty LastErrorProperty =
			DependencyProperty.Register("LastError", typeof(string), typeof(WpfWebBrowserWrapper),
			new UIPropertyMetadata(string.Empty));

		private readonly WebBrowser m_innerBrowser;
	}
}
