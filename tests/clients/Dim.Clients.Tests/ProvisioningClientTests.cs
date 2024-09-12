using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using Dim.Clients.Api.Dim;
using Dim.Clients.Api.Dim.Models;
using Dim.Clients.Api.Div;
using Dim.Clients.Api.Div.Models;
using Dim.Clients.Extensions;
using Dim.Clients.Tests.Extensions;
using Dim.Clients.Token;
using FakeItEasy;
using FluentAssertions;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using System.Net;
using System.Text.Json;
using Xunit;

namespace Dim.Clients.Tests;

public class ProvisioningClientTests
{
    #region Initialization

    private readonly IFixture _fixture;

    public ProvisioningClientTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    #endregion

    #region GetOperation

    [Fact]
    public async Task GetOperation_WithStatePending_Returns()
    {
        // Arrange
        var data = new OperationResponse(Guid.NewGuid(), OperationResponseStatus.pending, null, null, null);
        _fixture.ConfigureTokenServiceFixture<ProvisioningClient>(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(data, JsonSerializerExtensions.Options)) });
        var sut = _fixture.Create<ProvisioningClient>();

        // Act
        var response = await sut.GetOperation(Guid.NewGuid(), CancellationToken.None);

        // Assert
        response.Should().Be(data);
    }

    [Fact]
    public async Task GetOperation_WithStatusFailed_ThrowsServiceException()
    {
        // Arrange
        var data = new OperationResponse(Guid.NewGuid(), OperationResponseStatus.failed, null, "test error", null);
        _fixture.ConfigureTokenServiceFixture<ProvisioningClient>(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(data, JsonSerializerExtensions.Options)) });
        var sut = _fixture.Create<ProvisioningClient>();
        Task Act() => sut.GetOperation(Guid.NewGuid(), CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);

        // Assert
        ex.Message.Should().Be("Operation Creation failed with error: test error");
    }

    [Fact]
    public async Task GetOperation_WithValidData_ReturnsCompanyData()
    {
        // Arrange
        var data = new OperationResponse(Guid.NewGuid(), OperationResponseStatus.completed, null, null, _fixture.Create<OperationResponseData>());
        _fixture.ConfigureTokenServiceFixture<ProvisioningClient>(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(data, JsonSerializerExtensions.Options)) });
        var sut = _fixture.Create<ProvisioningClient>();

        // Act
        var result = await sut.GetOperation(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().Be(data);
    }

    #endregion

    #region CreateOperation

    [Fact]
    public async Task CreateOperation_WithNoContent_ThrowsServiceException()
    {
        // Arrange
        _fixture.ConfigureTokenServiceFixture<ProvisioningClient>(new HttpResponseMessage(HttpStatusCode.OK));
        var sut = _fixture.Create<ProvisioningClient>();
        Task Act() => sut.CreateOperation(Guid.NewGuid(), "corp", "application1", "test corp", "https://example.org/did", false, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);

        // Assert
        ex.Message.Should().Contain("The input does not contain any JSON tokens");
    }

    [Fact]
    public async Task CreateOperation_WithNoSpaceLeft_ThrowsConflictException()
    {
        // Arrange
        _fixture.ConfigureTokenServiceFixture<ProvisioningClient>(new HttpResponseMessage(HttpStatusCode.BadRequest));
        var sut = _fixture.Create<ProvisioningClient>();
        Task Act() => sut.CreateOperation(Guid.NewGuid(), "corp", "application1", "test corp", "https://example.org/did", false, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);

        // Assert
        ex.Message.Should().Be("call to external system create-operation failed with statuscode 400");
    }

    [Fact]
    public async Task CreateOperation_WithValidData_ReturnsCompanyData()
    {
        // Arrange
        var operationId = Guid.NewGuid();
        var data = new CreateOperationRequest(operationId);

        _fixture.ConfigureTokenServiceFixture<ProvisioningClient>(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(data, JsonSerializerExtensions.Options))
            });
        var sut = _fixture.Create<ProvisioningClient>();

        // Act
        var result = await sut.CreateOperation(Guid.NewGuid(), "corp", "application1", "test corp", "https://example.org/did", false, CancellationToken.None);

        // Assert
        result.Should().Be(operationId);
    }

    #endregion
}
