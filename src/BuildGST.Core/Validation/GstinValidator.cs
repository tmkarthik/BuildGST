using System;
using System.Text.RegularExpressions;
using BuildGST.Abstractions.Interfaces;

namespace BuildGST.Core.Validation;

/// <summary>
/// Validates GSTIN values using format, state code, PAN structure, entity code, and checksum rules.
/// </summary>
public sealed class GstinValidator : IGstinValidator
{
    private const string Charset = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private static readonly Regex PanRegex = new Regex("^[A-Z]{5}[0-9]{4}[A-Z]$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex EntityCodeRegex = new Regex("^[1-9A-Z]$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex ChecksumCharacterRegex = new Regex("^[0-9A-Z]$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly string[] ValidStateCodes =
    {
        "01", "02", "03", "04", "05", "06", "07", "08", "09", "10",
        "11", "12", "13", "14", "15", "16", "17", "18", "19", "20",
        "21", "22", "23", "24", "25", "26", "27", "28", "29", "30",
        "31", "32", "33", "34", "35", "36", "37", "38", "97", "99"
    };

    /// <inheritdoc />
    public bool IsValid(string gstin)
    {
        return string.IsNullOrEmpty(GetValidationError(gstin));
    }

    /// <inheritdoc />
    public string GetValidationError(string gstin)
    {
        var normalized = Normalize(gstin);

        if (normalized.Length != 15)
        {
            return "GSTIN must contain exactly 15 characters.";
        }

        if (!IsValidStateCode(normalized.Substring(0, 2)))
        {
            return "GSTIN state code is invalid.";
        }

        if (!PanRegex.IsMatch(normalized.Substring(2, 10)))
        {
            return "GSTIN PAN structure is invalid.";
        }

        if (!EntityCodeRegex.IsMatch(normalized.Substring(12, 1)))
        {
            return "GSTIN entity code is invalid.";
        }

        if (normalized[13] != 'Z')
        {
            return "GSTIN 14th character must be 'Z'.";
        }

        if (!ChecksumCharacterRegex.IsMatch(normalized.Substring(14, 1)))
        {
            return "GSTIN checksum character is invalid.";
        }

        var expectedChecksum = CalculateChecksum(normalized.Substring(0, 14));
        if (normalized[14] != expectedChecksum)
        {
            return "GSTIN checksum validation failed.";
        }

        return string.Empty;
    }

    internal static char CalculateChecksum(string input)
    {
        if (string.IsNullOrWhiteSpace(input) || input.Length != 14)
        {
            throw new ArgumentException("Checksum input must be 14 characters long.", nameof(input));
        }

        var factor = 2;
        var sum = 0;

        for (var index = input.Length - 1; index >= 0; index--)
        {
            var codePoint = Charset.IndexOf(char.ToUpperInvariant(input[index]));
            if (codePoint < 0)
            {
                throw new ArgumentException("Checksum input contains unsupported characters.", nameof(input));
            }

            var addend = factor * codePoint;
            factor = factor == 2 ? 1 : 2;
            addend = (addend / Charset.Length) + (addend % Charset.Length);
            sum += addend;
        }

        var remainder = sum % Charset.Length;
        var checkCodePoint = (Charset.Length - remainder) % Charset.Length;
        return Charset[checkCodePoint];
    }

    internal static string Normalize(string gstin)
    {
        return (gstin ?? string.Empty).Trim().ToUpperInvariant();
    }

    private static bool IsValidStateCode(string stateCode)
    {
        for (var index = 0; index < ValidStateCodes.Length; index++)
        {
            if (string.Equals(ValidStateCodes[index], stateCode, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
