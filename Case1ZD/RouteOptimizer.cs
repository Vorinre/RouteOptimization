using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
namespace Case1ZD
{
    public static class RouteOptimizer
    {
        public static int[] CreateOptimizedRoute(Order[] parcels, GeoPoint hub)
        {
            var validOrders = parcels.Where(order => order.ID != -1).ToArray();
            if (validOrders.Length == 0)
                return new[] { -1, -1 };

            var points = new List<GeoPoint> { hub };
            points.AddRange(validOrders.Select(o => o.Destination));

            double[,] distMatrix = DistanceCalculator.CalculateDistanceMatrix(points.ToArray());
            List<int> routeIndices = ChristofidesAlgorithm.Solve(distMatrix);

            return ConvertRouteToOrderIds(routeIndices, validOrders);
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
    }
}