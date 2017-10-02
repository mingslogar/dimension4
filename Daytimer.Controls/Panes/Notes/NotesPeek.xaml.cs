using Daytimer.Fundamentals;
using System;
using System.Windows;
using System.Windows.Input;

namespace Daytimer.Controls.Panes.Notes
{
	/// <summary>
	/// Interaction logic for NotesPeek.xaml
	/// </summary>
	public partial class NotesPeek : UndockedPeek
	{
		static NotesPeek()
		{
			Type ownerType = typeof(NotesPeek);
			CommandBinding newNote = new CommandBinding(NotesPeekContent.NewNoteCommand, NewNoteExecuted);
			CommandManager.RegisterClassCommandBinding(ownerType, newNote);
		}

		public NotesPeek()
		{
			InitializeComponent();
		}
		
		private static void NewNoteExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			NotesPeek content = sender as NotesPeek;

			BalloonTip tip = Window.GetWindow(content) as BalloonTip;
			tip.FastClose();
		}

		#region Public Methods

		public override UIElement PeekContent()
		{
			return new NotesPeekContent();
		}

		#endregion
	}
}
