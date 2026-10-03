using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using Newtonsoft.Json;

namespace CodeScanner
{
    public class LineItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public static class LineConfigHelper
    {
        private static readonly object _fileLock = new object();

        private static string GetFilePath()
        {
            string path = HostingEnvironment.MapPath("~/App_Data/lines.json");
            if (string.IsNullOrEmpty(path))
            {
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "lines.json");
            }
            return path;
        }

        private static readonly List<LineItem> DefaultLines = new List<LineItem>
        {
            new LineItem { Id = 1, Name = "Line 1" },
            new LineItem { Id = 2, Name = "Line 2" },
            new LineItem { Id = 3, Name = "Line 3" },
            new LineItem { Id = 4, Name = "Line 4" }
        };

        public static List<LineItem> GetLines()
        {
            lock (_fileLock)
            {
                try
                {
                    string filePath = GetFilePath();
                    if (!File.Exists(filePath))
                    {
                        SaveLines(DefaultLines);
                        return new List<LineItem>(DefaultLines);
                    }

                    string json = File.ReadAllText(filePath);
                    var lines = JsonConvert.DeserializeObject<List<LineItem>>(json);
                    if (lines == null)
                    {
                        SaveLines(DefaultLines);
                        return new List<LineItem>(DefaultLines);
                    }

                    return lines;
                }
                catch
                {
                    SaveLines(DefaultLines);
                    return new List<LineItem>(DefaultLines);
                }
            }
        }

        public static bool SaveLines(List<LineItem> lines)
        {
            lock (_fileLock)
            {
                try
                {
                    string filePath = GetFilePath();
                    string dir = Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    var cleanList = (lines ?? new List<LineItem>())
                        .Where(l => l != null && !string.IsNullOrWhiteSpace(l.Name))
                        .ToList();

                    string json = JsonConvert.SerializeObject(cleanList, Formatting.Indented);
                    File.WriteAllText(filePath, json);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public static bool AddLine(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            lock (_fileLock)
            {
                var lines = GetLines();
                int nextId = lines.Any() ? lines.Max(l => l.Id) + 1 : 1;
                lines.Add(new LineItem { Id = nextId, Name = name.Trim() });
                return SaveLines(lines);
            }
        }

        public static bool UpdateLine(int id, string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
                return false;

            lock (_fileLock)
            {
                var lines = GetLines();
                var item = lines.FirstOrDefault(l => l.Id == id);
                if (item == null)
                    return false;

                item.Name = newName.Trim();
                return SaveLines(lines);
            }
        }

        public static bool DeleteLine(int id)
        {
            lock (_fileLock)
            {
                if (id <= 0)
                {
                    return DeleteAllLines();
                }

                var lines = GetLines();
                var item = lines.FirstOrDefault(l => l.Id == id);
                if (item == null)
                    return false;

                lines.Remove(item);
                bool saved = SaveLines(lines);

                // Update Response table: set Line = NULL for responses with this line id
                try
                {
                    BusinessLogicLayer.blResponse.ResetLine(id);
                }
                catch (Exception ex)
                {
                    Log.Error("Error resetting line in Response for line id " + id + ": " + ex.ToString());
                }

                // If no lines remain, also ensure all responses are reset
                if (lines.Count == 0)
                {
                    try
                    {
                        BusinessLogicLayer.blResponse.ResetLine(null);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Error resetting all lines in Response: " + ex.ToString());
                    }
                }

                return saved;
            }
        }

        public static bool DeleteAllLines()
        {
            lock (_fileLock)
            {
                bool saved = SaveLines(new List<LineItem>());

                try
                {
                    BusinessLogicLayer.blResponse.ResetLine(null);
                }
                catch (Exception ex)
                {
                    Log.Error("Error resetting all lines in Response: " + ex.ToString());
                }

                return saved;
            }
        }

        public static string GetLineName(int? id)
        {
            if (!id.HasValue || id.Value <= 0)
                return string.Empty;

            var lines = GetLines();
            var item = lines.FirstOrDefault(l => l.Id == id.Value);
            return item != null ? item.Name : "Line " + id.Value;
        }
    }
}
