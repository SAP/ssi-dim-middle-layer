using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using Dim.DbAccess.Repositories;
using Dim.DbAccess.Tests.Setup;
using Dim.Entities;
using Dim.Entities.Entities;
using Dim.Entities.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Extensions.AssemblyFixture;

namespace Dim.DbAccess.Tests;

public class TenantRepositoryTests : IAssemblyFixture<TestDbFixture>
{
    private readonly TestDbFixture _dbTestDbFixture;

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

    #region CreateTechnicalUser

    [Fact]
    public async Task CreateTechnicalUser_CreatesSuccessfully()
    {
        // Arrange
        var (sut, dbContext) = await CreateSutWithContext();
        var changeTracker = dbContext.ChangeTracker;

        // Act
        sut.CreateTenantTechnicalUser(Guid.NewGuid(), "testUser", Guid.NewGuid(), Guid.NewGuid());

        // Assert
        changeTracker.HasChanges().Should().BeTrue();
        changeTracker.Entries().Should().HaveCount(1)
            .And.AllSatisfy(x =>
            {
                x.State.Should().Be(EntityState.Added);
                x.Entity.Should().BeOfType<TechnicalUser>();
            });
        changeTracker.Entries().Select(x => x.Entity).Cast<TechnicalUser>()
            .Should().Satisfy(
                x => x.TechnicalUserName == "testUser"
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

    #region AttachAndModifyTechnicalUser

    [Fact]
    public async Task AttachAndModifyTechnicalUser_WithExistingTechnicalUser_UpdatesStatus()
    {
        // Arrange
        var (sut, dbContext) = await CreateSutWithContext();

        // Act
        sut.AttachAndModifyTechnicalUser(new Guid("48f35f84-8d98-4fbd-ba80-8cbce5eeadb5"),
            existing =>
            {
                existing.ClientId = "cl1";
            },
            modify =>
            {
                modify.ClientId = "clNew";
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
        changedEntity.Entity.Should().BeOfType<TechnicalUser>().Which.ClientId.Should().Be("clNew");
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

    #region GetTenantDataForTechnicalUserProcessId

    [Fact]
    public async Task GetTenantDataForTechnicalUserProcessId_WithExistingTechnicalUser_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetTenantDataForTechnicalUserProcessId(new Guid("e64393ad-a885-45ad-8e7b-265ef1b4c691"));

        // Assert
        result.Exists.Should().BeTrue();
        result.Bpn.Should().Be("BPNL000001ISSUER");
        result.CompanyName.Should().Be("issuer company");
        result.TechnicalUserId.Should().Be(new Guid("abb769d6-337f-4d1f-9f42-5230541a2d51"));
    }

    #endregion

    #region GetTechnicalUserCallbackData

    [Fact]
    public async Task GetTechnicalUserCallbackData_WithExistingTechnicalUser_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetTechnicalUserCallbackData(new Guid("abb769d6-337f-4d1f-9f42-5230541a2d51"));

        // Assert
        result.ExternalId.Should().Be(new Guid("a140e80f-f9fb-4e68-bd34-52943622c63d"));
    }

    #endregion

    #region GetTechnicalUserForBpn

    [Fact]
    public async Task GetTechnicalUserForBpn_WithExistingTechnicalUser_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetTechnicalUserForBpn("BPNL000001ISSUER", "dim-sa-1");

        // Assert
        result.Exists.Should().BeTrue();
        result.TechnicalUserId.Should().Be(new Guid("abb769d6-337f-4d1f-9f42-5230541a2d51"));
        result.ProcessId.Should().Be(new Guid("e64393ad-a885-45ad-8e7b-265ef1b4c691"));
    }

    #endregion

    #region GetExternalIdForTechnicalUser

    [Fact]
    public async Task GetExternalIdForTechnicalUser_WithExistingTechnicalUser_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetExternalIdForTechnicalUser(new Guid("abb769d6-337f-4d1f-9f42-5230541a2d51"));

        // Assert
        result.Should().Be(new Guid("a140e80f-f9fb-4e68-bd34-52943622c63d"));
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

    #region CreateTechnicalUser

    [Fact]
    public async Task RemoveTechnicalUser_CreatesSuccessfully()
    {
        // Arrange
        var technicalUserId = Guid.NewGuid();
        var (sut, dbContext) = await CreateSutWithContext();
        var changeTracker = dbContext.ChangeTracker;

        // Act
        sut.RemoveTechnicalUser(technicalUserId);

        // Assert
        changeTracker.HasChanges().Should().BeTrue();
        changeTracker.Entries().Should().HaveCount(1)
            .And.AllSatisfy(x =>
            {
                x.State.Should().Be(EntityState.Deleted);
                x.Entity.Should().BeOfType<TechnicalUser>();
            });
        changeTracker.Entries().Select(x => x.Entity).Cast<TechnicalUser>()
            .Should().Satisfy(
                x => x.Id == technicalUserId
            );
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
