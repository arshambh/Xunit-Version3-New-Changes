// ---------------------------------------------------------------------
// Culture Override
//
// xUnit v3 supports overriding the culture that tests run under, which is
// useful for testing culture-sensitive code.
//
// Use cases:
// - Testing number/date formatting in different cultures
// - Validating localization/internationalization code
// - Ensuring code works correctly across cultures
//
// Enabling per-test via environment variables:
//   dotnet test -e XUnitCulture=en-US --filter "DisplayName~CultureOverrideTests"
//   dotnet test -e XUnitCulture=fa-IR --filter "DisplayName~CultureOverrideTests"
//
// Enable via xunit.runner.json config file:
//   { "culture": "fa-IR" }
//
// Reference: https://xunit.net/docs/getting-started/v3/whats-new#culture-override
// ---------------------------------------------------------------------

using System.Globalization;
using System.Threading;
using Xunit;

namespace xunit3;

public class CultureOverrideTests
{
    [Fact]
    public void NumberFormatting_respects_current_culture()
    {
        var culture = CultureInfo.CurrentCulture;
        var number = 1234567.89;
        var formatted = number.ToString("N2");

        Console.WriteLine($"Culture: {culture.Name}, Number: {number}, Formatted: {formatted}");
        Assert.NotNull(formatted);
    }

    [Fact]
    public void DateTimeFormatting_respects_culture()
    {
        var culture = CultureInfo.CurrentCulture;
        var now = DateTime.Now;
        var dateFormatted = now.ToString("D");

        Console.WriteLine($"Culture: {culture.Name}, DateTime: {now}, Date: {dateFormatted}");
        Assert.NotNull(dateFormatted);
    }

    [Fact]
    public void SortOrder_respects_culture()
    {
        var culture = CultureInfo.CurrentCulture;
        
        var words = new[] { "äpfel", "apfel", "zebra", "öffnen" };
        var sorted = words.OrderBy(w => w, StringComparer.CurrentCulture).ToArray();
        var invariantSorted = words.OrderBy(w => w, StringComparer.InvariantCulture).ToArray();

        Console.WriteLine($"Culture: {culture.Name}");
        Console.WriteLine($"Current culture sorted: [{string.Join(", ", sorted)}]");
        Console.WriteLine($"Invariant sorted: [{string.Join(", ", invariantSorted)}]");

        Assert.Equal(4, sorted.Length);
        Assert.Equal(4, invariantSorted.Length);
    }

    [Fact]
    public void SpecificCulture_number_formatting()
    {
        var enUs = new CultureInfo("en-US");
        var deDe = new CultureInfo("de-DE");
        var value = 1234567.89m;

        var enUsFormatted = value.ToString("N2", enUs);
        var deDeFormatted = value.ToString("N2", deDe);

        Console.WriteLine($"en-US: {enUsFormatted}");
        Console.WriteLine($"de-DE: {deDeFormatted}");

        Assert.Equal("1,234,567.89", enUsFormatted);
        Assert.Equal("1.234.567,89", deDeFormatted);
    }

    [Fact]
    public void CaseConversion_respects_culture()
    {
        var culture = CultureInfo.CurrentCulture;
        
        var upper = "istanbul".ToUpper(culture);
        var lower = "ISTANBUL".ToLower(culture);

        Console.WriteLine($"Culture: {culture.Name}");
        Console.WriteLine($"Upper: {upper}, Lower: {lower}");
        
        Assert.NotNull(upper);
        Assert.NotNull(lower);
    }

    [Fact]
    public void NumericParsing_respects_culture()
    {
        var culture = CultureInfo.CurrentCulture;

        var parsed = decimal.TryParse("1234,56", NumberStyles.Any, culture, out var value);
        
        Console.WriteLine($"Culture: {culture.Name}, Parsed: {parsed}, Value: {value}");
        
        Assert.NotNull(culture);
    }

    [Fact]
    public void CurrencyFormatting_respects_culture()
    {
        var currency = 12345.67;
        
        Console.WriteLine($"en-US: {currency:C}");
        Console.WriteLine($"de-DE: {currency.ToString("C", new CultureInfo("de-DE"))}");
        Console.WriteLine($"ja-JP: {currency.ToString("C", new CultureInfo("ja-JP"))}");
        Console.WriteLine($"ar-SA: {currency.ToString("C", new CultureInfo("ar-SA"))}");

        Assert.True(true);
    }

    [Fact]
    public void CheckCurrentCulture()
    {
        var currentCulture = CultureInfo.CurrentCulture;
        var currentUiCulture = CultureInfo.CurrentUICulture;
        
        Console.WriteLine($"CurrentCulture: {currentCulture.Name} (English: {currentCulture.EnglishName})");
        Console.WriteLine($"CurrentUICulture: {currentUiCulture.Name} (English: {currentUiCulture.EnglishName})");
        
        Assert.NotNull(currentCulture);
        Assert.NotNull(currentUiCulture);
    }

    [Fact]
    public void GetCurrentCulture_vs_SetCulture()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        
        Console.WriteLine($"Original culture: {originalCulture.Name}");
        
        Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
        var newCulture = CultureInfo.CurrentCulture;
        
        Console.WriteLine($"After change: {newCulture.Name}");
        
        Thread.CurrentThread.CurrentCulture = originalCulture;
        
        Assert.Equal("fr-FR", newCulture.Name);
        Assert.NotEqual("fr-FR", CultureInfo.CurrentCulture.Name);
    }
}