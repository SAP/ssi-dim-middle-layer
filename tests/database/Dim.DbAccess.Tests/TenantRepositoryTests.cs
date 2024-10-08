using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using Dim.DbAccess.Repositories;
using Dim.DbAccess.Tests.Setup;
using Dim.Entities;
using Dim.Entities.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Extensions.AssemblyFixture;

namespace Dim.DbAccess.Tests;

public class TenantRepositoryTests : IAssemblyFixture<TestDbFixture>
{
    private readonly TestDbFixture _dbTestDbFixture;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "xUnit1041:Fixture arguments to test classes must have fixture sources", Justification = "<Pending>")]
    public TenantRepositoryTests(TestDbFixture testDbFixture)
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region CreateTenant

    [Fact]
    public async Task CreateTenant_CreatesSuccessfully()
    {
        // Arrange
        var (sut, dbContext) = await CreateSutWithContext();
        var changeTracker = dbContext.ChangeTracker;

        // Act
        var result = sut.CreateTenant("test corp", "BPNL00001TEST", "https://example.org/test", false, Guid.NewGuid(), Guid.NewGuid());

        // Assert
        changeTracker.HasChanges().Should().BeTrue();
        changeTracker.Entries().Should().HaveCount(1)
            .And.AllSatisfy(x =>
            {
                x.State.Should().Be(EntityState.Added);
                x.Entity.Should().BeOfType<Tenant>();
            });
        changeTracker.Entries().Select(x => x.Entity).Cast<Tenant>()
            .Should().Satisfy(
                x => x.Id == result.Id && x.CompanyName == "test corp"
            );
    }

    #endregion

    #region AttachAndModifyTenant

    [Fact]
    public async Task AttachAndModifyTenant_WithExistingTenant_UpdatesStatus()
    {
        // Arrange
        var (sut, dbContext) = await CreateSutWithContext();

        // Act
        sut.AttachAndModifyTenant(new Guid("48f35f84-8d98-4fbd-ba80-8cbce5eeadb5"),
            existing =>
            {
                existing.Bpn = "BPNL000001TEST";
            },
            modify =>
            {
                modify.Bpn = "BPNL000001NEW";
            }
        );

        // Assert
        var changeTracker = dbContext.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().NotBeEmpty();
        changedEntries.Should().HaveCount(1);
        var changedEntity = changedEntries.Single();
        changedEntity.State.Should().Be(EntityState.Modified);
        changedEntity.Entity.Should().BeOfType<Tenant>().Which.Bpn.Should().Be("BPNL000001NEW");
    }

    #endregion

    #region GetTenantDataForProcessId

    [Fact]
    public async Task GetTenantDataForProcessId_WithExistingTenant_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetTenantDataForProcessId(new Guid("dd371565-9489-4907-a2e4-b8cbfe7a8cd1"));

        // Assert
        result.Exists.Should().BeTrue();
        result.Bpn.Should().Be("BPNL000001ISSUER");
        result.CompanyName.Should().Be("issuer company");
        result.TenantId.Should().Be(new Guid("5c9a4f56-0609-49a5-ab86-dd8f93dfd3fa"));
    }

    [Fact]
    public async Task GetTenantDataForProcessId_WithoutExistingTenant_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetTenantDataForProcessId(Guid.NewGuid());

        // Assert
        result.Exists.Should().BeFalse();
        result.Bpn.Should().BeNull();
        result.CompanyName.Should().BeNull();
        result.TenantId.Should().BeEmpty();
    }

    #endregion

    #region GetHostingUrlAndIsIssuer

    [Fact]
    public async Task GetHostingUrlAndIsIssuer_WithExistingTenant_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetHostingUrlAndIsIssuer(new Guid("5c9a4f56-0609-49a5-ab86-dd8f93dfd3fa"));

        // Assert
        result.IsIssuer.Should().BeTrue();
        result.HostingUrl.Should().Be("https://example.org/BPNL000001ISSUER");
    }

    #endregion

    #region GetTenantForBpn

    [Fact]
    public async Task GetTenantForBpn_WithExistingTenant_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetTenantForBpn("BPNL000001ISSUER");

        // Assert
        result.Exists.Should().BeTrue();
        result.TenantId.Should().Be(new Guid("5c9a4f56-0609-49a5-ab86-dd8f93dfd3fa"));
    }

    #endregion

    #region IsTenantExisting

    [Fact]
    public async Task IsTenantExisting_WithExisting_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.IsTenantExisting("issuer company", "BPNL000001ISSUER");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsTenantExisting_WithoutExisting_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.IsTenantExisting("issuer company", "BPNL000NOTEXISTING");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetOperationId

    [Fact]
    public async Task GetOperationId_WithExisting_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetOperationId(new Guid("5c9a4f56-0609-49a5-ab86-dd8f93dfd3fa"));

        // Assert
        result.Should().Be(new Guid("6dcac248-57ab-4309-9477-ee21586b3738"));
    }

    #endregion

    #region GetCompanyRequestData

    [Fact]
    public async Task GetCompanyRequestData_WithExisting_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetCompanyRequestData(new Guid("5c9a4f56-0609-49a5-ab86-dd8f93dfd3fa"));

        // Assert
        result.BaseUrl.Should().Be("https://example.org/base");
    }

    #endregion

    #region GetCompanyAndWalletDataForBpn

    [Fact]
    public async Task GetCompanyAndWalletDataForBpn_WithExistingTechnicalUser_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetCompanyAndWalletDataForBpn("BPNL000001ISSUER");

        // Assert
        result.Exists.Should().BeTrue();
        result.CompanyId.Should().Be(new Guid("6dcac248-57ab-4309-9477-ee21586b3666"));
    }

    #endregion

    #region GetStatusListCreationData

    [Fact]
    public async Task GetStatusListCreationData_WithExisting_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetStatusListCreationData(new Guid("5c9a4f56-0609-49a5-ab86-dd8f93dfd3fa"));

        // Assert
        result.CompanyId.Should().Be(new Guid("6dcac248-57ab-4309-9477-ee21586b3666"));
        result.BaseUrl.Should().Be("https://example.org/base");
    }

    #endregion

    #region GetCallbackData

    [Fact]
    public async Task GetCallbackData_WithExisting_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetCallbackData(new Guid("5c9a4f56-0609-49a5-ab86-dd8f93dfd3fa"));

        // Assert
        result.Bpn.Should().Be("BPNL000001ISSUER");
        result.BaseUrl.Should().Be("https://example.org/base");
    }

    #endregion

    #region GetDownloadUrlAndIsIssuer

    [Fact]
    public async Task GetDownloadUrlAndIsIssuer_WithExisting_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetDownloadUrlAndIsIssuer(new Guid("5c9a4f56-0609-49a5-ab86-dd8f93dfd3fa"));

        // Assert
        result.IsIssuer.Should().BeTrue();
    }

    #endregion

    #region GetWalletProcessForTenant

    [Fact]
    public async Task GetWalletProcessForTenant_WithExisting_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetWalletProcessForTenant("BPNL0000001CORP", "test corp");

        // Assert
        result.Should().NotBeNull();
        result!.ProcessId.Should().Be(new Guid("dd371565-9489-4907-a2e4-b8cbfe7a8cd2"));
    }

    #endregion

    private async Task<(TenantRepository sut, DimDbContext dbContext)> CreateSutWithContext()
    {
        var context = await _dbTestDbFixture.GetDbContext();
        var sut = new TenantRepository(context);
        return (sut, context);
    }

    private async Task<TenantRepository> CreateSut()
    {
        var context = await _dbTestDbFixture.GetDbContext();
        return new TenantRepository(context);
    }
}
