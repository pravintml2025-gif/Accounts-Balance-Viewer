using Adra.Application.Interfaces;

namespace Adra.Application.Interfaces;

/// <summary>
/// Factory interface for creating appropriate file parsers based on file extension
/// </summary>
public interface IFileParserFactory
{
    /// <summary>
    /// Creates a parser for the given file extension
    /// </summary>
    /// <param name="fileExtension">File extension (e.g., ".xlsx", ".csv")</param>
    /// <returns>Appropriate file parser</returns>
    /// <exception cref="NotSupportedException">Thrown when file extension is not supported</exception>
    IFileParser CreateParser(string fileExtension);

    /// <summary>
    /// Gets all supported file extensions
    /// </summary>
    IEnumerable<string> GetSupportedExtensions();
}
