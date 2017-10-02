using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace Modern.FileBrowser
{
	class PathData : INotifyPropertyChanged
	{
		public PathData()
		{

		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}

		private double _strokeThickness;
		private Brush _stroke;
		private Brush _fill;
		private Geometry _data;
		private Thickness _margin;

		public double StrokeThickness
		{
			get { return _strokeThickness; }
			set
			{
				_strokeThickness = value;
				OnPropertyChanged("StrokeThickness");
			}
		}

		public Brush Stroke
		{
			get { return _stroke; }
			set
			{
				_stroke = value;
				OnPropertyChanged("Stroke");
			}
		}

		public Brush Fill
		{
			get { return _fill; }
			set
			{
				_fill = value;
				OnPropertyChanged("Fill");
			}
		}

		public Geometry Data
		{
			get { return _data; }
			set
			{
				_data = value;
				OnPropertyChanged("Data");
			}
		}

		public Thickness Margin
		{
			get { return _margin; }
			set
			{
				_margin = value;
				OnPropertyChanged("Margin");
			}
		}
	}
}
