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
using Dim.Clients.Token;
using Dim.DbAccess;
using Dim.DbAccess.Models;
using Dim.DbAccess.Repositories;
using Dim.Entities.Entities;
using Dim.Entities.Enums;
using Dim.Web.BusinessLogic;
using Dim.Web.ErrorHandling;
using Dim.Web.Models;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using System.Security.Cryptography;

namespace Dim.Web.Tests;

public class DimBusinessLogicTests
{
    private static readonly Guid OperatorId = Guid.NewGuid();
    private readonly IDimBusinessLogic _sut;
    private readonly IDimClient _dimClient;
    private readonly ITenantRepository _tenantRepository;
    private readonly ITechnicalUserRepository _technicalUserRepository;
    private readonly IProcessStepRepository _processStepRepository;
    private readonly DimSettings _settings;
    private readonly IFixture _fixture;

    public DimBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var repositories = A.Fake<IDimRepositories>();
        _dimClient = A.Fake<IDimClient>();

        _tenantRepository = A.Fake<ITenantRepository>();
        _technicalUserRepository = A.Fake<ITechnicalUserRepository>();
        _processStepRepository = A.Fake<IProcessStepRepository>();

        A.CallTo(() => repositories.GetInstance<ITenantRepository>()).Returns(_tenantRepository);
        A.CallTo(() => repositories.GetInstance<ITechnicalUserRepository>()).Returns(_technicalUserRepository);
        A.CallTo(() => repositories.GetInstance<IProcessStepRepository>()).Returns(_processStepRepository);

        _settings = new DimSettings
        {
            OperatorId = OperatorId,
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
        _sut = new DimBusinessLogic(repositories, _dimClient, Options.Create(_settings));
    }

    #region StartSetupDim

    [Fact]
    public async Task StartSetupDim_WithExisting_ThrowsConflictException()
    {
        // Arrange
        A.CallTo(() => _tenantRepository.IsTenantExisting(A<string>._, A<string>._))
            .Returns(true);
        async Task Act() => await _sut.StartSetupDim("testCompany", "BPNL00000001TEST", "https://example.org/test", false);

        // Act
        var result = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        result.Message.Should().Be(DimErrors.TENANT_ALREADY_EXISTS.ToString());
    }

    [Theory]
    [InlineData("testCompany", "testcompany")]
    [InlineData("-abc123", "abc123")]
    [InlineData("abc-123", "abc123")]
    [InlineData("abc#123", "abc123")]
    [InlineData("abc'123", "abc123")]
    [InlineData("ä+slidfböü123üü", "slidfb123")]
    [InlineData("averylongnamethatexeedsthemaxlengthbysomecharacters", "averylongnametha")]
    [InlineData("a test company", "atestcompany")]
    public async Task StartSetupDim_WithNewData_CreatesExpected(string companyName, string expectedCompanyName)
    {
        // Arrange
        var processId = Guid.NewGuid();
        var processes = new List<Process>();
        var processSteps = new List<ProcessStep>();
        var tenants = new List<Tenant>();
        A.CallTo(() => _tenantRepository.IsTenantExisting(A<string>._, A<string>._))
            .Returns(false);
        A.CallTo(() => _processStepRepository.CreateProcess(A<ProcessTypeId>._))
            .Invokes((ProcessTypeId processTypeId) =>
            {
                processes.Add(new Process(processId, processTypeId, Guid.NewGuid()));
            })
            .Returns(new Process(processId, ProcessTypeId.TECHNICAL_USER, Guid.NewGuid()));
        A.CallTo(() => _processStepRepository.CreateProcessStep(A<ProcessStepTypeId>._, A<ProcessStepStatusId>._, A<Guid>._))
            .Invokes((ProcessStepTypeId processStepTypeId, ProcessStepStatusId processStepStatusId, Guid pId) =>
            {
                processSteps.Add(new ProcessStep(Guid.NewGuid(), processStepTypeId, processStepStatusId, pId, DateTimeOffset.UtcNow));
            });
        A.CallTo(() =>
                _tenantRepository.CreateTenant(A<string>._, A<string>._, A<string>._, A<bool>._, A<Guid>._, A<Guid>._))
            .Invokes((string companyName, string bpn, string didDocumentLocation, bool isIssuer, Guid pId,
                Guid operatorId) =>
            {
                tenants.Add(new Tenant(Guid.NewGuid(), companyName, bpn, didDocumentLocation, isIssuer, pId, operatorId));
            });

        // Act
        await _sut.StartSetupDim(companyName, "BPNL00000001TEST", "https://example.org/test", false);

        // Assert
        processes.Should().ContainSingle()
            .Which.ProcessTypeId.Should().Be(ProcessTypeId.SETUP_DIM);
        processSteps.Should().ContainSingle()
            .And.Satisfy(x => x.ProcessId == processId && x.ProcessStepTypeId == ProcessStepTypeId.CREATE_WALLET);
        tenants.Should().ContainSingle()
            .And.Satisfy(x => x.CompanyName == expectedCompanyName && x.Bpn == "BPNL00000001TEST");
    }

