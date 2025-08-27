using System.Globalization;
using Adra.Application.DTOs;
using Adra.Application.Interfaces;

namespace Adra.Application.Services.FileParser;

/// <summary>
/// Parser for delimited files (CSV, TSV)
/// </summary>
public class DelimitedFileParser : IFileParser
{
    private readonly char _delimiter;
    private readonly string[] _supportedExtensions;

    public DelimitedFileParser(char delimiter, params string[] supportedExtensions)
    {
        _delimiter = delimiter;
        _supportedExtensions = supportedExtensions;
    }

    public IEnumerable<string> SupportedExtensions => _supportedExtensions;

    public async Task<List<BalanceUploadRecord>> ParseAsync(
        Stream fileStream,
        UploadBalanceResponse response,
        CancellationToken ct)
    {
        var records = new List<BalanceUploadRecord>();

        try
        {
            using var reader = new StreamReader(fileStream);

            var lineNumber = 0;

            while (!reader.EndOfStream)
            {
                ct.ThrowIfCancellationRequested();

                var line = await reader.ReadLineAsync();
                lineNumber++;

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // Skip header row (detect by checking if first field is not a number)
                if (lineNumber == 1 && !IsNumericLine(line, _delimiter))
                {
                    continue;
                }

                try
                {
                    var record = ParseDelimitedLine(line, _delimiter);
                    if (record != null)
                        records.Add(record);
                }
                catch (Exception ex)
                {
                    response.Errors.Add($"Error parsing line {lineNumber}: {ex.Message}");
                }
            }

            if (records.Count == 0)
            {
                response.Errors.Add("No valid records found in the file");
                return records;
            }

            response.Success = true;
        }
        catch (Exception ex)
        {
            response.Errors.Add($"Error reading file: {ex.Message}");
        }

        return records;
    }

    private static bool IsNumericLine(string line, char delimiter)
    {
        var parts = line.Split(delimiter);
        if (parts.Length < 2) return false;

        return decimal.TryParse(parts[1].Trim().Trim('"'), out _);
    }

    private static BalanceUploadRecord? ParseDelimitedLine(string line, char delimiter)
    {
        var parts = line.Split(delimiter);

        if (parts.Length < 2)
            throw new FormatException("Expected at least 2 columns: Account Name, Amount");

        var accountName = parts[0].Trim().Trim('"');
        var amountText = parts[1].Trim().Trim('"');

        return ParseRecord(accountName, amountText);
    }

    private static BalanceUploadRecord? ParseRecord(string accountName, string amountText)
    {
        if (string.IsNullOrWhiteSpace(accountName))
            throw new FormatException("Account name cannot be empty");

        if (string.IsNullOrWhiteSpace(amountText))
            throw new FormatException("Amount cannot be empty");

        if (!decimal.TryParse(amountText, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
            throw new FormatException($"Invalid amount format: {amountText}");

        return new BalanceUploadRecord
        {
            AccountName = accountName,
            Amount = amount
        };
    }
}
