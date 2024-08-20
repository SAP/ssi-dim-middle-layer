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
using Dim.Clients.Api.Entitlements;
using Dim.Clients.Api.Provisioning;
using Dim.Clients.Api.Services;
using Dim.Clients.Api.SubAccounts;
using Dim.Clients.Api.Subscriptions;
using Dim.Clients.Token;
using Dim.DbAccess;
using Dim.DbAccess.Repositories;
using Dim.Entities.Entities;
using Dim.Entities.Enums;
using DimProcess.Library.Callback;
using DimProcess.Library.DependencyInjection;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using System.Text.Json;

namespace DimProcess.Library.Tests;

public class DimProcessHandlerTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _processId = Guid.NewGuid();
    private readonly Guid _operatorId = Guid.NewGuid();
    private readonly string _tenantName = "testCorp";
    private readonly Guid _rootDirectoryId = Guid.NewGuid();

    private readonly IDimRepositories _repositories;
    private readonly ITenantRepository _tenantRepositories;
    private readonly ISubAccountClient _subAccountClient;
    private readonly IServiceClient _serviceClient;
    private readonly ISubscriptionClient _subscriptionClient;
    private readonly IEntitlementClient _entitlementClient;
    private readonly IProvisioningClient _provisioningClient;
    private readonly ICfClient _cfClient;
    private readonly IDimClient _dimClient;
    private readonly ICallbackService _callbackService;
    private readonly IOptions<DimHandlerSettings> _options;

    private readonly DimProcessHandler _sut;
    private readonly IFixture _fixture;

    public DimProcessHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _repositories = A.Fake<IDimRepositories>();
        _tenantRepositories = A.Fake<ITenantRepository>();

        A.CallTo(() => _repositories.GetInstance<ITenantRepository>()).Returns(_tenantRepositories);

        _subAccountClient = A.Fake<ISubAccountClient>();
        _serviceClient = A.Fake<IServiceClient>();
        _subscriptionClient = A.Fake<ISubscriptionClient>();
        _entitlementClient = A.Fake<IEntitlementClient>();
        _provisioningClient = A.Fake<IProvisioningClient>();
        _cfClient = A.Fake<ICfClient>();
        _dimClient = A.Fake<IDimClient>();
        _callbackService = A.Fake<ICallbackService>();
        _options = Options.Create(new DimHandlerSettings
        {
            AdminMail = "test@example.org",
            AuthUrl = "https://example.org/auth",
            ClientidCisCentral = "test123",
            ClientsecretCisCentral = "test654",
            EncryptionKey = "test123",
            RootDirectoryId = _rootDirectoryId
        });

        _sut = new DimProcessHandler(_repositories, _subAccountClient, _serviceClient, _subscriptionClient,
            _entitlementClient, _provisioningClient, _cfClient, _dimClient, _callbackService, _options);
    }

    #region CreateSubaccount

    [Fact]
    public async Task CreateSubaccount_WithValidData_ReturnsExpected()
    {
        // Arrange
        var subAccountId = Guid.NewGuid();
        var tenant = new Tenant(_tenantId, "test", "Corp", "https://example.org/did", false, _processId, _operatorId);
        A.CallTo(() => _tenantRepositories.AttachAndModifyTenant(_tenantId, A<Action<Tenant>>._, A<Action<Tenant>>._))
            .Invokes((Guid _, Action<Tenant>? initialize, Action<Tenant> modify) =>
            {
                initialize?.Invoke(tenant);
                modify(tenant);
            });
        A.CallTo(() => _subAccountClient.CreateSubaccount(A<BasicAuthSettings>._, A<string>._, _tenantName, A<Guid>._, A<string>._, A<CancellationToken>._))
            .Returns(subAccountId);

        // Act
        var result = await _sut.CreateSubaccount(_tenantId, _tenantName, CancellationToken.None);

        // Assert
        result.modified.Should().BeFalse();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().ContainSingle().Which.Should().Be(ProcessStepTypeId.CREATE_SERVICEMANAGER_BINDINGS);
        tenant.SubAccountId.Should().Be(subAccountId);
    }

    #endregion

    #region CreateServiceManagerBindings

    [Fact]
    public async Task CreateServiceManagerBindings_WithNotExisting_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _tenantRepositories.GetSubAccountIdByTenantId(_tenantId))
            .Returns((Guid?)null);
        async Task Act() => await _sut.CreateServiceManagerBindings(_tenantId, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("SubAccountId must not be null.");
    }

    [Fact]
    public async Task CreateServiceManagerBindings_WithValidData_ReturnsExpected()
    {
        // Arrange
        var subAccountId = Guid.NewGuid();
        A.CallTo(() => _tenantRepositories.GetSubAccountIdByTenantId(_tenantId))
            .Returns(subAccountId);

        // Act
        var result = await _sut.CreateServiceManagerBindings(_tenantId, CancellationToken.None);

        // Assert
        A.CallTo(() => _subAccountClient.CreateServiceManagerBindings(A<BasicAuthSettings>._, subAccountId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        result.modified.Should().BeFalse();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().ContainSingle().Which.Should().Be(ProcessStepTypeId.ASSIGN_ENTITLEMENTS);
    }

    #endregion

    #region AssignEntitlements

    [Fact]
    public async Task AssignEntitlements_WithNotExisting_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _tenantRepositories.GetSubAccountIdByTenantId(_tenantId))
            .Returns((Guid?)null);
        async Task Act() => await _sut.AssignEntitlements(_tenantId, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("SubAccountId must not be null.");
    }

    [Fact]
    public async Task AssignEntitlements_WithValidData_ReturnsExpected()
    {
        // Arrange
        var subAccountId = Guid.NewGuid();
        A.CallTo(() => _tenantRepositories.GetSubAccountIdByTenantId(_tenantId))
            .Returns(subAccountId);

        // Act
        var result = await _sut.AssignEntitlements(_tenantId, CancellationToken.None);

        // Assert
        A.CallTo(() => _entitlementClient.AssignEntitlements(A<BasicAuthSettings>._, subAccountId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        result.modified.Should().BeFalse();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().ContainSingle().Which.Should().Be(ProcessStepTypeId.CREATE_SERVICE_INSTANCE);
    }

    #endregion

    #region CreateServiceInstance

    [Fact]
    public async Task CreateServiceInstance_WithNotExisting_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _tenantRepositories.GetSubAccountIdByTenantId(_tenantId))
            .Returns((Guid?)null);
        async Task Act() => await _sut.CreateServiceInstance(_tenantId, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("SubAccountId must not be null.");
    }

    [Fact]
    public async Task CreateServiceInstance_WithValidData_ReturnsExpected()
    {
        // Arrange
        var subAccountId = Guid.NewGuid();
        var serviceInstance = new CreateServiceInstanceResponse(Guid.NewGuid().ToString(), "test");
        var tenant = new Tenant(_tenantId, "test", "Corp", "https://example.org/did", false, _processId, _operatorId);
        A.CallTo(() => _tenantRepositories.GetSubAccountIdByTenantId(_tenantId))
            .Returns(subAccountId);
        A.CallTo(() => _subAccountClient.GetServiceManagerBindings(A<BasicAuthSettings>._, subAccountId, A<CancellationToken>._))
            .Returns(new ServiceManagementBindingItem("test", "test123", "https://example.org/sm", "https://example.org"));
        A.CallTo(() => _tenantRepositories.AttachAndModifyTenant(_tenantId, A<Action<Tenant>>._, A<Action<Tenant>>._))
            .Invokes((Guid _, Action<Tenant>? initialize, Action<Tenant> modify) =>
            {
                initialize?.Invoke(tenant);
                modify(tenant);
            });
        A.CallTo(() => _serviceClient.CreateServiceInstance(A<ServiceManagementBindingItem>._, A<CancellationToken>._))
            .Returns(serviceInstance);

        // Act
        var result = await _sut.CreateServiceInstance(_tenantId, CancellationToken.None);

        // Assert
        A.CallTo(() => _serviceClient.CreateServiceInstance(A<ServiceManagementBindingItem>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        result.modified.Should().BeFalse();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().ContainSingle().Which.Should().Be(ProcessStepTypeId.CREATE_SERVICE_BINDING);
        tenant.ServiceInstanceId.Should().Be(serviceInstance.Id);
    }

    #endregion

    #region CreateServiceBindings

    [Fact]
    public async Task CreateServiceBindings_WithNotExistingSubAccount_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _tenantRepositories.GetSubAccountAndServiceInstanceIdsByTenantId(_tenantId))
            .Returns((null, null));
        async Task Act() => await _sut.CreateServiceBindings(_tenantId, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("SubAccountId must not be null.");
    }

    [Fact]
    public async Task CreateServiceBindings_WithNotExistingServiceInstance_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _tenantRepositories.GetSubAccountAndServiceInstanceIdsByTenantId(_tenantId))
            .Returns((Guid.NewGuid(), null));
        async Task Act() => await _sut.CreateServiceBindings(_tenantId, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("ServiceInstanceId must not be null.");
    }

    [Fact]
    public async Task CreateServiceBindings_WithValidData_ReturnsExpected()
    {
        // Arrange
        var subAccountId = Guid.NewGuid();
        var serviceInstanceId = Guid.NewGuid().ToString();
        var binding = new ServiceManagementBindingItem("cl1", "s1", "https://example.org/sm", "https://example.org");
        var tenant = new Tenant(_tenantId, "test", "Corp", "https://example.org/did", false, _processId, _operatorId);
        A.CallTo(() => _tenantRepositories.GetSubAccountAndServiceInstanceIdsByTenantId(_tenantId))
            .Returns((subAccountId, serviceInstanceId));
        A.CallTo(() => _subAccountClient.GetServiceManagerBindings(A<BasicAuthSettings>._, subAccountId, A<CancellationToken>._))
            .Returns(new ServiceManagementBindingItem("test", "test123", "https://example.org/sm", "https://example.org"));
        A.CallTo(() => _tenantRepositories.AttachAndModifyTenant(_tenantId, A<Action<Tenant>>._, A<Action<Tenant>>._))
            .Invokes((Guid _, Action<Tenant>? initialize, Action<Tenant> modify) =>
            {
                initialize?.Invoke(tenant);
                modify(tenant);
            });
        A.CallTo(() => _subAccountClient.GetServiceManagerBindings(A<BasicAuthSettings>._, subAccountId, A<CancellationToken>._))
            .Returns(binding);
        A.CallTo(() => _serviceClient.CreateServiceBinding(binding, serviceInstanceId, A<CancellationToken>._))
            .Returns(new CreateServiceBindingResponse(Guid.NewGuid().ToString(), "expectedName"));

        // Act
        var result = await _sut.CreateServiceBindings(_tenantId, CancellationToken.None);

        // Assert
        A.CallTo(() => _serviceClient.CreateServiceBinding(binding, serviceInstanceId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        result.modified.Should().BeFalse();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().ContainSingle().Which.Should().Be(ProcessStepTypeId.SUBSCRIBE_APPLICATION);
        tenant.ServiceBindingName.Should().Be("expectedName");
    }

    #endregion

    #region SubscribeApplication

    [Fact]
    public async Task SubscribeApplication_WithNotExistingSubAccount_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _tenantRepositories.GetSubAccountAndServiceInstanceIdsByTenantId(_tenantId))
            .Returns((null, null));
        async Task Act() => await _sut.SubscribeApplication(_tenantId, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("SubAccountId must not be null.");
    }

    [Fact]
    public async Task SubscribeApplication_WithNotExistingServiceBindingName_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _tenantRepositories.GetSubAccountAndServiceInstanceIdsByTenantId(_tenantId))
            .Returns((Guid.NewGuid(), null));
        async Task Act() => await _sut.SubscribeApplication(_tenantId, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("SubAccountId must not be null.");
    }

    [Fact]
    public async Task SubscribeApplication_WithValidData_ReturnsExpected()
    {
        // Arrange
        var subAccountId = Guid.NewGuid();
        var serviceInstanceId = Guid.NewGuid();
        var serviceBindingName = Guid.NewGuid().ToString();
        var serviceManagementBinding = new ServiceManagementBindingItem("c1", "cs1", "https://example.org/sm", "https://example.org/");
        var binding = new BindingItem("binding1", serviceInstanceId, _fixture.Create<BindingCredentials>());
        A.CallTo(() => _tenantRepositories.GetSubAccountIdAndServiceBindingNameByTenantId(_tenantId))
            .Returns((subAccountId, serviceBindingName));
        A.CallTo(() => _subAccountClient.GetServiceManagerBindings(A<BasicAuthSettings>._, subAccountId, A<CancellationToken>._))
            .Returns(new ServiceManagementBindingItem("test", "test123", "https://example.org/sm", "https://example.org"));
        A.CallTo(() => _subAccountClient.GetServiceManagerBindings(A<BasicAuthSettings>._, subAccountId, A<CancellationToken>._))
            .Returns(serviceManagementBinding);
        A.CallTo(() => _serviceClient.GetServiceBinding(A<ServiceManagementBindingItem>._, serviceBindingName, A<CancellationToken>._))
            .Returns(binding);
        A.CallTo(() => _serviceClient.CreateServiceBinding(serviceManagementBinding, serviceBindingName, A<CancellationToken>._))
            .Returns(new CreateServiceBindingResponse(Guid.NewGuid().ToString(), "expectedName"));

        // Act
        var result = await _sut.SubscribeApplication(_tenantId, CancellationToken.None);

        // Assert
        A.CallTo(() => _subscriptionClient.SubscribeApplication(A<string>._, binding, "decentralized-identity-management-app", "standard", A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        result.modified.Should().BeFalse();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().ContainSingle().Which.Should().Be(ProcessStepTypeId.CREATE_CLOUD_FOUNDRY_ENVIRONMENT);
    }

    #endregion

    #region CreateCloudFoundryEnvironment

    [Fact]
    public async Task CreateCloudFoundryEnvironment_WithNotExistingSubAccount_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _tenantRepositories.GetSubAccountIdAndServiceBindingNameByTenantId(_tenantId))
            .Returns((null, null));
        async Task Act() => await _sut.CreateCloudFoundryEnvironment(_tenantId, _tenantName, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("SubAccountId must not be null.");
    }

    [Fact]
    public async Task CreateCloudFoundryEnvironment_WithNotExistingServiceInstance_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _tenantRepositories.GetSubAccountIdAndServiceBindingNameByTenantId(_tenantId))
            .Returns((Guid.NewGuid(), null));
        async Task Act() => await _sut.CreateCloudFoundryEnvironment(_tenantId, _tenantName, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("ServiceBindingName must not be null.");
    }

    [Fact]
    public async Task CreateCloudFoundryEnvironment_WithValidData_ReturnsExpected()
    {
        // Arrange
        var subAccountId = Guid.NewGuid();
        var serviceInstanceId = Guid.NewGuid();
        var serviceManagementBinding = new ServiceManagementBindingItem("c1", "cs1", "https://example.org/sm", "https://example.org/");
        var binding = new BindingItem("binding1", serviceInstanceId, _fixture.Create<BindingCredentials>());
        A.CallTo(() => _tenantRepositories.GetSubAccountIdAndServiceBindingNameByTenantId(_tenantId))
            .Returns((subAccountId, binding.Name));
        A.CallTo(() => _subAccountClient.GetServiceManagerBindings(A<BasicAuthSettings>._, subAccountId, A<CancellationToken>._))
            .Returns(new ServiceManagementBindingItem("test", "test123", "https://example.org/sm", "https://example.org"));
        A.CallTo(() => _subAccountClient.GetServiceManagerBindings(A<BasicAuthSettings>._, subAccountId, A<CancellationToken>._))
            .Returns(serviceManagementBinding);
        A.CallTo(() => _serviceClient.GetServiceBinding(serviceManagementBinding, binding.Name, A<CancellationToken>._))
            .Returns(binding);

        // Act
        var result = await _sut.CreateCloudFoundryEnvironment(_tenantId, _tenantName, CancellationToken.None);

        // Assert
        A.CallTo(() => _provisioningClient.CreateCloudFoundryEnvironment(A<string>._, binding, _tenantName, A<string>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        result.modified.Should().BeFalse();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().ContainSingle().Which.Should().Be(ProcessStepTypeId.CREATE_CLOUD_FOUNDRY_SPACE);
    }

    #endregion

    #region CreateCloudFoundrySpace

    [Fact]
    public async Task CreateCloudFoundrySpace_WithValidData_ReturnsExpected()
    {
        // Arrange
        var spaceId = Guid.NewGuid();
        var tenant = new Tenant(_tenantId, "test", "Corp", "https://example.org/did", false, _processId, _operatorId);
        A.CallTo(() => _tenantRepositories.AttachAndModifyTenant(_tenantId, A<Action<Tenant>>._, A<Action<Tenant>>._))
            .Invokes((Guid _, Action<Tenant>? initialize, Action<Tenant> modify) =>
            {
                initialize?.Invoke(tenant);
                modify(tenant);
            });
        A.CallTo(() => _cfClient.CreateCloudFoundrySpace(_tenantName, A<CancellationToken>._))
            .Returns(spaceId);

        // Act
        var result = await _sut.CreateCloudFoundrySpace(_tenantId, _tenantName, CancellationToken.None);

        // Assert
        result.modified.Should().BeFalse();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().ContainSingle().Which.Should().Be(ProcessStepTypeId.ADD_SPACE_MANAGER_ROLE);
        tenant.SpaceId.Should().Be(spaceId);
    }

    #endregion

    #region AddSpaceManagerRole

    [Fact]
    public async Task AddSpaceManagerRole_WithNotExisting_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _tenantRepositories.GetSpaceId(_tenantId))
            .Returns((Guid?)null);
        async Task Act() => await _sut.AddSpaceManagerRole(_tenantId, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("SpaceId must not be null.");
    }

    [Fact]
    public async Task AddSpaceManagerRole_WithValidData_ReturnsExpected()
    {
        // Arrange
        var spaceId = Guid.NewGuid();
        A.CallTo(() => _tenantRepositories.GetSpaceId(_tenantId))
            .Returns(spaceId);

        // Act
        var result = await _sut.AddSpaceManagerRole(_tenantId, CancellationToken.None);

        // Assert
        A.CallTo(() => _cfClient.AddSpaceRoleToUser("space_manager", A<string>._, spaceId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        result.modified.Should().BeFalse();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().ContainSingle().Which.Should().Be(ProcessStepTypeId.ADD_SPACE_DEVELOPER_ROLE);
    }

    #endregion

    #region AddSpaceManagerRole

    [Fact]
    public async Task AddSpaceDeveloperRole_WithNotExisting_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _tenantRepositories.GetSpaceId(_tenantId))
            .Returns((Guid?)null);
        async Task Act() => await _sut.AddSpaceDeveloperRole(_tenantId, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("SpaceId must not be null.");
    }

    [Fact]
    public async Task AddSpaceDeveloperRole_WithValidData_ReturnsExpected()
    {
        // Arrange
        var spaceId = Guid.NewGuid();
        A.CallTo(() => _tenantRepositories.GetSpaceId(_tenantId))
            .Returns(spaceId);

        // Act
        var result = await _sut.AddSpaceDeveloperRole(_tenantId, CancellationToken.None);

        // Assert
        A.CallTo(() => _cfClient.AddSpaceRoleToUser("space_developer", A<string>._, spaceId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        result.modified.Should().BeFalse();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().ContainSingle().Which.Should().Be(ProcessStepTypeId.CREATE_DIM_SERVICE_INSTANCE);
    }

    #endregion

    #region CreateDimServiceInstance

    [Fact]
    public async Task CreateDimServiceInstance_WithValidData_ReturnsExpected()
    {
        // Arrange
        var spaceId = Guid.NewGuid();
        var servicePlanId = Guid.NewGuid();
        A.CallTo(() => _cfClient.GetSpace(_tenantName, A<CancellationToken>._))
            .Returns(spaceId);
        A.CallTo(() => _cfClient.GetServicePlan("decentralized-identity-management", "standard", A<CancellationToken>._))
            .Returns(servicePlanId);

        // Act
        var result = await _sut.CreateDimServiceInstance(_tenantName, _tenantId, CancellationToken.None);

        // Assert
        A.CallTo(() => _cfClient.CreateDimServiceInstance(_tenantName, spaceId, servicePlanId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        result.modified.Should().BeFalse();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().ContainSingle().Which.Should().Be(ProcessStepTypeId.CREATE_SERVICE_INSTANCE_BINDING);
    }

    #endregion

    #region CreateServiceInstanceBindings

    [Fact]
    public async Task CreateServiceInstanceBindings_WithNotExisting_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _tenantRepositories.GetSpaceId(_tenantId))
            .Returns((Guid?)null);
        async Task Act() => await _sut.CreateServiceInstanceBindings(_tenantName, _tenantId, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("SpaceId must not be null.");
    }

    [Fact]
    public async Task CreateServiceInstanceBindings_WithValidData_ReturnsExpected()
    {
        // Arrange
        var spaceId = Guid.NewGuid();
        var servicePlanId = Guid.NewGuid();
        A.CallTo(() => _tenantRepositories.GetSpaceId(_tenantId))
            .Returns(spaceId);
        A.CallTo(() => _cfClient.GetServicePlan("decentralized-identity-management", "standard", A<CancellationToken>._))
            .Returns(servicePlanId);

        // Act
        var result = await _sut.CreateServiceInstanceBindings(_tenantName, _tenantId, CancellationToken.None);

        // Assert
        A.CallTo(() => _cfClient.CreateServiceInstanceBindings(_tenantName, null, spaceId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        result.modified.Should().BeFalse();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().ContainSingle().Which.Should().Be(ProcessStepTypeId.GET_DIM_DETAILS);
    }

    #endregion

    #region GetDimDetails

    [Fact]
    public async Task GetDimDetails_WithNotExisting_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _tenantRepositories.GetSpaceId(_tenantId))
            .Returns((Guid?)null);
        async Task Act() => await _sut.GetDimDetails(_tenantName, _tenantId, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("SpaceId must not be null.");
    }

    [Fact]
    public async Task GetDimDetails_WithValidData_ReturnsExpected()
    {
        // Arrange
        var spaceId = Guid.NewGuid();
        var dimInstanceId = Guid.NewGuid();
        var tenant = new Tenant(_tenantId, "test", "Corp", "https://example.org/did", false, _processId, _operatorId);
        A.CallTo(() => _tenantRepositories.GetSpaceId(_tenantId))
            .Returns(spaceId);
        A.CallTo(() => _cfClient.GetServiceBinding(_tenantName, spaceId, $"{_tenantName}-dim-key01", A<CancellationToken>._))
            .Returns(dimInstanceId);
        A.CallTo(() => _tenantRepositories.AttachAndModifyTenant(_tenantId, A<Action<Tenant>>._, A<Action<Tenant>>._))
            .Invokes((Guid _, Action<Tenant>? initialize, Action<Tenant> modify) =>
            {
                initialize?.Invoke(tenant);
                modify(tenant);
            });

        // Act
        var result = await _sut.GetDimDetails(_tenantName, _tenantId, CancellationToken.None);

        // Assert
        result.modified.Should().BeFalse();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().ContainSingle().Which.Should().Be(ProcessStepTypeId.CREATE_APPLICATION);
        tenant.DimInstanceId.Should().Be(dimInstanceId);
    }

    #endregion

    #region CreateApplication

    [Fact]
    public async Task CreateApplication_WithNotExisting_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _tenantRepositories.GetDimInstanceIdAndHostingUrl(_tenantId))
            .Returns(((Guid?)null, string.Empty, false));
        async Task Act() => await _sut.CreateApplication(_tenantName, _tenantId, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("DimInstanceId must not be null.");
    }

    [Fact]
    public async Task CreateApplication_WithValidData_ReturnsExpected()
    {
        // Arrange
        var serviceCrenentialBinding = _fixture.Create<ServiceCredentialBindingDetailResponse>();
        var dimInstanceId = Guid.NewGuid();
        var applicationId = Guid.NewGuid().ToString();
        var tenant = new Tenant(_tenantId, "test", "Corp", "https://example.org/did", false, _processId, _operatorId);
        A.CallTo(() => _tenantRepositories.GetDimInstanceIdAndHostingUrl(_tenantId))
            .Returns((dimInstanceId, string.Empty, false));
        A.CallTo(() => _cfClient.GetServiceBindingDetails(dimInstanceId, A<CancellationToken>._))
            .Returns(serviceCrenentialBinding);
        A.CallTo(() => _tenantRepositories.AttachAndModifyTenant(_tenantId, A<Action<Tenant>>._, A<Action<Tenant>>._))
            .Invokes((Guid _, Action<Tenant>? initialize, Action<Tenant> modify) =>
            {
                initialize?.Invoke(tenant);
                modify(tenant);
            });
        A.CallTo(() => _dimClient.CreateApplication(A<BasicAuthSettings>._, A<string>._, _tenantName, A<CancellationToken>._))
            .Returns(applicationId);

        // Act
        var result = await _sut.CreateApplication(_tenantName, _tenantId, CancellationToken.None);

        // Assert
        A.CallTo(() => _dimClient.CreateApplication(A<BasicAuthSettings>._, A<string>._, _tenantName, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        result.modified.Should().BeFalse();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().ContainSingle().Which.Should().Be(ProcessStepTypeId.CREATE_COMPANY_IDENTITY);
        tenant.ApplicationId.Should().Be(applicationId);
    }

    #endregion

    #region CreateCompanyIdentity

    [Fact]
    public async Task CreateCompanyIdentity_WithNotExisting_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _tenantRepositories.GetDimInstanceIdAndHostingUrl(_tenantId))
            .Returns(((Guid?)null, string.Empty, false));
        async Task Act() => await _sut.CreateCompanyIdentity(_tenantId, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("DimInstanceId must not be null.");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CreateCompanyIdentity_WithValidData_ReturnsExpected(bool isIssuer)
    {
        // Arrange
        var serviceCrenentialBinding = _fixture.Create<ServiceCredentialBindingDetailResponse>();
        var identityResponse = _fixture.Create<CreateCompanyIdentityResponse>();
        var dimInstanceId = Guid.NewGuid();
        var tenant = new Tenant(_tenantId, "test", "Corp", "https://example.org/did", isIssuer, _processId, _operatorId);
        A.CallTo(() => _tenantRepositories.GetDimInstanceIdAndHostingUrl(_tenantId))
            .Returns((dimInstanceId, "https://example.org/hosting", tenant.IsIssuer));
        A.CallTo(() => _cfClient.GetServiceBindingDetails(dimInstanceId, A<CancellationToken>._))
            .Returns(serviceCrenentialBinding);
        A.CallTo(() => _tenantRepositories.AttachAndModifyTenant(_tenantId, A<Action<Tenant>>._, A<Action<Tenant>>._))
            .Invokes((Guid _, Action<Tenant>? initialize, Action<Tenant> modify) =>
            {
                initialize?.Invoke(tenant);
                modify(tenant);
            });
        A.CallTo(() => _dimClient.CreateCompanyIdentity(A<BasicAuthSettings>._, _tenantId, "https://example.org/hosting", A<string>._, tenant.IsIssuer, A<CancellationToken>._))
            .Returns(identityResponse);

        // Act
        var result = await _sut.CreateCompanyIdentity(_tenantId, CancellationToken.None);

        // Assert
        A.CallTo(() => _dimClient.CreateCompanyIdentity(A<BasicAuthSettings>._, A<Guid>._, A<string>._, A<string>._, tenant.IsIssuer, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        result.modified.Should().BeFalse();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().ContainSingle().Which.Should().Be(ProcessStepTypeId.ASSIGN_COMPANY_APPLICATION);
        tenant.Did.Should().Be(identityResponse.Did);
        tenant.DidDownloadUrl.Should().Be(identityResponse.DownloadUrl);
        tenant.CompanyId.Should().Be(identityResponse.CompanyId);
    }

    #endregion

    #region AssignCompanyApplication

    [Fact]
    public async Task AssignCompanyApplication_WithNotExisting_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _tenantRepositories.GetApplicationAndCompanyId(_tenantId))
            .Returns(((string?)null, (Guid?)null, (Guid?)null, false));
        async Task Act() => await _sut.AssignCompanyApplication(_tenantId, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("ApplicationId must always be set here");
    }

    [Fact]
    public async Task AssignCompanyApplication_WithNoCompanyId_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _tenantRepositories.GetApplicationAndCompanyId(_tenantId))
            .Returns((Guid.NewGuid().ToString(), (Guid?)null, (Guid?)null, false));
        async Task Act() => await _sut.AssignCompanyApplication(_tenantId, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("CompanyId must always be set here");
    }

    [Fact]
    public async Task AssignCompanyApplication_WithNoDimInstanceId_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _tenantRepositories.GetApplicationAndCompanyId(_tenantId))
            .Returns((Guid.NewGuid().ToString(), Guid.NewGuid(), null, false));
        async Task Act() => await _sut.AssignCompanyApplication(_tenantId, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("DimInstanceId must not be null.");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task AssignCompanyApplication_WithValidData_ReturnsExpected(bool isIssuer)
    {
        // Arrange
        var serviceCrenentialBinding = _fixture.Create<ServiceCredentialBindingDetailResponse>();
        var identityResponse = _fixture.Create<CreateCompanyIdentityResponse>();
        var applicationId = Guid.NewGuid().ToString();
        var applicationKey = Guid.NewGuid().ToString();
        var companyId = Guid.NewGuid();
        var dimInstanceId = Guid.NewGuid();
        var tenant = new Tenant(_tenantId, "test", "Corp", "https://example.org/did", false, _processId, _operatorId);
        A.CallTo(() => _tenantRepositories.GetApplicationAndCompanyId(_tenantId))
            .Returns((applicationId, companyId, dimInstanceId, isIssuer));
        A.CallTo(() => _cfClient.GetServiceBindingDetails(dimInstanceId, A<CancellationToken>._))
            .Returns(serviceCrenentialBinding);
        A.CallTo(() => _dimClient.GetApplication(A<BasicAuthSettings>._, A<string>._, applicationId, A<CancellationToken>._))
            .Returns(applicationKey);
        A.CallTo(() => _tenantRepositories.AttachAndModifyTenant(_tenantId, A<Action<Tenant>>._, A<Action<Tenant>>._))
            .Invokes((Guid _, Action<Tenant>? initialize, Action<Tenant> modify) =>
            {
                initialize?.Invoke(tenant);
                modify(tenant);
            });
        A.CallTo(() => _dimClient.CreateCompanyIdentity(A<BasicAuthSettings>._, _tenantId, "https://example.org/hosting", A<string>._, false, A<CancellationToken>._))
            .Returns(identityResponse);

        // Act
        var result = await _sut.AssignCompanyApplication(_tenantId, CancellationToken.None);

        // Assert
        A.CallTo(() => _dimClient.AssignApplicationToCompany(A<BasicAuthSettings>._, A<string>._, applicationKey, companyId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        result.modified.Should().BeFalse();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().ContainSingle().Which.Should().Be(isIssuer ? ProcessStepTypeId.CREATE_STATUS_LIST : ProcessStepTypeId.SEND_CALLBACK);
        tenant.ApplicationKey.Should().Be(applicationKey);
    }

    #endregion

    #region CreateStatusList

    [Fact]
    public async Task CreateStatusList_WithNoCompanyId_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _tenantRepositories.GetApplicationAndCompanyId(_tenantId))
            .Returns((Guid.NewGuid().ToString(), (Guid?)null, (Guid?)null, false));
        async Task Act() => await _sut.CreateStatusList(_tenantId, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("CompanyId must always be set here");
    }

    [Fact]
    public async Task CreateStatusList_WithNoDimInstanceId_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _tenantRepositories.GetApplicationAndCompanyId(_tenantId))
            .Returns((Guid.NewGuid().ToString(), Guid.NewGuid(), null, false));
        async Task Act() => await _sut.CreateStatusList(_tenantId, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("DimInstanceId must not be null.");
    }

    [Fact]
    public async Task CreateStatusList_WithValidData_ReturnsExpected()
    {
        // Arrange
        var serviceCrenentialBinding = _fixture.Create<ServiceCredentialBindingDetailResponse>();
        var applicationId = Guid.NewGuid().ToString();
        var companyId = Guid.NewGuid();
        var dimInstanceId = Guid.NewGuid();
        A.CallTo(() => _tenantRepositories.GetApplicationAndCompanyId(_tenantId))
            .Returns((applicationId, companyId, dimInstanceId, false));
        A.CallTo(() => _cfClient.GetServiceBindingDetails(dimInstanceId, A<CancellationToken>._))
            .Returns(serviceCrenentialBinding);

        // Act
        var result = await _sut.CreateStatusList(_tenantId, CancellationToken.None);

        // Assert
        A.CallTo(() => _dimClient.CreateStatusList(A<BasicAuthSettings>._, A<string>._, companyId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        result.modified.Should().BeFalse();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().ContainSingle().Which.Should().Be(ProcessStepTypeId.SEND_CALLBACK);
    }

    #endregion

    #region SendCallback

    [Fact]
    public async Task SendCallback_WithNotExisting_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _tenantRepositories.GetCallbackData(_tenantId))
            .Returns(("bpn123", (string?)null, (string?)null, (Guid?)null));
        async Task Act() => await _sut.SendCallback(_tenantId, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("DownloadUrl must not be null.");
    }

    [Fact]
    public async Task SendCallback_WithNoCompanyId_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _tenantRepositories.GetCallbackData(_tenantId))
            .Returns(("bpn123", "https://example.org/did", (string?)null, (Guid?)null));
        async Task Act() => await _sut.SendCallback(_tenantId, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("Did must not be null.");
    }

    [Fact]
    public async Task SendCallback_WithNoDimInstanceId_ReturnsExpected()
    {
        // Arrange
        A.CallTo(() => _tenantRepositories.GetCallbackData(_tenantId))
            .Returns(("bpn123", "https://example.org/did", Guid.NewGuid().ToString(), (Guid?)null));
        async Task Act() => await _sut.SendCallback(_tenantId, CancellationToken.None).ConfigureAwait(false);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("DimInstanceId must not be null.");
    }

    [Fact]
    public async Task SendCallback_WithValidData_ReturnsExpected()
    {
        // Arrange
        var serviceCrenentialBinding = _fixture.Create<ServiceCredentialBindingDetailResponse>();
        var identityResponse = _fixture.Create<CreateCompanyIdentityResponse>();
        var dimInstanceId = Guid.NewGuid();
        var tenant = new Tenant(_tenantId, "test", "Corp", "https://example.org/did", false, _processId, _operatorId);
        var did = Guid.NewGuid().ToString();
        A.CallTo(() => _tenantRepositories.GetCallbackData(_tenantId))
            .Returns(("bpn123", "https://example.org/did", did, dimInstanceId));
        A.CallTo(() => _cfClient.GetServiceBindingDetails(dimInstanceId, A<CancellationToken>._))
            .Returns(serviceCrenentialBinding);
        A.CallTo(() => _dimClient.GetDidDocument(A<string>._, A<CancellationToken>._))
            .Returns(JsonDocument.Parse("{}"));
        A.CallTo(() => _tenantRepositories.AttachAndModifyTenant(_tenantId, A<Action<Tenant>>._, A<Action<Tenant>>._))
            .Invokes((Guid _, Action<Tenant>? initialize, Action<Tenant> modify) =>
            {
                initialize?.Invoke(tenant);
                modify(tenant);
            });
        A.CallTo(() => _dimClient.CreateCompanyIdentity(A<BasicAuthSettings>._, _tenantId, "https://example.org/hosting", A<string>._, false, A<CancellationToken>._))
            .Returns(identityResponse);

        // Act
        var result = await _sut.SendCallback(_tenantId, CancellationToken.None);

        // Assert
        A.CallTo(() => _callbackService.SendCallback("bpn123", A<ServiceCredentialBindingDetailResponse>._, A<JsonDocument>._, did, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        result.modified.Should().BeFalse();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().BeNull();
    }

    #endregion
}
