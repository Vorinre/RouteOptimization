using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Case1ZD
{
    public class PointConverter
    {
        private readonly Canvas canvas;
        private readonly List<GeoPoint> points;
        private readonly double margin = 50;

        public PointConverter(Canvas canvas, IEnumerable<Order> orders)
        {
            this.canvas = canvas;
            this.points = orders.Select(o => o.Destination).ToList();
        }

        public Point Convert(GeoPoint p)
        {
            double width = canvas.ActualWidth > 0 ? canvas.ActualWidth : 800;
            double height = canvas.ActualHeight > 0 ? canvas.ActualHeight : 600;

            double minX = points.Min(p => p.X);
            double maxX = points.Max(p => p.X);
            double minY = points.Min(p => p.Y);
            double maxY = points.Max(p => p.Y);

            double scaleX = (width - 2 * margin) / (maxX - minX);
            double scaleY = (height - 2 * margin) / (maxY - minY);
            double scale = Math.Min(scaleX, scaleY);

            double shiftX = margin - minX * scale;
            double shiftY = height - margin + minY * scale;

            return new Point(p.X * scale + shiftX, shiftY - p.Y * scale);
        }

        public GeoPoint ConvertToGeoPoint(Point displayPoint)
        {
            double width = canvas.ActualWidth > 0 ? canvas.ActualWidth : 800;
            double height = canvas.ActualHeight > 0 ? canvas.ActualHeight : 600;

            double minX = points.Min(p => p.X);
            double maxX = points.Max(p => p.X);
            double minY = points.Min(p => p.Y);
            double maxY = points.Max(p => p.Y);

            double scaleX = (width - 2 * margin) / (maxX - minX);
            double scaleY = (height - 2 * margin) / (maxY - minY);
            double scale = Math.Min(scaleX, scaleY);

            double shiftX = margin - minX * scale;
            double shiftY = height - margin + minY * scale;

            double x = (displayPoint.X - shiftX) / scale;
            double y = (shiftY - displayPoint.Y) / scale;

            return new GeoPoint { X = x, Y = y };
        }
    }
}