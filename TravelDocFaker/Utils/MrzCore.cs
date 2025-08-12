using System.Globalization;
using System.Text;

namespace TravelDocFaker.Utils
{
    public static class MrzCore
    {
        private static readonly int[] Weights = { 7, 3, 1 };

        public static int CheckDigit(string field)
        {
            int sum = 0;
            for (int i = 0; i < field.Length; i++)
                sum += CharValue(field[i]) * Weights[i % 3];
            return sum % 10;
        }

        public static int CharValue(char c)
        {
            if (c >= '0' && c <= '9') return c - '0';
            if (c >= 'A' && c <= 'Z') return c - 'A' + 10;
            if (c == '<') return 0;
            return 0;
        }

        public static string Sanitize(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            var sb = new StringBuilder(input.Length * 2);

            foreach (var raw in input)
            {
                switch (char.ToUpperInvariant(raw))
                {
                    case 'Æ': sb.Append("AE"); continue;
                    case 'Œ': sb.Append("OE"); continue;
                    case 'Ø': sb.Append("OE"); continue;
                    case 'Ð': sb.Append('D'); continue;
                    case 'Þ': sb.Append("TH"); continue;
                    case 'Ł': sb.Append('L'); continue;
                    case 'ß': sb.Append("SS"); continue;
                }

                string normalized = raw.ToString().Normalize(NormalizationForm.FormD);
                foreach (var c in normalized)
                {
                    var cat = CharUnicodeInfo.GetUnicodeCategory(c);
                    if (cat == UnicodeCategory.NonSpacingMark) continue;

                    char u = char.ToUpperInvariant(c);

                    if (u >= 'A' && u <= 'Z')
                    {
                        sb.Append(u);
                    }
                    else if (u >= '0' && u <= '9')
                    {
                        sb.Append(u);
                    }
                    else
                    {
                        sb.Append('<');
                    }
                }
            }

            return sb.ToString();
        }

        public static string Pad(string s, int len) =>
            (s.Length > len) ? s[..len] : s + new string('<', len - s.Length);

        public static string YYMMDD(DateOnly d) => $"{d.Year % 100:00}{d.Month:00}{d.Day:00}";
    }
}
