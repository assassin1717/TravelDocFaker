using TravelDocFaker.Utils;
using TravelDocFaker.Models;

namespace TravelDocFaker.Generators
{
    public class PassportGenerator
    {
        /// <summary>
        /// Builds a TD3 (Machine Readable Zone) passport record for a given person,
        /// including MRZ lines 1 and 2, following ICAO 9303 standards.
        /// </summary>
        /// <param name="p">The <see cref="Person"/> object containing the holder's personal information.</param>
        /// <param name="passportNumber">
        /// Optional passport/document number.  
        /// If null, a random 9-character document number is generated.
        /// </param>
        /// <param name="expiry">
        /// Optional expiry date.  
        /// If null, the expiry date is calculated as 10 to 19 years after the holder's date of birth.
        /// </param>
        /// <returns>
        /// A <see cref="Passport"/> instance containing:
        /// <list type="bullet">
        /// <item><description>Document number</description></item>
        /// <item><description>Expiry date</description></item>
        /// <item><description>MRZ Line 1 (44 chars)</description></item>
        /// <item><description>MRZ Line 2 (44 chars)</description></item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// MRZ line generation:
        /// <list type="number">
        /// <item>
        /// <b>Line 1:</b> Document type, issuing country code, and the holder's name (surname << given name),
        /// padded with '<' characters to 44 characters total.
        /// </item>
        /// <item>
        /// <b>Line 2:</b> Document number + check digit, nationality, date of birth + check digit,
        /// sex, expiry date + check digit, optional data + check digit, and a final composite check digit,
        /// all padded to 44 characters.
        /// </item>
        /// </remarks>
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
