using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Case1ZD
{
    public partial class MainWindow : Window
    {
        private Order[] activeParcels = Array.Empty<Order>();
        private GeoPoint hubLocation;
        private int[] deliveryOrder = Array.Empty<int>();

        public MainWindow()
        {
            InitializeComponent();
            RouteCanvas.SizeChanged += RouteCanvas_SizeChanged;
        }

        // Обработчики кнопок сценариев
        private void OptionOneTrigger(object sender, RoutedEventArgs e) => LoadScenario(OrderArrays.GetOrderArray1);
        private void OptionTwoTrigger(object sender, RoutedEventArgs e) => LoadScenario(OrderArrays.GetOrderArray2);
        private void OptionThreeTrigger(object sender, RoutedEventArgs e) => LoadScenario(OrderArrays.GetOrderArray3);
        private void OptionFourTrigger(object sender, RoutedEventArgs e) => LoadScenario(OrderArrays.GetOrderArray4);
        private void OptionFiveTrigger(object sender, RoutedEventArgs e) => LoadScenario(OrderArrays.GetOrderArray5);
        private void OptionSixTrigger(object sender, RoutedEventArgs e) => LoadScenario(OrderArrays.GetOrderArray6);

        private void LoadScenario(Func<Order[]> getOrders)
        {
            try
            {
                activeParcels = getOrders();
                hubLocation = activeParcels.First(o => o.ID == -1).Destination;
                deliveryOrder = RouteOptimizer.CreateOptimizedRoute(activeParcels, hubLocation);
                UpdateUI();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки сценария: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateUI()
        {
            UpdateParcelList();
            UpdateRouteInfo();
            DrawRoute();
        }

        private void UpdateParcelList()
        {
            ParcelList.Items.Clear();
            foreach (var order in activeParcels)
            {
                ParcelList.Items.Add(order.ID == -1
                    ? $"СКЛАД: ({order.Destination.X:F1}, {order.Destination.Y:F1})"
                    : $"Заказ #{order.ID}: ({order.Destination.X:F1}, {order.Destination.Y:F1}), Приоритет: {order.Priority:F1}");
            }
        }

        private void UpdateRouteInfo()
        {
            if (RouteValidator.ValidateRoute(hubLocation, activeParcels, deliveryOrder, out double distance))
            {
                DistanceInfo.Text = $"Расстояние: {distance:F2} км";
                RouteSequence.Text = "Маршрут: " + string.Join(" → ",
                    deliveryOrder.Select(id => id == -1 ? "СКЛАД" : $"#{id}"));
            }
            else
            {
                DistanceInfo.Text = "Некорректный маршрут";
                RouteSequence.Text = string.Empty;
            }
        }

        private void DrawRoute()
        {
            RouteCanvas.Children.Clear();
            if (activeParcels.Length == 0) return;

            var converter = new CanvasPointConverter(RouteCanvas, activeParcels.Select(o => o.Destination));
            var positions = activeParcels.ToDictionary(
                o => o.ID,
                o => converter.Convert(o.Destination));

            // Сначала рисуем линии маршрута
            for (int i = 0; i < deliveryOrder.Length - 1; i++)
            {
                if (!positions.ContainsKey(deliveryOrder[i])) continue;
                if (!positions.ContainsKey(deliveryOrder[i + 1])) continue;

                var line = new Line
                {
                    X1 = positions[deliveryOrder[i]].X,
                    Y1 = positions[deliveryOrder[i]].Y,
                    X2 = positions[deliveryOrder[i + 1]].X,
                    Y2 = positions[deliveryOrder[i + 1]].Y,
                    Stroke = Brushes.LimeGreen,
                    StrokeThickness = 2,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap = PenLineCap.Round
                };
                RouteCanvas.Children.Add(line);
            }

            // Затем рисуем маркеры (чтобы они были поверх линий)
            foreach (var order in activeParcels)
            {
                if (!positions.ContainsKey(order.ID)) continue;

                var point = positions[order.ID];
                var marker = new Ellipse
                {
                    Width = 12,
                    Height = 12,
                    Fill = order.ID == -1 ? Brushes.Red : Brushes.Yellow,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1,
                    ToolTip = order.ID == -1
                        ? "Склад"
                        : $"Заказ #{order.ID}\n({order.Destination.X:F2}, {order.Destination.Y:F2})\nПриоритет: {order.Priority:F2}"
                };
                Canvas.SetLeft(marker, point.X - 6);
                Canvas.SetTop(marker, point.Y - 6);
                RouteCanvas.Children.Add(marker);
            }
        }

        private void RouteCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (activeParcels.Length == 0)
            {
                MessageBox.Show("Сначала загрузите сценарий", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var clickPos = e.GetPosition(RouteCanvas);
            var dialog = new PriorityInputDialog { Owner = this };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var currentPoints = activeParcels.Select(o => o.Destination).ToList();
                    var converter = new CanvasPointConverter(RouteCanvas, currentPoints);
                    var newGeoPoint = converter.ConvertBack(clickPos);

                    // Проверка минимального расстояния
                    foreach (var point in currentPoints)
                    {
                        if (CalculateDistance(point, newGeoPoint) < 0.5)
                        {
                            MessageBox.Show("Точка слишком близко к существующей", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }

                    var newOrder = new Order
                    {
                        ID = activeParcels.Max(o => o.ID) + 1,
                        Destination = newGeoPoint,
                        Priority = dialog.Priority
                    };

                    activeParcels = activeParcels.Append(newOrder).ToArray();
                    deliveryOrder = RouteOptimizer.CreateOptimizedRoute(activeParcels, hubLocation);
                    UpdateUI();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка добавления точки: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private double CalculateDistance(GeoPoint a, GeoPoint b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private void RouteCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (activeParcels.Length > 0)
            {
                DrawRoute();
            }
        }
    }
}