namespace TravelDocFaker.Models
{
    /// <summary>
    /// Represents the list of supported countries for passport and MRZ generation.
    /// </summary>
    public enum SupportedCountry
    {
        /// <summary>
        /// Portugal (PRT).
        /// </summary>
        Portugal,

        /// <summary>
        /// Spain (ESP).
        /// </summary>
        Spain,

        /// <summary>
        /// United Kingdom (GBR).
        /// </summary>
        UnitedKingdom,

        /// <summary>
        /// France (FRA).
        /// </summary>
        France
    }

    /// <summary>
    /// Immutable record containing country-related information for MRZ and locale usage.
    /// </summary>
    /// <param name="Alpha3">
    /// ISO 3166-1 alpha-3 country code (e.g., "PRT", "ESP").
    /// </param>
    /// <param name="Locale">
    /// Locale string used for data generation (e.g., "pt_PT", "en_GB").
    /// </param>
    public sealed record CountryInfo(
        string Alpha3,
        string Locale
    );

    /// <summary>
    /// Provides a mapping between <see cref="SupportedCountry"/> values and their corresponding <see cref="CountryInfo"/>.
    /// </summary>
    public static class CountryCatalog
    {
        private static readonly Dictionary<SupportedCountry, CountryInfo> _map = new()
    {
        { SupportedCountry.Portugal,       new("PRT", "pt_PT") },
        { SupportedCountry.Spain,          new("ESP", "es")    },
        { SupportedCountry.UnitedKingdom,  new("GBR", "en_GB") },
        { SupportedCountry.France,         new("FRA", "fr")    },
    };

        /// <summary>
        /// Gets the <see cref="CountryInfo"/> for the specified <see cref="SupportedCountry"/>.
        /// </summary>
        /// <param name="c">The supported country to retrieve information for.</param>
        /// <returns>The associated <see cref="CountryInfo"/>.</returns>
        public static CountryInfo Get(SupportedCountry c) => _map[c];
    }
}
