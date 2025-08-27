using OfficeOpenXml;
using Adra.Application.Interfaces;

namespace Adra.Application.Services.FileParser;

/// <summary>
/// Factory for creating appropriate file parsers based on file extension
/// </summary>
public class FileParserFactory : IFileParserFactory
{
    private readonly Dictionary<string, Func<IFileParser>> _parserFactories;

    public FileParserFactory()
    {
        // Set EPPlus license context for non-commercial use
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        _parserFactories = new Dictionary<string, Func<IFileParser>>(StringComparer.OrdinalIgnoreCase)
        {
            { ".xlsx", () => new ExcelFileParser() },
            { ".xls", () => new ExcelFileParser() },
            { ".csv", () => new DelimitedFileParser(',', ".csv", ".txt") },
            { ".txt", () => new DelimitedFileParser(',', ".csv", ".txt") },
            { ".tsv", () => new DelimitedFileParser('\t', ".tsv") }
        };
    }

    public IFileParser CreateParser(string fileExtension)
    {
        var normalizedExtension = fileExtension?.ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(normalizedExtension))
            throw new ArgumentException("File extension cannot be null or empty", nameof(fileExtension));

        if (!_parserFactories.TryGetValue(normalizedExtension, out var factory))
            throw new NotSupportedException($"File type '{fileExtension}' is not supported");

        return factory();
    }

    public IEnumerable<string> GetSupportedExtensions()
    {
        return _parserFactories.Keys;
    }
}
