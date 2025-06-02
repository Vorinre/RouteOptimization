namespace Case1ZD
{
    public class Order
    {
        public int ID { get; set; }
        public GeoPoint Destination { get; set; }
        public double Priority { get; set; }
    }

    public class GeoPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
    }
}