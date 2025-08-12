namespace TravelDocFaker.Utils
{
    public class Strings
    {
        public static string RandomDocNumber(int len)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var chars = new char[len];
            for (int i = 0; i < len; i++) chars[i] = alphabet[Random.Shared.Next(alphabet.Length)];
            return new string(chars);
        }
    }
}
