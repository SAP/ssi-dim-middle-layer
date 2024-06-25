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
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;

namespace DimProcess.Library;

public class TechnicalUserDeletionProcessHandler(
    IDimRepositories dimRepositories,
    ICfClient cfClient,
    ICallbackService callbackService) : ITechnicalUserDeletionProcessHandler
{
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

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> SendCallback(Guid technicalUserId, CancellationToken cancellationToken)
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
