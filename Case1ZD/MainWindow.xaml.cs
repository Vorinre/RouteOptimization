using BestDelivery;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using DisplayPoint = System.Windows.Point;
using GeoPoint = BestDelivery.Point;

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
        }

        private void OptionOneTrigger(object sender, RoutedEventArgs e) => HandleDeliveryScenario(OrderArrays.GetOrderArray1, "Центр города");
        private void OptionTwoTrigger(object sender, RoutedEventArgs e) => HandleDeliveryScenario(OrderArrays.GetOrderArray2, "Окраины");
        private void OptionThreeTrigger(object sender, RoutedEventArgs e) => HandleDeliveryScenario(OrderArrays.GetOrderArray3, "Один район");
        private void OptionFourTrigger(object sender, RoutedEventArgs e) => HandleDeliveryScenario(OrderArrays.GetOrderArray4, "Разные районы");
        private void OptionFiveTrigger(object sender, RoutedEventArgs e) => HandleDeliveryScenario(OrderArrays.GetOrderArray5, "Разные приоритеты");
        private void OptionSixTrigger(object sender, RoutedEventArgs e) => HandleDeliveryScenario(OrderArrays.GetOrderArray6, "Много заказов");

        private void HandleDeliveryScenario(Func<Order[]> fetchOrders, string description)
        {
            activeParcels = fetchOrders();
            hubLocation = activeParcels.First(o => o.ID == -1).Destination;
            deliveryOrder = CreateOptimizedRoute(activeParcels, hubLocation);
            RefreshParcelList();
            UpdateRouteInfo();
        }

        private void RefreshParcelList()
        {
            ParcelList.Items.Clear();
            foreach (var order in activeParcels)
            {
                ParcelList.Items.Add(order.ID == -1
                    ? $"СКЛАД: ({order.Destination.X:F2}, {order.Destination.Y:F2})"
                    : $"Заказ #{order.ID}: ({order.Destination.X:F2}, {order.Destination.Y:F2}), Приоритет: {order.Priority:F2}");
            }
        }

        private void UpdateRouteInfo()
        {
            if (ConfirmRouteValidity(hubLocation, activeParcels, deliveryOrder, out double routeLength))
            {
                DistanceInfo.Text = $"Расстояние: {routeLength:F2} усл. ед.";
                RouteSequence.Text = "Маршрут: " + string.Join(" → ", deliveryOrder.Select(id => id == -1 ? "СКЛАД" : "#" + id));
                DrawRoute();
            }
            else
            {
                DistanceInfo.Text = "Маршрут некорректен";
                RouteSequence.Text = "";
                RouteCanvas.Children.Clear();
            }
        }

        private void DrawRoute()
        {
            RouteCanvas.Children.Clear();
            var positions = new Dictionary<int, DisplayPoint>();

            double margin = 50;
            double width = RouteCanvas.ActualWidth > 0 ? RouteCanvas.ActualWidth : 800;
            double height = RouteCanvas.ActualHeight > 0 ? RouteCanvas.ActualHeight : 600;

            var points = activeParcels.Select(p => p.Destination).ToList();
            double minX = points.Min(p => p.X);
            double maxX = points.Max(p => p.X);
            double minY = points.Min(p => p.Y);
            double maxY = points.Max(p => p.Y);

            double scaleX = (width - 2 * margin) / (maxX - minX);
            double scaleY = (height - 2 * margin) / (maxY - minY);
            double scale = Math.Min(scaleX, scaleY);

            double shiftX = margin - minX * scale;
            double shiftY = height - margin + minY * scale;

            DisplayPoint Map(GeoPoint p) => new DisplayPoint(p.X * scale + shiftX, shiftY - p.Y * scale);

            foreach (var order in activeParcels)
            {
                var point = Map(order.Destination);
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
                RouteCanvas.Children.Add(marker);
            }

            for (int i = 0; i < deliveryOrder.Length - 1; i++)
            {
                var from = positions[deliveryOrder[i]];
                var to = positions[deliveryOrder[i + 1]];
                var line = new Line
                {
                    X1 = from.X,
                    Y1 = from.Y,
                    X2 = to.X,
                    Y2 = to.Y,
                    Stroke = Brushes.DarkOliveGreen,
                    StrokeThickness = 2
                };
                RouteCanvas.Children.Add(line);
            }
        }

        private void RouteCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(RouteCanvas);

            double margin = 50;
            double width = RouteCanvas.ActualWidth > 0 ? RouteCanvas.ActualWidth : 800;
            double height = RouteCanvas.ActualHeight > 0 ? RouteCanvas.ActualHeight : 600;

            var points = activeParcels.Select(p => p.Destination).ToList();
            double minX = points.Min(p => p.X);
            double maxX = points.Max(p => p.X);
            double minY = points.Min(p => p.Y);
            double maxY = points.Max(p => p.Y);

            double scaleX = (width - 2 * margin) / (maxX - minX);
            double scaleY = (height - 2 * margin) / (maxY - minY);
            double scale = Math.Min(scaleX, scaleY);

            double shiftX = margin - minX * scale;
            double shiftY = height - margin + minY * scale;

            double x = (pos.X - shiftX) / scale;
            double y = (shiftY - pos.Y) / scale;

            var dialog = new PriorityInputDialog();
            if (dialog.ShowDialog() == true)
            {
                var newId = activeParcels.Max(o => o.ID) + 1;
                var newOrder = new Order
                {
                    ID = newId,
                    Destination = new GeoPoint { X = x, Y = y },
                    Priority = dialog.Priority
                };

                var list = activeParcels.ToList();
                list.Add(newOrder);
                activeParcels = list.ToArray();

                deliveryOrder = CreateOptimizedRoute(activeParcels, hubLocation);
                RefreshParcelList();
                UpdateRouteInfo();
            }
        }

        public static int[] CreateOptimizedRoute(Order[] parcels, GeoPoint hub)
        {
            var validOrders = parcels.Where(order => order.ID != -1).ToArray();
            if (validOrders.Length == 0)
                return new[] { -1, -1 };

            var points = new List<GeoPoint> { hub };
            points.AddRange(validOrders.Select(o => o.Destination));

            double[,] distMatrix = CalculateDistanceMatrix(points.ToArray());
            List<int> routeIndices = ChristofidesTSP(distMatrix);

            return ConvertRouteToOrderIds(routeIndices, validOrders);
        }

        private static List<int> ChristofidesTSP(double[,] dist)
        {
            int n = dist.GetLength(0);
            List<int>[] mst = PrimMST(dist);
            List<int> oddVertices = GetOddDegreeVertices(mst, n);
            List<(int, int)> matching = MinWeightMatching(oddVertices, dist);
            List<int>[] multigraph = CombineMSTAndMatching(mst, matching, n);
            List<int> eulerianCircuit = FindEulerianCircuit(multigraph);
            return MakeHamiltonian(eulerianCircuit);
        }

        private static List<int>[] PrimMST(double[,] dist)
        {
            int n = dist.GetLength(0);
            var mst = new List<int>[n];
            for (int i = 0; i < n; i++) mst[i] = new List<int>();

            bool[] inMST = new bool[n];
            double[] key = new double[n];
            int[] parent = new int[n];

            for (int i = 0; i < n; i++)
            {
                key[i] = double.MaxValue;
                inMST[i] = false;
            }

            key[0] = 0;
            parent[0] = -1;

            for (int count = 0; count < n - 1; count++)
            {
                int u = -1;
                double min = double.MaxValue;
                for (int v = 0; v < n; v++)
                {
                    if (!inMST[v] && key[v] < min)
                    {
                        min = key[v];
                        u = v;
                    }
                }

                if (u == -1) break;
                inMST[u] = true;

                for (int v = 0; v < n; v++)
                {
                    if (dist[u, v] > 0 && !inMST[v] && dist[u, v] < key[v])
                    {
                        parent[v] = u;
                        key[v] = dist[u, v];
                    }
                }
            }

            for (int i = 1; i < n; i++)
            {
                mst[parent[i]].Add(i);
                mst[i].Add(parent[i]);
            }

            return mst;
        }

        private static List<int> GetOddDegreeVertices(List<int>[] mst, int n)
        {
            var oddVertices = new List<int>();
            for (int i = 0; i < n; i++)
            {
                if (mst[i].Count % 2 != 0)
                    oddVertices.Add(i);
            }
            return oddVertices;
        }

        private static List<(int, int)> MinWeightMatching(List<int> oddVertices, double[,] dist)
        {
            var matching = new List<(int, int)>();
            var remaining = new List<int>(oddVertices);

            while (remaining.Count > 0)
            {
                int u = remaining[0];
                int bestV = -1;
                double minDist = double.MaxValue;

                for (int i = 1; i < remaining.Count; i++)
                {
                    int v = remaining[i];
                    if (dist[u, v] < minDist)
                    {
                        minDist = dist[u, v];
                        bestV = v;
                    }
                }

                if (bestV != -1)
                {
                    matching.Add((u, bestV));
                    remaining.Remove(u);
                    remaining.Remove(bestV);
                }
                else
                {
                    remaining.Remove(u);
                }
            }

            return matching;
        }

        private static List<int>[] CombineMSTAndMatching(List<int>[] mst, List<(int, int)> matching, int n)
        {
            var multigraph = new List<int>[n];
            for (int i = 0; i < n; i++) multigraph[i] = new List<int>(mst[i]);

            foreach (var (u, v) in matching)
            {
                multigraph[u].Add(v);
                multigraph[v].Add(u);
            }

            return multigraph;
        }

        private static List<int> FindEulerianCircuit(List<int>[] multigraph)
        {
            var circuit = new List<int>();
            var stack = new Stack<int>();
            stack.Push(0);

            while (stack.Count > 0)
            {
                int current = stack.Peek();
                if (multigraph[current].Count > 0)
                {
                    int next = multigraph[current][0];
                    stack.Push(next);
                    multigraph[current].Remove(next);
                    multigraph[next].Remove(current);
                }
                else
                {
                    circuit.Add(stack.Pop());
                }
            }

            circuit.Reverse();
            return circuit;
        }

        private static List<int> MakeHamiltonian(List<int> eulerCircuit)
        {
            var visited = new HashSet<int>();
            var path = new List<int>();

            foreach (var vertex in eulerCircuit)
            {
                if (!visited.Contains(vertex))
                {
                    visited.Add(vertex);
                    path.Add(vertex);
                }
            }

            path.Add(path[0]);
            return path;
        }

        private static double[,] CalculateDistanceMatrix(GeoPoint[] points)
        {
            int n = points.Length;
            var matrix = new double[n, n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i == j) continue;

                    double dx = points[i].X - points[j].X;
                    double dy = points[i].Y - points[j].Y;
                    matrix[i, j] = Math.Sqrt(dx * dx + dy * dy);
                }
            }

            return matrix;
        }

        private static int[] ConvertRouteToOrderIds(List<int> route, Order[] orders)
        {
            var result = new int[route.Count];
            result[0] = -1;
            result[^1] = -1;

            for (int i = 1; i < route.Count - 1; i++)
                result[i] = orders[route[i] - 1].ID;
            return result;
        }

        public static bool ConfirmRouteValidity(GeoPoint hub, Order[] parcels, int[] route, out double routeLength)
        {
            routeLength = 0;
            if (parcels == null || route == null || parcels.Length == 0 || route.Length == 0) return false;

            var routeList = new List<int>(route);
            if (routeList.First() != -1 || routeList.Last() != -1) return false;

            var allIds = parcels.Where(p => p.ID != -1).Select(p => p.ID).ToHashSet();
            var visited = routeList.Where(id => id != -1).ToHashSet();
            if (!allIds.SetEquals(visited)) return false;

            GeoPoint current = hub;
            foreach (var id in routeList.Skip(1))
            {
                var o = parcels.First(p => p.ID == id);
                routeLength += Math.Sqrt(Math.Pow(current.X - o.Destination.X, 2) + Math.Pow(current.Y - o.Destination.Y, 2));
                current = o.Destination;
            }
            return true;
        }

        public class PriorityInputDialog : Window
        {
            public double Priority { get; private set; } = 0.5;
            private TextBox priorityTextBox;

            public PriorityInputDialog()
            {
                Title = "Введите приоритет заказа";
                Width = 300;
                Height = 150;
                WindowStartupLocation = WindowStartupLocation.CenterOwner;
                ResizeMode = ResizeMode.NoResize;
                Background = Brushes.DarkGray;

                var stackPanel = new StackPanel { Margin = new Thickness(10) };
                var label = new Label { Content = "Приоритет заказа (0,0 - 1.0):" };
                priorityTextBox = new TextBox { Text = "0,5" };

                var buttonPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 10, 0, 0)
                };

                var okButton = new Button { Content = "Да", Width = 80, Margin = new Thickness(5) };
                okButton.Click += OkButton_Click;

                var cancelButton = new Button { Content = "Отмена", Width = 80, Margin = new Thickness(5) };
                cancelButton.Click += (s, e) => { DialogResult = false; Close(); };

                buttonPanel.Children.Add(okButton);
                buttonPanel.Children.Add(cancelButton);

                stackPanel.Children.Add(label);
                stackPanel.Children.Add(priorityTextBox);
                stackPanel.Children.Add(buttonPanel);

                Content = stackPanel;
            }

            private void OkButton_Click(object sender, RoutedEventArgs e)
            {
                if (double.TryParse(priorityTextBox.Text, out double priority) && priority >= 0 && priority <= 1)
                {
                    Priority = priority;
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Введите число от 0,0 до 1.0", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                    priorityTextBox.Focus();
                }
            }
        }
    }
}