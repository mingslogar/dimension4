using System;
using System.Windows;

namespace Daytimer.Functions
{
	public static class MathFunctions
	{
		/// <summary>
		/// Calculates the distance between two <see cref="Point"/> objects.
		/// </summary>
		/// <param name="p1"></param>
		/// <param name="p2"></param>
		/// <returns></returns>
		public static double Distance(Point p1, Point p2)
		{
			return Distance(p1.X, p1.Y, p2.X, p2.Y);
		}

		/// <summary>
		/// Calculates the distance between two points.
		/// </summary>
		/// <returns></returns>
		public static double Distance(double x1, double y1, double x2, double y2)
		{
			// Distance theorem
			return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
		}

		/// <summary>
		/// Calculates the distance between a <see cref="Point"/> and a <see cref="LineSegment"/>.
		/// </summary>
		/// <param name="point"></param>
		/// <param name="line"></param>
		/// <returns></returns>
		public static double Distance(Point point, LineSegment line)
		{
			double A = point.X - line.Point1.X;
			double B = point.Y - line.Point1.Y;
			double C = line.Point2.X - line.Point1.X;
			double D = line.Point2.Y - line.Point1.Y;

			double dot = A * C + B * D;
			double len_sq = C * C + D * D;
			double param = dot / len_sq;

			double xx, yy;

			if (param < 0 || (line.Point1.X == line.Point2.X && line.Point1.Y == line.Point2.Y))
			{
				xx = line.Point1.X;
				yy = line.Point1.Y;
			}
			else if (param > 1)
			{
				xx = line.Point2.X;
				yy = line.Point2.Y;
			}
			else
			{
				xx = line.Point1.X + param * C;
				yy = line.Point1.Y + param * D;
			}

			double dx = point.X - xx;
			double dy = point.Y - yy;

			return Math.Sqrt(dx * dx + dy * dy);
		}

		/// <summary>
		/// Calculates the shortest distance between a <see cref="Point"/> and any
		/// side of a <see cref="Rect"/>.
		/// </summary>
		/// <param name="point"></param>
		/// <param name="rect"></param>
		/// <returns></returns>
		public static double Distance(Point point, Rect rect)
		{
			double dx1 = point.X - rect.Left;
			double dx2 = point.X - rect.Right;
			double dy1 = point.Y - rect.Top;
			double dy2 = point.Y - rect.Bottom;

			if (dx1 * dx2 < 0)
			{
				// point.X is between rect.Left and rect.Right

				if (dy1 * dy2 < 0)
				{
					// (point.X,point.Y) is inside the rectangle
					return Math.Min(Math.Min(Math.Abs(dx1), Math.Abs(dx2)), Math.Min(Math.Abs(dy1), Math.Abs(dy2)));
				}
				return Math.Min(Math.Abs(dy1), Math.Abs(dy2));
			}

			if (dy1 * dy2 < 0)
			{
				// point.Y is between rect.Top and rect.Bottom

				// we don't have to test for being inside the rectangle, it's alreadpoint.Y tested.
				return Math.Min(Math.Abs(dx1), Math.Abs(dx2));
			}

			return Math.Min(Math.Min(Distance(point.X, point.Y, rect.Left, rect.Top),
				Distance(point.X, point.Y, rect.Right, rect.Bottom)),
				Math.Min(Distance(point.X, point.Y, rect.Left, rect.Bottom),
				Distance(point.X, point.Y, rect.Right, rect.Top)));
		}

		/// <summary>
		/// Calculate the area of a triangle denoted by the lengths
		/// of its sides (Heron's formula).
		/// </summary>
		/// <param name="side1"></param>
		/// <param name="side2"></param>
		/// <param name="side3"></param>
		/// <returns></returns>
		public static double Area(double side1, double side2, double side3)
		{
			double s = (side1 + side2 + side3) / 2;
			return Math.Sqrt(s * (s - side1) * (s - side2) * (s - side3));
		}

		/// <summary>
		/// Round a number to the specified amount of significant digits.
		/// </summary>
		/// <param name="d">The number to round.</param>
		/// <param name="digits">The number of significant digits.</param>
		/// <returns></returns>
		public static double RoundToSignificantDigits(this double d, int digits)
		{
			if (d == 0)
				return 0;

			double scale = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(d))) + 1);
			return scale * Math.Round(d / scale, digits);
		}
	}
}
