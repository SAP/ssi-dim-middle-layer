using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using Dim.Clients.Api.Dim;
using Dim.Clients.Api.Dim.Models;
using Dim.Clients.Extensions;
using Dim.Clients.Tests.Extensions;
using Dim.Clients.Token;
using Dim.DbAccess.Models;
using FluentAssertions;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using System.Net;
using System.Text.Json;
using Xunit;

namespace Dim.Clients.Tests;

public class DimClientTests
{
    #region Initialization

    private readonly IFixture _fixture;

    public DimClientTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    #endregion

    #region GetCompanyData

    [Fact]
    public async Task GetCompanyData_WithNoContent_ThrowsServiceException()
    {
        // Arrange
        _fixture.ConfigureTokenServiceFixture<DimClient>(new HttpResponseMessage(HttpStatusCode.OK));
        var sut = _fixture.Create<DimClient>();
        Task Act() => sut.GetCompanyData(_fixture.Create<BasicAuthSettings>(), "https://example.org", "tenant", "app", CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);

        // Assert
        ex.Message.Should().Contain("The input does not contain any JSON tokens");
    }

    [Fact]
    public async Task GetCompanyData_WithNoCompany_ThrowsConflictException()
    {
        // Arrange
        var data = new CompanyIdentitiesResponse(Enumerable.Empty<CompanyData>());
        _fixture.ConfigureTokenServiceFixture<DimClient>(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(data, JsonSerializerExtensions.Options)) });
        var sut = _fixture.Create<DimClient>();
        Task Act() => sut.GetCompanyData(_fixture.Create<BasicAuthSettings>(), "https://example.org", "tenant", "app", CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("There is no matching company");
    }

    [Fact]
    public async Task GetCompanyData_WithValidData_ReturnsCompanyData()
    {
        // Arrange
        var companyData = new CompanyData(Guid.NewGuid(), "https://example.org/download");
        var data = new CompanyIdentitiesResponse(Enumerable.Repeat(companyData, 1));
        _fixture.ConfigureTokenServiceFixture<DimClient>(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(data, JsonSerializerExtensions.Options)) });
        var sut = _fixture.Create<DimClient>();

        // Act
        var result = await sut.GetCompanyData(_fixture.Create<BasicAuthSettings>(), "https://example.org", "tenant", "app", CancellationToken.None);

        // Assert
        result.CompanyId.Should().Be(companyData.CompanyId);
        result.DownloadUrl.Should().Be(companyData.DownloadUrl);
    }

    #endregion

    #region GetStatusList

    [Fact]
    public async Task GetStatusList_WithNoContent_ThrowsServiceException()
    {
        // Arrange
        _fixture.ConfigureTokenServiceFixture<DimClient>(new HttpResponseMessage(HttpStatusCode.OK));
        var sut = _fixture.Create<DimClient>();
        Task Act() => sut.GetStatusList(_fixture.Create<BasicAuthSettings>(), "https://example.org", Guid.NewGuid(), StatusListType.BitstringStatusList, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);

        // Assert
        ex.Message.Should().Contain("The input does not contain any JSON tokens");
    }

    [Fact]
    public async Task GetStatusList_WithNoSpaceLeft_ThrowsConflictException()
    {
        // Arrange
        var data = new StatusListListResponse(1, new[] { new StatusListResponse(Guid.NewGuid().ToString(), "test", "BitstringStatusList", "test", 1024, 0) });
        _fixture.ConfigureTokenServiceFixture<DimClient>(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(data, JsonSerializerExtensions.Options)) });
        var sut = _fixture.Create<DimClient>();
        Task Act() => sut.GetStatusList(_fixture.Create<BasicAuthSettings>(), "https://example.org", Guid.NewGuid(), StatusListType.BitstringStatusList, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("There is no status list with remaining space, please create a new one.");
    }

    [Fact]
    public async Task GetStatusList_WithValidData_ReturnsCompanyData()
    {
        // Arrange
        var data = new StatusListListResponse(1, new[] { new StatusListResponse(Guid.NewGuid().ToString(), "test", "testCred", "BitstringStatusList", 1024, 100) });
        _fixture.ConfigureTokenServiceFixture<DimClient>(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(data, JsonSerializerExtensions.Options)) });
        var sut = _fixture.Create<DimClient>();

        // Act
        var result = await sut.GetStatusList(_fixture.Create<BasicAuthSettings>(), "https://example.org", Guid.NewGuid(), StatusListType.BitstringStatusList, CancellationToken.None);

        // Assert
        result.Should().Be("testCred");
    }

    #endregion

    #region CreateStatusList

    [Fact]
    public async Task CreateStatusList_WithNoContent_ThrowsServiceException()
    {
        // Arrange
        HttpRequestMessage? request = null;
        _fixture.ConfigureTokenServiceFixture<DimClient>(
            new HttpResponseMessage(HttpStatusCode.OK),
            requestMessage => request = requestMessage);
        var sut = _fixture.Create<DimClient>();
        Task Act() => sut.CreateStatusList(_fixture.Create<BasicAuthSettings>(), "https://example.org", Guid.NewGuid(), StatusListType.StatusList2021, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);

        // Assert
        ex.Message.Should().Contain("The input does not contain any JSON tokens");
    }

    [Fact]
    public async Task CreateStatusList_WithNoSpaceLeft_ThrowsConflictException()
    {
        // Arrange
        _fixture.ConfigureTokenServiceFixture<DimClient>(new HttpResponseMessage(HttpStatusCode.BadRequest));
        var sut = _fixture.Create<DimClient>();
        Task Act() => sut.CreateStatusList(_fixture.Create<BasicAuthSettings>(), "https://example.org", Guid.NewGuid(), StatusListType.StatusList2021, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);

        // Assert
        ex.Message.Should().Be("call to external system assign-application failed with statuscode 400");
    }

    [Fact]
    public async Task CreateStatusList_WithValidData_ReturnsCompanyData()
    {
        // Arrange
        var recovationVc = Guid.NewGuid().ToString();
        var data = new CreateStatusListResponse(Guid.NewGuid(), new RevocationVc(recovationVc));

        _fixture.ConfigureTokenServiceFixture<DimClient>(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(data, JsonSerializerExtensions.Options))
            });
        var sut = _fixture.Create<DimClient>();

        // Act
        var result = await sut.CreateStatusList(_fixture.Create<BasicAuthSettings>(), "https://example.org", Guid.NewGuid(), StatusListType.StatusList2021, CancellationToken.None);

        // Assert
        result.Should().Be(recovationVc);
    }

    #endregion
}
