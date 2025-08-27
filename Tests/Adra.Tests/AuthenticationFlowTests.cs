using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Adra.Application.DTOs;
using Adra.Application.Interfaces;
using Adra.Api.Controllers;
using Adra.Infrastructure.Identity;
using Adra.Core.Entities;

namespace Adra.Tests;

public class AuthenticationFlowTests
{
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<IJwtService> _mockJwtService;
    private readonly AuthController _controller;

    public AuthenticationFlowTests()
    {
        _mockUserManager = CreateMockUserManager();
        _mockJwtService = new Mock<IJwtService>();
        _controller = new AuthController(_mockUserManager.Object, _mockJwtService.Object);
    }

    [Fact]
    public async Task Login_ValidAdminCredentials_ShouldReturnTokenWithAdminRole()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Username = "admin",
            Password = "Admin@123"
        };

        var applicationUser = new ApplicationUser
        {
            UserName = "admin",
            Email = "admin@adra.com",
            IsActive = true
        };

        var expectedResponse = new LoginResponse
        {
            AccessToken = "mock-jwt-token-admin",
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            Roles = new List<string> { "Admin" }
        };

        _mockUserManager.Setup(x => x.FindByNameAsync("admin"))
            .ReturnsAsync(applicationUser);
        _mockUserManager.Setup(x => x.CheckPasswordAsync(applicationUser, "Admin@123"))
            .ReturnsAsync(true);
        _mockUserManager.Setup(x => x.GetRolesAsync(applicationUser))
            .ReturnsAsync(new List<string> { "Admin" });
        _mockJwtService.Setup(x => x.GenerateToken(It.IsAny<User>(), It.IsAny<List<string>>()))
            .Returns(expectedResponse);

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as LoginResponse;

        response.Should().NotBeNull();
        response!.AccessToken.Should().Be("mock-jwt-token-admin");
        response.Roles.Should().Contain("Admin");
    }

    [Fact]
    public async Task Login_InvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Username = "wronguser",
            Password = "wrongpass"
        };

        _mockUserManager.Setup(x => x.FindByNameAsync("wronguser"))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = result.Result as UnauthorizedObjectResult;
        unauthorizedResult!.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Login_ValidUserButWrongPassword_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Username = "admin",
            Password = "wrongpassword"
        };

        var applicationUser = new ApplicationUser
        {
            UserName = "admin",
            Email = "admin@adra.com",
            IsActive = true
        };

        _mockUserManager.Setup(x => x.FindByNameAsync("admin"))
            .ReturnsAsync(applicationUser);
        _mockUserManager.Setup(x => x.CheckPasswordAsync(applicationUser, "wrongpassword"))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_InactiveUser_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Username = "inactive",
            Password = "User@123"
        };

        var inactiveUser = new ApplicationUser
        {
            UserName = "inactive",
            Email = "inactive@adra.com",
            IsActive = false  // Inactive user
        };

        _mockUserManager.Setup(x => x.FindByNameAsync("inactive"))
            .ReturnsAsync(inactiveUser);
        _mockUserManager.Setup(x => x.CheckPasswordAsync(inactiveUser, "User@123"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = result.Result as UnauthorizedObjectResult;
        var errorMessage = unauthorizedResult!.Value!.ToString();
        errorMessage.Should().Contain("inactive");
    }

    [Fact]
    public async Task Login_ValidUserCredentials_ShouldReturnTokenWithUserRole()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Username = "john.doe",
            Password = "User@123"
        };

        var applicationUser = new ApplicationUser
        {
            UserName = "john.doe",
            Email = "john.doe@adra.com",
            IsActive = true
        };

        var expectedResponse = new LoginResponse
        {
            AccessToken = "mock-jwt-token-user",
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            Roles = new List<string> { "User" }
        };

        _mockUserManager.Setup(x => x.FindByNameAsync("john.doe"))
            .ReturnsAsync(applicationUser);
        _mockUserManager.Setup(x => x.CheckPasswordAsync(applicationUser, "User@123"))
            .ReturnsAsync(true);
        _mockUserManager.Setup(x => x.GetRolesAsync(applicationUser))
            .ReturnsAsync(new List<string> { "User" });
        _mockJwtService.Setup(x => x.GenerateToken(It.IsAny<User>(), It.IsAny<List<string>>()))
            .Returns(expectedResponse);

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as LoginResponse;

        response.Should().NotBeNull();
        response!.AccessToken.Should().Be("mock-jwt-token-user");
        response.Roles.Should().Contain("User");
        response.Roles.Should().NotContain("Admin");
    }

    private Mock<UserManager<ApplicationUser>> CreateMockUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        var mockUserManager = new Mock<UserManager<ApplicationUser>>(
            store.Object,
            new Mock<IOptions<IdentityOptions>>().Object,
            new Mock<IPasswordHasher<ApplicationUser>>().Object,
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            new Mock<ILookupNormalizer>().Object,
            new Mock<IdentityErrorDescriber>().Object,
            new Mock<IServiceProvider>().Object,
            new Mock<ILogger<UserManager<ApplicationUser>>>().Object);

        return mockUserManager;
    }
}
