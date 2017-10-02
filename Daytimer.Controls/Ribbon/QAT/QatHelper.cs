using Daytimer.DatabaseHelpers;
using Microsoft.Windows.Controls.Ribbon;
using System.Collections;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;

namespace Daytimer.Controls.Ribbon.QAT
{
	public class QatHelper
	{
		public QatHelper(Microsoft.Windows.Controls.Ribbon.Ribbon ribbon)
		{
			_ribbon = ribbon;
		}

		private Microsoft.Windows.Controls.Ribbon.Ribbon _ribbon;
		private string _qatFileName = Static.LocalAppData + "\\QatLayout.xml";

		public void Load()
		{
			if (_ribbon.QuickAccessToolBar == null)
				_ribbon.QuickAccessToolBar = new RibbonQuickAccessToolBar();

			LoadQatItems(_ribbon.QuickAccessToolBar);
		}

		public void Save()
		{
			SaveQatItems(_ribbon.QuickAccessToolBar);
		}

		private void LoadQatItems(RibbonQuickAccessToolBar qat)
		{
			Dispatcher.CurrentDispatcher.BeginInvoke(() =>
			{
				try
				{
					if (qat != null)
					{
						if (File.Exists(_qatFileName))
						{
							XmlReader xmlReader = XmlReader.Create(_qatFileName);
							QatItemCollection qatItems = (QatItemCollection)XamlReader.Load(xmlReader);
							xmlReader.Close();

							if (qatItems != null)
							{
								int remainingItemsCount = qatItems.Count;
								QatItemCollection matchedItems = new QatItemCollection();

								if (qatItems.Count > 0)
								{
									for (int tabIndex = 0; tabIndex < _ribbon.Items.Count && remainingItemsCount > 0; tabIndex++)
									{
										matchedItems.Clear();

										for (int qatIndex = 0; qatIndex < qatItems.Count; qatIndex++)
										{
											QatItem qatItem = qatItems[qatIndex];

											if (qatItem.ControlIndices[0] == tabIndex)
												matchedItems.Add(qatItem);
										}

										RibbonTab tab = _ribbon.Items[tabIndex] as RibbonTab;

										if (tab != null)
											LoadQatItemsAmongChildren(matchedItems, 0, tabIndex, tab, ref remainingItemsCount);
									}

									for (int qatIndex = 0; qatIndex < qatItems.Count; qatIndex++)
									{
										QatItem qatItem = qatItems[qatIndex];
										Control control = qatItem.Instance as Control;

										if (control != null)
											if (RibbonCommands.AddToQuickAccessToolBarCommand.CanExecute(null, control))
												RibbonCommands.AddToQuickAccessToolBarCommand.Execute(null, control);
									}
								}
							}
						}
					}
				}
				catch { }
			});
		}

		private void LoadQatItemsAmongChildren(
					QatItemCollection previouslyMatchedItems,
					int matchLevel,
					int controlIndex,
					object parent,
					ref int remainingItemsCount)
		{
			if (previouslyMatchedItems.Count == 0)
				return;

			if (IsLeaf(parent))
				return;

			int childIndex = 0;
			DependencyObject dependencyObject = parent as DependencyObject;

			if (dependencyObject != null)
			{
				IEnumerable children = LogicalTreeHelper.GetChildren(dependencyObject);

				foreach (object child in children)
				{
					if (remainingItemsCount == 0)
						break;

					QatItemCollection matchedItems = new QatItemCollection();
					LoadQatItemIfMatchesControl(previouslyMatchedItems, matchedItems, matchLevel + 1, childIndex, child, ref remainingItemsCount);
					LoadQatItemsAmongChildren(matchedItems, matchLevel + 1, childIndex, child, ref remainingItemsCount);
					childIndex++;
				}
			}

			if (childIndex != 0)
				return;

			// if we failed to get any logical children, enumerate the visual ones
			Visual visual = parent as Visual;

			if (visual == null)
				return;

			for (childIndex = 0; childIndex < VisualTreeHelper.GetChildrenCount(visual); childIndex++)
			{
				if (remainingItemsCount == 0)
					break;

				Visual child = VisualTreeHelper.GetChild(visual, childIndex) as Visual;
				QatItemCollection matchedItems = new QatItemCollection();
				LoadQatItemIfMatchesControl(previouslyMatchedItems, matchedItems, matchLevel + 1, childIndex, child, ref remainingItemsCount);
				LoadQatItemsAmongChildren(matchedItems, matchLevel + 1, childIndex, child, ref remainingItemsCount);
			}
		}

		private void LoadQatItemIfMatchesControl(
					QatItemCollection previouslyMatchedItems,
					QatItemCollection matchedItems,
					int matchLevel,
					int controlIndex,
					object control,
					ref int remainingItemsCount)
		{
			for (int qatIndex = 0; qatIndex < previouslyMatchedItems.Count; qatIndex++)
			{
				QatItem qatItem = previouslyMatchedItems[qatIndex];

				if (qatItem.ControlIndices[matchLevel] == controlIndex)
				{
					if (qatItem.ControlIndices.Count == matchLevel + 1)
					{
						qatItem.Instance = control;
						remainingItemsCount--;
					}
					else if (qatItem.ControlIndices.Count == matchLevel + 2 && qatItem.ControlIndices[matchLevel + 1] == -1)
					{
						qatItem.IsSplitHeader = true;
						Control element = control as Control;

						if (element != null)
						{
							object splitControl = element.Template.FindName("PART_HeaderButton", element);

							if (splitControl == null)
							{
								element.ApplyTemplate();
								splitControl = element.Template.FindName("PART_HeaderButton", element);
							}

							if (splitControl != null)
								qatItem.Instance = splitControl;
						}

						remainingItemsCount--;
					}
					else
					{
						matchedItems.Add(qatItem);
					}
				}
			}
		}

