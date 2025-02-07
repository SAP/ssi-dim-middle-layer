using Dim.DbAccess.Repositories;
using Dim.DbAccess.Tests.Setup;
using Dim.Entities;
using Dim.Entities.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Extensions.AssemblyFixture;

namespace Dim.DbAccess.Tests;

public class TechnicalUserRepositoryTests(TestDbFixture testDbFixture)
    : IAssemblyFixture<TestDbFixture>
{
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

    #region GetTechnicalUserNameAndWalletId

    [Fact]
    public async Task GetTechnicalUserNameAndWalletId_WithExistingTechnicalUser_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetTechnicalUserNameAndWalletId(new Guid("abb769d6-337f-4d1f-9f42-5230541a2d51"));

        // Assert
        result.WalletId.Should().BeNull();
        result.TechnicalUserName.Should().Be("dim-sa-1");
    }

    #endregion

    #region GetOperationIdForTechnicalUser

    [Fact]
    public async Task GetOperationIdForTechnicalUser_WithExistingTechnicalUser_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetOperationIdForTechnicalUser(new Guid("abb769d6-337f-4d1f-9f42-5230541a2d51"));

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetOperationAndExternalIdForTechnicalUser

    [Fact]
    public async Task GetOperationAndExternalIdForTechnicalUser_WithExistingTechnicalUser_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetOperationAndExternalIdForTechnicalUser(new Guid("abb769d6-337f-4d1f-9f42-5230541a2d51"));

        // Assert
        result.OperationId.Should().BeNull();
        result.ExternalId.Should().Be(new Guid("a140e80f-f9fb-4e68-bd34-52943622c63d"));
    }

    #endregion

    #region GetTechnicalUserDataAndWalletId

    [Fact]
    public async Task GetTechnicalUserDataAndWalletId_WithExistingTechnicalUser_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetServiceKeyAndWalletId(new Guid("abb769d6-337f-4d1f-9f42-5230541a2d51"));

        // Assert
        result.WalletId.Should().BeNull();
    }

    #endregion

    #region GetTechnicalUserDataAndWalletId

    [Fact]
    public async Task GetTechnicalUserProcess_WithExistingTechnicalUser_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetTechnicalUserProcess("BPNL000001ISSUER", "issuer company", "dim-sa-1");

        // Assert
        result.Should().NotBeNull();
        result!.ProcessId.Should().Be(new Guid("e64393ad-a885-45ad-8e7b-265ef1b4c691"));
    }

    #endregion

    #region GetWalletIdAndNameForTechnicalUser

    [Fact]
    public async Task GetWalletIdAndNameForTechnicalUser_WithExistingTechnicalUser_ReturnsExpected()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = await sut.GetWalletIdAndNameForTechnicalUser(new Guid("abb769d6-337f-4d1f-9f42-5230541a2d51"));

        // Assert
        result.Should().NotBeNull();
        result!.TechnicalUserName.Should().Be("dim-sa-1");
        result!.WalletId.Should().BeNull();
    }

    #endregion

    private async Task<(TechnicalUserRepository sut, DimDbContext dbContext)> CreateSutWithContext()
    {
        var context = await testDbFixture.GetDbContext();
        var sut = new TechnicalUserRepository(context);
        return (sut, context);
    }

    private async Task<TechnicalUserRepository> CreateSut()
    {
        var context = await testDbFixture.GetDbContext();
        return new TechnicalUserRepository(context);
    }
}
