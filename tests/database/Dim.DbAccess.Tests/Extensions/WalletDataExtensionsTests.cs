using Dim.DbAccess.Extensions;
using Dim.DbAccess.Models;
using FluentAssertions;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Xunit;

namespace Dim.DbAccess.Tests.Extensions;

public class WalletDataExtensionsTests
{
    [Fact]
    public void ValidateData_WithMissingTokenAddress_ThrowsConflictException()
    {
        // Arrange
        var walletData = new WalletData(null, null, null, null, null);
        void Act() => walletData.ValidateData();

        // Act
        var ex = Assert.Throws<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("TokenAddress must not be null");
    }

    [Fact]
    public void ValidateData_WithMissingClientId_ThrowsConflictException()
    {
        // Arrange
        var walletData = new WalletData("https://example.org/token", null, null, null, null);
        void Act() => walletData.ValidateData();

        // Act
        var ex = Assert.Throws<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("ClientId must not be null");
    }

    [Fact]
    public void ValidateData_WithMissingSecret_ThrowsConflictException()
    {
        // Arrange
        var walletData = new WalletData("https://example.org/token", "cl1", null, null, null);
        void Act() => walletData.ValidateData();

        // Act
        var ex = Assert.Throws<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("Secret must not be null");
    }

    [Fact]
    public void ValidateData_WithMissingVector_ThrowsConflictException()
    {
        // Arrange
        var walletData = new WalletData("https://example.org/token", "cl1", "test"u8.ToArray(), null, null);
        void Act() => walletData.ValidateData();

        // Act
        var ex = Assert.Throws<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("Vector must not be null");
    }

    [Fact]
    public void ValidateData_WithMissingMode_ThrowsConflictException()
    {
        // Arrange
        var walletData = new WalletData("https://example.org/token", "cl1", "test"u8.ToArray(), "test"u8.ToArray(), null);
        void Act() => walletData.ValidateData();

        // Act
        var ex = Assert.Throws<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("EncryptionMode must not be null");
    }
}
