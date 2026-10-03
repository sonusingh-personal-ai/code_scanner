using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using BusinessLogicLayer;
using Entity;
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
        private static readonly object _syncLock = new object();

        private static readonly List<LineItem> DefaultLines = new List<LineItem>
        {
            new LineItem { Id = 1, Name = "Line 1" },
            new LineItem { Id = 2, Name = "Line 2" },
            new LineItem { Id = 3, Name = "Line 3" },
            new LineItem { Id = 4, Name = "Line 4" }
        };

        private static string GetFilePath()
        {
            string path = null;
            try
            {
                if (HostingEnvironment.IsHosted)
                {
                    path = HostingEnvironment.MapPath("~/App_Data/lines.json");
                }
            }
            catch { }

            if (string.IsNullOrEmpty(path))
            {
                string[] candidates = new[]
                {
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "lines.json"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CodeScanner", "App_Data", "lines.json"),
                    Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "lines.json"),
                    Path.Combine(Directory.GetCurrentDirectory(), "CodeScanner", "App_Data", "lines.json")
                };

                foreach (var c in candidates)
                {
                    if (File.Exists(c))
                    {
                        path = c;
                        break;
                    }
                }
            }

            return string.IsNullOrEmpty(path) ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "lines.json") : path;
        }

        public static List<LineItem> GetLines()
        {
            lock (_syncLock)
            {
                try
                {
                    var bl = new blLine();
                    var dbLines = bl.ReadAll();

                    if (dbLines != null && dbLines.Count > 0)
                    {
                        var result = dbLines
                            .Where(l => l.Id.HasValue && !string.IsNullOrWhiteSpace(l.Name))
                            .OrderBy(l => l.Id.Value)
                            .Select(l => new LineItem { Id = l.Id.Value, Name = l.Name })
                            .ToList();

                        SyncBackupJson(result);
                        return result;
                    }

                    // If table is empty, seed defaults
                    SeedDefaultLines();
                    dbLines = bl.ReadAll();
                    if (dbLines != null && dbLines.Count > 0)
                    {
                        var result = dbLines
                            .Where(l => l.Id.HasValue && !string.IsNullOrWhiteSpace(l.Name))
                            .OrderBy(l => l.Id.Value)
                            .Select(l => new LineItem { Id = l.Id.Value, Name = l.Name })
                            .ToList();

                        SyncBackupJson(result);
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("Error reading lines from database: " + ex.ToString());
                }

                // Fallback to local JSON file or defaults if DB is unavailable
                return GetLinesFromJsonFallback();
            }
        }

        public static bool SaveLines(List<LineItem> lines)
        {
            lock (_syncLock)
            {
                SyncBackupJson(lines);
                return true;
            }
        }

        public static bool AddLine(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            lock (_syncLock)
            {
                try
                {
                    var en = new enLine { Name = name.Trim(), CreatedOn = DateTime.Now };
                    var bl = new blLine(en);
                    int newId = bl.Create();
                    if (newId > 0)
                    {
                        GetLines(); // refresh & sync backup
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("Error adding line to database: " + ex.ToString());
                }

                return false;
            }
        }

        public static bool UpdateLine(int id, string newName)
        {
            if (id <= 0 || string.IsNullOrWhiteSpace(newName))
                return false;

            lock (_syncLock)
            {
                try
                {
                    var en = new enLine { Id = id, Name = newName.Trim(), ModifiedOn = DateTime.Now };
                    var bl = new blLine(en);
                    int rows = bl.Update();
                    if (rows > 0)
                    {
                        GetLines(); // refresh & sync backup
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("Error updating line in database: " + ex.ToString());
                }

                return false;
            }
        }

        public static bool DeleteLine(int id)
        {
            lock (_syncLock)
            {
                if (id <= 0)
                {
                    return DeleteAllLines();
                }

                try
                {
                    var en = new enLine { Id = id };
                    var bl = new blLine(en);
                    int rows = bl.Delete();

                    // Ensure foreign key references in Response are cleared
                    try
                    {
                        blResponse.ResetLine(id);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Error resetting line in Response for line id " + id + ": " + ex.ToString());
                    }

                    GetLines(); // refresh & sync backup
                    return rows > 0;
                }
                catch (Exception ex)
                {
                    Log.Error("Error deleting line from database: " + ex.ToString());
                    return false;
                }
            }
        }

        public static bool DeleteAllLines()
        {
            lock (_syncLock)
            {
                try
                {
                    var bl = new blLine();
                    int rows = bl.Delete();

                    try
                    {
                        blResponse.ResetLine(null);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Error resetting all lines in Response: " + ex.ToString());
                    }

                    GetLines(); // refresh & sync backup
                    return true;
                }
                catch (Exception ex)
                {
                    Log.Error("Error deleting all lines from database: " + ex.ToString());
                    return false;
                }
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

        private static void SeedDefaultLines()
        {
            try
            {
                var existing = GetLinesFromJsonFallback();
                var toSeed = (existing != null && existing.Count > 0) ? existing : DefaultLines;

                foreach (var line in toSeed)
                {
                    var en = new enLine { Name = line.Name, CreatedOn = DateTime.Now };
                    var bl = new blLine(en);
                    bl.Create();
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error seeding default lines: " + ex.ToString());
            }
        }

        private static List<LineItem> GetLinesFromJsonFallback()
        {
            try
            {
                string filePath = GetFilePath();
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    var lines = JsonConvert.DeserializeObject<List<LineItem>>(json);
                    if (lines != null && lines.Count > 0)
                        return lines;
                }
            }
            catch { }

            return new List<LineItem>(DefaultLines);
        }

        private static void SyncBackupJson(List<LineItem> lines)
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
            }
            catch { }
        }
    }
}
