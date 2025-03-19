namespace SilkySouls.Models
{
    public class Location
    {
        public int Id { get; set; }
        public float[] Coords { get; set; }
        public string Name { get; set; }
        
        public bool HasCoordinates => Coords != null && Coords.Length > 0;
    }
}