namespace TravelDocFaker.Models
{
    public sealed class Passport
    {
        public string Number { get; }
        public DateOnly ExpiryDate { get; }
        public string Line1 { get; }
        public string Line2 { get; }
        public Passport(string number, DateOnly expiryDate, string line1, string lin2)
        {
            Number = number;
            ExpiryDate = expiryDate;
            Line1 = line1;
            Line2 = lin2;
        }
    }
}
