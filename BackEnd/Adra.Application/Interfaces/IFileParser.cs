using Adra.Application.DTOs;

namespace Adra.Application.Interfaces;

/// <summary>
/// Interface for parsing different file types into balance upload records
/// </summary>
public interface IFileParser
{
    /// <summary>
    /// Parses the file stream and returns a list of balance upload records
    /// </summary>
    /// <param name="fileStream">The file stream to parse</param>
    /// <param name="response">Response object to collect errors and status</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of parsed balance upload records</returns>
    Task<List<BalanceUploadRecord>> ParseAsync(
        Stream fileStream,
        UploadBalanceResponse response,
        CancellationToken ct);

    /// <summary>
    /// Gets the supported file extensions for this parser
    /// </summary>
    IEnumerable<string> SupportedExtensions { get; }
}
