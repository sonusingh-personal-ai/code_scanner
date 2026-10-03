using System;
using System.Collections.Generic;
using System.Linq;
using BusinessLogicLayer;
using Entity;

namespace CodeScanner
{
    public class LineItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Sequence { get; set; }
    }

    public static class LineConfigHelper
    {
        private static readonly object _syncLock = new object();

        private static readonly List<LineItem> DefaultLines = new List<LineItem>
        {
            new LineItem { Id = 1, Name = "Line 1", Sequence = 1 },
            new LineItem { Id = 2, Name = "Line 2", Sequence = 2 },
            new LineItem { Id = 3, Name = "Line 3", Sequence = 3 },
            new LineItem { Id = 4, Name = "Line 4", Sequence = 4 }
        };

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
                        return dbLines
                            .Where(l => l.Id.HasValue && !string.IsNullOrWhiteSpace(l.Name))
                            .OrderBy(l => l.Sequence)
                            .ThenBy(l => l.Id.Value)
                            .Select(l => new LineItem { Id = l.Id.Value, Name = l.Name, Sequence = l.Sequence })
                            .ToList();
                    }

                    // If table is empty, seed defaults
                    SeedDefaultLines();
                    dbLines = bl.ReadAll();
                    if (dbLines != null && dbLines.Count > 0)
                    {
                        return dbLines
                            .Where(l => l.Id.HasValue && !string.IsNullOrWhiteSpace(l.Name))
                            .OrderBy(l => l.Sequence)
                            .ThenBy(l => l.Id.Value)
                            .Select(l => new LineItem { Id = l.Id.Value, Name = l.Name, Sequence = l.Sequence })
                            .ToList();
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("Error reading lines from database: " + ex.ToString());
                }

                return new List<LineItem>(DefaultLines);
            }
        }

        public static bool AddLine(string name, int? sequence = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            lock (_syncLock)
            {
                try
                {
                    int seq = sequence ?? 0;
                    if (seq <= 0)
                    {
                        var existing = GetLines();
                        seq = existing.Any() ? existing.Max(l => l.Sequence) + 1 : 1;
                    }

                    var en = new enLine { Name = name.Trim(), Sequence = seq, CreatedOn = DateTime.Now };
                    var bl = new blLine(en);
                    int newId = bl.Create();
                    return newId > 0;
                }
                catch (Exception ex)
                {
                    Log.Error("Error adding line to database: " + ex.ToString());
                    return false;
                }
            }
        }

        public static bool UpdateLine(int id, string newName, int? sequence = null)
        {
            if (id <= 0 || string.IsNullOrWhiteSpace(newName))
                return false;

            lock (_syncLock)
            {
                try
                {
                    var en = new enLine { Id = id };
                    var bl = new blLine(en);
                    bl.Read();

                    en.Name = newName.Trim();
                    if (sequence.HasValue && sequence.Value > 0)
                    {
                        en.Sequence = sequence.Value;
                    }
                    en.ModifiedOn = DateTime.Now;

                    int rows = bl.Update();
                    return rows > 0;
                }
                catch (Exception ex)
                {
                    Log.Error("Error updating line in database: " + ex.ToString());
                    return false;
                }
            }
        }

        public static bool UpdateLineSequence(int id, int sequence)
        {
            if (id <= 0 || sequence <= 0)
                return false;

            lock (_syncLock)
            {
                try
                {
                    var bl = new blLine();
                    int rows = bl.UpdateSequence(id, sequence);
                    return rows > 0;
                }
                catch (Exception ex)
                {
                    Log.Error("Error updating line sequence: " + ex.ToString());
                    return false;
                }
            }
        }

        public static bool MoveLine(int id, string direction)
        {
            lock (_syncLock)
            {
                try
                {
                    var lines = GetLines();
                    int index = lines.FindIndex(l => l.Id == id);
                    if (index < 0) return false;

                    int swapIndex = string.Equals(direction, "up", StringComparison.OrdinalIgnoreCase) ? index - 1 : index + 1;
                    if (swapIndex < 0 || swapIndex >= lines.Count) return false;

                    var current = lines[index];
                    var other = lines[swapIndex];

                    // If sequences are the same, normalize all to 1..N first
                    if (current.Sequence == other.Sequence)
                    {
                        for (int i = 0; i < lines.Count; i++)
                        {
                            lines[i].Sequence = (i + 1);
                        }
                    }

                    int tempSeq = current.Sequence;
                    current.Sequence = other.Sequence;
                    other.Sequence = tempSeq;

                    var blCur = new blLine(new enLine { Id = current.Id, Name = current.Name, Sequence = current.Sequence, ModifiedOn = DateTime.Now });
                    blCur.Update();
                    var blOth = new blLine(new enLine { Id = other.Id, Name = other.Name, Sequence = other.Sequence, ModifiedOn = DateTime.Now });
                    blOth.Update();

                    return true;
                }
                catch (Exception ex)
                {
                    Log.Error("Error moving line: " + ex.ToString());
                    return false;
                }
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
                int seq = 1;
                foreach (var line in DefaultLines)
                {
                    var en = new enLine { Name = line.Name, Sequence = seq, CreatedOn = DateTime.Now };
                    var bl = new blLine(en);
                    bl.Create();
                    seq++;
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error seeding default lines: " + ex.ToString());
            }
        }
    }
}
