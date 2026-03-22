using System;
using System.Text.RegularExpressions;
using BuildGST.Abstractions.Interfaces;
using BuildGST.Abstractions.Models;

namespace BuildGST.Core.Validation;

public sealed class GstinValidator : IGstinValidator
{
    private const string Charset = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private static readonly Regex GstinRegex = new Regex("^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z][1-9A-Z]Z[0-9A-Z]$", RegexOptions.Compiled);

    public GstinValidationResult Validate(string gstin)
    {
        var normalized = (gstin ?? string.Empty).Trim().ToUpperInvariant();

        if (normalized.Length != 15)
        {
            return new GstinValidationResult(false, normalized, "GSTIN must contain exactly 15 characters.");
        }

        if (!GstinRegex.IsMatch(normalized))
        {
            return new GstinValidationResult(false, normalized, "GSTIN format is invalid.");
        }

        var expectedChecksum = CalculateChecksum(normalized.Substring(0, 14));
        if (normalized[14] != expectedChecksum)
        {
            return new GstinValidationResult(false, normalized, "GSTIN checksum validation failed.");
        }

        return new GstinValidationResult(true, normalized);
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
}
