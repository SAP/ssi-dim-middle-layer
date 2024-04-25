using Dim.Clients.Api.Cf;
using Dim.DbAccess;
using Dim.DbAccess.Repositories;
using Dim.Entities.Entities;
using Dim.Entities.Enums;
using DimProcess.Library.Callback;
using DimProcess.Library.DependencyInjection;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using System.Security.Cryptography;

namespace DimProcess.Library.Tests;

public class TechnicalUserProcessHandlerTests
{
    private readonly ICallbackService _callbackService;
    private readonly IFixture _fixture;
    private readonly IDimRepositories _repositories;
    private readonly ITenantRepository _tenantRepositories;
    private readonly IOptions<TechnicalUserSettings> _options;
    private readonly ICfClient _cfClient;
    private readonly TechnicalUserProcessHandler _sut;

    public TechnicalUserProcessHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _repositories = A.Fake<IDimRepositories>();
        _tenantRepositories = A.Fake<ITenantRepository>();

        A.CallTo(() => _repositories.GetInstance<ITenantRepository>()).Returns(_tenantRepositories);

        _cfClient = A.Fake<ICfClient>();
        _callbackService = A.Fake<ICallbackService>();
        _options = Options.Create(new TechnicalUserSettings
        {
            EncryptionConfigIndex = 0,
            EncryptionConfigs = new[]
            {
                new EncryptionModeConfig
                {
                    Index = 0,
                    CipherMode = CipherMode.CBC,
                    PaddingMode = PaddingMode.PKCS7,
                    EncryptionKey = "2c68516f23467028602524534824437e417e253c29546c563c2f5e3d485e7667"
                }
            }
        });

        _sut = new TechnicalUserProcessHandler(_repositories, _cfClient, _callbackService, _options);
    }

    #region CreateSubaccount

    [Fact]
    public async Task CreateSubaccount_WithValidData_ReturnsExpected()
    {
        // Arrange
        var technicalUserId = Guid.NewGuid();
        var serviceBindingId = Guid.NewGuid();
        var technicalUser = new TechnicalUser(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "test", Guid.NewGuid());
        A.CallTo(() => _tenantRepositories.GetSpaceIdAndTechnicalUserName(technicalUserId))
            .Returns(new ValueTuple<Guid?, string>(Guid.NewGuid(), "test"));
        A.CallTo(() => _cfClient.GetServiceBinding("test", A<Guid>._, A<string>._, A<CancellationToken>._))
            .Returns(serviceBindingId);
        A.CallTo(() => _cfClient.GetServiceBindingDetails(serviceBindingId, A<CancellationToken>._))
            .Returns(new ServiceCredentialBindingDetailResponse(new Credentials("https://example.org", new Uaa("cl1", "test123", "https://example.org/test", "https://example.org/api"))));
        A.CallTo(() => _tenantRepositories.AttachAndModifyTechnicalUser(A<Guid>._, A<Action<TechnicalUser>>._, A<Action<TechnicalUser>>._))
            .Invokes((Guid _, Action<TechnicalUser>? initialize, Action<TechnicalUser> modify) =>
            {
                initialize?.Invoke(technicalUser);
                modify(technicalUser);
            });

        // Act
        var result = await _sut.GetTechnicalUserData("test", technicalUserId, CancellationToken.None);

        // Assert
        result.modified.Should().BeFalse();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().ContainSingle().Which.Should().Be(ProcessStepTypeId.SEND_TECHNICAL_USER_CALLBACK);
        technicalUser.EncryptionMode.Should().NotBeNull().And.Be(0);
        technicalUser.ClientId.Should().Be("cl1");
    }

    #endregion

}
