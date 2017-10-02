using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace Daytimer.Controls
{
	public static partial class Extensions
	{
		/// <summary>
		/// Find the ancestor of the given type of an object.
		/// </summary>
		/// <param name="child"></param>
		/// <param name="type"></param>
		/// <returns>FrameworkElement if element has been found, null otherwise.</returns>
		public static FrameworkElement FindAncestor(this FrameworkElement child, Type type)
		{
			DependencyObject parent = child.Parent;

			if (parent == null)
				parent = child.TemplatedParent;

			if (parent != null)
			{
				FrameworkElement _parent = parent as FrameworkElement;

				if (_parent != null)
				{
					Type _parentType = _parent.GetType();

					while (_parentType != null)
					{
						if (_parentType == type)
							return _parent;

						_parentType = _parentType.BaseType;
					}

					return FindAncestor(_parent, type);
				}
			}

			return null;
		}

		/// <summary>
		/// Find the ancestor of the given type of an object.
		/// </summary>
		/// <param name="child"></param>
		/// <param name="type"></param>
		/// <returns>FrameworkElement if element has been found, null otherwise.</returns>
		public static FrameworkElement FindTemplatedAncestor(this FrameworkElement child, Type type)
		{
			DependencyObject parent = child.TemplatedParent;

			if (parent == null)
				parent = child.Parent;

			if (parent != null)
			{
				FrameworkElement _parent = parent as FrameworkElement;

				if (_parent != null)
				{
					Type _parentType = _parent.GetType();

					while (_parentType != null)
					{
						if (_parentType == type)
							return _parent;

						_parentType = _parentType.BaseType;
					}

					return FindTemplatedAncestor(_parent, type);
				}
			}

			return null;
		}
		
		/// <summary>
		/// Removes an entry from the array, if it exists.
		/// </summary>
		/// <param name="array"></param>
		/// <param name="entry"></param>
		/// <returns></returns>
		public static string[] RemoveEntry(this string[] array, string entry)
		{
			if (array == null)
				return new string[0];

			int index = Array.IndexOf(array, entry);

			if (index == -1)
				return array;

			string[] shrunk = new string[array.Length - 1];

			Array.Copy(array, shrunk, index);
			Array.Copy(array, index + 1, shrunk, index, array.Length - index - 1);

			return shrunk;
		}

		/// <summary>
		/// Execute the specified command on multiple targets.
		/// </summary>
		/// <param name="command"></param>
		/// <param name="parameter"></param>
		/// <param name="targets"></param>
		public static void MassExecute(this RoutedCommand command, object parameter, IEnumerable targets)
		{
			foreach (IInputElement each in targets)
				command.Execute(parameter, each);
		}

		/// <summary>
		/// Execute the specified command on multiple targets, and skip a certain target. Useful
		/// for when the sender is part of the target group.
		/// </summary>
		/// <param name="command"></param>
		/// <param name="parameter"></param>
		/// <param name="targets"></param>
		/// <param name="skip"></param>
		public static void MassExecute(this RoutedCommand command, object parameter, IEnumerable targets, IInputElement skip)
		{
			foreach (IInputElement each in targets)
				if (each != skip)
					command.Execute(parameter, each);
		}

		/// <summary>
		/// Repeatedly attempts to focus the text box until focus is achieved.
		/// </summary>
		/// <param name="textBox"></param>
		public static void Activate(this TextBoxBase textBox)
		{
			DispatcherTimer timer = new DispatcherTimer();
			timer.Interval = TimeSpan.FromMilliseconds(100);

			timer.Tick += (tmr, args) =>
			{
				if (textBox.IsKeyboardFocused)
				{
					timer.Stop();
					timer = null;
				}

				textBox.Focus();
			};

			timer.Start();
		}
	}
}
