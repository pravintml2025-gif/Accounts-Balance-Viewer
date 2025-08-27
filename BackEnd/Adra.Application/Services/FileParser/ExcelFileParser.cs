using System.Globalization;
using OfficeOpenXml;
using Adra.Application.DTOs;
using Adra.Application.Interfaces;

namespace Adra.Application.Services.FileParser;

/// <summary>
/// Parser for Excel files (.xlsx, .xls)
/// </summary>
public class ExcelFileParser : IFileParser
{
    public IEnumerable<string> SupportedExtensions => new[] { ".xlsx", ".xls" };

    public Task<List<BalanceUploadRecord>> ParseAsync(
        Stream fileStream,
        UploadBalanceResponse response,
        CancellationToken ct)
    {
        var records = new List<BalanceUploadRecord>();

        try
        {
            using var package = new ExcelPackage(fileStream);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();

            if (worksheet == null)
            {
                response.Errors.Add("Excel file contains no worksheets");
                return Task.FromResult(records);
            }

            var rowCount = worksheet.Dimension?.Rows ?? 0;
            if (rowCount == 0)
            {
                response.Errors.Add("Excel worksheet is empty");
                return Task.FromResult(records);
            }

            var startRow = 1;

            // Check if first row is a header
            if (IsHeaderRow(worksheet, 1))
            {
                startRow = 2;
            }

            for (var row = startRow; row <= rowCount; row++)
            {
                ct.ThrowIfCancellationRequested();

                var accountName = worksheet.Cells[row, 1].Text?.Trim();
                var amountText = worksheet.Cells[row, 2].Text?.Trim();

                if (string.IsNullOrWhiteSpace(accountName) && string.IsNullOrWhiteSpace(amountText))
                    continue; // Skip empty rows

                try
                {
                    var record = ParseRecord(accountName ?? "", amountText ?? "");
                    if (record != null)
                        records.Add(record);
                }
                catch (Exception ex)
                {
                    response.Errors.Add($"Error parsing row {row}: {ex.Message}");
                }
            }

            if (records.Count == 0)
            {
                response.Errors.Add("No valid records found in the Excel file");
                return Task.FromResult(records);
            }

            response.Success = true;
        }
        catch (Exception ex)
        {
            response.Errors.Add($"Error reading Excel file: {ex.Message}");
        }

        return Task.FromResult(records);
    }

    private static bool IsHeaderRow(ExcelWorksheet worksheet, int row)
    {
        var firstCell = worksheet.Cells[row, 1].Text?.Trim().ToLower();
        var secondCell = worksheet.Cells[row, 2].Text?.Trim();

        // Check if first column contains typical header text and second column is not numeric
        return (firstCell?.Contains("account") == true || firstCell?.Contains("name") == true) &&
               !decimal.TryParse(secondCell, out _);
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
