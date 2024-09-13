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

using Dim.Clients.Api.Div;
using Dim.Clients.Api.Div.Models;
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
using System.Security.Cryptography;

namespace DimProcess.Library.Tests;

public class TechnicalUserProcessHandlerTests
{
    private readonly ITenantRepository _tenantRepositories;
    private readonly ICallbackService _callbackService;
    private readonly TechnicalUserProcessHandler _sut;
    private readonly TechnicalUserSettings _settings;
    private readonly IProvisioningClient _provisioningClient;

    public TechnicalUserProcessHandlerTests()
    {
        var fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        var repositories = A.Fake<IDimRepositories>();
        _tenantRepositories = A.Fake<ITenantRepository>();

        A.CallTo(() => repositories.GetInstance<ITenantRepository>()).Returns(_tenantRepositories);

        _provisioningClient = A.Fake<IProvisioningClient>();
        _callbackService = A.Fake<ICallbackService>();
        _settings = new TechnicalUserSettings
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
        };
        var options = Options.Create(_settings);

        _sut = new TechnicalUserProcessHandler(repositories, _provisioningClient, _callbackService, options);
    }

    #region CreateServiceInstanceBindings

    [Fact]
    public async Task CreateServiceInstanceBindings_WithValid_SavesOperationId()
    {
        // Arrange
        var operationId = Guid.NewGuid();
        var technicalUser = new TechnicalUser(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "saTest", Guid.NewGuid());
        A.CallTo(() => _tenantRepositories.GetTechnicalUserNameAndWalletId(technicalUser.Id))
            .Returns((Guid.NewGuid(), technicalUser.TechnicalUserName));
        A.CallTo(() => _tenantRepositories.AttachAndModifyTechnicalUser(technicalUser.Id, A<Action<TechnicalUser>>._, A<Action<TechnicalUser>>._))
            .Invokes((Guid _, Action<TechnicalUser>? initialize, Action<TechnicalUser> modify) =>
            {
                initialize!.Invoke(technicalUser);
                modify(technicalUser);
            });
        A.CallTo(() => _provisioningClient.CreateServiceKey(technicalUser.TechnicalUserName, A<Guid>._, A<CancellationToken>._))
            .Returns(operationId);

        // Act
        var result = await _sut.CreateServiceInstanceBindings("test", technicalUser.Id, CancellationToken.None);

        // Assert
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.processMessage.Should().BeNull();
        result.modified.Should().BeFalse();
        result.nextStepTypeIds.Should().ContainSingle().And.Satisfy(x => x == ProcessStepTypeId.GET_TECHNICAL_USER_DATA);
        technicalUser.OperationId.Should().Be(operationId);
    }

    [Fact]
    public async Task CreateServiceInstanceBindings_WithWalletIdNotSet_ThrowsConflictException()
    {
        // Arrange
        var technicalUserId = Guid.NewGuid();
        Task Act() => _sut.CreateServiceInstanceBindings("test", technicalUserId, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("WalletId must not be null");
    }

    #endregion

    #region GetTechnicalUserData

    [Fact]
    public async Task GetTechnicalUserData_WithValid_SavesData()
    {
        // Arrange
        var operationId = Guid.NewGuid();
        var responseData = new OperationResponseData(
            Guid.NewGuid(),
            Guid.NewGuid().ToString(),
            "test name",
            new ServiceKey(
                new ServiceUaa("https://example.org/api", "https://example.org", "cl1", "test123"),
                "https://example.org/test",
                "test"));
        var technicalUser = new TechnicalUser(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "saTest", Guid.NewGuid());
        A.CallTo(() => _tenantRepositories.GetOperationIdForTechnicalUser(technicalUser.Id))
            .Returns(operationId);
        A.CallTo(() => _tenantRepositories.AttachAndModifyTechnicalUser(technicalUser.Id, A<Action<TechnicalUser>>._, A<Action<TechnicalUser>>._))
            .Invokes((Guid _, Action<TechnicalUser>? initialize, Action<TechnicalUser> modify) =>
            {
                initialize!.Invoke(technicalUser);
                modify(technicalUser);
            });
        A.CallTo(() => _provisioningClient.GetOperation(A<Guid>._, A<CancellationToken>._))
            .Returns(new OperationResponse(operationId, OperationResponseStatus.completed, null, null, responseData));

        // Act
        var result = await _sut.GetTechnicalUserData("test", technicalUser.Id, CancellationToken.None);

        // Assert
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.processMessage.Should().BeNull();
        result.modified.Should().BeFalse();
        result.nextStepTypeIds.Should().ContainSingle().And.Satisfy(x => x == ProcessStepTypeId.SEND_TECHNICAL_USER_CREATION_CALLBACK);

        technicalUser.TokenAddress.Should().Be(responseData.ServiceKey.Uaa.Url);
        technicalUser.ClientId.Should().Be(responseData.ServiceKey.Uaa.ClientId);
    }

    [Fact]
    public async Task GetTechnicalUserData_WithPending_StaysInTodo()
    {
        // Arrange
        var operationId = Guid.NewGuid();
        var technicalUserId = Guid.NewGuid();
        A.CallTo(() => _tenantRepositories.GetOperationIdForTechnicalUser(technicalUserId))
            .Returns(operationId);
        A.CallTo(() => _provisioningClient.GetOperation(A<Guid>._, A<CancellationToken>._))
            .Returns(new OperationResponse(operationId, OperationResponseStatus.pending, null, null, null));

        // Act
        var result = await _sut.GetTechnicalUserData("test", technicalUserId, CancellationToken.None);

        // Assert
        result.stepStatusId.Should().Be(ProcessStepStatusId.TODO);
        result.processMessage.Should().BeNull();
        result.modified.Should().BeTrue();
        result.nextStepTypeIds.Should().BeNull();
    }

    [Fact]
    public async Task GetTechnicalUserData_WithCompletedAndNoData_ThrowsUnexpectedConditionException()
    {
        // Arrange
        var operationId = Guid.NewGuid();
        var technicalUserId = Guid.NewGuid();
        A.CallTo(() => _tenantRepositories.GetOperationIdForTechnicalUser(technicalUserId))
            .Returns(operationId);
        A.CallTo(() => _provisioningClient.GetOperation(A<Guid>._, A<CancellationToken>._))
            .Returns(new OperationResponse(operationId, OperationResponseStatus.completed, null, null, null));
        Task Act() => _sut.GetTechnicalUserData("test", technicalUserId, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<UnexpectedConditionException>(Act);

        // Assert
        ex.Message.Should().Be("Data should never be null when in status completed");
    }

    [Fact]
    public async Task GetTechnicalUserData_WithOperationIdNotSet_ThrowsConflictException()
    {
        // Arrange
        var technicalUserId = Guid.NewGuid();
        Task Act() => _sut.GetTechnicalUserData("test", technicalUserId, CancellationToken.None);

        // Act
        var ex = await Assert.ThrowsAsync<ConflictException>(Act);

        // Assert
        ex.Message.Should().Be("OperationId must not be null");
    }

    #endregion

    #region SendCreateCallback

    [Fact]
    public async Task SendCreateCallback_WithValidData_ReturnsExpected()
    {
        // Arrange
        var technicalUserId = Guid.NewGuid();
        A.CallTo(() => _tenantRepositories.GetTechnicalUserCallbackData(technicalUserId))
            .Returns((Guid.NewGuid(), GetWalletData()));

        // Act
        var result = await _sut.SendCreateCallback(technicalUserId, CancellationToken.None);

        // Assert
        A.CallTo(() => _callbackService.SendTechnicalUserCallback(A<Guid>._, A<string>._, A<string>._, A<string>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();

        result.modified.Should().BeFalse();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().BeNull();
    }

    #endregion

    #region DeleteServiceInstanceBindings

    [Fact]
    public async Task DeleteServiceInstanceBindings_WithValidData_ReturnsExpected()
    {
        // Arrange
        var technicalUserId = Guid.NewGuid();

        // Act
        var result = await _sut.DeleteServiceInstanceBindings("test", technicalUserId, CancellationToken.None);

        // Assert
        result.modified.Should().BeFalse();
        result.processMessage.Should().Be("Technical User deletion is currently not supported");
        result.stepStatusId.Should().Be(ProcessStepStatusId.FAILED);
        result.nextStepTypeIds.Should().BeNull();
    }

    #endregion

    #region SendCallback

    [Fact]
    public async Task SendCallback_WithValidData_ReturnsExpected()
    {
        // Arrange
        var technicalUserId = Guid.NewGuid();
        var externalId = Guid.NewGuid();
        var technicalUsers = new List<TechnicalUser>
        {
            new(technicalUserId, Guid.NewGuid(), Guid.NewGuid(), "sa-t", Guid.NewGuid())
        };
        A.CallTo(() => _tenantRepositories.GetExternalIdForTechnicalUser(technicalUserId))
            .Returns(externalId);
        A.CallTo(() => _tenantRepositories.RemoveTechnicalUser(A<Guid>._))
            .Invokes((Guid tuId) =>
            {
                var user = technicalUsers.Single(x => x.Id == tuId);
                technicalUsers.Remove(user);
            });

        // Act
        var result = await _sut.SendDeleteCallback(technicalUserId, CancellationToken.None);

        // Assert
        result.modified.Should().BeFalse();
        result.processMessage.Should().BeNull();
        result.stepStatusId.Should().Be(ProcessStepStatusId.DONE);
        result.nextStepTypeIds.Should().BeNull();
        technicalUsers.Should().BeEmpty();
        A.CallTo(() => _callbackService.SendTechnicalUserDeletionCallback(A<Guid>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
        A.CallTo(() => _callbackService.SendTechnicalUserDeletionCallback(externalId, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    #endregion

    private WalletData GetWalletData()
    {
        var cryptoHelper = _settings.EncryptionConfigs.GetCryptoHelper(_settings.EncryptionConfigIndex);
        var (secret, initializationVector) = cryptoHelper.Encrypt("test123");

        return new WalletData("https://example.org/token", "cl1", secret, initializationVector, _settings.EncryptionConfigIndex);
    }
}
