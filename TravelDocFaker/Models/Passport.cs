namespace TravelDocFaker.Models
{
    /// <summary>
    /// Represents a passport's core data, including document number, expiry date,
    /// and both MRZ (Machine Readable Zone) lines.
    /// </summary>
    public sealed class Passport
    {
        /// <summary>
        /// The passport/document number.
        /// </summary>
        public string Number { get; }

        /// <summary>
        /// The expiry date of the passport.
        /// </summary>
        public DateOnly ExpiryDate { get; }

        /// <summary>
        /// MRZ line 1 (44 characters, formatted according to ICAO 9303).
        /// </summary>
        public string Line1 { get; }

        /// <summary>
        /// MRZ line 2 (44 characters, formatted according to ICAO 9303).
        /// </summary>
        public string Line2 { get; }

        /// <summary>
        /// Creates a new <see cref="Passport"/> instance.
        /// </summary>
        /// <param name="number">The passport/document number.</param>
        /// <param name="expiryDate">The expiry date of the passport.</param>
        /// <param name="line1">The first MRZ line.</param>
        /// <param name="lin2">The second MRZ line.</param>
        public Passport(string number, DateOnly expiryDate, string line1, string lin2)
        {
            Number = number;
            ExpiryDate = expiryDate;
            Line1 = line1;
            Line2 = lin2;
        }
    }
}
