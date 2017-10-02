using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace Demo.Main
{
	class InputHelpers
	{
		/// <summary>
		/// Move the mouse to the specified location.
		/// </summary>
		/// <param name="point"></param>
		public static void MouseMove(Point point)
		{
			SetCursorPos((int)point.X, (int)point.Y);
		}
		
		/// <summary>
		/// Sends the given keys to the active application, and then waits for the messages
		/// to be processed.
		/// </summary>
		/// <param name="text">The string of keystrokes to send.</param>
		public static void Type(string text)
		{
			System.Windows.Forms.SendKeys.SendWait(text);
		}

		/// <summary>
		/// Simulate a mouse double-click.
		/// </summary>
		public static void MouseDoubleClick(MouseButton button)
		{
			MouseClick(button);
			MouseClick(button);
		}

		/// <summary>
		/// Simulates a mouse click see http://pinvoke.net/default.aspx/user32/mouse_event.html?diff=y
		/// </summary>
		public static void MouseClick(MouseButton button)
		{
			MouseClick(button, ButtonState.Down);
			MouseClick(button, ButtonState.Up);
		}

		/// <summary>
		/// Simulates a mouse button state change.
		/// </summary>
		/// <param name="button"></param>
		/// <param name="state"></param>
		public static void MouseClick(MouseButton button, ButtonState state)
		{
			switch (button)
			{
				case MouseButton.Left:
					switch (state)
					{
						case ButtonState.Up:
							mouse_event((uint)MouseEventFlags.LEFTUP, 0, 0, 0, 0);
							break;

						case ButtonState.Down:
							mouse_event((uint)MouseEventFlags.LEFTDOWN, 0, 0, 0, 0);
							break;
					}
					break;

				case MouseButton.Right:
					switch (state)
					{
						case ButtonState.Up:
							mouse_event((uint)MouseEventFlags.RIGHTUP, 0, 0, 0, 0);
							break;

						case ButtonState.Down:
							mouse_event((uint)MouseEventFlags.RIGHTDOWN, 0, 0, 0, 0);
							break;
					}
					break;

				case MouseButton.Middle:
					switch (state)
					{
						case ButtonState.Up:
							mouse_event((uint)MouseEventFlags.MIDDLEUP, 0, 0, 0, 0);
							break;

						case ButtonState.Down:
							mouse_event((uint)MouseEventFlags.MIDDLEDOWN, 0, 0, 0, 0);
							break;
					}
					break;
			}
		}

		#region Imports

		[DllImport("user32")]
		private static extern int SetCursorPos(int x, int y);

		[DllImport("user32.dll")]
		private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);

		[Flags]
		private enum MouseEventFlags : uint
		{
			LEFTDOWN = 0x00000002,
			LEFTUP = 0x00000004,
			MIDDLEDOWN = 0x00000020,
			MIDDLEUP = 0x00000040,
			MOVE = 0x00000001,
			ABSOLUTE = 0x00008000,
			RIGHTDOWN = 0x00000008,
			RIGHTUP = 0x00000010
		}

		#endregion
	}

	enum MouseButton : byte
	{
		Left,
		Right,
		Middle
	};

	enum ButtonState : byte
	{
		Up,
		Down
	};
}
