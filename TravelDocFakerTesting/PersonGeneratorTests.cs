using SkiaSharp;
using TravelDocFaker.Generators;
using TravelDocFaker.Models;
using TravelDocFaker.Utils;

namespace TravelDocFakerTesting
{
    [TestFixture]
    public class PersonGeneratorTests
    {
        private static readonly SupportedCountry[] Countries =
        {
            SupportedCountry.Portugal,
            SupportedCountry.Spain,
            SupportedCountry.UnitedKingdom,
            SupportedCountry.France,
        };

        [TestCaseSource(nameof(Countries))]
        public void Create_ShouldReturn_ValidPerson_ForEachSupportedCountry(SupportedCountry country)
        {
            var minAge = 21;
            var maxAge = 45;
            var p = PersonGenerator.Create(country, Gender.Male, minAge, maxAge);

            Assert.IsNotNull(p);
            Assert.IsFalse(string.IsNullOrWhiteSpace(p.GivenName), "GivenName empty");
            Assert.IsFalse(string.IsNullOrWhiteSpace(p.Surname), "Surname empty");
            Assert.IsTrue("PRT,ESP,GBR,FRA,DEU,NLD".Contains(p.CountryCode3), "CountryCode3 not in allowed set");
            Assert.That(p.CountryCode3.Length, Is.EqualTo(3), "CountryCode3 must be alpha-3");

            // Age window check
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var age = today.Year - p.DateOfBirth.Year - (new DateOnly(today.Year, p.DateOfBirth.Month, Math.Min(p.DateOfBirth.Day, 28)) > today ? 1 : 0);
            Assert.GreaterOrEqual(age, minAge, $"Age {age} < minAge {minAge}");
            Assert.LessOrEqual(age, maxAge, $"Age {age} > maxAge {maxAge}");
        }

        [TestCaseSource(nameof(Countries))]
        public void Create_ShouldRespect_GenderSelection(SupportedCountry country)
        {
            var pF = PersonGenerator.Create(country, Gender.Female, 25, 35);
            var pM = PersonGenerator.Create(country, Gender.Male, 25, 35);

            Assert.That(pF.Gender, Is.EqualTo(Gender.Female));
            Assert.That(pM.Gender, Is.EqualTo(Gender.Male));
        }

        [Test]
        public void Create_MultipleSamples_ShouldVaryNames_AndStayInRange()
        {
            var minAge = 18; var maxAge = 60;
            var people = Enumerable.Range(0, 50)
                .Select(_ => PersonGenerator.Create(SupportedCountry.Portugal, Gender.Male, minAge, maxAge))
                .ToList();

            var distinctNames = people.Select(p => $"{p.GivenName} {p.Surname}").Distinct().Count();
            Assert.That(distinctNames, Is.GreaterThan(5), "Names seem not varied");

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            foreach (var p in people)
            {
                Assert.That(p.CountryCode3, Is.EqualTo("PRT"));
                var age = today.Year - p.DateOfBirth.Year - (new DateOnly(today.Year, p.DateOfBirth.Month, Math.Min(p.DateOfBirth.Day, 28)) > today ? 1 : 0);
                Assert.That(age, Is.InRange(minAge, maxAge), $"Age out of range: {age}");
            }
        }

        [TestCase(40, 25)]
        [TestCase(18, 18)]
        public void Create_WithEdgeAges_ShouldNotCrash_AndRespectBounds(int minAge, int maxAge)
        {
            var p = PersonGenerator.Create(SupportedCountry.Portugal, Gender.Male, Math.Min(minAge, maxAge), Math.Max(minAge, maxAge));

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var age = today.Year - p.DateOfBirth.Year - (new DateOnly(today.Year, p.DateOfBirth.Month, Math.Min(p.DateOfBirth.Day, 28)) > today ? 1 : 0);
            Assert.That(age, Is.InRange(Math.Min(minAge, maxAge), Math.Max(minAge, maxAge)));
        }

        [TestCaseSource(nameof(Countries))]
        public void Create_ShouldReturn_Alpha3CountryCorrect(SupportedCountry country)
        {
            var p = PersonGenerator.Create(country, Gender.Male, 25, 35);

            string expected = country switch
            {
                SupportedCountry.Portugal => "PRT",
                SupportedCountry.Spain => "ESP",
                SupportedCountry.UnitedKingdom => "GBR",
                SupportedCountry.France => "FRA",
                _ => throw new ArgumentOutOfRangeException()
            };

            Assert.That(p.CountryCode3, Is.EqualTo(expected));
        }
    }
}
