using System;
using System.Windows;
using System.Windows.Controls;

namespace Modern.FileBrowser
{
	class FileSystemListView : ListView
	{
		#region Constructors

		static FileSystemListView()
		{
			Type ownerType = typeof(FileSystemListView);
			DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));
		}

		public FileSystemListView()
		{

		}

		#endregion

		#region Dependency Properties

		public static readonly DependencyProperty ViewModeProperty = DependencyProperty.Register(
			"ViewMode", typeof(ListViewMode), typeof(FileSystemListView), new PropertyMetadata(ListViewMode.Detail));

		/// <summary>
		/// Gets or sets the <see cref="Modern.FileBrowser.ListViewMode"/> which defines how items are arranged.
		/// </summary>
		public ListViewMode ViewMode
		{
			get { return (ListViewMode)GetValue(ViewModeProperty); }
			set { SetValue(ViewModeProperty, value); }
		}

		#endregion
	}

	public enum ListViewMode : byte
	{
		/// <summary>
		/// 256x256 icons.
		/// </summary>
		ExtraLargeIcon,

		/// <summary>
		/// 96x96 icons.
		/// </summary>
		LargeIcon,

		/// <summary>
		/// 48x48 icons.
		/// </summary>
		MediumIcon,

		/// <summary>
		/// 16x16 icons.
		/// </summary>
		SmallIcon,

		///// <summary>
		///// 
		///// </summary>
		//List,

		/// <summary>
		/// 16x16 icons with details.
		/// </summary>
		Detail,

		/// <summary>
		/// 48x48 icons with some details.
		/// </summary>
		Tile,

		/// <summary>
		/// 32x32 icons with content details.
		/// </summary>
		Content
	}
}