    #endregion

    #region GetStatusList

    [Fact]
    public async Task GetStatusList_WithNotExisting_ThrowsNotFoundException()
    {
        // Arrange
        var bpn = "BPNL00000001TEST";
        A.CallTo(() => _tenantRepository.GetCompanyAndWalletDataForBpn(bpn))
            .Returns((false, null, null, GetWalletData()));
        Task Act() => _sut.GetStatusList(bpn, CancellationToken.None);

        // Act
        var result = await Assert.ThrowsAsync<NotFoundException>(Act);

        // Assert
        result.Message.Should().Be(DimErrors.NO_COMPANY_FOR_BPN.ToString());
    }

    [Fact]
    public async Task GetStatusList_WithoutCompanyId_ThrowsConflictException()
    {
        // Arrange
        const string Bpn = "BPNL00000001TEST";
        A.CallTo(() => _tenantRepository.GetCompanyAndWalletDataForBpn(Bpn))
            .Returns((true, null, null, GetWalletData()));
        Task Act() => _sut.GetStatusList(Bpn, CancellationToken.None);

        // Act
        var result = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        result.Message.Should().Be(DimErrors.NO_COMPANY_ID_SET.ToString());
    }

    [Fact]
    public async Task GetStatusList_WithBaseUrlNotSet_ThrowsConflictException()
    {
        // Arrange
        const string Bpn = "BPNL00000001TEST";
        var companyId = Guid.NewGuid();
        A.CallTo(() => _tenantRepository.GetCompanyAndWalletDataForBpn(Bpn))
            .Returns((true, companyId, null, GetWalletData()));
        Task Act() => _sut.GetStatusList(Bpn, CancellationToken.None);

        // Act
        var result = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        result.Message.Should().Be(DimErrors.NO_BASE_URL_SET.ToString());
    }

    [Fact]
    public async Task GetStatusList_WithValid_ReturnsExpected()
    {
        // Arrange
        const string Bpn = "BPNL00000001TEST";
        const string BaseUrl = "https://example.org/base";
        var companyId = Guid.NewGuid();
        A.CallTo(() => _tenantRepository.GetCompanyAndWalletDataForBpn(Bpn))
            .Returns((true, companyId, BaseUrl, GetWalletData()));
        A.CallTo(() => _dimClient.GetStatusList(A<BasicAuthSettings>._, BaseUrl, companyId, A<CancellationToken>._))
            .Returns("https://example.org/statuslist");

        // Act
        var result = await _sut.GetStatusList(Bpn, CancellationToken.None);

        // Assert
        result.Should().Be("https://example.org/statuslist");
    }

    #endregion

    #region CreateStatusList

    [Fact]
    public async Task CreateStatusList_WithNotExisting_ThrowsNotFoundException()
    {
        // Arrange
        var bpn = "BPNL00000001TEST";
        A.CallTo(() => _tenantRepository.GetCompanyAndWalletDataForBpn(bpn))
            .Returns((false, null, null, GetWalletData()));
        Task Act() => _sut.CreateStatusList(bpn, CancellationToken.None);

        // Act
        var result = await Assert.ThrowsAsync<NotFoundException>(Act);

        // Assert
        result.Message.Should().Be(DimErrors.NO_COMPANY_FOR_BPN.ToString());
    }

