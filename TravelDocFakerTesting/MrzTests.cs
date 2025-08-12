using TravelDocFaker.Generators;
using TravelDocFaker.Models;
using TravelDocFaker.Utils;

namespace TravelDocFakerTesting
{
    [TestFixture]
    public class MrzCheckDigitTests
    {
        [TestCase("P9664258R", 0, TestName = "DocCd-P9664258R")]
        [TestCase("950606", 6, TestName = "DobCd-950606")]
        [TestCase("070606", 9, TestName = "ExpCd-070606")]
        [TestCase("<<<<<<<<<<<<<<", 0, TestName = "OptionalCd-14Fillers")]
        [TestCase("72J8TWKTC", 8, TestName = "DocCd-72J8TWKTC")]
        public void CheckDigit_Field_ReturnsExpected(string field, int expectedCd)
        {
            var cd = MrzCore.CheckDigit(field);
            Assert.That(cd, Is.EqualTo(expectedCd));
        }

        [TestCase("P9664258R", "950606", "070606", "<<<<<<<<<<<<<<", 2, TestName = "FinalCd-Example1")]
        [TestCase("72J8TWKTC", "970504", "150504", "<<<<<<<<<<<<<<", 4, TestName = "FinalCd-Example2")]
        public void CheckDigit_FinalComposite_ReturnsExpected(
            string docField, string dobField, string expField, string optionalField, int expectedFinalCd)
        {
            var docCd = MrzCore.CheckDigit(docField);
            var dobCd = MrzCore.CheckDigit(dobField);
            var expCd = MrzCore.CheckDigit(expField);
            var optionalCd = MrzCore.CheckDigit(optionalField);

            var composite = docField + docCd + dobField + dobCd + expField + expCd + optionalField + optionalCd;

            var finalCd = MrzCore.CheckDigit(composite);
            Assert.That(finalCd, Is.EqualTo(expectedFinalCd));
        }
    }

    [TestFixture]
    public class MrzLineAssemblyTests
    {
        [TestCase(Gender.Male, 'M', TestName = "BuildTD3_PRT_SexM")]
        [TestCase(Gender.Female, 'F', TestName = "BuildTD3_PRT_SexF")]
        public void BuildTD3_Matches_Validator_PRT(Gender gender, char expectedSex)
        {
            var p = new Person("TIAGO", "BARBOSA", gender, new DateOnly(1995, 6, 6), "PRT");

            var passport = PassportGenerator.BuildTD3(
                p,
                passportNumber: "P9664258R",
                expiry: new DateOnly(2007, 6, 6)
            );

            const string expectedL1 = "P<PRTBARBOSA<<TIAGO<<<<<<<<<<<<<<<<<<<<<<<<<";
            var expectedL2 = $"P9664258R0PRT9506066{expectedSex}0706069<<<<<<<<<<<<<<02";

            Assert.Multiple(() =>
            {
                Assert.That(passport.Line1, Has.Length.EqualTo(44), "L1 length");
                Assert.That(passport.Line2, Has.Length.EqualTo(44), "L2 length");
            });
            Assert.Multiple(() =>
            {
                Assert.That(passport.Line1, Is.EqualTo(expectedL1), "L1 content");
                Assert.That(passport.Line2, Is.EqualTo(expectedL2), "L2 content");
            });
        }

        [TestCase(Gender.Male, 'M', TestName = "BuildTD3_GBR_SexM")]
        [TestCase(Gender.Female, 'F', TestName = "BuildTD3_GBR_SexF")]
        public void BuildTD3_Matches_Validator_GBR(Gender gender, char expectedSex)
        {
            var p = new Person("HAYDEN", "STONE", gender, new DateOnly(1936, 11, 8), "GBR");

            var passport = PassportGenerator.BuildTD3(
                p,
                passportNumber: "892296009",
                expiry: new DateOnly(2016, 1, 1)
            );

            const string expectedL1 = "P<GBRSTONE<<HAYDEN<<<<<<<<<<<<<<<<<<<<<<<<<<";
            var expectedL2 = $"8922960091GBR3611085{expectedSex}1601013<<<<<<<<<<<<<<04";

            Assert.Multiple(() =>
            {
                Assert.That(passport.Line1, Has.Length.EqualTo(44), "L1 length");
                Assert.That(passport.Line2, Has.Length.EqualTo(44), "L2 length");
            });
            Assert.Multiple(() =>
            {
                Assert.That(passport.Line1, Is.EqualTo(expectedL1), "L1 content");
                Assert.That(passport.Line2, Is.EqualTo(expectedL2), "L2 content");
            });
        }
    }
}
