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

public class DimRepositoriesTests : IAssemblyFixture<TestDbFixture>
{
    private readonly TestDbFixture _dbTestDbFixture;

    public DimRepositoriesTests(TestDbFixture testDbFixture)
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _dbTestDbFixture = testDbFixture;
    }

    #region GetInstance

    [Fact]
    public async Task GetInstance_TenantRepository_CreatesSuccessfully()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = sut.GetInstance<ITenantRepository>();

        // Assert
        result.Should().BeOfType<TenantRepository>();
    }

    [Fact]
    public async Task GetInstance_TechnicalUserRepository_CreatesSuccessfully()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = sut.GetInstance<ITechnicalUserRepository>();

        // Assert
        result.Should().BeOfType<TechnicalUserRepository>();
    }

    [Fact]
    public async Task GetInstance_ProcessStepRepository_CreatesSuccessfully()
    {
        // Arrange
        var sut = await CreateSut();

        // Act
        var result = sut.GetInstance<IProcessStepRepository>();

        // Assert
        result.Should().BeOfType<ProcessStepRepository>();
    }

    #endregion

    #region Clear

    [Fact]
    public async Task Clear_CreateSuccessfully()
    {
        // Arrange
        var (sut, dbContext) = await CreateSutWithContext();
        var changeTracker = dbContext.ChangeTracker;
        dbContext.Processes.Add(new Process(Guid.NewGuid(), ProcessTypeId.SETUP_DIM, Guid.NewGuid()));

        // Act
        sut.Clear();

        // Assert
        changeTracker.HasChanges().Should().BeFalse();
        changeTracker.Entries().Should().BeEmpty();
    }

    #endregion

    #region Attach

    [Fact]
    public async Task Attach_CreateSuccessfully()
    {
        // Arrange
        var (sut, dbContext) = await CreateSutWithContext();
        var changeTracker = dbContext.ChangeTracker;
        var now = DateTimeOffset.Now;

        // Act
        sut.Attach(new Process(new Guid("dd371565-9489-4907-a2e4-b8cbfe7a8cd2"), default, Guid.Empty), p =>
        {
            p.LockExpiryDate = now;
            p.ProcessTypeId = ProcessTypeId.SETUP_DIM;
        });

        // Assert
        changeTracker.HasChanges().Should().BeTrue();
        changeTracker.Entries().Should()
            .ContainSingle()
            .Which.State.Should().Be(EntityState.Modified);
        changeTracker.Entries().Select(x => x.Entity).Cast<Process>()
            .Should().Satisfy(x => x.ProcessTypeId == ProcessTypeId.SETUP_DIM);
    }

    #endregion

    private async Task<(DimRepositories sut, DimDbContext dbContext)> CreateSutWithContext()
    {
        var context = await _dbTestDbFixture.GetDbContext();
        var sut = new DimRepositories(context);
        return (sut, context);
    }

    private async Task<DimRepositories> CreateSut()
    {
        var context = await _dbTestDbFixture.GetDbContext();
        return new DimRepositories(context);
    }
}
