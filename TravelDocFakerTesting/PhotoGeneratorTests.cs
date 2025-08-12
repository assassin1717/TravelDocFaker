using SkiaSharp;
using TravelDocFaker.Generators;
using TravelDocFaker.Models;
using TravelDocFaker.Utils;

namespace TravelDocFakerTesting
{
    [TestFixture]
    public class PhotoGeneratorTests
    {
        private Person _person = null;
        private Passport _passport = null;

        [SetUp]
        public void SetUp()
        {
            _person = new Person(
                givenName: "TIAGO",
                surname: "BARBOSA",
                gender: Gender.Male,
                dateOfBirth: new DateOnly(1995, 6, 6),
                countryCode3: "PRT"
            );

            var mrz = PassportGenerator.BuildTD3(_person, passportNumber: "P9664258R", expiry: new DateOnly(2007, 6, 6));
            _passport = new Passport("P9664258R", new DateOnly(2007, 6, 6), mrz.Line1, mrz.Line2);
        }

        [Test]
        public void Generate_DefaultDimensions_ShouldDecode_AndHaveSize()
        {
            var bytes = PhotoGenerator.Generate(_person, _passport);
            Assert.IsNotNull(bytes);
            Assert.Greater(bytes.Length, 0);

            using var bmp = SKBitmap.Decode(bytes);
            Assert.IsNotNull(bmp, "Bitmap decode failed");
            Assert.Multiple(() =>
            {
                Assert.That(bmp.Width, Is.EqualTo(750));
                Assert.That(bmp.Height, Is.EqualTo(400));
            });
        }

        [Test]
        public void Generate_CustomDimensions_ShouldDecode_AndRespectSize()
        {
            var bytes = PhotoGenerator.Generate(_person, _passport, width: 1600, height: 500);
            using var bmp = SKBitmap.Decode(bytes);
            Assert.IsNotNull(bmp);
            Assert.Multiple(() =>
            {
                Assert.That(bmp.Width, Is.EqualTo(1600));
                Assert.That(bmp.Height, Is.EqualTo(500));
            });
        }

        [Test]
        public void SavePassportPhoto_ShouldCreateFolder_AndNotOverwrite()
        {
            var bytes = PhotoGenerator.Generate(_person, _passport, 600, 300);

            var path1 = PhotoGenerator.SavePassportPhoto(bytes, _person);
            var path2 = PhotoGenerator.SavePassportPhoto(bytes, _person);

            Assert.IsTrue(File.Exists(path1), "First save file missing");
            Assert.IsTrue(File.Exists(path2), "Second save file missing");
            Assert.That(path2, Is.Not.EqualTo(path1), "Files should have unique names");

            TryDelete(path1);
            TryDelete(path2);

            static void TryDelete(string p)
            {
                try { if (File.Exists(p)) File.Delete(p); } catch { /* ignore */ }
            }
        }
    }
}
