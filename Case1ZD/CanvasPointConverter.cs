using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Case1ZD
{
    public class CanvasPointConverter
    {
        private readonly Canvas _canvas;
        private readonly List<GeoPoint> _points;
        private const double Margin = 50;

        private double _minX, _maxX, _minY, _maxY;
        private double _scale;
        private double _shiftX, _shiftY;

        public CanvasPointConverter(Canvas canvas, IEnumerable<GeoPoint> points)
        {
            _canvas = canvas;
            _points = points.ToList();
            CalculateConversionParameters();
        }

        private void CalculateConversionParameters()
        {
            _minX = _points.Min(p => p.X);
            _maxX = _points.Max(p => p.X);
            _minY = _points.Min(p => p.Y);
            _maxY = _points.Max(p => p.Y);

            double width = _canvas.ActualWidth > 0 ? _canvas.ActualWidth : 800;
            double height = _canvas.ActualHeight > 0 ? _canvas.ActualHeight : 600;

            double scaleX = (width - 2 * Margin) / (_maxX - _minX);
            double scaleY = (height - 2 * Margin) / (_maxY - _minY);
            _scale = Math.Min(scaleX, scaleY);

            _shiftX = Margin - _minX * _scale;
            _shiftY = height - Margin + _minY * _scale;
        }

        public Point Convert(GeoPoint geoPoint) => new(
            geoPoint.X * _scale + _shiftX,
            _shiftY - geoPoint.Y * _scale);

        public GeoPoint ConvertBack(Point canvasPoint) => new()
        {
            X = (canvasPoint.X - _shiftX) / _scale,
            Y = (_shiftY - canvasPoint.Y) / _scale
        };
    }
}