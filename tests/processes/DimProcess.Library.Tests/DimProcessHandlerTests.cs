/********************************************************************************
 * Copyright (c) 2024 BMW Group AG
 * Copyright 2024 SAP SE or an SAP affiliate company and ssi-dim-middle-layer contributors.
 *
 * See the NOTICE file(s) distributed with this work for additional
 * information regarding copyright ownership.
 *
 * This program and the accompanying materials are made available under the
 * terms of the Apache License, Version 2.0 which is available at
 * https://www.apache.org/licenses/LICENSE-2.0.
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 * SPDX-License-Identifier: Apache-2.0
 ********************************************************************************/

using Dim.Clients.Api.Dim;
using Dim.Clients.Api.Dim.Models;
using Dim.Clients.Api.Div;
using Dim.Clients.Api.Div.Models;
using Dim.Clients.Token;
using Dim.DbAccess;
using Dim.DbAccess.Models;
using Dim.DbAccess.Repositories;
using Dim.Entities.Entities;
using Dim.Entities.Enums;
using DimProcess.Library.Callback;
using DimProcess.Library.DependencyInjection;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json;

namespace DimProcess.Library.Tests;

public class DimProcessHandlerTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _processId = Guid.NewGuid();
    private readonly Guid _operatorId = Guid.NewGuid();
    private const string TenantName = "testCorp";

    private readonly ITenantRepository _tenantRepositories;
    private readonly IProvisioningClient _provisioningClient;
    private readonly IDimClient _dimClient;
    private readonly ICallbackService _callbackService;

    private readonly DimProcessHandler _sut;
    private readonly IFixture _fixture;
    private readonly DimHandlerSettings _settings;

    public DimProcessHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var repositories = A.Fake<IDimRepositories>();
        _tenantRepositories = A.Fake<ITenantRepository>();

        A.CallTo(() => repositories.GetInstance<ITenantRepository>()).Returns(_tenantRepositories);

        _provisioningClient = A.Fake<IProvisioningClient>();
        _dimClient = A.Fake<IDimClient>();
        _callbackService = A.Fake<ICallbackService>();
        _settings = new DimHandlerSettings
        {
            ApplicationName = "catena-x-portal",
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
        };

        var messageHandler = A.Fake<HttpMessageHandler>();
        A.CallTo(messageHandler)
            .Where(x => x.Method.Name == "SendAsync")
            .WithReturnType<Task<HttpResponseMessage>>()
            .Returns(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"id\": \"did:web:example:org:123TEST\"}") });
        var httpClient = new HttpClient(messageHandler) { BaseAddress = new Uri("http://localhost") };

        var httpClientFactory = _fixture.Freeze<Fake<IHttpClientFactory>>();
        A.CallTo(() => httpClientFactory.FakedObject.CreateClient("didDocumentDownload")).Returns(httpClient);
        _sut = new DimProcessHandler(repositories, _provisioningClient, _dimClient, _callbackService, httpClientFactory.FakedObject, Options.Create(_settings));
    }

    #region CreateOperation

    [Fact]
    public async Task CreateOperation_WithDidLocationNull_ThrowsUnexpectedConditionException()
    {
        // Arrange
        A.CallTo(() => _tenantRepositories.GetHostingUrlAndIsIssuer(_tenantId))
            .Returns((true, null));
        Task Act() => _sut.CreateWallet(_tenantId, TenantName, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);

        // Assert
        ex.Message.Should().Be("DidDocumentLocation must always be set");
    }

    [Fact]
    public async Task CreateOperation_WithValidData_ReturnsExpected()
    {
        // Arrange
        var operationId = Guid.NewGuid();
        var tenant = new Tenant(_tenantId, "test", "Corp", "https://example.org/did", false, _processId, _operatorId);
        A.CallTo(() => _tenantRepositories.AttachAndModifyTenant(_tenantId, A<Action<Tenant>>._, A<Action<Tenant>>._))
            .Invokes((Guid _, Action<Tenant>? initialize, Action<Tenant> modify) =>
            {
                initialize?.Invoke(tenant);
                modify(tenant);
            });
        A.CallTo(() => _provisioningClient.CreateOperation(A<Guid>._, TenantName, A<string>._, A<string>._, A<string>._, A<bool>._, A<CancellationToken>._))
            .Returns(operationId);

        // Act
        var result = await _sut.CreateWallet(_tenantId, TenantName, CancellationToken.None);

        // Assert
        result.modified.Should().BeFalse();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().ContainSingle().Which.Should().Be(ProcessStepTypeId.CHECK_OPERATION);
        tenant.OperationId.Should().Be(operationId);
    }

    #endregion

    #region CheckOperation

    [Fact]
    public async Task CheckOperation_WithoutOperationId_ThrowsUnexpectedException()
    {
        // Arrange
        A.CallTo(() => _tenantRepositories.GetOperationId(_tenantId))
            .Returns<Guid?>(null);
        Task Act() => _sut.CheckOperation(_tenantId, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);

        // Assert
        ex.Message.Should().Be("OperationId must always be set");
    }

    [Fact]
    public async Task CheckOperation_WithCompletedAndDataNull_ThrowsUnexpectedException()
    {
        // Arrange
        var operationId = Guid.NewGuid();
        A.CallTo(() => _tenantRepositories.GetOperationId(_tenantId))
            .Returns(operationId);
        A.CallTo(() => _provisioningClient.GetOperation(A<Guid>._, A<CancellationToken>._))
            .Returns(new OperationResponse(operationId, OperationResponseStatus.completed, null, null, null));
        Task Act() => _sut.CheckOperation(_tenantId, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);

        // Assert
        ex.Message.Should().Be($"Data should never be null when in status {OperationResponseStatus.completed}");
    }

    [Fact]
    public async Task CheckOperation_WithPending_StaysInTodo()
    {
        // Arrange
        var operationId = Guid.NewGuid();
        A.CallTo(() => _tenantRepositories.GetOperationId(_tenantId))
            .Returns(operationId);
        A.CallTo(() => _provisioningClient.GetOperation(A<Guid>._, A<CancellationToken>._))
            .Returns(new OperationResponse(operationId, OperationResponseStatus.pending, null, null, null));

        // Act
        var result = await _sut.CheckOperation(_tenantId, CancellationToken.None);

        // Assert
        result.modified.Should().BeFalse();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.TODO);
        result.nextStepTypeIds.Should().BeNull();
    }

    [Fact]
    public async Task CheckOperation_WithValid_UpdatesTenant()
    {
        // Arrange
        var operationId = Guid.NewGuid();
        var customerWalletId = Guid.NewGuid();
        var responseData = new OperationResponseData(
            customerWalletId,
            Guid.NewGuid().ToString(),
            "test name",
            new ServiceKey(
                new ServiceUaa("https://example.org/api", "https://example.org", "cl1", "test123"),
                "https://example.org/test",
                "test"));
        var tenant = new Tenant(_tenantId, "test", "Corp", "https://example.org/did", false, _processId, _operatorId);
        A.CallTo(() => _tenantRepositories.AttachAndModifyTenant(_tenantId, A<Action<Tenant>>._, A<Action<Tenant>>._))
            .Invokes((Guid _, Action<Tenant>? initialize, Action<Tenant> modify) =>
            {
                initialize?.Invoke(tenant);
                modify(tenant);
            });
        A.CallTo(() => _tenantRepositories.GetOperationId(_tenantId))
            .Returns(operationId);
        A.CallTo(() => _provisioningClient.GetOperation(A<Guid>._, A<CancellationToken>._))
            .Returns(new OperationResponse(operationId, OperationResponseStatus.completed, null, null, responseData));

        // Act
        var result = await _sut.CheckOperation(_tenantId, CancellationToken.None);

        // Assert
        result.modified.Should().BeFalse();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().ContainSingle().And.Satisfy(x => x == ProcessStepTypeId.GET_COMPANY);
        tenant.WalletId.Should().Be(customerWalletId);
        tenant.BaseUrl.Should().Be(responseData.ServiceKey.Url);
        tenant.TokenAddress.Should().Be(responseData.ServiceKey.Uaa.Url);
        tenant.ClientId.Should().Be(responseData.ServiceKey.Uaa.ClientId);
    }

    #endregion

    #region GetCompany

    [Fact]
    public async Task GetCompany_WithDidLocationNull_ThrowsUnexpectedConditionException()
    {
        // Arrange
        A.CallTo(() => _tenantRepositories.GetCompanyRequestData(_tenantId))
            .Returns((null, GetWalletData()));
        Task Act() => _sut.GetCompany(_tenantId, TenantName, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);

        // Assert
        ex.Message.Should().Be("BaseAddress must not be null");
    }

    [Fact]
    public async Task GetCompany_WithValidData_ReturnsExpected()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var tenant = new Tenant(_tenantId, "test", "Corp", "https://example.org/did", false, _processId, _operatorId);
        var baseUrl = "https://example.org/base";
        var downloadUrl = "https://example.org/download";
        A.CallTo(() => _tenantRepositories.GetCompanyRequestData(_tenantId))
            .Returns((baseUrl, GetWalletData()));
        A.CallTo(() => _tenantRepositories.AttachAndModifyTenant(_tenantId, A<Action<Tenant>>._, A<Action<Tenant>>._))
            .Invokes((Guid _, Action<Tenant>? initialize, Action<Tenant> modify) =>
            {
                initialize?.Invoke(tenant);
                modify(tenant);
            });
        A.CallTo(() => _dimClient.GetCompanyData(A<BasicAuthSettings>._, baseUrl, TenantName, _settings.ApplicationName, A<CancellationToken>._))
            .Returns(new CompanyData(companyId, downloadUrl));

        // Act
        var result = await _sut.GetCompany(_tenantId, TenantName, CancellationToken.None);

        // Assert
        result.modified.Should().BeFalse();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().ContainSingle().Which.Should().Be(ProcessStepTypeId.GET_DID_DOCUMENT);
        tenant.CompanyId.Should().Be(companyId);
        tenant.DidDownloadUrl.Should().Be(downloadUrl);
    }

    #endregion

    #region GetDidDocument

    [Fact]
    public async Task GetDidDocument_WithDownloadUrlNull_ThrowsUnexpectedConditionException()
    {
        // Arrange
        A.CallTo(() => _tenantRepositories.GetDownloadUrlAndIsIssuer(_tenantId))
            .Returns((null, false));
        Task Act() => _sut.GetDidDocument(_tenantId, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);

        // Assert
        ex.Message.Should().Be("DownloadUrl must not be null");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetDidDocument_WithValidData_ReturnsExpected(bool isIssuer)
    {
        // Arrange
        var tenant = new Tenant(_tenantId, "test", "Corp", "https://example.org/did", false, _processId, _operatorId);
        const string DownloadUrl = "https://example.org/download";
        A.CallTo(() => _tenantRepositories.GetDownloadUrlAndIsIssuer(_tenantId))
            .Returns((DownloadUrl, isIssuer));
        A.CallTo(() => _tenantRepositories.AttachAndModifyTenant(_tenantId, A<Action<Tenant>>._, A<Action<Tenant>>._))
            .Invokes((Guid _, Action<Tenant>? initialize, Action<Tenant> modify) =>
            {
                initialize?.Invoke(tenant);
                modify(tenant);
            });

        // Act
        var result = await _sut.GetDidDocument(_tenantId, CancellationToken.None);

        // Assert
        result.modified.Should().BeFalse();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().ContainSingle().Which.Should().Be(isIssuer ? ProcessStepTypeId.CREATE_STATUS_LIST : ProcessStepTypeId.SEND_CALLBACK);
        tenant.Did.Should().Be("did:web:example:org:123TEST");
    }

    #endregion

    #region CreateStatusList

    [Fact]
    public async Task CreateStatusList_WithCompanyIdNull_ThrowsUnexpectedConditionException()
    {
        // Arrange
        A.CallTo(() => _tenantRepositories.GetStatusListCreationData(_tenantId))
            .Returns((null, null, GetWalletData()));
        Task Act() => _sut.CreateStatusList(_tenantId, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);

        // Assert
        ex.Message.Should().Be("CompanyId must not be null");
    }

    [Fact]
    public async Task CreateStatusList_WithBaseUrlNull_ThrowsUnexpectedConditionException()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        A.CallTo(() => _tenantRepositories.GetStatusListCreationData(_tenantId))
            .Returns((companyId, null, GetWalletData()));
        Task Act() => _sut.CreateStatusList(_tenantId, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);

        // Assert
        ex.Message.Should().Be("BaseUrl must not be null");
    }

    [Fact]
    public async Task CreateStatusList_WithValidData_ReturnsExpected()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        const string BaseUrl = "https://example.org";
        A.CallTo(() => _tenantRepositories.GetStatusListCreationData(_tenantId))
            .Returns((companyId, BaseUrl, GetWalletData()));

        // Act
        var result = await _sut.CreateStatusList(_tenantId, CancellationToken.None);

        // Assert
        A.CallTo(() => _dimClient.CreateStatusList(A<BasicAuthSettings>._, BaseUrl, companyId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        result.modified.Should().BeFalse();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().ContainSingle().Which.Should().Be(ProcessStepTypeId.SEND_CALLBACK);
    }

    #endregion

    #region SendCallback

    [Fact]
    public async Task SendCallback_WithoutBaseUrl_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _tenantRepositories.GetCallbackData(_tenantId))
            .Returns(("bpn123", null, _fixture.Create<WalletData>(), null, null));
        async Task Act() => await _sut.SendCallback(_tenantId, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);

        // Assert
        ex.Message.Should().Be("BaseUrl must always be set");
    }

    [Fact]
    public async Task SendCallback_WithoutDid_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _tenantRepositories.GetCallbackData(_tenantId))
            .Returns(("bpn123", "https://example.org/base", _fixture.Create<WalletData>(), null, null));
        async Task Act() => await _sut.SendCallback(_tenantId, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);

        // Assert
        ex.Message.Should().Be("Did must always be set");
    }

    [Fact]
    public async Task SendCallback_WithoutDownloadUrl_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _tenantRepositories.GetCallbackData(_tenantId))
            .Returns(("bpn123", "https://example.org/base", _fixture.Create<WalletData>(), "did:web:example:org:base", null));
        async Task Act() => await _sut.SendCallback(_tenantId, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);

        // Assert
        ex.Message.Should().Be("DownloadUrl must always be set");
    }

    [Fact]
    public async Task SendCallback_WithValidData_ReturnsExpected()
    {
        // Arrange
        var tenant = new Tenant(_tenantId, "test", "Corp", "https://example.org/did", false, _processId, _operatorId)
        {
            Did = "did:web:example:org:base",
            DidDownloadUrl = "https://example.org/download",
        };

        var cryptoHelper = _settings.EncryptionConfigs.GetCryptoHelper(_settings.EncryptionConfigIndex);
        var (encryptSecret, initializationVector) = cryptoHelper.Encrypt("test123");
        var walletData = new WalletData("https://example.org/token", "cl1", encryptSecret, initializationVector, _settings.EncryptionConfigIndex);
        A.CallTo(() => _tenantRepositories.GetCallbackData(_tenantId))
            .Returns(("bpn123", "https://example.org/base", walletData, tenant.Did, tenant.DidDownloadUrl));

        // Act
        var result = await _sut.SendCallback(_tenantId, CancellationToken.None);

        // Assert
        A.CallTo(() => _callbackService.SendCallback(A<string>._, A<AuthenticationDetail>._, A<JsonDocument>._, A<string>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _callbackService.SendCallback("bpn123", A<AuthenticationDetail>._, A<JsonDocument>._, "did:web:example:org:base", A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        result.modified.Should().BeFalse();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().BeNull();
    }

    #endregion

    private WalletData GetWalletData()
    {
        var cryptoHelper = _settings.EncryptionConfigs.GetCryptoHelper(_settings.EncryptionConfigIndex);
        var (secret, initializationVector) = cryptoHelper.Encrypt("test123");

        return new WalletData("https://example.org/token", "cl1", secret, initializationVector, _settings.EncryptionConfigIndex);
    }
}