		private void SaveQatItems(RibbonQuickAccessToolBar qat)
		{
			if (qat != null)
			{
				QatItemCollection qatItems = new QatItemCollection();
				QatItemCollection remainingItems = new QatItemCollection();

				if (qat.Items.Count > 0)
				{
					for (int qatIndex = 0; qatIndex < qat.Items.Count; qatIndex++)
					{
						object instance = qat.Items[qatIndex];
						bool isSplitHeader = false;

						if (instance is ICommandSource)
						{
							ICommandSource element = (ICommandSource)instance;

							if (element.Command != null)
							{
								isSplitHeader = instance is RibbonSplitMenuItem || instance is RibbonSplitButton;
								instance = element.Command;
							}
						}

						QatItem qatItem = new QatItem(instance, isSplitHeader);
						qatItems.Add(qatItem);
						remainingItems.Add(qatItem);
					}

					for (int tabIndex = 0; tabIndex < _ribbon.Items.Count && remainingItems.Count > 0; tabIndex++)
					{
						RibbonTab tab = _ribbon.Items[tabIndex] as RibbonTab;
						SaveQatItemsAmongChildren(remainingItems, tab, tabIndex);
					}
				}

				if (!Directory.Exists(Static.LocalAppData))
					Directory.CreateDirectory(Static.LocalAppData);

				XmlWriter xmlWriter = XmlWriter.Create(_qatFileName);
				XamlWriter.Save(qatItems, xmlWriter);
				xmlWriter.Close();
			}
		}

		private bool IsLeaf(object element)
		{
			if ((element is RibbonButton) ||
				(element is RibbonToggleButton) ||
				(element is RibbonRadioButton) ||
				(element is RibbonCheckBox) ||
				(element is RibbonTextBox) ||
				(element is RibbonSeparator))
				return true;

			RibbonMenuItem menuItem = element as RibbonMenuItem;
			if (menuItem != null &&
				menuItem.Items.Count == 0)
				return true;

			return false;
		}

		private void SaveQatItemsAmongChildrenInner(QatItemCollection remainingItems, object parent)
		{
			SaveQatItemsIfMatchesControl(remainingItems, parent);

			if (IsLeaf(parent))
				return;

			int childIndex = 0;
			DependencyObject dependencyObject = parent as DependencyObject;

			if (dependencyObject != null)
			{
				IEnumerable children = LogicalTreeHelper.GetChildren(dependencyObject);

				foreach (object child in children)
				{
					SaveQatItemsAmongChildren(remainingItems, child, childIndex);
					childIndex++;
				}
			}

			if (childIndex != 0)
				return;

			// if we failed to get any logical children, enumerate the visual ones
			Visual visual = parent as Visual;

			if (visual == null)
				return;

			for (childIndex = 0; childIndex < VisualTreeHelper.GetChildrenCount(visual); childIndex++)
			{
				Visual child = VisualTreeHelper.GetChild(visual, childIndex) as Visual;
				SaveQatItemsAmongChildren(remainingItems, child, childIndex);
			}
		}

		private void SaveQatItemsAmongChildren(QatItemCollection remainingItems, object control, int controlIndex)
		{
			if (control == null)
				return;

			for (int qatIndex = 0; qatIndex < remainingItems.Count; qatIndex++)
			{
				QatItem qatItem = remainingItems[qatIndex];
				qatItem.ControlIndices.Add(controlIndex);
			}

			SaveQatItemsAmongChildrenInner(remainingItems, control);

			for (int qatIndex = 0; qatIndex < remainingItems.Count; qatIndex++)
			{
				QatItem qatItem = remainingItems[qatIndex];
				int tail = qatItem.ControlIndices.Count - 1;
				qatItem.ControlIndices.RemoveAt(tail);
			}
		}

		private bool SaveQatItemsIfMatchesControl(QatItemCollection remainingItems, object control)
		{
			bool matched = false;
			ICommandSource element = control as ICommandSource;

			if (element != null)
			{
				object data = element.Command;

				if (data != null)
				{
					for (int qatIndex = 0; qatIndex < remainingItems.Count; qatIndex++)
					{
						QatItem qatItem = remainingItems[qatIndex];

						if (qatItem.Instance == data)
						{
							if (qatItem.IsSplitHeader)
							{
								// This is the case of the Header of a SplitButton 
								// or a SplitMenuItem added to the QAT. Note -1 is 
								// the sentinel being used to indicate this case.

								qatItem.ControlIndices.Add(-1);
							}

							remainingItems.Remove(qatItem);
							qatIndex--;
							matched = true;
						}
					}
				}
			}

			return matched;
		}

		#region public static methods

		public static void CheckQATButton(RibbonQuickAccessToolBar qatBar, object id, bool? isChecked)
		{
			DependencyObject obj = GetQATButton(qatBar, id);

			if (obj != null)
				(obj as ToggleButton).IsChecked = isChecked;
		}

		public static DependencyObject GetQATButton(RibbonQuickAccessToolBar qatBar, object id)
		{
			foreach (DependencyObject each in qatBar.Items)
			{
				if (RibbonControlService.GetQuickAccessToolBarId(each) == id)
					return each;
			}

			return null;
		}

		#endregion
	}
}
