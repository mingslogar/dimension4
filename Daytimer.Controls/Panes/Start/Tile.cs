using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Daytimer.Controls.Panes.Start
{
	[ComVisible(false)]
	public class Tile : HeaderedContentControl
	{
		#region Constructors

		static Tile()
		{
			Type ownerType = typeof(Tile);
			DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));
		}

		public Tile()
		{

		}

		#endregion

		#region Dependency Properties

		public static readonly DependencyProperty TileSizeProperty = DependencyProperty.Register(
			"TileSize", typeof(TileSize), typeof(Tile), new PropertyMetadata(TileSize.Large));

		public TileSize TileSize
		{
			get { return (TileSize)GetValue(TileSizeProperty); }
			set { SetValue(TileSizeProperty, value); }
		}

		public static readonly DependencyProperty SmallIconProperty = DependencyProperty.Register(
			"SmallIcon", typeof(ImageSource), typeof(Tile));

		public ImageSource SmallIcon
		{
			get { return (ImageSource)GetValue(SmallIconProperty); }
			set { SetValue(SmallIconProperty, value); }
		}

		public static readonly DependencyProperty LargeIconProperty = DependencyProperty.Register(
			"LargeIcon", typeof(ImageSource), typeof(Tile));

		public ImageSource LargeIcon
		{
			get { return (ImageSource)GetValue(LargeIconProperty); }
			set { SetValue(LargeIconProperty, value); }
		}

		public static readonly DependencyProperty IsHeaderVisibleInContentProperty = DependencyProperty.Register(
			"IsHeaderVisibleInContent", typeof(bool), typeof(Tile), new PropertyMetadata(true));

		public bool IsHeaderVisibleInContent
		{
			get { return (bool)GetValue(IsHeaderVisibleInContentProperty); }
			set { SetValue(IsHeaderVisibleInContentProperty, value); }
		}

		#endregion
	}

	public enum TileSize : byte { Small, Medium, Wide, Large }
}
