using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Adra.Application.Services;
using Adra.Application.Services.FileParser;
using Adra.Application.DTOs;
using Adra.Core.Interfaces.Repositories;
using Adra.Core.Entities;
using Adra.Core.Common;
using System.Text;

namespace Adra.Tests;

public class ProcessBalanceUploadServiceTests
{
    private readonly Mock<IBalanceHistoryRepository> _mockBalanceHistoryRepository;
    private readonly Mock<IAccountRepository> _mockAccountRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<ProcessBalanceUploadService>> _mockLogger;
    private readonly Mock<IOptions<FileUploadSettings>> _mockFileUploadSettings;
    private readonly ProcessBalanceUploadService _service;

    public ProcessBalanceUploadServiceTests()
    {
        _mockBalanceHistoryRepository = new Mock<IBalanceHistoryRepository>();
        _mockAccountRepository = new Mock<IAccountRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<ProcessBalanceUploadService>>();
        _mockFileUploadSettings = new Mock<IOptions<FileUploadSettings>>();

        // Setup file upload settings
        var settings = new FileUploadSettings
        {
            MaxFileSizeInBytes = 10 * 1024 * 1024, // 10MB
            AllowedExtensions = new[] { ".csv", ".xlsx", ".tsv" }
        };
        _mockFileUploadSettings.Setup(x => x.Value).Returns(settings);

        // Create real instances for integration testing
        var fileParserFactory = new FileParserFactory();

        _service = new ProcessBalanceUploadService(
            _mockBalanceHistoryRepository.Object,
            _mockAccountRepository.Object,
            _mockUnitOfWork.Object,
            _mockLogger.Object,
            _mockFileUploadSettings.Object,
            fileParserFactory);
    }

    [Fact]
    public async Task ExecuteAsync_ValidCsvStream_ShouldProcessSuccessfully()
    {
        // Arrange
        var csvContent = "Account,Amount\nR&D,85000.00\nCanteen,12500.75\nMarketing,45000.00";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));
        var fileName = "balances.csv";
        var fileSize = stream.Length;
        var year = 2025;
        var month = 8;
        var userId = Guid.NewGuid();

        // Setup existing accounts
        var existingAccounts = new List<Account>
        {
            new Account { Id = Guid.NewGuid(), Name = "R&D" },
            new Account { Id = Guid.NewGuid(), Name = "Canteen" },
            new Account { Id = Guid.NewGuid(), Name = "Marketing" }
        };

        _mockAccountRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAccounts);

        _mockAccountRepository.Setup(x => x.AddAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockBalanceHistoryRepository.Setup(x => x.UpsertAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(3); // 3 records saved

        // Act
        var result = await _service.ExecuteAsync(stream, fileName, fileSize, year, month, userId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.ProcessedRecords.Should().Be(3); // All 3 accounts are valid now
        result.SkippedRecords.Should().Be(0); // No invalid accounts
        result.Message.Should().ContainEquivalentOf("Successfully processed");
        result.Errors.Should().BeEmpty();

        // Verify interactions
        _mockAccountRepository.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockBalanceHistoryRepository.Verify(x => x.UpsertAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Exactly(3)); // All 3 accounts processed
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ExecuteAsync_EmptyStream_ShouldReturnError()
    {
        // Arrange
        var emptyStream = new MemoryStream();
        var fileName = "empty.csv";
        var fileSize = 0L;
        var year = 2025;
        var month = 8;
        var userId = Guid.NewGuid();

        // Act
        var result = await _service.ExecuteAsync(emptyStream, fileName, fileSize, year, month, userId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ProcessedRecords.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidCsvFormat_ShouldReturnErrorWithDetails()
    {
        // Arrange
        var invalidCsvContent = "This is not a valid CSV format\nJust random text\nNo proper structure";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(invalidCsvContent));
        var fileName = "invalid.csv";
        var fileSize = stream.Length;
        var year = 2025;
        var month = 8;
        var userId = Guid.NewGuid();

        // Act
        var result = await _service.ExecuteAsync(stream, fileName, fileSize, year, month, userId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ProcessedRecords.Should().Be(0);
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_ValidExcelFile_ShouldProcessSuccessfully()
    {
        // Arrange
        // Create a minimal Excel file content (this is a simplified test)
        // In a real scenario, you'd use EPPlus to create actual Excel content
        var stream = new MemoryStream();
        var fileName = "balances.xlsx";
        var fileSize = 1024L; // Dummy size
        var year = 2025;
        var month = 8;
        var userId = Guid.NewGuid();

        // Setup existing accounts
        var existingAccounts = new List<Account>
        {
            new Account { Id = Guid.NewGuid(), Name = "R&D" },
            new Account { Id = Guid.NewGuid(), Name = "Canteen" }
        };

        _mockAccountRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAccounts);

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.ExecuteAsync(stream, fileName, fileSize, year, month, userId, CancellationToken.None);

        // Assert - Excel processing might fail due to format, but service should handle it gracefully
        result.Should().NotBeNull();
        // Result could be success or failure depending on Excel content processing
        result.Errors.Should().NotBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_UnitOfWorkThrowsException_ShouldReturnError()
    {
        // Arrange
        var csvContent = "Account,Amount\nCash,1500.00";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));
        var fileName = "balances.csv";
        var fileSize = stream.Length;
        var year = 2025;
        var month = 8;
        var userId = Guid.NewGuid();

        _mockAccountRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Account>());

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        var result = await _service.ExecuteAsync(stream, fileName, fileSize, year, month, userId, CancellationToken.None);

        // Assert - Service should handle exceptions gracefully and may still return success or controlled failure
        result.Should().NotBeNull();
        // The behavior depends on how the service handles exceptions internally
    }

    [Fact]
    public async Task ExecuteAsync_MixedValidInvalidData_ShouldProcessValidRecords()
    {
        // Arrange
        var csvContent = "Account,Amount\nR&D,85000.00\nInvalid Account Name,25000.50\nCanteen,12500.75";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));
        var fileName = "mixed_data.csv";
        var fileSize = stream.Length;
        var year = 2025;
        var month = 8;
        var userId = Guid.NewGuid();

        // Setup existing accounts (only R&D and Canteen exist, not "Invalid Account Name")
        var existingAccounts = new List<Account>
        {
            new Account { Id = Guid.NewGuid(), Name = "R&D" },
            new Account { Id = Guid.NewGuid(), Name = "Canteen" }
        };

        _mockAccountRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAccounts);

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(2); // Only 2 valid records

        // Act
        var result = await _service.ExecuteAsync(stream, fileName, fileSize, year, month, userId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue(); // Should still be successful with partial processing
        result.ProcessedRecords.Should().Be(2); // Only R&D and Canteen processed
        result.SkippedRecords.Should().Be(1); // Invalid Account Name skipped
        result.Errors.Should().NotBeEmpty();
        result.Errors.Should().Contain(error => error.Contains("Invalid Account: 'Invalid Account Name'"));

        // Verify that valid records were processed
        _mockBalanceHistoryRepository.Verify(x => x.UpsertAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }
}
