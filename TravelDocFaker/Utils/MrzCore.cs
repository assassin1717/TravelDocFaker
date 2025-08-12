using System.Globalization;
using System.Text;

namespace TravelDocFaker.Utils
{
    public static class MrzCore
    {
        private static readonly int[] Weights = { 7, 3, 1 };

        /// <summary>
        /// Calculates the ICAO 9303 check digit for a given MRZ field.
        /// </summary>
        /// <param name="field">
        /// The string representing the MRZ field to calculate the check digit for.  
        /// May contain digits, uppercase letters (A–Z), or the filler character '&lt;'.
        /// </param>
        /// <returns>
        /// An integer between 0 and 9 representing the calculated check digit.
        /// </returns>
        /// <remarks>
        /// <para>
        /// The check digit is computed according to ICAO Doc 9303 specifications:  
        /// each character in <paramref name="field"/> is converted to its MRZ value
        /// using <see cref="CharValue(char)"/> and multiplied by a repeating weight sequence of 7, 3, 1.
        /// </para>
        /// <para>
        /// The products are summed, and the sum is taken modulo 10 to produce the check digit.
        /// </para>
        /// </remarks>
        public static int CheckDigit(string field)
        {
            int sum = 0;
            for (int i = 0; i < field.Length; i++)
                sum += CharValue(field[i]) * Weights[i % 3];
            return sum % 10;
        }

        /// <summary>
        /// Returns the ICAO 9303 MRZ numeric value of a given character.
        /// </summary>
        /// <param name="c">
        /// The character to convert.  
        /// Can be a digit ('0'–'9'), an uppercase letter ('A'–'Z'), or the filler character '&lt;'.
        /// </param>
        /// <returns>
        /// The numeric value of the character according to ICAO MRZ encoding rules:  
        /// <list type="bullet">
        /// <item><description>Digits '0'–'9' → values 0–9</description></item>
        /// <item><description>Letters 'A'–'Z' → values 10–35</description></item>
        /// <item><description>Filler '&lt;' → value 0</description></item>
        /// <item><description>Any other character → value 0</description></item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This method is used in check digit calculation and other MRZ parsing operations.
        /// </remarks>
        public static int CharValue(char c)
        {
            if (c >= '0' && c <= '9') return c - '0';
            if (c >= 'A' && c <= 'Z') return c - 'A' + 10;
            if (c == '<') return 0;
            return 0;
        }

        /// <summary>
        /// Sanitizes an input string according to ICAO 9303 MRZ (Machine Readable Zone) rules,
        /// replacing special characters and accents, converting to uppercase, and substituting
        /// invalid characters with the filler character '&lt;'.
        /// </summary>
        /// <param name="input">
        /// The original string to sanitize.  
        /// Can contain letters, digits, special characters, and diacritics.
        /// </param>
        /// <returns>
        /// A sanitized string suitable for MRZ usage, containing only uppercase A–Z, digits 0–9,
        /// and the filler character '&lt;'.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Special character replacements include:
        /// <list type="bullet">
        /// <item><description>'Æ' → "AE"</description></item>
        /// <item><description>'Œ' → "OE"</description></item>
        /// <item><description>'Ø' → "OE"</description></item>
        /// <item><description>'Ð' → "D"</description></item>
        /// <item><description>'Þ' → "TH"</description></item>
        /// <item><description>'Ł' → "L"</description></item>
        /// <item><description>'ß' → "SS"</description></item>
        /// </list>
        /// </para>
        /// <para>
        /// Accented letters are normalized (FormD) and diacritics are removed.  
        /// Any characters outside the allowed set are replaced with '&lt;'.
        /// </para>
        /// </remarks>
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

        /// <summary>
        /// Pads or truncates a string to a fixed length, filling with the MRZ filler character '&lt;' as needed.
        /// </summary>
        /// <param name="s">
        /// The input string to pad or truncate.
        /// </param>
        /// <param name="len">
        /// The required fixed length of the output string.
        /// </param>
        /// <returns>
        /// The input string truncated to <paramref name="len"/> characters if too long,  
        /// or padded with '&lt;' characters on the right if too short.
        /// </returns>
        /// <remarks>
        /// This method follows ICAO 9303 MRZ formatting rules for fixed-length fields.
        /// </remarks>
        public static string Pad(string s, int len) =>
            (s.Length > len) ? s[..len] : s + new string('<', len - s.Length);

        /// <summary>
        /// Formats a date as a six-digit string in YYMMDD format, following ICAO MRZ conventions.
        /// </summary>
        /// <param name="d">
        /// The <see cref="DateOnly"/> to format.
        /// </param>
        /// <returns>
        /// A string in the format <c>YYMMDD</c>, where:
        /// <list type="bullet">
        /// <item><description>YY = last two digits of the year</description></item>
        /// <item><description>MM = month (01–12)</description></item>
        /// <item><description>DD = day (01–31)</description></item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// This method is used for MRZ date fields such as date of birth and document expiry.
        /// </remarks>
        public static string YYMMDD(DateOnly d) => $"{d.Year % 100:00}{d.Month:00}{d.Day:00}";
    }
}
