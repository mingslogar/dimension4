using Daytimer.DatabaseHelpers;
using Daytimer.Functions;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Daytimer.Controls.Tasks
{
	/// <summary>
	/// Interaction logic for DraggableTreeView.xaml
	/// </summary>
	public partial class DraggableTreeView : TreeView
	{
		public DraggableTreeView()
		{
			InitializeComponent();
		}

		#region Initializers

		private Point _startPoint;
		private bool _isDown;
		private bool _isDragging;
		private bool _isAnimating;
		private TreeViewItem _originalElement;
		private DragDropImage _overlayElement;
		private Point _dragOffset;

		public bool IsDragging
		{
			get { return _isDragging; }
		}

		#endregion

		#region Functions

		protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			base.OnPreviewMouseLeftButtonDown(e);

			if (!_isAnimating)
			{
				if (e.Source is TreeViewItem && !(e.Source as TreeViewItem).HasItems)
				{
					_isDown = true;
					_startPoint = e.GetPosition(this);
					_originalElement = e.Source as TreeViewItem;

					Mouse.Capture(_originalElement);

					_dragOffset = e.GetPosition(_originalElement);
				}
			}
		}

		protected override void OnPreviewMouseMove(MouseEventArgs e)
		{
			base.OnPreviewMouseMove(e);

			if (_isDown)
			{
				if ((_isDragging == false) && ((Math.Abs(e.GetPosition(this).X - _startPoint.X) > SystemParameters.MinimumHorizontalDragDistance) ||
					(Math.Abs(e.GetPosition(this).Y - _startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)))
				{
					DragStarted();
				}
				if (_isDragging)
				{
					DragMoved();
				}
			}
		}

		protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			base.OnPreviewMouseLeftButtonUp(e);

			if (_isDown)
			{
				DragFinished(false);
				e.Handled = true;
			}
		}

		protected override void OnPreviewKeyDown(KeyEventArgs e)
		{
			base.OnPreviewKeyDown(e);

			if (e.Key == Key.Escape && _isDragging)
			{
				DragFinished(true);
			}
			else if ((e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl) && _isDragging)
				DragCopy = true;
		}

		protected override void OnPreviewKeyUp(KeyEventArgs e)
		{
			base.OnPreviewKeyUp(e);

			if ((e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl) && _isDragging)
				DragCopy = false;
		}

		protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
		{
			base.OnItemsChanged(e);

			if (e.NewItems != null)
			{
				foreach (TreeViewItem item in e.NewItems)
				{
					if (!(item.Parent is TreeViewItem))
					{
						item.MouseEnter += item_MouseEnter;
						item.Expanded += item_Expanded;
						item.Collapsed += item_Collapsed;
					}
				}
			}
		}

		private void item_MouseEnter(object sender, MouseEventArgs e)
		{
			if (_isDragging)
			{
				TreeViewItem _sender = sender as TreeViewItem;

				_sender.IsExpanded = true;

				if (Items.IndexOf(_sender) < Items.IndexOf(_originalElement.Parent))
				{
					_sender.SizeChanged -= _sender_SizeChanged;
					_sender.SizeChanged += _sender_SizeChanged;
				}
			}
		}

		private void item_Expanded(object sender, RoutedEventArgs e)
		{
			if (!Settings.AnimationsEnabled)
			{
				ScaleTransform transform = (sender as TreeViewItem).Template.FindName("itemsHostScale", sender as TreeViewItem) as ScaleTransform;

				if (transform != null)
					transform.ApplyAnimationClock(ScaleTransform.ScaleYProperty, null);
			}
		}

		private void item_Collapsed(object sender, RoutedEventArgs e)
		{
			if (!Settings.AnimationsEnabled)
			{
				((sender as Control).Template.FindName("itemsHostScale", sender as FrameworkElement) as Animatable).ApplyAnimationClock(ScaleTransform.ScaleYProperty, null);
			}
		}

		private void _sender_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (_isDragging)
			{
				_startPoint.Y -= e.PreviousSize.Height - e.NewSize.Height;
				DragMoved();
			}
		}

		private bool _dragCopy;

		public bool DragCopy
		{
			set
			{
				if (_dragCopy != value)
				{
					_dragCopy = value;

					if (value)
					{
						if (Settings.AnimationsEnabled)
							new AnimationHelpers.Fade(_originalElement, AnimationHelpers.FadeDirection.In, false, 0, 1, true);
						else
							_originalElement.Opacity = 1;
					}
					else
					{
						if (Settings.AnimationsEnabled)
							new AnimationHelpers.Fade(_originalElement, AnimationHelpers.FadeDirection.Out, false, 0, 1, true);
						else
							_originalElement.Opacity = 0;
					}
				}
			}
		}

		private void DragStarted()
		{
			_isDragging = true;
			_originalElement.Opacity = 1;
			_overlayElement = new DragDropImage(_originalElement);
			AdornerLayer layer = AdornerLayer.GetAdornerLayer(this);//Application.Current.MainWindow.Content as Grid);
			layer.Add(_overlayElement);
		}

		private void DragMoved()
		{
			Point CurrentPosition = Mouse.GetPosition(this);

			_overlayElement.LeftOffset = CurrentPosition.X - _startPoint.X;
			_overlayElement.TopOffset = CurrentPosition.Y - _startPoint.Y;

			//
			// Scroll the tree view
			//
			ScrollViewer tvs = Template.FindName("_tv_scrollviewer_", this) as ScrollViewer;
			double y = CurrentPosition.Y;
			double h = RenderSize.Height;

			if (y > h)
				tvs.ScrollToVerticalOffset(tvs.VerticalOffset + 10);
			else if (y > h - 10)
				tvs.ScrollToVerticalOffset(tvs.VerticalOffset + 5);
			else if (y > h - 20)
				tvs.ScrollToVerticalOffset(tvs.VerticalOffset + 1);
			else if (y < 0)
				tvs.ScrollToVerticalOffset(tvs.VerticalOffset - 10);
			else if (y < 10)
				tvs.ScrollToVerticalOffset(tvs.VerticalOffset - 5);
			else if (y < 20)
				tvs.ScrollToVerticalOffset(tvs.VerticalOffset - 1);

			//
			// Expand an item
			//
			foreach (TreeViewItem each in Items)
			{
				if (!each.IsExpanded)
				{
					Point pos = Mouse.GetPosition(each);

					if (pos.Y >= 0 && pos.Y <= each.ActualHeight)
					{
						if (expandTimer != null)
							expandTimer.Stop();
						else
						{
							expandTimer = new DispatcherTimer();
							expandTimer.Interval = SystemParameters.MouseHoverTime;
							expandTimer.Tick += expandTimer_Tick;
						}

						expandTimer.Tag = each;
						expandTimer.Start();

						break;
					}
				}
			}
		}

		private DispatcherTimer expandTimer;

		private void expandTimer_Tick(object sender, EventArgs e)
		{
			TreeViewItem each = (sender as DispatcherTimer).Tag as TreeViewItem;
			(sender as DispatcherTimer).Stop();

			Point pos = Mouse.GetPosition(each);

			if (pos.Y >= 0 && pos.Y <= each.ActualHeight)
			{
				if (Items.IndexOf(each) < Items.IndexOf(_originalElement.Parent))
				{
					each.SizeChanged -= _sender_SizeChanged;
					each.SizeChanged += _sender_SizeChanged;
				}

				each.IsExpanded = true;
			}
		}

		public void DragFinished(bool cancelled)
		{
			if (_isDragging)
			{
				if (cancelled)
				{
					_isDown = false;
					_isDragging = false;
					SlideBack();
				}
				else
				{
					Mouse.Capture(null);

					FrameworkElement directlyOver = Mouse.DirectlyOver as FrameworkElement;

					while (!(directlyOver is TreeViewItem) && !(directlyOver is TreeView))
					{
						if (directlyOver == null)
						{
							_isDragging = false;
							_isDown = false;
							_dragCopy = false;
							SlideBack();
							return;
						}

						directlyOver = directlyOver.TemplatedParent as FrameworkElement;
					}

					TreeViewItem newParent;
					TreeViewItem oldParent = _originalElement.Parent as TreeViewItem;
					int oldIndex = oldParent.Items.IndexOf(_originalElement);

					if (directlyOver is TreeViewItem)
					{
						newParent = directlyOver as TreeViewItem;
						int index;

						if (newParent.Parent is TreeViewItem)
						{
							newParent = newParent.Parent as TreeViewItem;
							index = newParent.Items.IndexOf(directlyOver);
						}
						else
						{
							index = 0;
						}

						//
						// Prevent items from being dragged between tree views.
						//
						if (newParent.Parent != oldParent.Parent)
						{
							DragFinished(true);
							return;
						}

						if (!_dragCopy)
						{
							oldParent.Items.Remove(_originalElement);
							newParent.Items.Insert(index, _originalElement);
							_originalElement.IsSelected = true;
							UpdateLayout();
						}
						else
						{
							//
							// Hard-coded functionality only for Task objects.
							//
							UserTask copy = new UserTask(_originalElement.Header as UserTask);
							copy.ID = IDGenerator.GenerateID();

							//
							// Copy the details file if it exists
							//
							if (File.Exists(TaskDatabase.TasksAppData + "\\" + (_originalElement.Header as UserTask).ID))
							{
								File.Copy(TaskDatabase.TasksAppData + "\\" + (_originalElement.Header as UserTask).ID,
									TaskDatabase.TasksAppData + "\\" + copy.ID);
							}

							_originalElement = new TreeViewItem();
							_originalElement.Header = copy;

							newParent.Items.Insert(index, _originalElement);
							_originalElement.IsSelected = true;

							// Force a layout update, to ensure that all elements are
							// in their correct locations.
							UpdateLayout();

							AdornerLayer layer = AdornerLayer.GetAdornerLayer(this);
							layer.Remove(_overlayElement);
							_overlayElement = new DragDropImage(_originalElement);
							layer.Add(_overlayElement);
						}
					}
					else
					{
						newParent = Items[Items.Count - 1] as TreeViewItem;

						//
						// Prevent items from being dragged between tree views.
						//
						if (newParent.Parent != oldParent.Parent)
						{
							DragFinished(true);
							return;
						}

						if (!_dragCopy)
						{
							oldParent.Items.Remove(_originalElement);
							newParent.Items.Add(_originalElement);
							_originalElement.IsSelected = true;
							UpdateLayout();
						}
						else
						{
							//
							// Hard-coded functionality only for Task objects.
							//
							UserTask copy = new UserTask(_originalElement.Header as UserTask);
							copy.ID = IDGenerator.GenerateID();

							//
							// Copy the details file if it exists
							//
							if (File.Exists(TaskDatabase.TasksAppData + "\\" + (_originalElement.Header as UserTask).ID))
							{
								File.Copy(TaskDatabase.TasksAppData + "\\" + (_originalElement.Header as UserTask).ID,
									TaskDatabase.TasksAppData + "\\" + copy.ID);
							}

							_originalElement = new TreeViewItem();
							_originalElement.Opacity = 1;
							_originalElement.Header = copy;

							newParent.Items.Add(_originalElement);
							_originalElement.IsSelected = true;

							// Force a layout update, to ensure that all elements are
							// in their correct locations.
							UpdateLayout();

							AdornerLayer layer = AdornerLayer.GetAdornerLayer(this);
							layer.Remove(_overlayElement);
							_overlayElement = new DragDropImage(_originalElement);
							layer.Add(_overlayElement);
						}
					}

					newParent.IsExpanded = true;

					Point mse = Mouse.GetPosition(_originalElement);
					_overlayElement.LeftOffset = mse.X - _dragOffset.X;
					_overlayElement.TopOffset = mse.Y - _dragOffset.Y;

					DragDirection direction;

					if (newParent == oldParent)
						direction = newParent.Items.IndexOf(_originalElement) < oldIndex ? DragDirection.Up : DragDirection.Down;
					else
						direction = Items.IndexOf(newParent) < Items.IndexOf(oldParent) ? DragDirection.Up : DragDirection.Down;

					if (!(newParent == oldParent && newParent.Items.IndexOf(_originalElement) == oldIndex))
						OnItemReorder(new ItemReorderEventArgs(_originalElement, oldParent, newParent, _dragCopy, direction));

					SlideBack();
				}
			}
			else
			{
				_dragCopy = false;
				Mouse.Capture(null);
			}

			_isDragging = false;
			_isDown = false;
		}

		private void SlideBack()
		{
			_isAnimating = true;

			if (Settings.AnimationsEnabled)
			{
				DispatcherTimer timer = new DispatcherTimer();
				timer.Interval = TimeSpan.FromMilliseconds(10);
				timer.Tick += timer_Tick;
				timer.Start();
			}
			else
			{
				AdornerLayer.GetAdornerLayer(this).Remove(_overlayElement);//Application.Current.MainWindow.Content as Grid).Remove(_overlayElement);
				_originalElement.ApplyAnimationClock(FrameworkElement.OpacityProperty, null);
				_originalElement.Opacity = 1;
				_overlayElement = null;
				_isAnimating = false;
				_dragCopy = false;

				Mouse.Capture(null);
			}
		}

		private void timer_Tick(object sender, EventArgs e)
		{
			if (_overlayElement != null)
			{
				if (Math.Abs(_overlayElement.LeftOffset) > 0.1
					|| Math.Abs(_overlayElement.TopOffset) > 0.1)
				{
					_overlayElement.LeftOffset -= _overlayElement.LeftOffset / 6;
					_overlayElement.TopOffset -= _overlayElement.TopOffset / 6;
				}
				else
				{
					AdornerLayer.GetAdornerLayer(this).Remove(_overlayElement);//Application.Current.MainWindow.Content as Grid).Remove(_overlayElement);
					_originalElement.ApplyAnimationClock(FrameworkElement.OpacityProperty, null);
					_originalElement.Opacity = 1;
					_overlayElement = null;
					_isAnimating = false;
					_dragCopy = false;

					Mouse.Capture(null);

					(sender as DispatcherTimer).Stop();
				}
			}
		}

		private void _tv_scrollviewer__ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			if (_isDragging)
			{
				_startPoint.Y -= e.VerticalChange;
			}
		}

		#endregion

		#region Events

		public delegate void ItemReorderEvent(object sender, ItemReorderEventArgs e);

		public event ItemReorderEvent ItemReorder;

		protected void OnItemReorder(ItemReorderEventArgs e)
		{
			if (ItemReorder != null)
				ItemReorder(this, e);
		}

		#endregion
	}

	public class ItemReorderEventArgs : EventArgs
	{
		public ItemReorderEventArgs(TreeViewItem item, TreeViewItem oldParent, TreeViewItem newParent,
			bool copied, DragDirection dragDirection)
		{
			_item = item;
			_oldParent = oldParent;
			_newParent = newParent;
			_copied = copied;
			_dragDirection = dragDirection;
		}

		private TreeViewItem _oldParent;
		private TreeViewItem _newParent;
		private TreeViewItem _item;
		private bool _copied;
		private DragDirection _dragDirection;

		public TreeViewItem OldParent
		{
			get { return _oldParent; }
		}

		public TreeViewItem NewParent
		{
			get { return _newParent; }
		}

		public TreeViewItem Item
		{
			get { return _item; }
		}

		public bool Copied
		{
			get { return _copied; }
		}

		public DragDirection DragDirection
		{
			get { return _dragDirection; }
		}
	}

	public enum DragDirection : byte { Up, Down };
}
