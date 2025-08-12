using SkiaSharp;
using TravelDocFaker.Models;

namespace TravelDocFaker.Generators
{
    public class PhotoGenerator
    {
        const int DefaultWidth = 750;
        const int DefaultHeight = 400;
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
