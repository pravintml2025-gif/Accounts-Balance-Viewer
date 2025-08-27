using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Adra.Application.DTOs;
using Adra.Application.Interfaces;
using Adra.Api.Controllers;

namespace Adra.Tests;

public class GetBalancesFlowTests
{
    private readonly Mock<IGetBalancesService> _mockGetBalancesService;
    private readonly Mock<IProcessBalanceUploadService> _mockProcessBalanceUploadService;
    private readonly BalancesController _controller;

    public GetBalancesFlowTests()
    {
        _mockGetBalancesService = new Mock<IGetBalancesService>();
        _mockProcessBalanceUploadService = new Mock<IProcessBalanceUploadService>();
        _controller = new BalancesController(_mockGetBalancesService.Object, _mockProcessBalanceUploadService.Object);
    }

    [Fact]
    public async Task GetLatest_WithValidBalances_ShouldReturnBalancesList()
    {
        // Arrange
        var expectedBalances = new List<BalanceDto>
        {
            new BalanceDto
            {
                Account = "R&D",
                Amount = 85000.00m,
                Year = 2025,
                Month = 8
            },
            new BalanceDto
            {
                Account = "Canteen",
                Amount = 12500.75m,
                Year = 2025,
                Month = 8
            },
            new BalanceDto
            {
                Account = "CEO's car expenses",
                Amount = 15200.50m,
                Year = 2025,
                Month = 8
            },
            new BalanceDto
            {
                Account = "Marketing",
                Amount = 45000.00m,
                Year = 2025,
                Month = 8
            },
            new BalanceDto
            {
                Account = "Parking fines",
                Amount = 1850.25m,
                Year = 2025,
                Month = 8
            }
        };

        _mockGetBalancesService.Setup(x => x.GetLatestAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedBalances);

        // Act
        var result = await _controller.GetLatest(CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var balances = okResult!.Value as List<BalanceDto>;

        balances.Should().HaveCount(5);
        balances![0].Account.Should().Be("R&D");
        balances[0].Amount.Should().Be(85000.00m);
        balances[1].Account.Should().Be("Canteen");
        balances[1].Amount.Should().Be(12500.75m);
        balances[2].Account.Should().Be("CEO's car expenses");
        balances[2].Amount.Should().Be(15200.50m);
        balances[3].Account.Should().Be("Marketing");
        balances[3].Amount.Should().Be(45000.00m);
        balances[4].Account.Should().Be("Parking fines");
        balances[4].Amount.Should().Be(1850.25m);
    }

    [Fact]
    public async Task GetLatest_NoBalancesAvailable_ShouldReturnEmptyList()
    {
        // Arrange
        var emptyBalances = new List<BalanceDto>();

        _mockGetBalancesService.Setup(x => x.GetLatestAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyBalances);

        // Act
        var result = await _controller.GetLatest(CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var balances = okResult!.Value as List<BalanceDto>;

        balances.Should().BeEmpty();
    }

    [Fact]
    public async Task GetLatest_ServiceThrowsException_ShouldPropagateException()
    {
        // Arrange
        _mockGetBalancesService.Setup(x => x.GetLatestAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _controller.GetLatest(CancellationToken.None));

        exception.Message.Should().Be("Database connection failed");
    }

    [Fact]
    public async Task GetByPeriod_ValidPeriod_ShouldReturnFilteredBalances()
    {
        // Arrange
        var year = 2025;
        var month = 7;
        var expectedBalances = new List<BalanceDto>
        {
            new BalanceDto
            {
                Account = "R&D",
                Amount = 72000.00m,
                Year = 2025,
                Month = 7
            },
            new BalanceDto
            {
                Account = "Canteen",
                Amount = 11500.50m,
                Year = 2025,
                Month = 7
            },
            new BalanceDto
            {
                Account = "Marketing",
                Amount = 38000.75m,
                Year = 2025,
                Month = 7
            }
        };

        _mockGetBalancesService.Setup(x => x.GetByPeriodAsync(year, month, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedBalances);

        // Act
        var result = await _controller.GetByPeriod(year, month, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var balances = okResult!.Value as List<BalanceDto>;

        balances.Should().HaveCount(3);
        balances![0].Year.Should().Be(2025);
        balances[0].Month.Should().Be(7);
        balances[0].Account.Should().Be("R&D");
        balances[1].Account.Should().Be("Canteen");
        balances[2].Account.Should().Be("Marketing");
    }

    [Fact]
    public async Task GetByPeriod_InvalidYear_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidYear = 1999; // Below minimum year 2000
        var month = 8;

        // Act
        var result = await _controller.GetByPeriod(invalidYear, month, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result.Result as BadRequestObjectResult;
        var errorMessage = badRequestResult!.Value!.ToString();
        errorMessage.Should().Contain("Invalid year");
    }

    [Fact]
    public async Task GetByPeriod_InvalidMonth_ShouldReturnBadRequest()
    {
        // Arrange
        var year = 2025;
        var invalidMonth = 13; // Above maximum month 12

        // Act
        var result = await _controller.GetByPeriod(year, invalidMonth, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result.Result as BadRequestObjectResult;
        var errorMessage = badRequestResult!.Value!.ToString();
        errorMessage.Should().Contain("Invalid month");
    }

    [Fact]
    public async Task GetByPeriod_FutureYear_ShouldReturnBadRequest()
    {
        // Arrange
        var futureYear = DateTime.Now.Year + 2; // More than 1 year in future
        var month = 6;

        // Act
        var result = await _controller.GetByPeriod(futureYear, month, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result.Result as BadRequestObjectResult;
        var errorMessage = badRequestResult!.Value!.ToString();
        errorMessage.Should().Contain("Invalid year");
    }
}
