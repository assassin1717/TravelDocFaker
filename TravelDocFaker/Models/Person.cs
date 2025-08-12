namespace TravelDocFaker.Models
{
    /// <summary>
    /// Represents a person used for passport/MRZ generation and rendering.
    /// Immutable value object with basic identity and nationality data.
    /// </summary>
    public sealed class Person
    {
        /// <summary>
        /// Holder's given/first name (unsanitized; use MRZ utilities when needed).
        /// </summary>
        public string GivenName { get; }

        /// <summary>
        /// Holder's surname/last name (unsanitized; use MRZ utilities when needed).
        /// </summary>
        public string Surname { get; }

        /// <summary>
        /// Holder's gender.
        /// </summary>
        public Gender Gender { get; }

        /// <summary>
        /// Holder's date of birth.
        /// </summary>
        public DateOnly DateOfBirth { get; }

        /// <summary>
        /// ISO 3166-1 alpha-3 country code (e.g., "USA", "PRT").
        /// </summary>
        public string CountryCode3 { get; }

        /// <summary>
        /// Creates a new <see cref="Person"/> instance.
        /// </summary>
        /// <param name="givenName">Given/first name.</param>
        /// <param name="surname">Surname/last name.</param>
        /// <param name="gender">Gender.</param>
        /// <param name="dateOfBirth">Date of birth.</param>
        /// <param name="countryCode3">ISO 3166-1 alpha-3 country code.</param>
        public Person(string givenName, string surname, Gender gender, DateOnly dateOfBirth, string countryCode3)
        {
            GivenName = givenName;
            Surname = surname;
            Gender = gender;
            DateOfBirth = dateOfBirth;
            CountryCode3 = countryCode3;
        }
    }
}
