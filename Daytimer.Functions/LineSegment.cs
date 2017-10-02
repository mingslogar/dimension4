using System.Windows;

namespace Daytimer.Functions
{
	public struct LineSegment
	{
		public LineSegment(Point point1, Point point2)
		{
			Point1 = point1;
			Point2 = point2;
		}

		public Point Point1;
		public Point Point2;

		/// <summary>
		/// Returns the point at which the two <see cref="LineSegment"/> objects intersect,
		/// or null if there is no intersection.
		/// <see cref="http://silverbling.blogspot.com/2010/06/2d-line-segment-intersection-detection.html"/>
		/// </summary>
		/// <param name="otherLineSegment"></param>
		/// <returns></returns>
		public Point? Intersects(LineSegment otherLineSegment)
		{
			double firstLineSlopeX = Point2.X - Point1.X;
			double firstLineSlopeY = Point2.Y - Point1.Y;

			double secondLineSlopeX = otherLineSegment.Point2.X - otherLineSegment.Point1.X;
			double secondLineSlopeY = otherLineSegment.Point2.Y - otherLineSegment.Point1.Y;

			double s = (-firstLineSlopeY * (Point1.X - otherLineSegment.Point1.X)
				+ firstLineSlopeX * (Point1.Y - otherLineSegment.Point1.Y))
				/ (-secondLineSlopeX * firstLineSlopeY + firstLineSlopeX * secondLineSlopeY);

			double t = (secondLineSlopeX * (Point1.Y - otherLineSegment.Point1.Y)
				- secondLineSlopeY * (Point1.X - otherLineSegment.Point1.X))
				/ (-secondLineSlopeX * firstLineSlopeY + firstLineSlopeX * secondLineSlopeY);

			if (s >= 0 && s <= 1 && t >= 0 && t <= 1)
			{
				double intersectionPointX = Point1.X + (t * firstLineSlopeX);
				double intersectionPointY = Point1.Y + (t * firstLineSlopeY);

				// Collision detected
				return new Point(intersectionPointX, intersectionPointY);
			}

			// No collision
			return null;
		}

		/// <summary>
		/// Gets if this <see cref="LineSegment"/> intersects with a <see cref="Point"/>.
		/// <see cref="http://forums.codeguru.com/showthread.php?419763-Check-a-Point-lies-in-a-Line-segment"/>
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		public bool Intersects(Point point)
		{
			return ((Point1.X == point.X || Point2.X == point.X) ? Point1.X == Point2.X
				: (point.Y - Point1.Y) / (point.X - Point1.X) == (point.Y - Point2.Y) / (point.X - Point2.X));
		}

		/// <summary>
		/// Gets the length of this <see cref="LineSegment"/>.
		/// </summary>
		public double Length
		{
			get { return MathFunctions.Distance(Point1, Point2); }
		}
	}
}
