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
using Dim.Clients.Token;
using Dim.DbAccess;
using Dim.DbAccess.Extensions;
using Dim.DbAccess.Repositories;
using Dim.Entities.Enums;
using DimProcess.Library.Callback;
using DimProcess.Library.DependencyInjection;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;

namespace DimProcess.Library;

public class TechnicalUserProcessHandler(
    IDimRepositories dimRepositories,
    IProvisioningClient provisioningClient,
    ICallbackService callbackService,
    IOptions<TechnicalUserSettings> options) : ITechnicalUserProcessHandler
{
    private readonly TechnicalUserSettings _settings = options.Value;

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> CreateServiceInstanceBindings(Guid technicalUserId, CancellationToken cancellationToken)
    {
        var tenantRepository = dimRepositories.GetInstance<ITechnicalUserRepository>();
        var (walletId, technicalUserName) = await tenantRepository.GetTechnicalUserNameAndWalletId(technicalUserId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (walletId == null)
        {
            throw new ConflictException("WalletId must not be null");
        }

        var operationId = await provisioningClient.CreateServiceKey(technicalUserName, walletId.Value, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);

        tenantRepository.AttachAndModifyTechnicalUser(technicalUserId, t => t.OperationId = null, t => t.OperationId = operationId);

        return new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(
            Enumerable.Repeat(ProcessStepTypeId.GET_TECHNICAL_USER_DATA, 1),
            ProcessStepStatusId.DONE,
            false,
            null);
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> GetTechnicalUserData(Guid technicalUserId, CancellationToken cancellationToken)
    {
        var tenantRepository = dimRepositories.GetInstance<ITechnicalUserRepository>();
        var operationId = await tenantRepository.GetOperationIdForTechnicalUser(technicalUserId)
            .ConfigureAwait(ConfigureAwaitOptions.None);
        if (operationId is null)
        {
            throw new ConflictException("OperationId must not be null");
        }

        var response = await provisioningClient.GetOperation(operationId.Value, cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);
        if (response.Status == OperationResponseStatus.pending)
        {
            return new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(
                null,
                ProcessStepStatusId.TODO,
                true,
                null);
        }

        if (response is { Status: OperationResponseStatus.completed, Data: null })
        {
            throw new UnexpectedConditionException($"Data should never be null when in status {OperationResponseStatus.completed}");
        }

        var serviceKey = response.Data!.ServiceKey;
        var cryptoHelper = _settings.EncryptionConfigs.GetCryptoHelper(_settings.EncryptionConfigIndex);
        var (secret, initializationVector) = cryptoHelper.Encrypt(serviceKey.Uaa.ClientSecret);

        tenantRepository.AttachAndModifyTechnicalUser(technicalUserId,
            t =>
            {
                t.TokenAddress = null;
                t.ClientId = null;
                t.ClientSecret = null;
            },
            t =>
            {
                t.TokenAddress = serviceKey.Uaa.Url;
                t.ClientId = serviceKey.Uaa.ClientId;
                t.ClientSecret = secret;
                t.InitializationVector = initializationVector;
                t.EncryptionMode = _settings.EncryptionConfigIndex;
            });

        return new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(
            Enumerable.Repeat(ProcessStepTypeId.GET_TECHNICAL_USER_SERVICE_KEY, 1),
            ProcessStepStatusId.DONE,
            false,
            null);
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> GetTechnicalUserServiceKey(Guid technicalUserId, CancellationToken cancellationToken)
    {
        var (walletId, technicalUserName) = await dimRepositories.GetInstance<ITechnicalUserRepository>().GetWalletIdAndNameForTechnicalUser(technicalUserId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (walletId == null)
        {
            throw new ConflictException("WalletId must be set");
        }

        var serviceKeyId = await provisioningClient.GetServiceKey(technicalUserName, walletId.Value, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        dimRepositories.GetInstance<ITechnicalUserRepository>().AttachAndModifyTechnicalUser(technicalUserId,
            t =>
            {
                t.ServiceKeyId = null;
            },
            t =>
            {
                t.ServiceKeyId = serviceKeyId;
            });

        return new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(
            Enumerable.Repeat(ProcessStepTypeId.SEND_TECHNICAL_USER_CREATION_CALLBACK, 1),
            ProcessStepStatusId.DONE,
            false,
            null);
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> SendCreateCallback(Guid technicalUserId, CancellationToken cancellationToken)
    {
        var (externalId, walletData) = await dimRepositories.GetInstance<ITechnicalUserRepository>().GetTechnicalUserCallbackData(technicalUserId).ConfigureAwait(ConfigureAwaitOptions.None);
        var (tokenAddress, clientId, clientSecret, initializationVector, encryptionMode) = walletData.ValidateData();

        var cryptoHelper = _settings.EncryptionConfigs.GetCryptoHelper(encryptionMode);
        var secret = cryptoHelper.Decrypt(clientSecret, initializationVector);

        await callbackService.SendTechnicalUserCallback(externalId, tokenAddress, clientId, secret, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);

        return new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(
            null,
            ProcessStepStatusId.DONE,
            false,
            null);
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> DeleteServiceInstanceBindings(Guid technicalUserId, CancellationToken cancellationToken)
    {
        var technicalUserRepository = dimRepositories.GetInstance<ITechnicalUserRepository>();
        var (serviceKeyId, walletId) = await technicalUserRepository.GetServiceKeyAndWalletId(technicalUserId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (walletId == null)
        {
            throw new ConflictException("WalletId must not be null");
        }

        if (serviceKeyId == null)
        {
            throw new ConflictException("ServiceKeyId must not be null");
        }

        var operationId = await provisioningClient.DeleteServiceKey(walletId.Value, serviceKeyId.Value, cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);

        technicalUserRepository.AttachAndModifyTechnicalUser(technicalUserId, t => t.OperationId = null, t => t.OperationId = operationId);

        return new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(
            Enumerable.Repeat(ProcessStepTypeId.SEND_TECHNICAL_USER_DELETION_CALLBACK, 1),
            ProcessStepStatusId.DONE,
            false,
            null);
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> SendDeleteCallback(Guid technicalUserId, CancellationToken cancellationToken)
    {
        var tenantRepository = dimRepositories.GetInstance<ITechnicalUserRepository>();

        var (operationId, externalId) = await tenantRepository.GetOperationAndExternalIdForTechnicalUser(technicalUserId)
            .ConfigureAwait(ConfigureAwaitOptions.None);
        if (operationId is null)
        {
            throw new ConflictException("OperationId must not be null");
        }

        var response = await provisioningClient.GetOperation(operationId.Value, cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);
        if (response.Status == OperationResponseStatus.pending)
        {
            return new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(
                null,
                ProcessStepStatusId.TODO,
                true,
                null);
        }

        tenantRepository.RemoveTechnicalUser(technicalUserId);
        await callbackService.SendTechnicalUserDeletionCallback(externalId, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);

        return new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(
            null,
            ProcessStepStatusId.DONE,
            false,
            null);
    }
}
