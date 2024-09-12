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
using Dim.DbAccess.Extensions;
using Dim.DbAccess.Repositories;
using Dim.Entities.Enums;
using DimProcess.Library.Callback;
using DimProcess.Library.DependencyInjection;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;

namespace DimProcess.Library;

public class TechnicalUserProcessHandler(
    IDimRepositories dimRepositories,
    ICallbackService callbackService,
    IOptions<TechnicalUserSettings> options) : ITechnicalUserProcessHandler
{
    private readonly TechnicalUserSettings _settings = options.Value;

    public Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> CreateServiceInstanceBindings(string tenantName, Guid technicalUserId, CancellationToken cancellationToken)
    {
        return Task.FromResult(new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(
            null,
            ProcessStepStatusId.FAILED,
            false,
            "Technical User Creation is currently not supported"));
    }

    public Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> GetTechnicalUserData(string tenantName, Guid technicalUserId, CancellationToken cancellationToken)
    {
        return Task.FromResult(new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(
            null,
            ProcessStepStatusId.FAILED,
            false,
            "Technical User Creation is currently not supported"));
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> SendCreateCallback(Guid technicalUserId, CancellationToken cancellationToken)
    {
        var (externalId, walletData) = await dimRepositories.GetInstance<ITenantRepository>().GetTechnicalUserCallbackData(technicalUserId).ConfigureAwait(false);
        var (tokenAddress, clientId, clientSecret, initializationVector, encryptionMode) = walletData.ValidateData();

        var cryptoHelper = _settings.EncryptionConfigs.GetCryptoHelper(encryptionMode);
        var secret = cryptoHelper.Decrypt(clientSecret, initializationVector);

        await callbackService.SendTechnicalUserCallback(externalId, tokenAddress, clientId, secret, cancellationToken).ConfigureAwait(false);

        return new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(
            null,
            ProcessStepStatusId.DONE,
            false,
            null);
    }

    public Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> DeleteServiceInstanceBindings(string tenantName, Guid technicalUserId, CancellationToken cancellationToken)
    {
        return Task.FromResult(new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(
            null,
            ProcessStepStatusId.FAILED,
            false,
            "Technical User deletion is currently not supported"));
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
