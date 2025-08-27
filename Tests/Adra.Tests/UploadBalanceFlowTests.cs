using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Http.Features;
using Adra.Application.DTOs;
using Adra.Application.Interfaces;
using Adra.Api.Controllers;
using System.Text;

namespace Adra.Tests;

public class UploadBalanceFlowTests
{
    private readonly Mock<IGetBalancesService> _mockGetBalancesService;
    private readonly Mock<IProcessBalanceUploadService> _mockProcessBalanceUploadService;
    private readonly BalancesController _controller;

    public UploadBalanceFlowTests()
    {
        _mockGetBalancesService = new Mock<IGetBalancesService>();
        _mockProcessBalanceUploadService = new Mock<IProcessBalanceUploadService>();
        _controller = new BalancesController(_mockGetBalancesService.Object, _mockProcessBalanceUploadService.Object);

        // Setup controller context with authenticated user
        SetupControllerContext();
    }

    [Fact]
    public async Task Upload_ValidCsvFile_ShouldProcessSuccessfully()
    {
        // Arrange
        var csvContent = "Account,Amount\nR&D,85000.00\nCanteen,12500.75\nMarketing,45000.00";
        var mockFile = CreateMockFile("balances.csv", csvContent);

        var expectedResult = new UploadBalanceResponse
        {
            Success = true,
            ProcessedRecords = 3,
            SkippedRecords = 0,
            Message = "Upload completed successfully",
            Errors = new List<string>()
        };

        _mockProcessBalanceUploadService.Setup(x => x.ExecuteAsync(
            It.IsAny<Stream>(),
            It.IsAny<string>(),
            It.IsAny<long>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<Guid>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _controller.Upload(mockFile, 2025, 8, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var uploadResult = okResult!.Value as UploadBalanceResponse;

        uploadResult!.Success.Should().BeTrue();
        uploadResult.ProcessedRecords.Should().Be(3);
        uploadResult.SkippedRecords.Should().Be(0);
        uploadResult.Message.Should().Be("Upload completed successfully");
        uploadResult.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Upload_EmptyFile_ShouldReturnBadRequest()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.Length).Returns(0);
        mockFile.Setup(f => f.FileName).Returns("empty.csv");

        // Act
        var result = await _controller.Upload(mockFile.Object, 2025, 8, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result.Result as BadRequestObjectResult;
        var errorMessage = badRequestResult!.Value!.ToString();
        errorMessage.Should().Contain("File is required");
    }

    [Fact]
    public async Task Upload_NullFile_ShouldReturnBadRequest()
    {
        // Arrange
        IFormFile nullFile = null!;

        // Act
        var result = await _controller.Upload(nullFile, 2025, 8, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result.Result as BadRequestObjectResult;
        var errorMessage = badRequestResult!.Value!.ToString();
        errorMessage.Should().Contain("File is required");
    }

    [Fact]
    public async Task Upload_InvalidYear_ShouldReturnBadRequest()
    {
        // Arrange
        var csvContent = "Account,Amount\nCash,1500.00";
        var mockFile = CreateMockFile("balances.csv", csvContent);

        // Act
        var result = await _controller.Upload(mockFile, 1999, 8, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result.Result as BadRequestObjectResult;
        var errorMessage = badRequestResult!.Value!.ToString();
        errorMessage.Should().Contain("Year cannot be before 2000");
    }

    [Fact]
    public async Task Upload_FutureYear_ShouldReturnBadRequest()
    {
        // Arrange
        var csvContent = "Account,Amount\nCash,1500.00";
        var mockFile = CreateMockFile("balances.csv", csvContent);
        var futureYear = DateTime.Now.Year + 1;

        // Act
        var result = await _controller.Upload(mockFile, futureYear, 8, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result.Result as BadRequestObjectResult;
        var errorMessage = badRequestResult!.Value!.ToString();
        errorMessage.Should().Contain("Cannot upload balance data for future years");
    }

    [Fact]
    public async Task Upload_FutureMonth_ShouldReturnBadRequest()
    {
        // Arrange
        var csvContent = "Account,Amount\nCash,1500.00";
        var mockFile = CreateMockFile("balances.csv", csvContent);
        var currentYear = DateTime.Now.Year;
        var futureMonth = DateTime.Now.Month + 2;

        // Only test if future month is valid (≤12)
        if (futureMonth <= 12)
        {
            // Act
            var result = await _controller.Upload(mockFile, currentYear, futureMonth, CancellationToken.None);

            // Assert
            result.Result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result.Result as BadRequestObjectResult;
            var errorMessage = badRequestResult!.Value!.ToString();
            errorMessage.Should().Contain("Cannot upload balance data for future months");
        }
    }

    [Fact]
    public async Task Upload_InvalidMonth_ShouldReturnBadRequest()
    {
        // Arrange
        var csvContent = "Account,Amount\nR&D,85000.00";
        var mockFile = CreateMockFile("balances.csv", csvContent);

        // Act
        var result = await _controller.Upload(mockFile, 2025, 13, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result.Result as BadRequestObjectResult;
        var errorMessage = badRequestResult!.Value!.ToString();
        errorMessage.Should().Contain("month");
    }

    [Fact]
    public async Task Upload_ServiceReturnsError_ShouldReturnBadRequest()
    {
        // Arrange
        var csvContent = "Invalid CSV Content with wrong format";
        var mockFile = CreateMockFile("invalid.csv", csvContent);

        var errorResult = new UploadBalanceResponse
        {
            Success = false,
            ProcessedRecords = 0,
            SkippedRecords = 0,
            Message = "Invalid file format",
            Errors = new List<string> { "Header row missing", "Invalid data format" }
        };

        _mockProcessBalanceUploadService.Setup(x => x.ExecuteAsync(
            It.IsAny<Stream>(),
            It.IsAny<string>(),
            It.IsAny<long>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<Guid>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(errorResult);

        // Act
        var result = await _controller.Upload(mockFile, 2025, 8, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result.Result as BadRequestObjectResult;
        var uploadResult = badRequestResult!.Value as UploadBalanceResponse;

        uploadResult!.Success.Should().BeFalse();
        uploadResult.Message.Should().Be("Invalid file format");
        uploadResult.Errors.Should().HaveCount(2);
    }

    [Fact]
    public async Task Upload_ServiceThrowsException_ShouldReturn500()
    {
        // Arrange
        var csvContent = "Account,Amount\nCash,1500.00";
        var mockFile = CreateMockFile("balances.csv", csvContent);

        _mockProcessBalanceUploadService.Setup(x => x.ExecuteAsync(
            It.IsAny<Stream>(),
            It.IsAny<string>(),
            It.IsAny<long>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<Guid>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        var result = await _controller.Upload(mockFile, 2025, 8, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    private IFormFile CreateMockFile(string fileName, string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var mockFile = new Mock<IFormFile>();

        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.Length).Returns(bytes.Length);
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(bytes));

        return mockFile.Object;
    }

    private void SetupControllerContext()
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, "admin"),
            new Claim(ClaimTypes.Role, "Admin")
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = new Mock<HttpContext>();
        httpContext.Setup(x => x.User).Returns(claimsPrincipal);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext.Object
        };
    }
}
