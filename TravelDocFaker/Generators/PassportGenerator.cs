using TravelDocFaker.Utils;
using TravelDocFaker.Models;

namespace TravelDocFaker.Generators
{
    public class PassportGenerator
    {
        public static Passport BuildTD3(Person p, string passportNumber = null, DateOnly? expiry = null)
        {
            var rnd = Random.Shared;
            var docNo = passportNumber ?? Strings.RandomDocNumber(9);
            var exp = expiry ?? p.DateOfBirth.AddYears(10 + rnd.Next(0, 10));

            // L1
            var surname = MrzCore.Sanitize(p.Surname).Replace(" ", "<");
            var given = MrzCore.Sanitize(p.GivenName);
            var names39 = MrzCore.Pad($"{surname}<<{given}", 39);
            var l1 = MrzCore.Pad($"P<{p.CountryCode3}{names39}", 44);

            // L2
            var docField = MrzCore.Pad(MrzCore.Sanitize(docNo), 9);
            var docCd = MrzCore.CheckDigit(docField);
            var nationality = p.CountryCode3;

            var dobField = MrzCore.YYMMDD(p.DateOfBirth);
            var dobCd = MrzCore.CheckDigit(dobField);

            var sexChar = p.Gender switch { Gender.Female => 'F', Gender.Male => 'M', _ => '<' };

            var expField = MrzCore.YYMMDD(exp);
            var expCd = MrzCore.CheckDigit(expField);

            var optional = MrzCore.Pad("", 14);
            var optionalCd = MrzCore.CheckDigit(optional);

            var composite = docField + docCd
                          + dobField + dobCd
                          + expField + expCd
                          + optional + optionalCd;

            var finalCd = MrzCore.CheckDigit(composite);

            var l2 = MrzCore.Pad($"{docField}{docCd}{nationality}{dobField}{dobCd}{sexChar}{expField}{expCd}{optional}{optionalCd}{finalCd}", 44);

            return new Passport(docNo, exp, l1, l2);
        }
    }
}