    [Fact]
    public async Task CreateStatusList_WithoutCompanyId_ThrowsConflictException()
    {
        // Arrange
        const string Bpn = "BPNL00000001TEST";
        A.CallTo(() => _tenantRepository.GetCompanyAndWalletDataForBpn(Bpn))
            .Returns((true, null, null, GetWalletData()));
        Task Act() => _sut.CreateStatusList(Bpn, CancellationToken.None);

        // Act
        var result = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        result.Message.Should().Be(DimErrors.NO_COMPANY_ID_SET.ToString());
    }

    [Fact]
    public async Task CreateStatusList_WithBaseUrlNotSet_ThrowsConflictException()
    {
        // Arrange
        const string Bpn = "BPNL00000001TEST";
        var companyId = Guid.NewGuid();
        A.CallTo(() => _tenantRepository.GetCompanyAndWalletDataForBpn(Bpn))
            .Returns((true, companyId, null, GetWalletData()));
        Task Act() => _sut.CreateStatusList(Bpn, CancellationToken.None);

        // Act
        var result = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        result.Message.Should().Be(DimErrors.NO_BASE_URL_SET.ToString());
    }

    [Fact]
    public async Task CreateStatusList_WithValid_ReturnsExpected()
    {
        // Arrange
        const string Bpn = "BPNL00000001TEST";
        const string BaseUrl = "https://example.org/base";
        var companyId = Guid.NewGuid();
        A.CallTo(() => _tenantRepository.GetCompanyAndWalletDataForBpn(Bpn))
            .Returns((true, companyId, BaseUrl, GetWalletData()));
        A.CallTo(() => _dimClient.CreateStatusList(A<BasicAuthSettings>._, BaseUrl, companyId, A<CancellationToken>._))
            .Returns("https://example.org/statuslist");

        // Act
        var result = await _sut.CreateStatusList(Bpn, CancellationToken.None);

        // Assert
        result.Should().Be("https://example.org/statuslist");
    }

    #endregion

    #region StartSetupDim

    [Fact]
    public async Task CreateTechnicalUser_WithExisting_ThrowsNotFoundException()
    {
        // Arrange
        const string Bpn = "BPNL00000001TEST";
        A.CallTo(() => _tenantRepository.GetTenantForBpn(Bpn))
            .Returns((false, Guid.NewGuid()));
        async Task Act() => await _sut.CreateTechnicalUser(Bpn, _fixture.Create<TechnicalUserData>());

        // Act
        var result = await Assert.ThrowsAsync<NotFoundException>(Act);

        // Assert
        result.Message.Should().Be(DimErrors.NO_COMPANY_FOR_BPN.ToString());
    }

    [Theory]
    [InlineData("testCompany", "testcompany")]
    [InlineData("-abc123", "abc123")]
    [InlineData("abc-123", "abc123")]
    [InlineData("abc#123", "abc123")]
    [InlineData("abc'123", "abc123")]
    [InlineData("ä+slidfböü123üü", "slidfb123")]
    [InlineData("averylongnamethatexeedsthemaxlengthbysomecharacters", "averylongnamethatexeedsthemaxlen")]
    [InlineData("a test company", "atestcompany")]
    public async Task CreateTechnicalUser_WithNewData_CreatesExpected(string name, string expectedName)
    {
        // Arrange
        const string Bpn = "BPNL00001Test";
        var processId = Guid.NewGuid();
        var processes = new List<Process>();
        var processSteps = new List<ProcessStep>();
        var technicalUsers = new List<TechnicalUser>();
        A.CallTo(() => _tenantRepository.GetTenantForBpn(Bpn))
            .Returns((true, Guid.NewGuid()));
        A.CallTo(() => _processStepRepository.CreateProcess(A<ProcessTypeId>._))
            .Invokes((ProcessTypeId processTypeId) =>
            {
                processes.Add(new Process(processId, processTypeId, Guid.NewGuid()));
            })
            .Returns(new Process(processId, ProcessTypeId.TECHNICAL_USER, Guid.NewGuid()));
        A.CallTo(() => _processStepRepository.CreateProcessStep(A<ProcessStepTypeId>._, A<ProcessStepStatusId>._, A<Guid>._))
            .Invokes((ProcessStepTypeId processStepTypeId, ProcessStepStatusId processStepStatusId, Guid pId) =>
            {
                processSteps.Add(new ProcessStep(Guid.NewGuid(), processStepTypeId, processStepStatusId, pId, DateTimeOffset.UtcNow));
            });
        A.CallTo(() =>
                _technicalUserRepository.CreateTenantTechnicalUser(A<Guid>._, A<string>._, A<Guid>._, A<Guid>._))
            .Invokes((Guid tenantId, string technicalUserName, Guid externalId, Guid pId) =>
            {
                technicalUsers.Add(new TechnicalUser(Guid.NewGuid(), tenantId, externalId, technicalUserName, pId));
            });

        // Act
        await _sut.CreateTechnicalUser(Bpn, _fixture.Build<TechnicalUserData>().With(x => x.Name, name).Create());

        // Assert
        processes.Should().ContainSingle()
            .Which.ProcessTypeId.Should().Be(ProcessTypeId.TECHNICAL_USER);
        processSteps.Should().ContainSingle()
            .And.Satisfy(x => x.ProcessId == processId && x.ProcessStepTypeId == ProcessStepTypeId.CREATE_TECHNICAL_USER);
        technicalUsers.Should().ContainSingle().And.Satisfy(x => x.TechnicalUserName == expectedName);
    }

