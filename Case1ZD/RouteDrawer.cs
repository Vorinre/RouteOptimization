using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Case1ZD
{
    public static class RouteDrawer
    {
        public static void Draw(Canvas canvas, IEnumerable<Order> orders, IEnumerable<int> route)
        {
            canvas.Children.Clear();

            var converter = new CanvasPointConverter(canvas, orders.Select(o => o.Destination));
            var positions = new Dictionary<int, Point>();

            // Рисуем маркеры
            foreach (var order in orders)
            {
                var point = converter.Convert(order.Destination);
                positions[order.ID] = point;

                var marker = new Ellipse
                {
                    Width = 12,
                    Height = 12,
                    Fill = order.ID == -1 ? Brushes.Red : Brushes.Yellow,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1.5
                };

                Canvas.SetLeft(marker, point.X - 6);
                Canvas.SetTop(marker, point.Y - 6);
                canvas.Children.Add(marker);
            }

            // Рисуем линии маршрута
            var routeArray = route.ToArray();
            for (int i = 0; i < routeArray.Length - 1; i++)
            {
                if (positions.TryGetValue(routeArray[i], out var from) &&
                    positions.TryGetValue(routeArray[i + 1], out var to))
                {
                    var line = new Line
                    {
                        X1 = from.X,
                        Y1 = from.Y,
                        X2 = to.X,
                        Y2 = to.Y,
                        Stroke = Brushes.DarkOliveGreen,
                        StrokeThickness = 2
                    };
                    canvas.Children.Add(line);
                }
            }
        }
    }
}