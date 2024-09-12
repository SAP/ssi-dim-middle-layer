/********************************************************************************
 * Copyright (c) 2024 Contributors to the Eclipse Foundation
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

using AutoFixture;
using AutoFixture.AutoFakeItEasy;
using Dim.DbAccess.Repositories;
using Dim.DbAccess.Tests.Setup;
using Dim.Entities;
using Dim.Entities.Entities;
using Dim.Entities.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;
using Xunit;
using Xunit.Extensions.AssemblyFixture;

namespace Dim.DbAccess.Tests;

public class ProcessStepRepositoryTests : IAssemblyFixture<TestDbFixture>
{
    private readonly IFixture _fixture;
    private readonly TestDbFixture _dbTestDbFixture;

    public ProcessStepRepositoryTests(TestDbFixture testDbFixture)
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region CreateProcess

    [Fact]
    public async Task CreateProcess_CreatesSuccessfully()
    {
        // Arrange
        var (sut, dbContext) = await CreateSutWithContext();
        var changeTracker = dbContext.ChangeTracker;

        // Act
        var result = sut.CreateProcess(ProcessTypeId.SETUP_DIM);

        // Assert
        changeTracker.HasChanges().Should().BeTrue();
        changeTracker.Entries().Should().HaveCount(1)
            .And.AllSatisfy(x =>
            {
                x.State.Should().Be(EntityState.Added);
                x.Entity.Should().BeOfType<Process>();
            });
        changeTracker.Entries().Select(x => x.Entity).Cast<Process>()
            .Should().Satisfy(
                x => x.Id == result.Id && x.ProcessTypeId == ProcessTypeId.SETUP_DIM
            );
    }

    #endregion

    #region CreateProcessStepRange

    [Fact]
    public async Task CreateProcessStepRange_CreateSuccessfully()
    {
        // Arrange
        var processId = Guid.NewGuid();
        var processStepTypeIds = _fixture.CreateMany<ProcessStepTypeId>(3).ToImmutableArray();
        var (sut, dbContext) = await CreateSutWithContext();
        var changeTracker = dbContext.ChangeTracker;

        // Act
        var result = sut.CreateProcessStepRange(processStepTypeIds.Select(processStepTypeId => (processStepTypeId, ProcessStepStatusId.TODO, processId)));

        // Assert
        changeTracker.HasChanges().Should().BeTrue();
        changeTracker.Entries().Should()
            .HaveSameCount(processStepTypeIds)
            .And.AllSatisfy(x =>
            {
                x.State.Should().Be(EntityState.Added);
                x.Entity.Should().BeOfType<ProcessStep>();
            });
        changeTracker.Entries().Select(x => x.Entity).Cast<ProcessStep>()
            .Should().Satisfy(
                x => x.Id == result.ElementAt(0).Id && x.ProcessId == processId && x.ProcessStepTypeId == processStepTypeIds[0] && x.ProcessStepStatusId == ProcessStepStatusId.TODO,
                x => x.Id == result.ElementAt(1).Id && x.ProcessId == processId && x.ProcessStepTypeId == processStepTypeIds[1] && x.ProcessStepStatusId == ProcessStepStatusId.TODO,
                x => x.Id == result.ElementAt(2).Id && x.ProcessId == processId && x.ProcessStepTypeId == processStepTypeIds[2] && x.ProcessStepStatusId == ProcessStepStatusId.TODO
            );
    }

    #endregion

    #region CreateProcessStep

    [Fact]
    public async Task CreateProcessStep_CreateSuccessfully()
    {
        // Arrange
        var processId = Guid.NewGuid();
        var (sut, dbContext) = await CreateSutWithContext();
        var changeTracker = dbContext.ChangeTracker;

        // Act
        sut.CreateProcessStep(ProcessStepTypeId.CREATE_WALLET, ProcessStepStatusId.TODO, processId);

        // Assert
        changeTracker.HasChanges().Should().BeTrue();
        changeTracker.Entries().Should()
            .ContainSingle()
            .Which.State.Should().Be(EntityState.Added);
        changeTracker.Entries().Select(x => x.Entity).Cast<ProcessStep>()
            .Should().Satisfy(
                x => x.ProcessId == processId && x.ProcessStepTypeId == ProcessStepTypeId.CREATE_WALLET && x.ProcessStepStatusId == ProcessStepStatusId.TODO
            );
    }

    #endregion

    #region AttachAndModifyProcessStep

    [Fact]
    public async Task AttachAndModifyProcessStep_WithExistingProcessStep_UpdatesStatus()
    {
        // Arrange
        var (sut, dbContext) = await CreateSutWithContext();

        // Act
        sut.AttachAndModifyProcessStep(new Guid("48f35f84-8d98-4fbd-ba80-8cbce5eeadb5"),
            existing =>
            {
                existing.ProcessStepStatusId = ProcessStepStatusId.TODO;
            },
            modify =>
            {
                modify.ProcessStepStatusId = ProcessStepStatusId.DONE;
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
        changedEntity.Entity.Should().BeOfType<ProcessStep>().Which.ProcessStepStatusId.Should().Be(ProcessStepStatusId.DONE);
    }

    #endregion

    #region AttachAndModifyProcessSteps

    [Fact]
    public async Task AttachAndModifyProcessSteps_UpdatesStatus()
    {
        // Arrange
        var stepData = _fixture.CreateMany<(Guid ProcessStepId, ProcessStep InitialStep, ProcessStep ModifiedStep)>(5).ToImmutableArray();

        var (sut, dbContext) = await CreateSutWithContext();

        // Act
        sut.AttachAndModifyProcessSteps(stepData.Select(data => new ValueTuple<Guid, Action<ProcessStep>?, Action<ProcessStep>>(
            data.ProcessStepId,
            step =>
                {
                    step.ProcessStepStatusId = data.InitialStep.ProcessStepStatusId;
                    step.DateLastChanged = data.InitialStep.DateLastChanged;
                    step.Message = data.InitialStep.Message;
                },
            step =>
                {
                    step.ProcessStepStatusId = data.ModifiedStep.ProcessStepStatusId;
                    step.DateLastChanged = data.ModifiedStep.DateLastChanged;
                    step.Message = data.ModifiedStep.Message;
                })));

        // Assert
        var changeTracker = dbContext.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().HaveCount(5).And.AllSatisfy(entry => entry.State.Should().Be(EntityState.Modified));
        changedEntries.Select(entry => entry.Entity).Should().AllBeOfType<ProcessStep>().Which.Should().Satisfy(
            step => step.Id == stepData[0].ProcessStepId && step.ProcessStepStatusId == stepData[0].ModifiedStep.ProcessStepStatusId && step.DateLastChanged == stepData[0].ModifiedStep.DateLastChanged && step.Message == stepData[0].ModifiedStep.Message,
            step => step.Id == stepData[1].ProcessStepId && step.ProcessStepStatusId == stepData[1].ModifiedStep.ProcessStepStatusId && step.DateLastChanged == stepData[1].ModifiedStep.DateLastChanged && step.Message == stepData[1].ModifiedStep.Message,
            step => step.Id == stepData[2].ProcessStepId && step.ProcessStepStatusId == stepData[2].ModifiedStep.ProcessStepStatusId && step.DateLastChanged == stepData[2].ModifiedStep.DateLastChanged && step.Message == stepData[2].ModifiedStep.Message,
            step => step.Id == stepData[3].ProcessStepId && step.ProcessStepStatusId == stepData[3].ModifiedStep.ProcessStepStatusId && step.DateLastChanged == stepData[3].ModifiedStep.DateLastChanged && step.Message == stepData[3].ModifiedStep.Message,
            step => step.Id == stepData[4].ProcessStepId && step.ProcessStepStatusId == stepData[4].ModifiedStep.ProcessStepStatusId && step.DateLastChanged == stepData[4].ModifiedStep.DateLastChanged && step.Message == stepData[4].ModifiedStep.Message
        );
    }

    [Fact]
    public async Task AttachAndModifyProcessSteps_WithUnmodifiedData_SkipsUpdateStatus()
    {
        // Arrange
        var stepData = _fixture.CreateMany<(Guid ProcessStepId, ProcessStep InitialStep)>(5).ToImmutableArray();

        var (sut, dbContext) = await CreateSutWithContext();

        // Act
        sut.AttachAndModifyProcessSteps(stepData.Select(data => new ValueTuple<Guid, Action<ProcessStep>?, Action<ProcessStep>>(
            data.ProcessStepId,
            step =>
                {
                    step.ProcessStepStatusId = data.InitialStep.ProcessStepStatusId;
                    step.DateLastChanged = data.InitialStep.DateLastChanged;
                    step.Message = data.InitialStep.Message;
                },
            step =>
                {
                    step.DateLastChanged = data.InitialStep.DateLastChanged;
                })));

        // Assert
        var changeTracker = dbContext.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeFalse();
        changedEntries.Should().HaveCount(5).And.AllSatisfy(entry => entry.State.Should().Be(EntityState.Unchanged));
        changedEntries.Select(entry => entry.Entity).Should().AllBeOfType<ProcessStep>().Which.Should().Satisfy(
            step => step.Id == stepData[0].ProcessStepId && step.ProcessStepStatusId == stepData[0].InitialStep.ProcessStepStatusId && step.DateLastChanged == stepData[0].InitialStep.DateLastChanged && step.Message == stepData[0].InitialStep.Message,
            step => step.Id == stepData[1].ProcessStepId && step.ProcessStepStatusId == stepData[1].InitialStep.ProcessStepStatusId && step.DateLastChanged == stepData[1].InitialStep.DateLastChanged && step.Message == stepData[1].InitialStep.Message,
            step => step.Id == stepData[2].ProcessStepId && step.ProcessStepStatusId == stepData[2].InitialStep.ProcessStepStatusId && step.DateLastChanged == stepData[2].InitialStep.DateLastChanged && step.Message == stepData[2].InitialStep.Message,
            step => step.Id == stepData[3].ProcessStepId && step.ProcessStepStatusId == stepData[3].InitialStep.ProcessStepStatusId && step.DateLastChanged == stepData[3].InitialStep.DateLastChanged && step.Message == stepData[3].InitialStep.Message,
            step => step.Id == stepData[4].ProcessStepId && step.ProcessStepStatusId == stepData[4].InitialStep.ProcessStepStatusId && step.DateLastChanged == stepData[4].InitialStep.DateLastChanged && step.Message == stepData[4].InitialStep.Message
        );
    }

    [Fact]
    public async Task AttachAndModifyProcessSteps_WithUnmodifiedData_UpdatesLastChanged()
    {
        // Arrange
        var stepData = _fixture.CreateMany<(Guid ProcessStepId, ProcessStep InitialStep)>(5).ToImmutableArray();

        var (sut, dbContext) = await CreateSutWithContext();

        // Act
        sut.AttachAndModifyProcessSteps(stepData.Select(data => new ValueTuple<Guid, Action<ProcessStep>?, Action<ProcessStep>>(
            data.ProcessStepId,
            step =>
                {
                    step.ProcessStepStatusId = data.InitialStep.ProcessStepStatusId;
                    step.DateLastChanged = data.InitialStep.DateLastChanged;
                    step.Message = data.InitialStep.Message;
                },
            _ => { })));

        // Assert
        var changeTracker = dbContext.ChangeTracker;
        var changedEntries = changeTracker.Entries().ToList();
        changeTracker.HasChanges().Should().BeTrue();
        changedEntries.Should().HaveCount(5).And.AllSatisfy(entry => entry.State.Should().Be(EntityState.Modified));
        changedEntries.Select(entry => entry.Entity).Should().AllBeOfType<ProcessStep>().Which.Should().Satisfy(
            step => step.Id == stepData[0].ProcessStepId && step.ProcessStepStatusId == stepData[0].InitialStep.ProcessStepStatusId && step.DateLastChanged != stepData[0].InitialStep.DateLastChanged && step.Message == stepData[0].InitialStep.Message,
            step => step.Id == stepData[1].ProcessStepId && step.ProcessStepStatusId == stepData[1].InitialStep.ProcessStepStatusId && step.DateLastChanged != stepData[1].InitialStep.DateLastChanged && step.Message == stepData[1].InitialStep.Message,
            step => step.Id == stepData[2].ProcessStepId && step.ProcessStepStatusId == stepData[2].InitialStep.ProcessStepStatusId && step.DateLastChanged != stepData[2].InitialStep.DateLastChanged && step.Message == stepData[2].InitialStep.Message,
            step => step.Id == stepData[3].ProcessStepId && step.ProcessStepStatusId == stepData[3].InitialStep.ProcessStepStatusId && step.DateLastChanged != stepData[3].InitialStep.DateLastChanged && step.Message == stepData[3].InitialStep.Message,
            step => step.Id == stepData[4].ProcessStepId && step.ProcessStepStatusId == stepData[4].InitialStep.ProcessStepStatusId && step.DateLastChanged != stepData[4].InitialStep.DateLastChanged && step.Message == stepData[4].InitialStep.Message
        );
    }

    #endregion

    #region GetActiveProcesses

    [Fact]
    public async Task GetActiveProcess_LockExpired_ReturnsExpected()
    {
        // Arrange
        var processTypeIds = new[] { ProcessTypeId.SETUP_DIM };
        var processStepTypeIds = new[] {
            ProcessStepTypeId.CREATE_WALLET,
            ProcessStepTypeId.CHECK_OPERATION,
            ProcessStepTypeId.GET_COMPANY,
            ProcessStepTypeId.GET_DID_DOCUMENT
        };

        var sut = await CreateSut();

        // Act
        var result = await sut.GetActiveProcesses(processTypeIds, processStepTypeIds, DateTimeOffset.UtcNow).ToListAsync();
        result.Should().HaveCount(1)
            .And.Satisfy(
                x => x.Id == new Guid("dd371565-9489-4907-a2e4-b8cbfe7a8cd2") && x.ProcessTypeId == ProcessTypeId.SETUP_DIM && x.LockExpiryDate == null
            );
    }

    [Fact]
    public async Task GetActiveProcess_Locked_ReturnsExpected()
    {
        // Arrange
        var processTypeIds = new[] { ProcessTypeId.SETUP_DIM };
        var processStepTypeIds = new[] {
            ProcessStepTypeId.CREATE_WALLET,
            ProcessStepTypeId.CHECK_OPERATION,
            ProcessStepTypeId.GET_COMPANY,
            ProcessStepTypeId.GET_DID_DOCUMENT
        };

        var sut = await CreateSut();

        // Act
        var result = await sut.GetActiveProcesses(processTypeIds, processStepTypeIds, DateTimeOffset.UtcNow).ToListAsync();
        result.Should().HaveCount(1);
    }

    #endregion

    #region GetProcessStepData

    [Fact]
    public async Task GetProcessStepData_ReturnsExpected()
    {
        // Arrange
        var processId = new Guid("dd371565-9489-4907-a2e4-b8cbfe7a8cd2");
        var sut = await CreateSut();

        // Act
        var result = await sut.GetProcessStepData(processId).ToListAsync();
        result.Should().HaveCount(2)
            .And.Satisfy(
                x => x.ProcessStepId == new Guid("80771e4a-0d69-43b8-b278-25884da7f97d") && x.ProcessStepTypeId == ProcessStepTypeId.CREATE_WALLET,
                x => x.ProcessStepId == new Guid("cd231cb8-55de-4ae4-b93f-d440512341fb") && x.ProcessStepTypeId == ProcessStepTypeId.GET_COMPANY
            );
    }

    #endregion

    private async Task<(ProcessStepRepository sut, DimDbContext dbContext)> CreateSutWithContext()
    {
        var context = await _dbTestDbFixture.GetDbContext();
        var sut = new ProcessStepRepository(context);
        return (sut, context);
    }

    private async Task<ProcessStepRepository> CreateSut()
    {
        var context = await _dbTestDbFixture.GetDbContext();
        return new ProcessStepRepository(context);
    }
}
