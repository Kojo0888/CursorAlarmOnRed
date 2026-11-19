namespace CursorAlarmOnRed
{
    /// <summary>Pure "is this pixel red?" logic (see PRD section 4).</summary>
    public static class RedDetector
    {
        public static bool IsRed(int r, int g, int b, int threshold)
        {
            if (r < 1 || r < threshold) return false;
            return g * 2 <= r && b * 2 <= r;
        }

        public static int ClampThreshold(int value)
        {
            if (value < 0) return 0;
            if (value > 255) return 255;
            return value;
        }

        /// <summary>Strict parse: whole number 0..255 only.</summary>
        public static bool TryParseThreshold(string text, out int threshold)
        {
            threshold = 0;
            if (text == null) return false;
            int v;
            if (!int.TryParse(text.Trim(), System.Globalization.NumberStyles.None,
                    System.Globalization.CultureInfo.InvariantCulture, out v)) return false;
            if (v < 0 || v > 255) return false;
            threshold = v;
            return true;
        }
    }
}
