# TravelDocFaker

**TravelDocFaker** is a .NET library for generating fake passport data in **TD3** format (ICAO 9303), including valid MRZ lines, consistent personal data, and a simulated document image.

> ⚠️ **Disclaimer**: This project is for **testing, development, and demonstration purposes only**. It must not be used to produce fake documents for illegal purposes.

---

## ✨ Features

- Generate **realistic fake persons**:
  - Random given and last names (using _Bogus_)
  - Gender, nationality, and age range specified by the user
  - Date of birth consistent with the chosen age
- Generate **TD3 passport**:
  - Valid MRZ (Machine Readable Zone) according to ICAO 9303
  - Correct check digits for all fields
  - Supported countries only:
    - 🇵🇹 Portugal
    - 🇪🇸 Spain
    - 🇬🇧 United Kingdom
    - 🇫🇷 France
- Generate a **simulated passport image** with personal details and MRZ
- Utility methods for check digit calculation and text sanitization

---

## 📦 Installation

Once published to NuGet:

```powershell
dotnet add package TravelDocFaker
```

---

## 🚀 Example Usage

```csharp
using TravelDocFaker.Generators;
using TravelDocFaker.Models;
using System;
using System.IO;

// 1. Create a fake person (male, Portugal, aged between 25 and 35)
var person = PersonGenerator.Create(SupportedCountry.Portugal, Gender.Male, 25, 35);

// 2. Generate a passport with MRZ
var passport = PassportGenerator.BuildTD3(person);

// 3. Generate a simulated passport image
var photoBytes = PhotoGenerator.Generate(person, passport);

// 4. Save the image to a file
// Default path is ./AppContext.BaseDirectory/PassportPhotos
var filename = PhotoGenerator.SavePassportPhoto(photoBytes, person);

Console.WriteLine($"Passport generated: {passport.Number}");
Console.WriteLine(passport.Line1);
Console.WriteLine(passport.Line2);
Console.WriteLine($"Image saved to: {filename}");
```

---

## 🧪 Unit Tests

The project has **NUnit** tests to validate:

- MRZ structure and correct length
- Proper calculation of individual and final check digits
- Consistent generation for all supported countries
- Fixed cases verified against external MRZ validators

Run the tests with:

```powershell
dotnet test
```

---

## 📄 License

Distributed under the MIT License. See `LICENSE` for more details.
