namespace Daytimer.Fundamentals
{
	public enum Location { Left = 0, Top, Right, Bottom, None };

	public struct PositionOrder
	{
		public PositionOrder(Location first, Location second, Location third, Location fourth, Location fallback)
		{
			_first = first;
			_second = second;
			_third = third;
			_fourth = fourth;
			_fallback = fallback;
		}

		private Location _first;
		private Location _second;
		private Location _third;
		private Location _fourth;
		private Location _fallback;

		public Location First
		{
			get { return _first; }
			set { _first = value; }
		}

		public Location Second
		{
			get { return _second; }
			set { _second = value; }
		}

		public Location Third
		{
			get { return _third; }
			set { _third = value; }
		}

		public Location Fourth
		{
			get { return _fourth; }
			set { _fourth = value; }
		}

		public Location Fallback
		{
			get { return _fallback; }
			set { _fallback = value; }
		}
	}
}
