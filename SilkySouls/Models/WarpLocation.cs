namespace SilkySouls.Models
{
    public class WarpLocation
    {
        public int Id { get; set; }
        public float[] Coords { get; set; }
        public float Angle { get; set; }
        public string Name { get; set; }
        public string MainArea { get; set; }
        
        public bool HasCoordinates => Coords != null && Coords.Length > 0;
    }
}