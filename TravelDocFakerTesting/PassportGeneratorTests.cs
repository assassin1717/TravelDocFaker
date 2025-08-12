using TravelDocFaker.Generators;
using TravelDocFaker.Models;
using TravelDocFaker.Utils;

namespace TravelDocFakerTesting
{
    [TestFixture]
    public class PassportGeneratorTests
    {
        // ---------- Helpers ----------
        private static string Sub(string s, int start, int len) => s.Substring(start, len);

        private static void AssertTd3Line1(string l1, string issuer, string surname, string given)
        {
            Assert.That(l1.Length, Is.EqualTo(44), "L1 length");
            Assert.That(l1.StartsWith("P<"), Is.True, "L1 should start with 'P<'");
            Assert.That(Sub(l1, 2, 3), Is.EqualTo(issuer), "Issuer mismatch");
            Assert.That(l1.Length - 5, Is.EqualTo(39), "Names block length != 39");

            for (int i = 0; i < l1.Length; i++)
            {
                char c = l1[i];
                bool ok = (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '<';
                Assert.IsTrue(ok, $"Invalid char '{c}' at pos {i} in L1");
            }

            var surn = MrzCore.Sanitize(surname).Replace(" ", "<");
            var giv = MrzCore.Sanitize(given);
            StringAssert.Contains($"{surn}<<{giv}", l1, "Name block not found (sanitized)");
        }

        private static void AssertTd3Line2FieldsAndChecks(string l2, string nationality, char sex)
        {
            Assert.That(l2.Length, Is.EqualTo(44), "L2 length");
            var docField = Sub(l2, 0, 9);
            var docCd = l2[9] - '0';
            var nat = Sub(l2, 10, 3);
            var dobField = Sub(l2, 13, 6);
            var dobCd = l2[19] - '0';
            var sexChar = l2[20];
            var expField = Sub(l2, 21, 6);
            var expCd = l2[27] - '0';
            var optional = Sub(l2, 28, 14);
            var optionalCd = l2[42] - '0';
            var finalCd = l2[43] - '0';

            Assert.That(nat, Is.EqualTo(nationality), "Nationality mismatch");
            Assert.That(sexChar, Is.EqualTo(sex), "Sex mismatch");

            Assert.That(docCd, Is.EqualTo(MrzCore.CheckDigit(docField)), "DocCd invalid");
            Assert.That(dobCd, Is.EqualTo(MrzCore.CheckDigit(dobField)), "DobCd invalid");
            Assert.That(expCd, Is.EqualTo(MrzCore.CheckDigit(expField)), "ExpCd invalid");
            Assert.That(optionalCd, Is.EqualTo(MrzCore.CheckDigit(optional)), "OptionalCd invalid");

            var composite = docField + docCd
                          + dobField + dobCd
                          + expField + expCd
                          + optional + optionalCd;
            Assert.That(finalCd, Is.EqualTo(MrzCore.CheckDigit(composite)), "FinalCd invalid");

            for (int i = 0; i < l2.Length; i++)
            {
                char c = l2[i];
                bool ok = (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '<';
                Assert.IsTrue(ok, $"Invalid char '{c}' at pos {i} in L2");
            }
        }

        // ---------- Tests with static data ----------

        [Test]
        public void BuildTD3_ShouldMatch_Example_P9664258R()
        {
            var p = new Person("TIAGO", "BARBOSA", Gender.Male, new DateOnly(1995, 6, 6), "PRT");
            var mrz = PassportGenerator.BuildTD3(p, passportNumber: "P9664258R", expiry: new DateOnly(2007, 6, 6));

            const string expectedL1 = "P<PRTBARBOSA<<TIAGO<<<<<<<<<<<<<<<<<<<<<<<<<";
            const string expectedL2 = "P9664258R0PRT9506066M0706069<<<<<<<<<<<<<<02";

            Assert.That(mrz.Line1, Is.EqualTo(expectedL1), "L1 content mismatch");
            Assert.That(mrz.Line2, Is.EqualTo(expectedL2), "L2 content mismatch");

            AssertTd3Line1(mrz.Line1, issuer: "PRT", surname: "BARBOSA", given: "TIAGO");
            AssertTd3Line2FieldsAndChecks(mrz.Line2, nationality: "PRT", sex: 'M');
        }

        [TestCase(Gender.Male, 'M', TestName = "BuildTD3_Example_72J8TWKTC_SexM")]
        [TestCase(Gender.Female, 'F', TestName = "BuildTD3_Example_72J8TWKTC_SexF")]
        public void BuildTD3_ShouldMatch_Example_72J8TWKTC(Gender gender, char expectedSex)
        {
            var p = new Person("STELLA", "MALBY", gender, new DateOnly(1997, 5, 4), "PRT");
            var mrz = PassportGenerator.BuildTD3(p, passportNumber: "72J8TWKTC", expiry: new DateOnly(2015, 5, 4));

            const string expectedL1 = "P<PRTMALBY<<STELLA<<<<<<<<<<<<<<<<<<<<<<<<<<";
            var expectedL2 = "72J8TWKTC8PRT9705043" + expectedSex + "1505041<<<<<<<<<<<<<<04";

            Assert.That(mrz.Line1, Is.EqualTo(expectedL1));
            Assert.That(mrz.Line2, Is.EqualTo(expectedL2));

            AssertTd3Line1(mrz.Line1, "PRT", "MALBY", "STELLA");
            AssertTd3Line2FieldsAndChecks(mrz.Line2, "PRT", expectedSex);
        }

        // ---------- Supported Countries ----------

        private static readonly (SupportedCountry c, string alpha3)[] _countries = new[]
        {
            (SupportedCountry.Portugal, "PRT"),
            (SupportedCountry.Spain, "ESP"),
            (SupportedCountry.UnitedKingdom, "GBR"),
            (SupportedCountry.France, "FRA"),
        };

        [TestCaseSource(nameof(_countries))]
        public void BuildTD3_AllSupportedCountries_ShouldPlaceIssuerAndNationalityCorrectly((SupportedCountry c, string alpha3) entry)
        {
            var (country, alpha3) = entry;
            var person = PersonGenerator.Create(country, Gender.Male, 25, 35);

            var mrz = PassportGenerator.BuildTD3(person, passportNumber: "A1234567Z", expiry: new DateOnly(2031, 1, 1));

            StringAssert.StartsWith($"P<{alpha3}", mrz.Line1);
            Assert.That(mrz.Line2.Substring(10, 3), Is.EqualTo(alpha3));
            AssertTd3Line2FieldsAndChecks(mrz.Line2, alpha3, mrz.Line2[20]);
        }

        // ---------- Optional variations ----------

        [Test]
        public void BuildTD3_WithCustomOptional_ShouldComputeOptionalCdAndFinalCd()
        {
            var p = new Person("ALICE", "ROSA", Gender.Female, new DateOnly(1990, 1, 2), "GBR");

            var mrz = PassportGenerator.BuildTD3(p, passportNumber: "B1C2D3E4F", expiry: new DateOnly(2030, 12, 31));

            var l2 = mrz.Line2;

            var optional = Sub(l2, 28, 14);
            var optionalCd = l2[42] - '0';
            Assert.That(optionalCd, Is.EqualTo(MrzCore.CheckDigit(optional)), "OptionalCd invalid");

            var docField = Sub(l2, 0, 9);
            var docCd = l2[9] - '0';
            var dobField = Sub(l2, 13, 6);
            var dobCd = l2[19] - '0';
            var expField = Sub(l2, 21, 6);
            var expCd = l2[27] - '0';
            var finalCd = l2[43] - '0';

            var composite = docField + docCd + dobField + dobCd + expField + expCd + optional + optionalCd;
            Assert.That(finalCd, Is.EqualTo(MrzCore.CheckDigit(composite)), "FinalCd invalid");
        }
    }
}
