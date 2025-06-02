using System;
using System.Collections.Generic;

namespace Case1ZD
{
    public static class OrderArrays
    {
        public static Order[] GetOrderArray1() // Центр города
        {
            return new Order[]
            {
                new Order { ID = -1, Destination = new GeoPoint { X = 0, Y = 0 }, Priority = 0 },
                new Order { ID = 1, Destination = new GeoPoint { X = 1.2, Y = 0.8 }, Priority = 0.5 },
                new Order { ID = 2, Destination = new GeoPoint { X = -0.5, Y = 0.3 }, Priority = 0.7 },
                new Order { ID = 3, Destination = new GeoPoint { X = 0.7, Y = -0.2 }, Priority = 0.6 },
                new Order { ID = 4, Destination = new GeoPoint { X = -0.3, Y = -0.6 }, Priority = 0.4 },
                new Order { ID = 5, Destination = new GeoPoint { X = 0.4, Y = 0.5 }, Priority = 0.8 }
            };
        }

        public static Order[] GetOrderArray2() // Окраины
        {
            return new Order[]
            {
                new Order { ID = -1, Destination = new GeoPoint { X = 0, Y = 0 }, Priority = 0 },
                new Order { ID = 1, Destination = new GeoPoint { X = 5.0, Y = 4.5 }, Priority = 0.3 },
                new Order { ID = 2, Destination = new GeoPoint { X = -3.2, Y = 4.0 }, Priority = 0.5 },
                new Order { ID = 3, Destination = new GeoPoint { X = 4.5, Y = -3.8 }, Priority = 0.4 },
                new Order { ID = 4, Destination = new GeoPoint { X = -2.7, Y = -3.5 }, Priority = 0.6 },
                new Order { ID = 5, Destination = new GeoPoint { X = 3.0, Y = 2.5 }, Priority = 0.7 }
            };
        }

        public static Order[] GetOrderArray3() // Один район
        {
            return new Order[]
            {
                new Order { ID = -1, Destination = new GeoPoint { X = 2.0, Y = 2.0 }, Priority = 0 },
                new Order { ID = 1, Destination = new GeoPoint { X = 2.3, Y = 1.8 }, Priority = 0.5 },
                new Order { ID = 2, Destination = new GeoPoint { X = 1.7, Y = 2.1 }, Priority = 0.6 },
                new Order { ID = 3, Destination = new GeoPoint { X = 2.5, Y = 2.4 }, Priority = 0.7 },
                new Order { ID = 4, Destination = new GeoPoint { X = 1.5, Y = 1.6 }, Priority = 0.4 },
                new Order { ID = 5, Destination = new GeoPoint { X = 2.2, Y = 2.6 }, Priority = 0.8 }
            };
        }

        public static Order[] GetOrderArray4() // Разные районы
        {
            return new Order[]
            {
                new Order { ID = -1, Destination = new GeoPoint { X = 0, Y = 0 }, Priority = 0 },
                new Order { ID = 1, Destination = new GeoPoint { X = 1.5, Y = 1.5 }, Priority = 0.6 },
                new Order { ID = 2, Destination = new GeoPoint { X = -2.0, Y = 1.8 }, Priority = 0.5 },
                new Order { ID = 3, Destination = new GeoPoint { X = 0.5, Y = -3.0 }, Priority = 0.7 },
                new Order { ID = 4, Destination = new GeoPoint { X = 3.5, Y = 0.5 }, Priority = 0.4 },
                new Order { ID = 5, Destination = new GeoPoint { X = -1.0, Y = -2.5 }, Priority = 0.8 }
            };
        }

        public static Order[] GetOrderArray5() // Разные приоритеты
        {
            return new Order[]
            {
                new Order { ID = -1, Destination = new GeoPoint { X = 0, Y = 0 }, Priority = 0 },
                new Order { ID = 1, Destination = new GeoPoint { X = 1.0, Y = 1.0 }, Priority = 1.0 },
                new Order { ID = 2, Destination = new GeoPoint { X = -1.5, Y = 0.5 }, Priority = 0.2 },
                new Order { ID = 3, Destination = new GeoPoint { X = 0.5, Y = -1.0 }, Priority = 0.8 },
                new Order { ID = 4, Destination = new GeoPoint { X = -0.5, Y = -1.5 }, Priority = 0.1 },
                new Order { ID = 5, Destination = new GeoPoint { X = 1.5, Y = -0.5 }, Priority = 0.9 }
            };
        }

        public static Order[] GetOrderArray6() // Много заказов
        {
            var orders = new List<Order>
            {
                new Order { ID = -1, Destination = new GeoPoint { X = 0, Y = 0 }, Priority = 0 }
            };

            var random = new Random(42);
            for (int i = 1; i <= 15; i++)
            {
                orders.Add(new Order
                {
                    ID = i,
                    Destination = new GeoPoint
                    {
                        X = (random.NextDouble() - 0.5) * 10,
                        Y = (random.NextDouble() - 0.5) * 10
                    },
                    Priority = Math.Round(random.NextDouble(), 2)
                });
            }

            return orders.ToArray();
        }
    }
}