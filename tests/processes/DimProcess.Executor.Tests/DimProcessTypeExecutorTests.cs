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

using Dim.DbAccess;
using Dim.DbAccess.Repositories;
using Dim.Entities.Enums;
using DimProcess.Library;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;

namespace DimProcess.Executor.Tests;

public class DimProcessTypeExecutorTests
{
    private readonly DimProcessTypeExecutor _sut;
    private readonly IDimProcessHandler _dimProcessHandler;
    private readonly ITenantRepository _tenantRepository;

    public DimProcessTypeExecutorTests()
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var repositories = A.Fake<IDimRepositories>();
        _dimProcessHandler = A.Fake<IDimProcessHandler>();

        _tenantRepository = A.Fake<ITenantRepository>();

        A.CallTo(() => repositories.GetInstance<ITenantRepository>()).Returns(_tenantRepository);

        _sut = new DimProcessTypeExecutor(repositories, _dimProcessHandler);
    }

    [Fact]
    public void GetProcessTypeId_ReturnsExpected()
    {
        // Assert
        _sut.GetProcessTypeId().Should().Be(ProcessTypeId.SETUP_DIM);
    }

    [Fact]
    public void IsExecutableStepTypeId_WithValid_ReturnsExpected()
    {
        // Assert
        _sut.IsExecutableStepTypeId(ProcessStepTypeId.SEND_CALLBACK).Should().BeTrue();
    }

    [Fact]
    public void GetExecutableStepTypeIds_ReturnsExpected()
    {
        // Assert
        _sut.GetExecutableStepTypeIds().Should().HaveCount(6).And.Satisfy(
            x => x == ProcessStepTypeId.CHECK_OPERATION,
            x => x == ProcessStepTypeId.GET_COMPANY,
            x => x == ProcessStepTypeId.GET_DID_DOCUMENT,
            x => x == ProcessStepTypeId.CREATE_WALLET,
            x => x == ProcessStepTypeId.CREATE_STATUS_LIST,
            x => x == ProcessStepTypeId.SEND_CALLBACK);
    }

    [Fact]
    public async Task IsLockRequested_ReturnsExpected()
    {
        // Act
        var result = await _sut.IsLockRequested(ProcessStepTypeId.SEND_CALLBACK);

        // Assert
        result.Should().BeFalse();
    }

    #region InitializeProcess

    [Fact]
    public async Task InitializeProcess_WithExistingProcess_ReturnsExpected()
    {
        // Arrange
        var validProcessId = Guid.NewGuid();
        A.CallTo(() => _tenantRepository.GetTenantDataForProcessId(validProcessId))
            .Returns(new ValueTuple<bool, Guid, string, string>(true, Guid.NewGuid(), "test", "test1"));

        // Act
        var result = await _sut.InitializeProcess(validProcessId, Enumerable.Empty<ProcessStepTypeId>());

        // Assert
        result.Modified.Should().BeFalse();
        result.ScheduleStepTypeIds.Should().BeNull();
    }

    [Fact]
    public async Task InitializeProcess_WithNotExistingProcess_ThrowsNotFoundException()
    {
        // Arrange
        var validProcessId = Guid.NewGuid();
        A.CallTo(() => _tenantRepository.GetTenantDataForProcessId(validProcessId))
            .Returns(new ValueTuple<bool, Guid, string, string>(false, Guid.Empty, string.Empty, string.Empty));

        // Act
        async Task Act() => await _sut.InitializeProcess(validProcessId, Enumerable.Empty<ProcessStepTypeId>());

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"process {validProcessId} does not exist or is not associated with an tenant");
    }

    #endregion

    #region ExecuteProcessStep

    [Fact]
    public async Task ExecuteProcessStep_WithoutRegistrationId_ThrowsUnexpectedConditionException()
    {
        // Act
        async Task Act() => await _sut.ExecuteProcessStep(ProcessStepTypeId.SEND_CALLBACK, Enumerable.Empty<ProcessStepTypeId>(), CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);
        ex.Message.Should().Be("tenantId and tenantName should never be empty here");
    }

    [Theory]
    [InlineData(ProcessStepTypeId.CREATE_WALLET)]
    [InlineData(ProcessStepTypeId.CHECK_OPERATION)]
    [InlineData(ProcessStepTypeId.GET_COMPANY)]
    [InlineData(ProcessStepTypeId.GET_DID_DOCUMENT)]
    [InlineData(ProcessStepTypeId.SEND_CALLBACK)]
    public async Task ExecuteProcessStep_WithValidData_CallsExpected(ProcessStepTypeId processStepTypeId)
    {
        // Arrange InitializeProcess
        var validProcessId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        A.CallTo(() => _tenantRepository.GetTenantDataForProcessId(validProcessId))
            .Returns(new ValueTuple<bool, Guid, string, string>(true, tenantId, "test", "test1"));

        // Act InitializeProcess
        var initializeResult = await _sut.InitializeProcess(validProcessId, Enumerable.Empty<ProcessStepTypeId>());

        // Assert InitializeProcess
        initializeResult.Modified.Should().BeFalse();
        initializeResult.ScheduleStepTypeIds.Should().BeNull();

        // Arrange
        SetupMock(tenantId, "test1test");

        // Act
        var result = await _sut.ExecuteProcessStep(processStepTypeId, Enumerable.Empty<ProcessStepTypeId>(), CancellationToken.None);

        // Assert
        result.Modified.Should().BeFalse();
        result.ScheduleStepTypeIds.Should().BeNull();
        result.ProcessStepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.ProcessMessage.Should().BeNull();
        result.SkipStepTypeIds.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteProcessStep_WithRecoverableServiceException_ReturnsToDo()
    {
        // Arrange InitializeProcess
        var validProcessId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        A.CallTo(() => _tenantRepository.GetTenantDataForProcessId(validProcessId))
            .Returns(new ValueTuple<bool, Guid, string, string>(true, tenantId, "test", "test1"));

        // Act InitializeProcess
        var initializeResult = await _sut.InitializeProcess(validProcessId, Enumerable.Empty<ProcessStepTypeId>());

        // Assert InitializeProcess
        initializeResult.Modified.Should().BeFalse();
        initializeResult.ScheduleStepTypeIds.Should().BeNull();

        // Arrange
        A.CallTo(() => _dimProcessHandler.CreateWallet(tenantId, "test1test", A<CancellationToken>._))
            .Throws(new ServiceException("this is a test", true));

        // Act
        var result = await _sut.ExecuteProcessStep(ProcessStepTypeId.CREATE_WALLET, Enumerable.Empty<ProcessStepTypeId>(), CancellationToken.None);

        // Assert
        result.Modified.Should().BeTrue();
        result.ScheduleStepTypeIds.Should().BeNull();
        result.ProcessStepStatusId.Should().Be(ProcessStepStatusId.TODO);
        result.ProcessMessage.Should().Be("this is a test");
        result.SkipStepTypeIds.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteProcessStep_WithServiceException_ReturnsFailedAndRetriggerStep()
    {
        // Arrange InitializeProcess
        var validProcessId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        A.CallTo(() => _tenantRepository.GetTenantDataForProcessId(validProcessId))
            .Returns(new ValueTuple<bool, Guid, string, string>(true, tenantId, "test", "test1"));

        // Act InitializeProcess
        var initializeResult = await _sut.InitializeProcess(validProcessId, Enumerable.Empty<ProcessStepTypeId>());

        // Assert InitializeProcess
        initializeResult.Modified.Should().BeFalse();
        initializeResult.ScheduleStepTypeIds.Should().BeNull();

        // Arrange
        A.CallTo(() => _dimProcessHandler.CreateWallet(tenantId, "test1test", A<CancellationToken>._))
            .Throws(new ServiceException("this is a test"));

        // Act
        var result = await _sut.ExecuteProcessStep(ProcessStepTypeId.CREATE_WALLET, Enumerable.Empty<ProcessStepTypeId>(), CancellationToken.None);

        // Assert
        result.Modified.Should().BeTrue();
        result.ScheduleStepTypeIds.Should().ContainSingle()
            .And.Satisfy(x => x == ProcessStepTypeId.RETRIGGER_CREATE_WALLET);
        result.ProcessStepStatusId.Should().Be(ProcessStepStatusId.FAILED);
        result.ProcessMessage.Should().Be("this is a test");
        result.SkipStepTypeIds.Should().BeNull();
    }

    #endregion

    #region Setup

    private void SetupMock(Guid tenantId, string tenantName)
    {
        A.CallTo(() => _dimProcessHandler.CreateWallet(tenantId, tenantName, A<CancellationToken>._))
            .Returns(new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(null, ProcessStepStatusId.DONE, false, null));

        A.CallTo(() => _dimProcessHandler.CheckOperation(tenantId, A<CancellationToken>._))
            .Returns(new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(null, ProcessStepStatusId.DONE, false, null));

        A.CallTo(() => _dimProcessHandler.GetCompany(tenantId, tenantName, A<CancellationToken>._))
            .Returns(new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(null, ProcessStepStatusId.DONE, false, null));

        A.CallTo(() => _dimProcessHandler.GetDidDocument(tenantId, A<CancellationToken>._))
            .Returns(new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(null, ProcessStepStatusId.DONE, false, null));

        A.CallTo(() => _dimProcessHandler.CreateStatusList(tenantId, A<CancellationToken>._))
            .Returns(new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(null, ProcessStepStatusId.DONE, false, null));

        A.CallTo(() => _dimProcessHandler.SendCallback(tenantId, A<CancellationToken>._))
            .Returns(new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(null, ProcessStepStatusId.DONE, false, null));
    }

    #endregion
}
