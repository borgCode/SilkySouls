using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;

namespace SilkySouls.Utilities
{
    public static class VersionChecker
    {
        
        private static readonly Dictionary<uint, string> VersionMap = new Dictionary<uint, string>
        {
            { 1530801206, "1.3.0.0" },
            { 1665452630, "1.3.0.1" }
        };
        
        private static readonly Dictionary<long, string> FileSizeVersionMap = new Dictionary<long, string>
        {
            { 50286344, "1.3.0.0" },  
            { 57067008, "1.3.0.1" } 
        };
        
        public static string GetPatch()
        {
            string gameExePath = GetDarkSoulsRemasteredExePath();
            uint timestamp = GetPeTimestamp(gameExePath);
            
            string patch = VersionMap.TryGetValue(timestamp, out string version) 
                ? version 
                : "Unknown";

            if (patch == "Unknown")
            {
                long size = new FileInfo(gameExePath).Length;
                patch = FileSizeVersionMap.TryGetValue(size, out string versionFromSize) 
                    ? versionFromSize 
                    : "Unknown";
            }
            return patch;
        }

        private static string GetDarkSoulsRemasteredExePath()
        {
            try
            {
                string steamPath =
                    Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam", "InstallPath",
                        null) as string
                    ?? Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam", "InstallPath", null) as string;

                if (string.IsNullOrEmpty(steamPath))
                    return null;

                string defaultPath = Path.Combine(steamPath, "steamapps", "common", "DARK SOULS REMASTERED",
                    "DarkSoulsRemastered.exe");
                if (File.Exists(defaultPath))
                    return defaultPath;

                var libraries = new HashSet<string> { steamPath };
                string configPath = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");

                if (File.Exists(configPath))
                {
                    var pathRegex = new System.Text.RegularExpressions.Regex(@"""path""\s+""([^""]+)""");

                    using (var reader = new StreamReader(configPath))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line.Contains("path"))
                            {
                                var match = pathRegex.Match(line);
                                if (match.Success)
                                {
                                    string path = match.Groups[1].Value.Replace(@"\\", @"\");
                                    if (Directory.Exists(path))
                                        libraries.Add(path);
                                }
                            }
                        }
                    }
                }

                foreach (string library in libraries)
                {
                    string exePath = Path.Combine(library, "steamapps", "common", "DARK SOULS REMASTERED",
                        "DarkSoulsRemastered.exe");
                    if (File.Exists(exePath))
                        return exePath;

                    string manifestPath = Path.Combine(library, "steamapps", "appmanifest_570940.acf");
                    if (File.Exists(manifestPath))
                    {
                        try
                        {
                            var installDirRegex =
                                new System.Text.RegularExpressions.Regex(@"""installdir""\s+""([^""]+)""");
                            using (var reader = new StreamReader(manifestPath))
                            {
                                string line;
                                while ((line = reader.ReadLine()) != null)
                                {
                                    if (line.Contains("installdir"))
                                    {
                                        var match = installDirRegex.Match(line);
                                        if (match.Success)
                                        {
                                            string installDir = match.Groups[1].Value;
                                            string manifestExePath = Path.Combine(library, "steamapps", "common",
                                                installDir, "DarkSoulsRemastered.exe");
                                            if (File.Exists(manifestExePath))
                                                return manifestExePath;
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error parsing manifest {manifestPath}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error finding Dark Souls Remastered: {ex.Message}");
            }
            return null;
        }

        private static uint GetPeTimestamp(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (BinaryReader reader = new BinaryReader(fs))
            {
                fs.Position = 0x3C;
                uint peHeaderOffset = reader.ReadUInt32();
                
                fs.Position = peHeaderOffset;
                uint signature = reader.ReadUInt32();
                if (signature != 0x00004550) 
                    throw new Exception("Invalid PE signature");
                
                fs.Position = peHeaderOffset + 4 + 2;
                
                reader.ReadUInt16();
                
                return reader.ReadUInt32();
            }
        }
        
    }
}