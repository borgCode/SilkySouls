using System.Collections.Generic;
using System.IO;
using SilkySouls.Models;

namespace SilkySouls.Utilities
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
        
        public static Dictionary<string, List<WarpLocation>> GetLocationDict()
        {
            Dictionary<string, List<WarpLocation>> warpDict = new Dictionary<string, List<WarpLocation>>();

            string csvData = Properties.Resources.WarpLocations;
    
            if (string.IsNullOrWhiteSpace(csvData))
                return warpDict;

            using (StringReader reader = new StringReader(csvData))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    string[] parts = line.Split(',');
                    if (parts.Length >= 4)
                    {
                        int id = int.Parse(parts[0]);

                        float[] coords = null;
                        float angle = 0f;
                        if (parts[1] != "0")
                        {
                            string[] coordsString = parts[1].Split('|');

                            angle = float.Parse(coordsString[coordsString.Length - 1], System.Globalization.CultureInfo.InvariantCulture);

                            coords = new float[coordsString.Length];
                            for (int i = 0; i < coordsString.Length; i++)
                            {
                                coords[i] = float.Parse(coordsString[i], System.Globalization.CultureInfo.InvariantCulture);
                            }
                        }

                        string mainArea = parts[2];
                        string name = parts[3];

                        WarpLocation location = new WarpLocation
                        {
                            Id = id,
                            Coords = coords,
                            Angle = angle,
                            MainArea = mainArea,
                            Name = name
                        };

                        if (!warpDict.ContainsKey(mainArea))
                        {
                            warpDict[mainArea] = new List<WarpLocation>();
                        }
                        warpDict[mainArea].Add(location);
                    }
                }
            }
            return warpDict;
        }
    }
}