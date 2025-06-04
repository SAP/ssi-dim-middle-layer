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
using Dim.Entities.Extensions;
using Dim.Web.BusinessLogic;
using Dim.Web.ErrorHandling;
using Dim.Web.Models;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Concrete.Entities;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.DBAccess;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Entities;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Models;
using System.Security.Cryptography;

namespace Dim.Web.Tests;

public class DimBusinessLogicTests
{
    private static readonly Guid OperatorId = Guid.NewGuid();
    private readonly IDimBusinessLogic _sut;
    private readonly IDimClient _dimClient;
    private readonly ITenantRepository _tenantRepository;
    private readonly ITechnicalUserRepository _technicalUserRepository;
    private readonly IProcessStepRepository<ProcessTypeId, ProcessStepTypeId> _processStepRepository;
    private readonly DimSettings _settings;
    private readonly IFixture _fixture;
    private readonly IDimRepositories _dimRepositories;

    public DimBusinessLogicTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _dimRepositories = A.Fake<IDimRepositories>();
        _dimClient = A.Fake<IDimClient>();

        _tenantRepository = A.Fake<ITenantRepository>();
        _technicalUserRepository = A.Fake<ITechnicalUserRepository>();
        _processStepRepository = A.Fake<IProcessStepRepository<ProcessTypeId, ProcessStepTypeId>>();

        A.CallTo(() => _dimRepositories.GetInstance<ITenantRepository>()).Returns(_tenantRepository);
        A.CallTo(() => _dimRepositories.GetInstance<ITechnicalUserRepository>()).Returns(_technicalUserRepository);
        A.CallTo(() => _dimRepositories.GetInstance<IProcessStepRepository<ProcessTypeId, ProcessStepTypeId>>()).Returns(_processStepRepository);

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
        _sut = new DimBusinessLogic(_dimRepositories, _dimClient, Options.Create(_settings));
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
        var processSteps = new List<ProcessStep<Process, ProcessTypeId, ProcessStepTypeId>>();
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
                processSteps.Add(new ProcessStep<Process, ProcessTypeId, ProcessStepTypeId>(Guid.NewGuid(), processStepTypeId, processStepStatusId, pId, DateTimeOffset.UtcNow));
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
        Task Act() => _sut.GetStatusList(bpn, StatusListType.BitstringStatusList, CancellationToken.None);

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
        Task Act() => _sut.GetStatusList(Bpn, StatusListType.BitstringStatusList, CancellationToken.None);

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
        Task Act() => _sut.GetStatusList(Bpn, StatusListType.BitstringStatusList, CancellationToken.None);

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
        A.CallTo(() => _dimClient.GetStatusList(A<BasicAuthSettings>._, BaseUrl, companyId, StatusListType.BitstringStatusList, A<CancellationToken>._))
            .Returns("https://example.org/statuslist");

        // Act
        var result = await _sut.GetStatusList(Bpn, StatusListType.BitstringStatusList, CancellationToken.None);

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
        Task Act() => _sut.CreateStatusList(bpn, StatusListType.BitstringStatusList, CancellationToken.None);

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
        Task Act() => _sut.CreateStatusList(Bpn, StatusListType.BitstringStatusList, CancellationToken.None);

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
        Task Act() => _sut.CreateStatusList(Bpn, StatusListType.BitstringStatusList, CancellationToken.None);

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
        A.CallTo(() => _dimClient.CreateStatusList(A<BasicAuthSettings>._, BaseUrl, companyId, A<StatusListType>._, A<CancellationToken>._))
            .Returns("https://example.org/statuslist");

