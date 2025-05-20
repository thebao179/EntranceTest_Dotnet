using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using DotNetEntranceExam.Services;
using DotNetEntranceExam.Entities;
using DotNetEntranceExam.Repositories;
using DotNetEntranceExam.DTOs;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<ITokenRepository> _tokenRepo = new();
    private readonly IConfiguration _config;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        var settings = new Dictionary<string, string>
        {
            { "Jwt:Key", "thisIsAveryUltimateStrongJWTkeyyyyyy" }
        };

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(settings!)
            .Build();

        _authService = new AuthService(_userRepo.Object, _tokenRepo.Object, _config);
    }

    [Fact]
    public async Task SignUpAsync_Should_CreateUser_Success()
    {
        var request = new SignUpRequest
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Password = "password123."
        };

        _userRepo.Setup(r => r.GetByEmailAsync(request.Email)).ReturnsAsync((User?)null);
        _userRepo.Setup(r => r.CreateAsync(It.IsAny<User>()))
                 .ReturnsAsync((User u) => u);

        // Act
        var result = await _authService.SignUpAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(request.Email);
        result.DisplayName.Should().Be("Test User");
    }

    [Fact]
    public async Task SignInAsync_Should_ReturnToken_Success()
    {
        var password = "password123.";
        var user = new User
        {
            Id = 1,
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Hash = BCrypt.Net.BCrypt.HashPassword(password)
        };

        _userRepo.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);
        _tokenRepo.Setup(r => r.AddAsync(It.IsAny<Token>())).Returns(Task.CompletedTask);

        var request = new SignInRequest
        {
            Email = user.Email,
            Password = password
        };

        // Act
        var result = await _authService.SignInAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.User!.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task RefreshTokenAsync_Should_ReturnNewToken_WhenValid()
    {
        // Arrange
        var refreshToken = Guid.NewGuid().ToString("N");
        var futureDate = DateTime.UtcNow.AddDays(10);

        var token = new Token
        {
            RefreshToken = refreshToken,
            ExpiresIn = futureDate.ToString(),
            User = new User
            {
                Id = 10,
                FirstName = "user",
                LastName = "test",
                Email = "test1@gmail.com"
            }
        };

        _tokenRepo.Setup(r => r.GetByRefreshTokenAsync(refreshToken)).ReturnsAsync(token);
        _tokenRepo.Setup(r => r.RemoveAsync(token)).Returns(Task.CompletedTask);
        _tokenRepo.Setup(r => r.AddAsync(It.IsAny<Token>())).Returns(Task.CompletedTask);

        // Act
        var result = await _authService.RefreshTokenAsync(refreshToken);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBe(refreshToken); 
    }

    [Fact]
    public async Task RefreshTokenAsync_Should_ReturnNull_WhenTokenIsExpired()
    {
        var token = new Token
        {
            RefreshToken = "expired123",
            ExpiresIn = DateTime.UtcNow.AddDays(-1).ToString(),
            User = new User { Id = 5, FirstName = "user", LastName = "test", Email = "usertest1@gmail.com" }
        };

        _tokenRepo.Setup(r => r.GetByRefreshTokenAsync("expired123")).ReturnsAsync(token);
        _tokenRepo.Setup(r => r.RemoveAsync(token)).Returns(Task.CompletedTask);

        var result = await _authService.RefreshTokenAsync("expired123");

        result.Should().BeNull();
    }

}
