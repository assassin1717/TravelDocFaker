namespace TravelDocFaker.Models
{
    public sealed class Person
    {
        public string GivenName { get; }
        public string Surname { get; }
        public Gender Gender { get; }
        public DateOnly DateOfBirth { get; }
        public string CountryCode3 { get; }

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
