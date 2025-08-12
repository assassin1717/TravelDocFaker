using Bogus;
using TravelDocFaker.Models;
using TravelDocFaker.Utils;

namespace TravelDocFaker.Generators
{
    public static class PersonGenerator
    {
        /// <summary>
        /// Creates a new <see cref="Models.Person"/> with random but realistic details
        /// based on the specified country, gender, and age range.
        /// </summary>
        /// <param name="country">
        /// The <see cref="SupportedCountry"/> used to determine the locale, naming conventions,
        /// and ISO alpha-3 country code of the generated person.
        /// </param>
        /// <param name="gender">
        /// The <see cref="Gender"/> of the person.  
        /// If set to <c>Female</c> or <c>Male</c>, the first name will match the gender;  
        /// otherwise, a random gendered first name is chosen.
        /// </param>
        /// <param name="minAge">
        /// Minimum age (inclusive) for the generated person. Default is 18.
        /// </param>
        /// <param name="maxAge">
        /// Maximum age (inclusive) for the generated person. Default is 65.
        /// </param>
        /// <returns>
        /// A <see cref="Models.Person"/> instance populated with:
        /// <list type="bullet">
        /// <item><description>Random first and last names based on the given locale and gender.</description></item>
        /// <item><description>Date of birth within the specified age range.</description></item>
        /// <item><description>Gender, and ISO alpha-3 country code for nationality.</description></item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// <b>Date of Birth Generation:</b>  
        /// The date of birth is randomly chosen between the calculated minimum and maximum allowed dates,  
        /// ensuring the person falls within the specified age range as of the current date.
        /// </remarks>
        public static Models.Person Create(SupportedCountry country, Gender gender, int minAge = 18, int maxAge = 65)
        {
            var info = CountryCatalog.Get(country);

            var faker = new Faker(info.Locale);

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var minDob = new DateOnly(today.Year - maxAge, today.Month, Math.Min(today.Day, 28));
            var maxDob = new DateOnly(today.Year - minAge, today.Month, Math.Min(today.Day, 28));
            var dob = Dates.RandomDate(minDob, maxDob);

            string first = gender switch
            {
                Gender.Female => faker.Name.FirstName(Bogus.DataSets.Name.Gender.Female),
                Gender.Male => faker.Name.FirstName(Bogus.DataSets.Name.Gender.Male),
                _ => faker.Name.FirstName()
            };
            string last = faker.Name.LastName();

            return new Models.Person(first, last, gender, dob, info.Alpha3);
        }
    }
}
