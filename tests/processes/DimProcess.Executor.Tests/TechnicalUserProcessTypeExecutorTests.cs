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

namespace DimProcess.Executor.Tests;

public class TechnicalUserProcessTypeExecutorTests
{
    private readonly TechnicalUserProcessTypeExecutor _sut;
    private readonly ITechnicalUserProcessHandler _technicalUserProcessHandler;
    private readonly ITenantRepository _tenantRepository;

    public TechnicalUserProcessTypeExecutorTests()
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var repositories = A.Fake<IDimRepositories>();
        _technicalUserProcessHandler = A.Fake<ITechnicalUserProcessHandler>();

        _tenantRepository = A.Fake<ITenantRepository>();

        A.CallTo(() => repositories.GetInstance<ITenantRepository>()).Returns(_tenantRepository);

        _sut = new TechnicalUserProcessTypeExecutor(repositories, _technicalUserProcessHandler);
    }

    [Fact]
    public void GetProcessTypeId_ReturnsExpected()
    {
        // Assert
        _sut.GetProcessTypeId().Should().Be(ProcessTypeId.TECHNICAL_USER);
    }

    [Fact]
    public void IsExecutableStepTypeId_WithValid_ReturnsExpected()
    {
        // Assert
        _sut.IsExecutableStepTypeId(ProcessStepTypeId.SEND_TECHNICAL_USER_CREATION_CALLBACK).Should().BeTrue();
    }

    [Fact]
    public void GetExecutableStepTypeIds_ReturnsExpected()
    {
        // Assert
        _sut.GetExecutableStepTypeIds().Should().HaveCount(5).And.Satisfy(
            x => x == ProcessStepTypeId.CREATE_TECHNICAL_USER,
            x => x == ProcessStepTypeId.GET_TECHNICAL_USER_DATA,
            x => x == ProcessStepTypeId.SEND_TECHNICAL_USER_CREATION_CALLBACK,
            x => x == ProcessStepTypeId.DELETE_TECHNICAL_USER,
            x => x == ProcessStepTypeId.SEND_TECHNICAL_USER_DELETION_CALLBACK);
    }

    [Fact]
    public async Task IsLockRequested_ReturnsExpected()
    {
        // Act
        var result = await _sut.IsLockRequested(ProcessStepTypeId.CREATE_TECHNICAL_USER);

        // Assert
        result.Should().BeFalse();
    }

    #region InitializeProcess

    [Fact]
    public async Task InitializeProcess_WithExistingProcess_ReturnsExpected()
    {
        // Arrange
        var validProcessId = Guid.NewGuid();
        A.CallTo(() => _tenantRepository.GetTenantDataForTechnicalUserProcessId(validProcessId))
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
        A.CallTo(() => _tenantRepository.GetTenantDataForTechnicalUserProcessId(validProcessId))
            .Returns(new ValueTuple<bool, Guid, string, string>(false, Guid.Empty, string.Empty, string.Empty));

        // Act
        async Task Act() => await _sut.InitializeProcess(validProcessId, Enumerable.Empty<ProcessStepTypeId>()).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(Act);
        ex.Message.Should().Be($"process {validProcessId} does not exist or is not associated with an technical user");
    }

    #endregion

    #region ExecuteProcessStep

    [Fact]
    public async Task ExecuteProcessStep_WithoutInitialization_ThrowsUnexpectedConditionException()
    {
        // Act
        async Task Act() => await _sut.ExecuteProcessStep(ProcessStepTypeId.CREATE_TECHNICAL_USER, Enumerable.Empty<ProcessStepTypeId>(), CancellationToken.None).ConfigureAwait(false);

        // Assert
        var ex = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);
        ex.Message.Should().Be("technicalUserId and tenantName should never be empty here");
    }

    [Theory]
    [InlineData(ProcessStepTypeId.CREATE_TECHNICAL_USER)]
    [InlineData(ProcessStepTypeId.GET_TECHNICAL_USER_DATA)]
    [InlineData(ProcessStepTypeId.SEND_TECHNICAL_USER_CREATION_CALLBACK)]
    [InlineData(ProcessStepTypeId.DELETE_TECHNICAL_USER)]
    [InlineData(ProcessStepTypeId.SEND_TECHNICAL_USER_DELETION_CALLBACK)]
    public async Task ExecuteProcessStep_WithValidData_CallsExpected(ProcessStepTypeId processStepTypeId)
    {
        // Arrange InitializeProcess
        var validProcessId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        A.CallTo(() => _tenantRepository.GetTenantDataForTechnicalUserProcessId(validProcessId))
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
        A.CallTo(() => _tenantRepository.GetTenantDataForTechnicalUserProcessId(validProcessId))
            .Returns(new ValueTuple<bool, Guid, string, string>(true, tenantId, "test", "test1"));

        // Act InitializeProcess
        var initializeResult = await _sut.InitializeProcess(validProcessId, Enumerable.Empty<ProcessStepTypeId>());

        // Assert InitializeProcess
        initializeResult.Modified.Should().BeFalse();
        initializeResult.ScheduleStepTypeIds.Should().BeNull();

        // Arrange
        A.CallTo(() => _technicalUserProcessHandler.CreateServiceInstanceBindings("test1test", tenantId, A<CancellationToken>._))
            .Throws(new ServiceException("this is a test", true));

        // Act
        var result = await _sut.ExecuteProcessStep(ProcessStepTypeId.CREATE_TECHNICAL_USER, Enumerable.Empty<ProcessStepTypeId>(), CancellationToken.None);

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
        A.CallTo(() => _tenantRepository.GetTenantDataForTechnicalUserProcessId(validProcessId))
            .Returns(new ValueTuple<bool, Guid, string, string>(true, tenantId, "test", "test1"));

        // Act InitializeProcess
        var initializeResult = await _sut.InitializeProcess(validProcessId, Enumerable.Empty<ProcessStepTypeId>());

        // Assert InitializeProcess
        initializeResult.Modified.Should().BeFalse();
        initializeResult.ScheduleStepTypeIds.Should().BeNull();

        // Arrange
        A.CallTo(() => _technicalUserProcessHandler.CreateServiceInstanceBindings("test1test", tenantId, A<CancellationToken>._))
            .Throws(new ServiceException("this is a test"));

        // Act
        var result = await _sut.ExecuteProcessStep(ProcessStepTypeId.CREATE_TECHNICAL_USER, Enumerable.Empty<ProcessStepTypeId>(), CancellationToken.None);

        // Assert
        result.Modified.Should().BeTrue();
        result.ScheduleStepTypeIds.Should().BeNull();
        result.ProcessStepStatusId.Should().Be(ProcessStepStatusId.FAILED);
        result.ProcessMessage.Should().Be("this is a test");
        result.SkipStepTypeIds.Should().BeNull();
    }

    #endregion

    #region Setup

    private void SetupMock(Guid tenantId, string tenantName)
    {
        A.CallTo(() => _technicalUserProcessHandler.CreateServiceInstanceBindings(tenantName, tenantId, A<CancellationToken>._))
            .Returns(new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(null, ProcessStepStatusId.DONE, false, null));

        A.CallTo(() => _technicalUserProcessHandler.GetTechnicalUserData(tenantName, tenantId, A<CancellationToken>._))
            .Returns(new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(null, ProcessStepStatusId.DONE, false, null));

        A.CallTo(() => _technicalUserProcessHandler.SendCreateCallback(tenantId, A<CancellationToken>._))
            .Returns(new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(null, ProcessStepStatusId.DONE, false, null));

        A.CallTo(() => _technicalUserProcessHandler.DeleteServiceInstanceBindings(tenantName, tenantId, A<CancellationToken>._))
            .Returns(new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(null, ProcessStepStatusId.DONE, false, null));

        A.CallTo(() => _technicalUserProcessHandler.SendDeleteCallback(tenantId, A<CancellationToken>._))
            .Returns(new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(null, ProcessStepStatusId.DONE, false, null));
    }

    #endregion
}
