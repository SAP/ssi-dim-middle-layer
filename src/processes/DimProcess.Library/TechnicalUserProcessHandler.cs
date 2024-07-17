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
using Dim.DbAccess;
using Dim.DbAccess.Repositories;
using Dim.Entities.Enums;
using DimProcess.Library.Callback;
using DimProcess.Library.DependencyInjection;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Encryption;

namespace DimProcess.Library;

public class TechnicalUserProcessHandler(
    IDimRepositories dimRepositories,
    ICfClient cfClient,
    ICallbackService callbackService,
    IOptions<TechnicalUserSettings> options) : ITechnicalUserProcessHandler
{
    private readonly TechnicalUserSettings _settings = options.Value;

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> CreateServiceInstanceBindings(string tenantName, Guid technicalUserId, CancellationToken cancellationToken)
    {
        var (spaceId, technicalUserName) = await dimRepositories.GetInstance<ITenantRepository>().GetSpaceIdAndTechnicalUserName(technicalUserId).ConfigureAwait(false);
        if (spaceId == null)
        {
            throw new ConflictException("SpaceId must not be null.");
        }

        await cfClient.CreateServiceInstanceBindings(tenantName, technicalUserName, spaceId.Value, cancellationToken).ConfigureAwait(false);

        return new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(
            Enumerable.Repeat(ProcessStepTypeId.GET_TECHNICAL_USER_DATA, 1),
            ProcessStepStatusId.DONE,
            false,
            null);
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> GetTechnicalUserData(string tenantName, Guid technicalUserId, CancellationToken cancellationToken)
    {
        var (spaceId, technicalUserName) = await dimRepositories.GetInstance<ITenantRepository>().GetSpaceIdAndTechnicalUserName(technicalUserId).ConfigureAwait(false);
        if (spaceId == null)
        {
            throw new ConflictException("SpaceId must not be null.");
        }

        var dimInstanceId = await cfClient.GetServiceBinding(tenantName, spaceId.Value, $"{technicalUserName}-dim-key01", cancellationToken).ConfigureAwait(false);
        var dimDetails = await cfClient.GetServiceBindingDetails(dimInstanceId, cancellationToken).ConfigureAwait(false);

        var cryptoHelper = _settings.EncryptionConfigs.GetCryptoHelper(_settings.EncryptionConfigIndex);
        var (secret, initializationVector) = cryptoHelper.Encrypt(dimDetails.Credentials.Uaa.ClientSecret);

        dimRepositories.GetInstance<ITenantRepository>().AttachAndModifyTechnicalUser(technicalUserId, technicalUser =>
            {
                technicalUser.TokenAddress = null;
                technicalUser.ClientId = null;
                technicalUser.ClientSecret = null;
                technicalUser.InitializationVector = null;
                technicalUser.EncryptionMode = null;
            },
            technicalUser =>
            {
                technicalUser.TokenAddress = $"{dimDetails.Credentials.Uaa.Url}/oauth/token";
                technicalUser.ClientId = dimDetails.Credentials.Uaa.ClientId;
                technicalUser.ClientSecret = secret;
                technicalUser.InitializationVector = initializationVector;
                technicalUser.EncryptionMode = _settings.EncryptionConfigIndex;
            });
        return new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(
            Enumerable.Repeat(ProcessStepTypeId.SEND_TECHNICAL_USER_CREATION_CALLBACK, 1),
            ProcessStepStatusId.DONE,
            false,
            null);
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> SendCreateCallback(Guid technicalUserId, CancellationToken cancellationToken)
    {
        var (externalId, tokenAddress, clientId, clientSecret, initializationVector, _) = await dimRepositories.GetInstance<ITenantRepository>().GetTechnicalUserCallbackData(technicalUserId).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(clientId))
        {
            throw new ConflictException("ClientId must not be null");
        }

        if (string.IsNullOrWhiteSpace(tokenAddress))
        {
            throw new ConflictException("TokenAddress must not be null");
        }

        if (clientSecret == null)
        {
            throw new ConflictException("Secret must not be null");
        }

        var cryptoHelper = _settings.EncryptionConfigs.GetCryptoHelper(_settings.EncryptionConfigIndex);
        var secret = cryptoHelper.Decrypt(clientSecret, initializationVector);

        await callbackService.SendTechnicalUserCallback(externalId, tokenAddress, clientId, secret, cancellationToken).ConfigureAwait(false);

        return new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(
            null,
            ProcessStepStatusId.DONE,
            false,
            null);
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> DeleteServiceInstanceBindings(string tenantName, Guid technicalUserId, CancellationToken cancellationToken)
    {
        var (spaceId, technicalUserName) = await dimRepositories.GetInstance<ITenantRepository>().GetSpaceIdAndTechnicalUserName(technicalUserId).ConfigureAwait(false);
        if (spaceId == null)
        {
            throw new ConflictException("SpaceId must not be null.");
        }

        var serviceBindingId = await cfClient.GetServiceBinding(tenantName, spaceId.Value, $"{technicalUserName}-dim-key01", cancellationToken).ConfigureAwait(false);
        await cfClient.DeleteServiceInstanceBindings(serviceBindingId, cancellationToken).ConfigureAwait(false);

        return new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(
            Enumerable.Repeat(ProcessStepTypeId.SEND_TECHNICAL_USER_DELETION_CALLBACK, 1),
            ProcessStepStatusId.DONE,
            false,
            null);
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> SendDeleteCallback(Guid technicalUserId, CancellationToken cancellationToken)
    {
        var tenantRepository = dimRepositories.GetInstance<ITenantRepository>();

        var externalId = await tenantRepository.GetExternalIdForTechnicalUser(technicalUserId).ConfigureAwait(false);
        tenantRepository.RemoveTechnicalUser(technicalUserId);
        await callbackService.SendTechnicalUserDeletionCallback(externalId, cancellationToken).ConfigureAwait(false);

        return new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(
            null,
            ProcessStepStatusId.DONE,
            false,
            null);
    }
}