    #endregion

    #region StartSetupDim

    [Fact]
    public async Task DeleteTechnicalUser_WithExisting_ThrowsNotFoundException()
    {
        // Arrange
        const string Bpn = "BPNL00000001TEST";
        var technicalUserData = new TechnicalUserData(Guid.NewGuid(), "test");
        A.CallTo(() => _technicalUserRepository.GetTechnicalUserForBpn(Bpn, technicalUserData.Name))
            .Returns((false, Guid.NewGuid(), Guid.NewGuid()));
        async Task Act() => await _sut.DeleteTechnicalUser(Bpn, technicalUserData);

        // Act
        var result = await Assert.ThrowsAsync<NotFoundException>(Act);

        // Assert
        result.Message.Should().Be(DimErrors.NO_TECHNICAL_USER_FOUND.ToString());
    }

    [Fact]
    public async Task DeleteTechnicalUser_WithValid_DeletesExpected()
    {
        // Arrange
        const string Bpn = "BPNL00001Test";
        var processId = Guid.NewGuid();
        var processSteps = new List<ProcessStep>();
        var technicalUserData = new TechnicalUserData(Guid.NewGuid(), "test");
        var technicalUser = new TechnicalUser(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "test", processId);
        A.CallTo(() => _technicalUserRepository.GetTechnicalUserForBpn(Bpn, technicalUserData.Name))
            .Returns((true, technicalUser.Id, technicalUser.ProcessId));
        A.CallTo(() => _processStepRepository.CreateProcessStep(A<ProcessStepTypeId>._, A<ProcessStepStatusId>._, A<Guid>._))
            .Invokes((ProcessStepTypeId processStepTypeId, ProcessStepStatusId processStepStatusId, Guid pId) =>
            {
                processSteps.Add(new ProcessStep(Guid.NewGuid(), processStepTypeId, processStepStatusId, pId, DateTimeOffset.UtcNow));
            });
        A.CallTo(() => _technicalUserRepository.AttachAndModifyTechnicalUser(A<Guid>._, A<Action<TechnicalUser>>._, A<Action<TechnicalUser>>._))
            .Invokes((Guid _, Action<TechnicalUser>? initialize, Action<TechnicalUser> modify) =>
            {
                initialize?.Invoke(technicalUser);
                modify(technicalUser);
            });

        // Act
        await _sut.DeleteTechnicalUser(Bpn, technicalUserData);

        // Assert
        processSteps.Should().ContainSingle()
            .And.Satisfy(x => x.ProcessId == processId && x.ProcessStepTypeId == ProcessStepTypeId.DELETE_TECHNICAL_USER);
        technicalUser.ProcessId.Should().Be(processId);
        technicalUser.ExternalId.Should().Be(technicalUserData.ExternalId);
    }

    #endregion

    private WalletData GetWalletData()
    {
        var cryptoHelper = _settings.EncryptionConfigs.GetCryptoHelper(0);
        var (secret, initializationVector) = cryptoHelper.Encrypt("test123");

        return new WalletData("https://example.org/token", "cl1", secret, initializationVector, 0);
    }
}
