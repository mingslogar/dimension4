using System.ComponentModel;
using System.Windows.Media;

namespace Daytimer.Controls.Ribbon.QAT
{
	public class QatItem
	{
		public QatItem()
		{
		}

		public QatItem(object instance, bool isSplitHeader)
		{
			Instance = instance;
			IsSplitHeader = isSplitHeader;
		}

		public int TabIndex { get; set; }
		public int GroupIndex { get; set; }

		public Int32Collection ControlIndices
		{
			get
			{
				if (_controlIndices == null)
				{
					_controlIndices = new Int32Collection();
				}
				return _controlIndices;
			}
			set
			{
				_controlIndices = value;
			}
		}
		Int32Collection _controlIndices;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public object Instance { get; set; }

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsSplitHeader { get; set; }
	}
}
