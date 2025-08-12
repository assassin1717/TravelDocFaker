namespace TravelDocFaker.Models
{
    public enum SupportedCountry
    {
        Portugal, Spain, UnitedKingdom, France
    }

    public sealed record CountryInfo(
        string Alpha3,
        string Locale 
    );

    public static class CountryCatalog
    {
        private static readonly Dictionary<SupportedCountry, CountryInfo> _map = new()
    {
        { SupportedCountry.Portugal,       new("PRT", "pt_PT") },
        { SupportedCountry.Spain,          new("ESP", "es")    },
        { SupportedCountry.UnitedKingdom,  new("GBR", "en_GB") },
        { SupportedCountry.France,         new("FRA", "fr")    },
    };
        public static CountryInfo Get(SupportedCountry c) => _map[c];
    }
}
