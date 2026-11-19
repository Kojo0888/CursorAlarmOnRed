using System;
using System.IO;

namespace CursorAlarmOnRed
{
    /// <summary>Reads/writes the one-line config file: Threshold=NNN</summary>
    public static class AppConfig
    {
        public const int DefaultThreshold = 200;
        private const string Key = "Threshold";

        public static string Format(int threshold)
        {
            return Key + "=" + RedDetector.ClampThreshold(threshold) + Environment.NewLine;
        }

        public static int Parse(string text)
        {
            if (string.IsNullOrEmpty(text)) return DefaultThreshold;
            foreach (string raw in text.Split('\n'))
            {
                string line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("#")) continue;
                int eq = line.IndexOf('=');
                if (eq < 0) continue;
                if (!string.Equals(line.Substring(0, eq).Trim(), Key, StringComparison.OrdinalIgnoreCase)) continue;
                int v;
                if (RedDetector.TryParseThreshold(line.Substring(eq + 1), out v)) return v;
            }
            return DefaultThreshold;
        }

        public static int Load(string path)
        {
            try { return File.Exists(path) ? Parse(File.ReadAllText(path)) : DefaultThreshold; }
            catch (Exception) { return DefaultThreshold; }
        }

        public static bool Save(string path, int threshold)
        {
            try { File.WriteAllText(path, Format(threshold)); return true; }
            catch (Exception) { return false; }
        }
    }
}
