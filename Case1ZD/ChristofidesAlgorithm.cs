using System;
using System.Collections.Generic;
using System.Linq;

namespace Case1ZD
{
    public static class ChristofidesAlgorithm
    {
        public static List<int> Solve(double[,] distanceMatrix)
        {
            if (distanceMatrix == null)
                throw new ArgumentNullException(nameof(distanceMatrix));

            if (distanceMatrix.GetLength(0) < 2)
                return new List<int> { 0 };

            // 1. Построение минимального остовного дерева (MST)
            List<int>[] mst = BuildMinimumSpanningTree(distanceMatrix);

            // 2. Нахождение вершин с нечетной степенью
            List<int> oddDegreeVertices = FindOddDegreeVertices(mst, distanceMatrix.GetLength(0));

            // 3. Построение минимального паросочетания
            List<(int, int)> matching = FindMinimumWeightMatching(oddDegreeVertices, distanceMatrix);

            // 4. Объединение MST и паросочетания в эйлеров мультиграф
            List<int>[] multigraph = CombineMstAndMatching(mst, matching, distanceMatrix.GetLength(0));

            // 5. Нахождение эйлерова цикла
            List<int> eulerianCircuit = FindEulerianCircuit(multigraph);

            // 6. Преобразование в гамильтонов цикл
            return MakeHamiltonianCycle(eulerianCircuit);
        }

        private static List<int>[] BuildMinimumSpanningTree(double[,] distanceMatrix)
        {
            int n = distanceMatrix.GetLength(0);
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
                    if (distanceMatrix[u, v] > 0 && !inMST[v] && distanceMatrix[u, v] < key[v])
                    {
                        parent[v] = u;
                        key[v] = distanceMatrix[u, v];
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

        private static List<int> FindOddDegreeVertices(List<int>[] mst, int vertexCount)
        {
            var oddVertices = new List<int>();
            for (int i = 0; i < vertexCount; i++)
            {
                if (mst[i].Count % 2 != 0)
                    oddVertices.Add(i);
            }
            return oddVertices;
        }

        private static List<(int, int)> FindMinimumWeightMatching(List<int> oddVertices, double[,] distanceMatrix)
        {
            var matching = new List<(int, int)>();
            var remainingVertices = new List<int>(oddVertices);

            while (remainingVertices.Count > 0)
            {
                int u = remainingVertices[0];
                int bestMatch = -1;
                double minDistance = double.MaxValue;

                for (int i = 1; i < remainingVertices.Count; i++)
                {
                    int v = remainingVertices[i];
                    double dist = distanceMatrix[u, v];
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        bestMatch = v;
                    }
                }

                if (bestMatch != -1)
                {
                    matching.Add((u, bestMatch));
                    remainingVertices.Remove(u);
                    remainingVertices.Remove(bestMatch);
                }
                else
                {
                    remainingVertices.Remove(u);
                }
            }

            return matching;
        }

        private static List<int>[] CombineMstAndMatching(List<int>[] mst, List<(int, int)> matching, int vertexCount)
        {
            var multigraph = new List<int>[vertexCount];
            for (int i = 0; i < vertexCount; i++)
            {
                multigraph[i] = new List<int>(mst[i]);
            }

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

        private static List<int> MakeHamiltonianCycle(List<int> eulerianCircuit)
        {
            var visited = new HashSet<int>();
            var path = new List<int>();

            foreach (var vertex in eulerianCircuit)
            {
                if (!visited.Contains(vertex))
                {
                    visited.Add(vertex);
                    path.Add(vertex);
                }
            }

            // Замыкаем цикл, возвращаясь в начальную точку
            path.Add(path[0]);
            return path;
        }
    }
}