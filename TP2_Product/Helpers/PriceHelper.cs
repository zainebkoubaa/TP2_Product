using System.Text.RegularExpressions;

namespace TP_Product.Helpers
{
    public static class PriceHelper
    {
        // 5.9f  ->  "5DT900"
        public static string Format(float price)
        {
            decimal d = Math.Round((decimal)price, 3);
            int whole = (int)Math.Floor(d);
            int millimes = (int)Math.Round((d - whole) * 1000);
            if (millimes >= 1000)
            {
                whole++;
                millimes = 0;
            }
            return $"{whole}DT{millimes:000}";
        }

        // "5DT900"  ->  5.9f   (false si le format n'est pas respecté)
        public static bool TryParse(string? text, out float price)
        {
            price = 0;
            if (string.IsNullOrWhiteSpace(text)) return false;

            var m = Regex.Match(text.Trim(), @"^(\d+)DT(\d{3})$", RegexOptions.IgnoreCase);
            if (!m.Success) return false;

            if (!int.TryParse(m.Groups[1].Value, out int whole)) return false;
            int millimes = int.Parse(m.Groups[2].Value);

            price = (float)(whole + millimes / 1000m);
            return true;
        }
    }
}