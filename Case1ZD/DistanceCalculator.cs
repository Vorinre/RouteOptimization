using System;
using System.Security.Cryptography;

namespace Case1ZD
{
    public static class DistanceCalculator
    {
        public static double[,] CalculateDistanceMatrix(GeoPoint[] points)
        {
            int n = points.Length;
            var matrix = new double[n, n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i == j) continue;
                    matrix[i, j] = CalculateDistance(points[i], points[j]);
                }
            }
            return matrix;
        }

        public static double CalculateDistance(GeoPoint a, GeoPoint b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}