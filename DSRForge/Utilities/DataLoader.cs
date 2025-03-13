using System.Collections.Generic;
using System.IO;
using DSRForge.Models;

namespace DSRForge.Utilities
{
    public static class DataLoader
    {
        public static List<Item> GetItemList(string listName)
        {
            List<Item> items = new List<Item>();

            string csvData = Properties.Resources.ResourceManager.GetString(listName);
            
            if (string.IsNullOrEmpty(csvData))
            {
                
                return new List<Item>();
            }

            using (StringReader reader = new StringReader(csvData))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    string[] parts = line.Split(',');
                    if (parts.Length >= 4)
                    {
                        items.Add(new Item(
                           int.Parse(parts[0]),
                           int.Parse(parts[1]),
                           (UpgradeType)int.Parse(parts[2]),
                           parts[3]
                        ));
                    }
                }
            }
            return items;
        }
        
        public static Dictionary<string, Location> GetLocationDict()
        {
            Dictionary<string, Location> locations = new Dictionary<string, Location>();

            string csvData = Properties.Resources.WarpLocations;
            
            if (string.IsNullOrEmpty(csvData))
            {
                
                return new Dictionary<string, Location>();
            }

            using (StringReader reader = new StringReader(csvData))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    string[] parts = line.Split(',');
                    if (parts.Length >= 3)
                    {
                        int id = int.Parse(parts[0]);
                        
                        float[] coords = null;
                        if (parts[1] != "0")
                        {
                            string[] coordsString = parts[1].Split('|');
                            coords = new float[coordsString.Length];
                            for (int i = 0; i < coordsString.Length; i++)
                            {
                                coords[i] = float.Parse(coordsString[i]);
                            }
                        }
                        
                        string name = parts[2];
                        
                        string key = id + "," + parts[1];
                        locations.Add(
                            key,
                            new Location
                        {
                            Id = id,
                            Coords = coords,
                            Name = name
                        });
                    }
                }
            }
            return locations;
        }
    }
}