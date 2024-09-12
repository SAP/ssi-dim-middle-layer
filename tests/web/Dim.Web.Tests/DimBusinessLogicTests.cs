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

using Dim.Clients.Api.Cf;
using Dim.Clients.Api.Dim;
using Dim.DbAccess;
using Dim.DbAccess.Repositories;
using Dim.Entities.Entities;
using Dim.Entities.Enums;
using Dim.Web.BusinessLogic;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;

namespace Dim.Web.Tests;

public class DimBusinessLogicTests
{
    private static readonly Guid OperatorId = Guid.NewGuid();
    private readonly IDimBusinessLogic _sut;
    private readonly ICfClient _cfClient;
    private readonly IDimClient _dimClient;
    private readonly ITenantRepository _tenantRepository;
    private readonly IProcessStepRepository _processStepRepository;

    public DimBusinessLogicTests()
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var repositories = A.Fake<IDimRepositories>();
        _dimClient = A.Fake<IDimClient>();
        _cfClient = A.Fake<ICfClient>();

        _tenantRepository = A.Fake<ITenantRepository>();
        _processStepRepository = A.Fake<IProcessStepRepository>();

        A.CallTo(() => repositories.GetInstance<ITenantRepository>()).Returns(_tenantRepository);
        A.CallTo(() => repositories.GetInstance<IProcessStepRepository>()).Returns(_processStepRepository);

        _sut = new DimBusinessLogic(repositories, _cfClient, _dimClient, Options.Create(new DimSettings
        {
            OperatorId = OperatorId
        }));
    }

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
        result.Message.Should().Be($"Tenant testCompany with Bpn BPNL00000001TEST already exists");
    }

    [Theory]
    [InlineData("testCompany", "testcompany")]
    [InlineData("-abc123", "abc123")]
    [InlineData("abc-123", "abc123")]
    [InlineData("abc#123", "abc123")]
    [InlineData("abc'123", "abc123")]
    [InlineData("abc\"123", "abc123")]
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
            });
        A.CallTo(() => _processStepRepository.CreateProcessStep(A<ProcessStepTypeId>._, A<ProcessStepStatusId>._, A<Guid>._))
            .Invokes((ProcessStepTypeId processStepTypeId, ProcessStepStatusId processStepStatusId, Guid pId) =>
            {
                processSteps.Add(new ProcessStep(Guid.NewGuid(), processStepTypeId, processStepStatusId, processId, DateTimeOffset.UtcNow));
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
            .And.Satisfy(x => x.ProcessId == processId && x.ProcessStepTypeId == ProcessStepTypeId.CREATE_SUBACCOUNT);
        tenants.Should().ContainSingle()
            .And.Satisfy(x => x.CompanyName == expectedCompanyName && x.Bpn == "BPNL00000001TEST");
    }
}
