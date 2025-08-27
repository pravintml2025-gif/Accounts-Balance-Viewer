using Adra.Application.DTOs;

namespace Adra.Application.Interfaces;

public interface IProcessBalanceUploadService
{
    Task<UploadBalanceResponse> ExecuteAsync(
        Stream fileStream,
        string fileName,
        long fileSize,
        int year,
        int month,
        Guid userId,
        CancellationToken ct);
}
