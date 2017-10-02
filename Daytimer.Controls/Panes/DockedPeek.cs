using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Daytimer.Controls.Panes
{
	[ComVisible(false)]
	[TemplatePart(Name = DockedPeek.ThumbName, Type = typeof(Thumb))]
	public class DockedPeek : Peek
	{
		#region Constructors

		static DockedPeek()
		{
			Type ownerType = typeof(DockedPeek);
			DefaultStyleKeyProperty.OverrideMetadata(ownerType, new FrameworkPropertyMetadata(ownerType));

			RemovePeekCommand = new RoutedCommand("RemovePeekCommand", ownerType);
		}

		public DockedPeek()
		{
			Loaded += DockedPeek_Loaded;
		}

		#endregion

		#region Fields

		private const string ThumbName = "PART_Thumb";

		#endregion

		#region Internal Properties

		internal Thumb PART_Thumb;

		#endregion

		#region Public Methods

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			PART_Thumb = GetTemplateChild(ThumbName) as Thumb;

			PART_Thumb.DragDelta += PART_Thumb_DragDelta;
		}

		public override void Load()
		{
			(Content as Peek).Load();
		}

		#endregion

		#region Private Methods

		private void PART_Thumb_DragDelta(object sender, DragDeltaEventArgs e)
		{
			if (!CanResizeToHeight(e.VerticalChange))
				return;

			int row = Grid.GetRow(this);
			Grid parent = (Parent as DockedPeekContainer).PART_ItemsHost;

			RowDefinition thisRow = parent.RowDefinitions[row];
			double height = (thisRow.Height.Value != 1 ? thisRow.Height.Value : thisRow.ActualHeight) + e.VerticalChange;
			thisRow.Height = new GridLength(height, GridUnitType.Star);

			for (int i = 0; i < parent.RowDefinitions.Count; i++)
			{
				RowDefinition rowDef = parent.RowDefinitions[i];

				if (i != row && i != row + 1)
				{
					if (rowDef.Height.Value == 1)
						rowDef.Height = new GridLength(rowDef.ActualHeight, GridUnitType.Star);
				}
				else if (i == row + 1)
					rowDef.Height = new GridLength((rowDef.Height.Value != 1 ? rowDef.Height.Value : rowDef.ActualHeight) - e.VerticalChange, GridUnitType.Star);
			}
		}

		private bool CanResizeToHeight(double change)
		{
			if (ActualHeight + change < 150)
				return false;

			int row = Grid.GetRow(this);
			Grid parent = (Parent as DockedPeekContainer).PART_ItemsHost;

			if (parent.RowDefinitions.Count > row + 1 && parent.RowDefinitions[row + 1].ActualHeight - change < 150)
				return false;

			return true;
		}

		private void DockedPeek_Loaded(object sender, RoutedEventArgs e)
		{
			(Content as Peek).Loaded += Content_Loaded;
		}

		private void Content_Loaded(object sender, RoutedEventArgs e)
		{
			Load();
		}

		#endregion

		#region Commands

		public static RoutedCommand RemovePeekCommand;

		#endregion
	}
}
