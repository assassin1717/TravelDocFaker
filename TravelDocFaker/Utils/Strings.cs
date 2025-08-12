namespace TravelDocFaker.Utils
{
    public class Strings
    {
        /// <summary>
        /// Generates a random document number consisting of uppercase letters (A–Z) and digits (0–9).
        /// </summary>
        /// <param name="len">
        /// The length of the document number to generate.
        /// </param>
        /// <returns>
        /// A string containing <paramref name="len"/> random alphanumeric characters in uppercase.
        /// </returns>
        /// <remarks>
        /// Characters are chosen uniformly at random from the set <c>A–Z</c> and <c>0–9</c>.  
        /// This method is typically used for generating unique passport or document identifiers.
        /// </remarks>
        public static string RandomDocNumber(int len)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var chars = new char[len];
            for (int i = 0; i < len; i++) chars[i] = alphabet[Random.Shared.Next(alphabet.Length)];
            return new string(chars);
        }
    }
}
