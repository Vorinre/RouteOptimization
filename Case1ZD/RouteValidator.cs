using System.Linq;

namespace Case1ZD
{
    public static class RouteValidator
    {
        public static bool ValidateRoute(GeoPoint warehouse, Order[] orders, int[] route, out double distance)
        {
            distance = 0;

            if (orders == null || route == null || route.Length < 2)
                return false;

            if (route[0] != -1 || route[^1] != -1)
                return false;

            var validIds = orders.Where(o => o.ID != -1).Select(o => o.ID).ToHashSet();
            var routeIds = route.Where(id => id != -1).ToHashSet();

            if (!validIds.SetEquals(routeIds))
                return false;

            distance = CalculateRouteDistance(warehouse, orders, route);
            return true;
        }

        private static double CalculateRouteDistance(GeoPoint start, Order[] orders, int[] route)
        {
            double total = 0;
            var current = start;

            foreach (var id in route.Skip(1))
            {
                var next = orders.First(o => o.ID == id).Destination;
                total += CalculateDistance(current, next);
                current = next;
            }

            return total;
        }

        private static double CalculateDistance(GeoPoint a, GeoPoint b)
        {
            var dx = a.X - b.X;
            var dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}