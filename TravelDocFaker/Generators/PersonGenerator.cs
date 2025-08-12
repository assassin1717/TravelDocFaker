using Bogus;
using TravelDocFaker.Models;

namespace TravelDocFaker.Generators
{
    public static class PersonGenerator
    {
        public static Models.Person Create(SupportedCountry country, Gender gender, int minAge = 18, int maxAge = 65)
        {
            var info = CountryCatalog.Get(country);

            var faker = new Faker(info.Locale);

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var minDob = new DateOnly(today.Year - maxAge, today.Month, Math.Min(today.Day, 28));
            var maxDob = new DateOnly(today.Year - minAge, today.Month, Math.Min(today.Day, 28));
            var dob = RandomDate(minDob, maxDob);

            string first = gender switch
            {
                Gender.Female => faker.Name.FirstName(Bogus.DataSets.Name.Gender.Female),
                Gender.Male => faker.Name.FirstName(Bogus.DataSets.Name.Gender.Male),
                _ => faker.Name.FirstName()
            };
            string last = faker.Name.LastName();

            return new Models.Person(first, last, gender, dob, info.Alpha3);
        }

        private static DateOnly RandomDate(DateOnly min, DateOnly max)
        {
            var days = max.DayNumber - min.DayNumber;
            return DateOnly.FromDayNumber(min.DayNumber + Random.Shared.Next(Math.Max(1, days)));
        }
    }
}