        // Act
        var result = await _sut.CreateStatusList(Bpn, StatusListType.BitstringStatusList, CancellationToken.None);

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
        var processSteps = new List<ProcessStep<Process, ProcessTypeId, ProcessStepTypeId>>();
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
                processSteps.Add(new ProcessStep<Process, ProcessTypeId, ProcessStepTypeId>(Guid.NewGuid(), processStepTypeId, processStepStatusId, pId, DateTimeOffset.UtcNow));
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
        var processSteps = new List<ProcessStep<Process, ProcessTypeId, ProcessStepTypeId>>();
        var technicalUserData = new TechnicalUserData(Guid.NewGuid(), "test");
        var technicalUser = new TechnicalUser(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "test", processId);
        A.CallTo(() => _technicalUserRepository.GetTechnicalUserForBpn(Bpn, technicalUserData.Name))
            .Returns((true, technicalUser.Id, technicalUser.ProcessId));
        A.CallTo(() => _processStepRepository.CreateProcessStep(A<ProcessStepTypeId>._, A<ProcessStepStatusId>._, A<Guid>._))
            .Invokes((ProcessStepTypeId processStepTypeId, ProcessStepStatusId processStepStatusId, Guid pId) =>
            {
                processSteps.Add(new ProcessStep<Process, ProcessTypeId, ProcessStepTypeId>(Guid.NewGuid(), processStepTypeId, processStepStatusId, pId, DateTimeOffset.UtcNow));
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

    private static string GetName(string name, string additionalName)
    {
        // Use reflection to call the private GetName method
        var getNameMethod = typeof(DimBusinessLogic)
            .GetMethod("GetName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        if (getNameMethod == null)
            throw new InvalidOperationException("GetName method not found on DimBusinessLogic.");

        var normalizedTenantObj = getNameMethod.Invoke(null, new object[] { name, additionalName });
        if (normalizedTenantObj is not string normalizedName)
            throw new InvalidOperationException("GetName method did not return a string.");
        return normalizedName;
    }

    #region GetSetupProcess

    [Fact]
    public async Task GetSetupProcess_WithValidBpnAndCompanyName_ReturnsExpectedProcessData()
    {
        // Arrange
        const string Bpn = "BPNL00000001TEST";
        const string CompanyName = "testCompany";
        var expectedProcessData = _fixture.Create<ProcessData>();
        var normalizedName = GetName(CompanyName, Bpn);
        A.CallTo(() => _tenantRepository.GetWalletProcessForTenant(Bpn, normalizedName))
            .Returns(expectedProcessData);

        // Act
        var result = await _sut.GetSetupProcess(Bpn, CompanyName);

        // Assert
        result.Should().BeEquivalentTo(expectedProcessData);
        A.CallTo(() => _tenantRepository.GetWalletProcessForTenant(Bpn, normalizedName))
            .MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData("BPNL00000001TEST", "")]  // Empty company name
    [InlineData("", "testCompany")]        // Empty BPN
    [InlineData("BPNL00000001TEST", " ")] // Whitespace company name
    [InlineData(" ", "testCompany")]      // Whitespace BPN
    public async Task GetSetupProcess_WithInvalidInput_ThrowsNotFoundException(string bpn, string companyName)
    {
        // Arrange
        var normalizedName = GetName(companyName, bpn);
        A.CallTo(() => _tenantRepository.GetWalletProcessForTenant(bpn, normalizedName))
            .Returns(Task.FromResult<ProcessData?>(null));

        // Act
        async Task Act() => await _sut.GetSetupProcess(bpn, companyName);

        // Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(Act);
        exception.Message.Should().Be(nameof(DimErrors.NO_PROCESS_FOR_COMPANY));
        A.CallTo(() => _tenantRepository.GetWalletProcessForTenant(bpn, normalizedName))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetSetupProcess_WithNonExistingProcess_ThrowsNotFoundException()
    {
        // Arrange
        const string Bpn = "BPNL00000001TEST";
        const string CompanyName = "nonExistingCompany";
        var normalizedName = GetName(CompanyName, Bpn);
        A.CallTo(() => _tenantRepository.GetWalletProcessForTenant(Bpn, normalizedName))
            .Returns(Task.FromResult<ProcessData?>(null));

        // Act
        async Task Act() => await _sut.GetSetupProcess(Bpn, CompanyName);

        // Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(Act);
        exception.Message.Should().Be(nameof(DimErrors.NO_PROCESS_FOR_COMPANY));
        A.CallTo(() => _tenantRepository.GetWalletProcessForTenant(Bpn, normalizedName))
            .MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData("BPNL00000001TEST", "testCompany1")]
    [InlineData("BPNL00000002TEST", "testCompany2")]
    [InlineData("BPNL00000003TEST", "testCompany3")]
    public async Task GetSetupProcess_WithDifferentValidInputs_ReturnsCorrectProcessData(string bpn, string companyName)
    {
        // Arrange
        var expectedProcessData = _fixture.Create<ProcessData>();
        var normalizedName = GetName(companyName, bpn);
        A.CallTo(() => _tenantRepository.GetWalletProcessForTenant(bpn, normalizedName))
            .Returns(expectedProcessData);

        // Act
        var result = await _sut.GetSetupProcess(bpn, companyName);

        // Assert
        result.Should().BeEquivalentTo(expectedProcessData);
        A.CallTo(() => _tenantRepository.GetWalletProcessForTenant(bpn, normalizedName))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region GetTechnicalUserProcess

    [Fact]
    public async Task GetTechnicalUserProcess_WithValidBpnAndCompanyName_ReturnsExpectedProcessData()
    {
        // Arrange
        const string Bpn = "BPNL00000001TEST";
        const string CompanyName = "testCompany";
        const string TechnicalUserName = "testUser";
        var normalizedName = GetName(CompanyName, Bpn);
        var normalizedTechName = GetName(TechnicalUserName, Bpn);
        var expectedProcessData = _fixture.Create<ProcessData>();

        A.CallTo(() => _technicalUserRepository.GetTechnicalUserProcess(Bpn, normalizedName, normalizedTechName))
            .Returns(expectedProcessData);

        // Act
        var result = await _sut.GetTechnicalUserProcess(Bpn, CompanyName, TechnicalUserName);

        // Assert
        result.Should().BeEquivalentTo(expectedProcessData);
        A.CallTo(() => _technicalUserRepository.GetTechnicalUserProcess(Bpn, normalizedName, normalizedTechName))
            .MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData("BPNL00000001TEST", "", "testUser")]  // Empty company name
    [InlineData("", "testCompany", "testUser")]        // Empty BPN
    [InlineData("BPNL00000001TEST", "testCompany", "")]      // Empty tech user name
    [InlineData("BPNL00000001TEST", " ", "testUser")] // Whitespace company name
    [InlineData(" ", "testCompany", "testUser")]      // Whitespace BPN
    [InlineData("BPNL00000001TEST", "testCompany", " ")]      // Whitespace tech user name
    public async Task GetTechnicalUserProcess_WithInvalidInput_ThrowsNotFoundException(string bpn, string companyName, string technicalUserName)
    {
        // Arrange
        var normalizedName = GetName(companyName, bpn);
        var normalizedTechName = GetName(technicalUserName, bpn);
        A.CallTo(() => _technicalUserRepository.GetTechnicalUserProcess(bpn, normalizedName, normalizedTechName))
            .Returns(Task.FromResult<ProcessData?>(null));

        // Act
        async Task Act() => await _sut.GetTechnicalUserProcess(bpn, companyName, technicalUserName);

        // Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(Act);
        exception.Message.Should().Be(nameof(DimErrors.NO_PROCESS_FOR_TECHNICAL_USER));
        A.CallTo(() => _technicalUserRepository.GetTechnicalUserProcess(bpn, normalizedName, normalizedTechName))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task GetTechnicalUserProcess_WithNonExistingProcess_ThrowsNotFoundException()
    {
        // Arrange
        const string Bpn = "BPNL00000001TEST";
        const string CompanyName = "testCompany";
        const string TechnicalUserName = "nonExistingTechUser";
        var normalizedName = GetName(CompanyName, Bpn);
        var normalizedTechName = GetName(TechnicalUserName, Bpn);
        A.CallTo(() => _technicalUserRepository.GetTechnicalUserProcess(Bpn, normalizedName, normalizedTechName))
            .Returns(Task.FromResult<ProcessData?>(null));

        // Act
        async Task Act() => await _sut.GetTechnicalUserProcess(Bpn, CompanyName, TechnicalUserName);

        // Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(Act);
        exception.Message.Should().Be(nameof(DimErrors.NO_PROCESS_FOR_TECHNICAL_USER));
        A.CallTo(() => _technicalUserRepository.GetTechnicalUserProcess(Bpn, normalizedName, normalizedTechName))
            .MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData("BPNL00000001TEST", "testCompany1", "testUser1")]
    [InlineData("BPNL00000002TEST", "testCompany2", "testUser2")]
    [InlineData("BPNL00000003TEST", "testCompany3", "testUser3")]
    public async Task GetTechnicalUserProcess_WithDifferentValidInputs_ReturnsCorrectProcessData(string bpn, string companyName, string technicalUserName)
    {
        // Arrange
        var normalizedName = GetName(companyName, bpn);
        var normalizedTechName = GetName(technicalUserName, bpn);
        var expectedProcessData = _fixture.Create<ProcessData>();

        A.CallTo(() => _technicalUserRepository.GetTechnicalUserProcess(bpn, normalizedName, normalizedTechName))
            .Returns(expectedProcessData);

        // Act
        var result = await _sut.GetTechnicalUserProcess(bpn, companyName, technicalUserName);

        // Assert
        result.Should().BeEquivalentTo(expectedProcessData);
        A.CallTo(() => _technicalUserRepository.GetTechnicalUserProcess(bpn, normalizedName, normalizedTechName))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    #region RetriggerProcess

    [Theory]
    [InlineData(ProcessTypeId.SETUP_DIM, ProcessStepTypeId.RETRIGGER_CREATE_WALLET)]
    [InlineData(ProcessTypeId.SETUP_DIM, ProcessStepTypeId.RETRIGGER_CHECK_OPERATION)]
    [InlineData(ProcessTypeId.TECHNICAL_USER, ProcessStepTypeId.RETRIGGER_CREATE_TECHNICAL_USER)]
    [InlineData(ProcessTypeId.TECHNICAL_USER, ProcessStepTypeId.RETRIGGER_DELETE_TECHNICAL_USER)]
    public async Task RetriggerProcess_WithValidData_ExecutesSuccessfully(ProcessTypeId processTypeId, ProcessStepTypeId processStepTypeId)
    {
        // Arrange
        var processId = _fixture.Create<Guid>();
        var stepToTrigger = processStepTypeId.GetStepForRetrigger(processTypeId);
        var processSteps = new List<ProcessStep<Process, ProcessTypeId, ProcessStepTypeId>>();
        var processStep = new ProcessStep<Process, ProcessTypeId, ProcessStepTypeId>(Guid.NewGuid(), processStepTypeId, ProcessStepStatusId.TODO, processId, DateTimeOffset.UtcNow);
        SetupFakesForRetrigger(processSteps, processStep);
        var verifyProcessData = new VerifyProcessData<ProcessTypeId, ProcessStepTypeId>(
            new Process(processId, processTypeId, Guid.NewGuid()),
            new[]
            {
            processStep
            }
        );

        A.CallTo(() => _processStepRepository.IsValidProcess(processId, processTypeId, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns((true, verifyProcessData));

        // Act
        await _sut.RetriggerProcess(processTypeId, processId, processStepTypeId);

        // Assert
        processSteps.Should().ContainSingle().And.Satisfy(x => x.ProcessStepTypeId == stepToTrigger && x.ProcessStepStatusId == ProcessStepStatusId.TODO);
        processStep.ProcessStepStatusId.Should().Be(ProcessStepStatusId.DONE);
        A.CallTo(() => _dimRepositories.SaveAsync()).MustHaveHappenedOnceExactly();
        A.CallTo(() => _processStepRepository.IsValidProcess(processId, processTypeId, A<IEnumerable<ProcessStepTypeId>>.That.Contains(processStepTypeId)))
            .MustHaveHappenedOnceExactly();
    }

    private void SetupFakesForRetrigger(List<ProcessStep<Process, ProcessTypeId, ProcessStepTypeId>> processSteps, ProcessStep<Process, ProcessTypeId, ProcessStepTypeId> processStep)
    {
        A.CallTo(() => _processStepRepository.CreateProcessStepRange(A<IEnumerable<(ProcessStepTypeId ProcessStepTypeId, ProcessStepStatusId ProcessStepStatusId, Guid ProcessId)>>._))
            .Invokes((IEnumerable<(ProcessStepTypeId ProcessStepTypeId, ProcessStepStatusId ProcessStepStatusId, Guid ProcessId)> processStepTypeStatus) =>
                {
                    processSteps.AddRange(processStepTypeStatus.Select(x => new ProcessStep<Process, ProcessTypeId, ProcessStepTypeId>(Guid.NewGuid(), x.ProcessStepTypeId, x.ProcessStepStatusId, x.ProcessId, DateTimeOffset.UtcNow)).ToList());
                });

        A.CallTo(() => _processStepRepository.AttachAndModifyProcessSteps(A<IEnumerable<ValueTuple<Guid, Action<IProcessStep<ProcessStepTypeId>>?, Action<IProcessStep<ProcessStepTypeId>>>>>._))
            .Invokes((IEnumerable<(Guid ProcessStepId, Action<IProcessStep<ProcessStepTypeId>>? Initialize, Action<IProcessStep<ProcessStepTypeId>> Modify)> processStepIdsInitializeModifyData) =>
                {
                    var modify = processStepIdsInitializeModifyData.SingleOrDefault(x => processStep.Id == x.ProcessStepId);
                    if (modify == default)
                        return;

                    modify.Initialize?.Invoke(processStep);
                    modify.Modify.Invoke(processStep);
                });
    }

    [Theory]
    [InlineData(ProcessTypeId.SETUP_DIM, ProcessStepTypeId.RETRIGGER_CREATE_WALLET)]
    [InlineData(ProcessTypeId.TECHNICAL_USER, ProcessStepTypeId.RETRIGGER_CREATE_TECHNICAL_USER)]
    public async Task RetriggerProcess_WithInvalidProcessId_ThrowsNotFoundException(ProcessTypeId processTypeId, ProcessStepTypeId processStepTypeId)
    {
        // Arrange
        var processId = _fixture.Create<Guid>();

        A.CallTo(() => _processStepRepository.IsValidProcess(processId, processTypeId, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns(Task.FromResult<(bool, VerifyProcessData<ProcessTypeId, ProcessStepTypeId>)>((false, new VerifyProcessData<ProcessTypeId, ProcessStepTypeId>(null, new List<ProcessStep<Process, ProcessTypeId, ProcessStepTypeId>>()))));

        // Act
        var act = () => _sut.RetriggerProcess(processTypeId, processId, processStepTypeId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage(nameof(DimErrors.NO_PROCESS));
        A.CallTo(() => _dimRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Theory]
    [InlineData(ProcessTypeId.TECHNICAL_USER, ProcessStepTypeId.RETRIGGER_CREATE_WALLET)]
    [InlineData(ProcessTypeId.SETUP_DIM, ProcessStepTypeId.RETRIGGER_CREATE_TECHNICAL_USER)]
    public async Task RetriggerProcess_WithMismatchedProcessTypeAndStep_ThrowsInvalidOperationException(ProcessTypeId processTypeId, ProcessStepTypeId processStepTypeId)
    {
        // Arrange
        var processId = _fixture.Create<Guid>();

        var processData = new VerifyProcessData<ProcessTypeId, ProcessStepTypeId>(
            new Process(processId, processTypeId, Guid.NewGuid()),
            new[] { new ProcessStep<Process, ProcessTypeId, ProcessStepTypeId>(Guid.NewGuid(), processStepTypeId, ProcessStepStatusId.TODO, processId, DateTimeOffset.UtcNow) });

        A.CallTo(() => _processStepRepository.IsValidProcess(processId, processTypeId, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns((true, processData));

        // Act
        var act = () => _sut.RetriggerProcess(processTypeId, processId, processStepTypeId);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
        A.CallTo(() => _dimRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Theory]
    [InlineData(ProcessTypeId.SETUP_DIM, ProcessStepTypeId.RETRIGGER_CREATE_WALLET)]
    [InlineData(ProcessTypeId.TECHNICAL_USER, ProcessStepTypeId.RETRIGGER_CREATE_TECHNICAL_USER)]
    public async Task RetriggerProcess_WithNonTodoStatus_ThrowsUnexpectedConditionException(ProcessTypeId processTypeId, ProcessStepTypeId processStepTypeId)
    {
        // Arrange
        var processId = _fixture.Create<Guid>();
        var processData = new VerifyProcessData<ProcessTypeId, ProcessStepTypeId>(
            new Process(processId, processTypeId, Guid.NewGuid()),
            new[]
            {
            new ProcessStep<Process, ProcessTypeId, ProcessStepTypeId>(Guid.NewGuid(), processStepTypeId, ProcessStepStatusId.DONE, processId, DateTimeOffset.UtcNow)
            }
        );

        A.CallTo(() => _processStepRepository.IsValidProcess(processId, processTypeId, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns((true, processData));

        // Act
        var act = () => _sut.RetriggerProcess(processTypeId, processId, processStepTypeId);

        // Assert
        await act.Should().ThrowAsync<UnexpectedConditionException>()
            .WithMessage($"processSteps should never have any other status than {ProcessStepStatusId.TODO} here");
        A.CallTo(() => _dimRepositories.SaveAsync()).MustNotHaveHappened();
    }

    [Fact]
    public async Task RetriggerProcess_WithNullProcessData_ThrowsConflictException()
    {
        // Arrange
        var processId = _fixture.Create<Guid>();
        var processTypeId = ProcessTypeId.SETUP_DIM;
        var processStepTypeId = ProcessStepTypeId.RETRIGGER_CREATE_WALLET;

        A.CallTo(() => _processStepRepository.IsValidProcess(processId, processTypeId, A<IEnumerable<ProcessStepTypeId>>._))
            .Returns(Task.FromResult<(bool, VerifyProcessData<ProcessTypeId, ProcessStepTypeId>)>((true, new VerifyProcessData<ProcessTypeId, ProcessStepTypeId>(null, new List<ProcessStep<Process, ProcessTypeId, ProcessStepTypeId>>()))));

        // Act
        var act = () => _sut.RetriggerProcess(processTypeId, processId, processStepTypeId);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage($"processId {processId} is not associated with any process");
        A.CallTo(() => _dimRepositories.SaveAsync()).MustNotHaveHappened();
    }

    #endregion
}
