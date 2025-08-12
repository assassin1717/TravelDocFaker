using SkiaSharp;
using TravelDocFaker.Models;

namespace TravelDocFaker.Generators
{
    public class PhotoGenerator
    {
        const int DefaultWidth = 750;
        const int DefaultHeight = 400;
        /// <summary>
        /// Generates a JPEG image representing a simplified passport visual layout
        /// with the holder's personal information and MRZ lines.
        /// </summary>
        /// <param name="person">
        /// The <see cref="Person"/> containing the passport holder's personal details (name, gender, DOB, country).
        /// </param>
        /// <param name="passport">
        /// The <see cref="Passport"/> containing the document number, expiry date, and MRZ lines.
        /// </param>
        /// <param name="width">
        /// Optional image width in pixels.  
        /// If less than <c>DefaultWidth</c>, <c>DefaultWidth</c> is used instead.
        /// </param>
        /// <param name="height">
        /// Optional image height in pixels.  
        /// If less than <c>DefaultHeight</c>, <c>DefaultHeight</c> is used instead.
        /// </param>
        /// <returns>
        /// A byte array containing the generated JPEG image data.
        /// </returns>
        /// <remarks>
        /// The image is generated using SkiaSharp with a light gray background
        /// and black text drawn in a default font.  
        /// The output includes:
        /// <list type="bullet">
        /// <item><description>Full name</description></item>
        /// <item><description>Gender</description></item>
        /// <item><description>Date of birth</description></item>
        /// <item><description>Country code</description></item>
        /// <item><description>Expiry date</description></item>
        /// <item><description>Document number</description></item>
        /// <item><description>MRZ line 1</description></item>
        /// <item><description>MRZ line 2</description></item>
        /// </list>
        /// The image is encoded as JPEG with 90% quality.
        /// </remarks>
        public static byte[] Generate(Person person, Passport passport, int width = 750, int height = 400)
        {
            width = width >= DefaultWidth ? width : DefaultWidth;
            height = height >= DefaultHeight ? height : DefaultHeight;
            using var bitmap = new SKBitmap(width, height);
            using var canvas = new SKCanvas(bitmap);
            canvas.Clear(SKColors.LightGray);

            using var paint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true
            };

            using var font = new SKFont(SKTypeface.Default, 24);

            canvas.DrawText($"Name: {person.GivenName} {person.Surname}", 10, 50, font, paint);
            canvas.DrawText($"Gender: {person.Gender}", 10, 90, font, paint);
            canvas.DrawText($"DOB: {person.DateOfBirth:yyyy-MM-dd}", 10, 130, font, paint);
            canvas.DrawText($"Country: {person.CountryCode3}", 10, 170, font, paint);
            canvas.DrawText($"Expiry: {passport.ExpiryDate:yyyy-MM-dd}", 10, 210, font, paint);
            canvas.DrawText($"Number: {passport.Number}", 10, 250, font, paint);
            canvas.DrawText($"{passport.Line1}", 10, 290, font, paint);
            canvas.DrawText($"{passport.Line2}", 10, 315, font, paint);

            using var img = SKImage.FromBitmap(bitmap);
            using var data = img.Encode(SKEncodedImageFormat.Jpeg, 90);
            return data.ToArray();
        }

        /// <summary>
        /// Saves a passport photo to the local "PassportPhotos" directory, generating a unique file name
        /// based on the person's name, the current timestamp, and a GUID.
        /// </summary>
        /// <param name="photo">
        /// A byte array containing the photo data in JPEG format.
        /// </param>
        /// <param name="person">
        /// The <see cref="Person"/> whose name will be included in the generated file name.
        /// </param>
        /// <returns>
        /// The full file system path where the photo was saved.
        /// </returns>
        /// <remarks>
        /// <para>
        /// The photo is saved inside a <c>PassportPhotos</c> folder located in the application's base directory.  
        /// The file name format is:
        /// <c>{GivenName}_{Surname}_{yyyyMMdd_HHmmss}_{GUID}.jpg</c>, with spaces in names replaced by underscores.
        /// </para>
        /// <para>
        /// The method ensures the target directory exists by creating it if necessary.
        /// </para>
        /// </remarks>
        public static string SavePassportPhoto(byte[] photo, Person person)
        {
            string picturesPath = Path.Combine(AppContext.BaseDirectory, "PassportPhotos");

            Directory.CreateDirectory(picturesPath);

            string safeName = $"{person.GivenName}_{person.Surname}".Replace(" ", "_");
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string fileName = $"{safeName}_{timestamp}_{Guid.NewGuid()}.jpg";

            string fullPath = Path.Combine(picturesPath, fileName);

            File.WriteAllBytes(fullPath, photo);

            return fullPath;
        }
    }
}
