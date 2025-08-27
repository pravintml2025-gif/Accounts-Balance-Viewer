using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Adra.Application.DTOs;
using Adra.Application.Interfaces;
using Adra.Core.Interfaces.Repositories;
using Adra.Core.Entities;
using Adra.Core.Common;

namespace Adra.Application.Services;

public class ProcessBalanceUploadService : IProcessBalanceUploadService
{
    private readonly IBalanceHistoryRepository _balanceHistoryRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProcessBalanceUploadService> _logger;
    private readonly FileUploadSettings _fileUploadSettings;
    private readonly IFileParserFactory _fileParserFactory;

    public ProcessBalanceUploadService(
        IBalanceHistoryRepository balanceHistoryRepository,
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        ILogger<ProcessBalanceUploadService> logger,
        IOptions<FileUploadSettings> fileUploadSettings,
        IFileParserFactory fileParserFactory)
    {
        _balanceHistoryRepository = balanceHistoryRepository;
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _fileUploadSettings = fileUploadSettings.Value;
        _fileParserFactory = fileParserFactory;
    }

    public async Task<UploadBalanceResponse> ExecuteAsync(
        Stream fileStream,
        string fileName,
        long fileSize,
        int year,
        int month,
        Guid userId,
        CancellationToken ct)
    {
        var response = new UploadBalanceResponse();

        try
        {
            // Validate file
            if (!IsValidFile(fileName, fileSize, response))
                return response;

            // Parse file using factory pattern
            var records = await ParseFileAsync(fileStream, fileName, response, ct);

            if (!response.Success && records.Count == 0)
                return response;

            // Get all existing accounts for lookup
            var existingAccounts = await _accountRepository.GetAllAsync(ct);
            var accountLookup = existingAccounts.ToDictionary(a => a.Name, a => a, StringComparer.OrdinalIgnoreCase);

            // Process records
            var processedCount = 0;
            var skippedCount = 0;
            var invalidAccounts = new List<string>();

            foreach (var record in records)
            {
                try
                {
                    // Check if account exists
                    if (!accountLookup.TryGetValue(record.AccountName, out var account))
                    {
                        invalidAccounts.Add(record.AccountName);
                        response.Errors.Add($"Invalid Account: '{record.AccountName}' - Account does not exist in the system");
                        skippedCount++;

                        _logger.LogWarning("Invalid account found during upload: {AccountName}", record.AccountName);
                        continue;
                    }

                    // Update or Insert balance history for existing account only
                    await _balanceHistoryRepository.UpsertAsync(
                        account.Id,
                        year,
                        month,
                        record.Amount,
                        userId,
                        ct);

                    processedCount++;
                    _logger.LogDebug("Updated balance for account: {AccountName} with amount: {Amount}",
                        record.AccountName, record.Amount);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to process record for account: {AccountName}", record.AccountName);
                    response.Errors.Add($"Failed to process account '{record.AccountName}': {ex.Message}");
                    skippedCount++;
                }
            }

            await _unitOfWork.SaveChangesAsync(ct);

            // Set success only if we processed some records
            response.Success = processedCount > 0;
            response.ProcessedRecords = processedCount;
            response.SkippedRecords = skippedCount;

            // Create comprehensive message
            if (processedCount > 0 && skippedCount > 0)
            {
                response.Message = $"Partially successful: {processedCount} records processed, {skippedCount} records skipped";
                if (invalidAccounts.Count > 0)
                {
                    response.Message += $". Invalid accounts found: {string.Join(", ", invalidAccounts.Take(5))}";
                    if (invalidAccounts.Count > 5)
                        response.Message += $" and {invalidAccounts.Count - 5} more";
                }
            }
            else if (processedCount > 0)
            {
                response.Message = $"Successfully processed {processedCount} records";
            }
            else
            {
                response.Message = $"No records processed. {skippedCount} records skipped due to invalid accounts";
            }

            _logger.LogInformation(
                "Balance upload completed. Processed: {ProcessedCount}, Skipped: {SkippedCount}, Invalid Accounts: {InvalidCount}, Year: {Year}, Month: {Month}",
                processedCount, skippedCount, invalidAccounts.Count, year, month);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing balance upload for {Year}-{Month}", year, month);
            response.Errors.Add($"Upload processing failed: {ex.Message}");
            response.Success = false;
        }

        return response;
    }

    private bool IsValidFile(string fileName, long fileSize, UploadBalanceResponse response)
    {
        if (string.IsNullOrWhiteSpace(fileName) || fileSize == 0)
        {
            response.Errors.Add("File is required and cannot be empty");
            return false;
        }

        if (!_fileUploadSettings.IsValidFileSize(fileSize))
        {
            response.Errors.Add($"File size cannot exceed {_fileUploadSettings.MaxFileSizeInBytes / (1024 * 1024)}MB");
            return false;
        }

        var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();

        if (!_fileUploadSettings.IsValidExtension(fileExtension))
        {
            var allowedExtensions = string.Join(", ", _fileUploadSettings.AllowedExtensions);
            response.Errors.Add($"Only the following file types are allowed: {allowedExtensions}");
            return false;
        }

        return true;
    }

    private async Task<List<BalanceUploadRecord>> ParseFileAsync(
        Stream fileStream,
        string fileName,
        UploadBalanceResponse response,
        CancellationToken ct)
    {
        try
        {
            var fileExtension = Path.GetExtension(fileName);
            var parser = _fileParserFactory.CreateParser(fileExtension);
            return await parser.ParseAsync(fileStream, response, ct);
        }
        catch (NotSupportedException ex)
        {
            response.Errors.Add(ex.Message);
            response.Success = false;
            return new List<BalanceUploadRecord>();
        }
        catch (Exception ex)
        {
            response.Errors.Add($"Error creating parser for file: {ex.Message}");
            response.Success = false;
            return new List<BalanceUploadRecord>();
        }
    }
}
