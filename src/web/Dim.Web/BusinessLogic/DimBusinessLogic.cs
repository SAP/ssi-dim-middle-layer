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
using Dim.Clients.Token;
using Dim.DbAccess;
using Dim.DbAccess.Models;
using Dim.DbAccess.Repositories;
using Dim.Entities.Enums;
using Dim.Entities.Extensions;
using Dim.Processes.Library;
using Dim.Web.ErrorHandling;
using Dim.Web.Models;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;

namespace Dim.Web.BusinessLogic;

public class DimBusinessLogic(
    IDimRepositories dimRepositories,
    ICfClient cfClient,
    IDimClient dimClient,
    IOptions<DimSettings> options)
    : IDimBusinessLogic
{
    private readonly DimSettings _settings = options.Value;

    public async Task StartSetupDim(string companyName, string bpn, string didDocumentLocation, bool isIssuer)
    {
        if (await dimRepositories.GetInstance<ITenantRepository>().IsTenantExisting(companyName, bpn).ConfigureAwait(ConfigureAwaitOptions.None))
        {
            throw new ConflictException($"Tenant {companyName} with Bpn {bpn} already exists");
        }

        var processStepRepository = dimRepositories.GetInstance<IProcessStepRepository>();
        var processId = processStepRepository.CreateProcess(ProcessTypeId.SETUP_DIM).Id;
        processStepRepository.CreateProcessStep(ProcessStepTypeId.CREATE_SUBACCOUNT, ProcessStepStatusId.TODO, processId);

        dimRepositories.GetInstance<ITenantRepository>().CreateTenant(companyName, bpn, didDocumentLocation, isIssuer, processId, _settings.OperatorId);

        await dimRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async Task<string> GetStatusList(string bpn, CancellationToken cancellationToken)
    {
        var (exists, companyId, instanceId) = await dimRepositories.GetInstance<ITenantRepository>().GetCompanyAndInstanceIdForBpn(bpn).ConfigureAwait(ConfigureAwaitOptions.None);
        if (!exists)
        {
            throw NotFoundException.Create(DimErrors.NO_COMPANY_FOR_BPN, new ErrorParameter[] { new("bpn", bpn) });
        }

        if (companyId is null)
        {
            throw ConflictException.Create(DimErrors.NO_COMPANY_ID_SET);
        }

        if (instanceId is null)
        {
            throw ConflictException.Create(DimErrors.NO_INSTANCE_ID_SET);
        }

        var dimDetails = await cfClient.GetServiceBindingDetails(instanceId.Value, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        var dimAuth = new BasicAuthSettings
        {
            TokenAddress = $"{dimDetails.Credentials.Uaa.Url}/oauth/token",
            ClientId = dimDetails.Credentials.Uaa.ClientId,
            ClientSecret = dimDetails.Credentials.Uaa.ClientSecret
        };
        var dimBaseUrl = dimDetails.Credentials.Url;
        return await dimClient.GetStatusList(dimAuth, dimBaseUrl, companyId.Value, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async Task<string> CreateStatusList(string bpn, CancellationToken cancellationToken)
    {
        var (exists, companyId, instanceId) = await dimRepositories.GetInstance<ITenantRepository>().GetCompanyAndInstanceIdForBpn(bpn).ConfigureAwait(ConfigureAwaitOptions.None);
        if (!exists)
        {
            throw NotFoundException.Create(DimErrors.NO_COMPANY_FOR_BPN, new ErrorParameter[] { new("bpn", bpn) });
        }

        if (companyId is null)
        {
            throw ConflictException.Create(DimErrors.NO_COMPANY_ID_SET);
        }

        if (instanceId is null)
        {
            throw ConflictException.Create(DimErrors.NO_INSTANCE_ID_SET);
        }

        var dimDetails = await cfClient.GetServiceBindingDetails(instanceId.Value, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        var dimAuth = new BasicAuthSettings
        {
            TokenAddress = $"{dimDetails.Credentials.Uaa.Url}/oauth/token",
            ClientId = dimDetails.Credentials.Uaa.ClientId,
            ClientSecret = dimDetails.Credentials.Uaa.ClientSecret
        };
        var dimBaseUrl = dimDetails.Credentials.Url;
        return await dimClient.CreateStatusList(dimAuth, dimBaseUrl, companyId.Value, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async Task CreateTechnicalUser(string bpn, TechnicalUserData technicalUserData)
    {
        var (exists, tenantId) = await dimRepositories.GetInstance<ITenantRepository>().GetTenantForBpn(bpn).ConfigureAwait(ConfigureAwaitOptions.None);

        if (!exists)
        {
            throw NotFoundException.Create(DimErrors.NO_COMPANY_FOR_BPN, new ErrorParameter[] { new("bpn", bpn) });
        }

        var processStepRepository = dimRepositories.GetInstance<IProcessStepRepository>();
        var processId = processStepRepository.CreateProcess(ProcessTypeId.TECHNICAL_USER).Id;
        processStepRepository.CreateProcessStep(ProcessStepTypeId.CREATE_TECHNICAL_USER, ProcessStepStatusId.TODO, processId);

        dimRepositories.GetInstance<ITenantRepository>().CreateTenantTechnicalUser(tenantId, technicalUserData.Name, technicalUserData.ExternalId, processId);

        await dimRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async Task DeleteTechnicalUser(string bpn, TechnicalUserData technicalUserData)
    {
        var (exists, technicalUserId, processId) = await dimRepositories.GetInstance<ITenantRepository>().GetTechnicalUserForBpn(bpn, technicalUserData.Name).ConfigureAwait(ConfigureAwaitOptions.None);
        if (!exists)
        {
            throw NotFoundException.Create(DimErrors.NO_TECHNICAL_USER_FOUND, new ErrorParameter[] { new("bpn", bpn) });
        }

        var processStepRepository = dimRepositories.GetInstance<IProcessStepRepository>();
        processStepRepository.CreateProcessStep(ProcessStepTypeId.DELETE_TECHNICAL_USER, ProcessStepStatusId.TODO, processId);

        dimRepositories.GetInstance<ITenantRepository>().AttachAndModifyTechnicalUser(technicalUserId, null, t =>
        {
            t.ExternalId = technicalUserData.ExternalId;
            t.ProcessId = processId;
        });

        await dimRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }

    public async Task<ProcessData> GetSetupProcess(string bpn, string companyName)
    {
        var processData = await dimRepositories.GetInstance<IProcessStepRepository>().GetWalletProcessForTenant(bpn, companyName)
            .ConfigureAwait(ConfigureAwaitOptions.None);
        if (processData == null)
        {
            throw new NotFoundException($"No process data found for BPN {bpn} and company name {companyName}");
        }

        return processData;
    }

    public async Task<ProcessData> GetTechnicalUserProcess(string technicalUserName)
    {
        var processData = await dimRepositories.GetInstance<IProcessStepRepository>().GetTechnicalUserProcess(technicalUserName)
            .ConfigureAwait(ConfigureAwaitOptions.None);
        if (processData == null)
        {
            throw new NotFoundException($"No process data found for technical user {technicalUserName}");
        }

        return processData;
    }

    public async Task RetriggerProcessStep(Guid processId, ProcessStepTypeId processStepTypeId)
    {
        var stepToTrigger = processStepTypeId.GetWalletStepForRetrigger();

        var (validProcessId, processData) = await dimRepositories.GetInstance<IProcessStepRepository>().IsValidProcess(processId, ProcessTypeId.SETUP_DIM, Enumerable.Repeat(processStepTypeId, 1)).ConfigureAwait(false);
        if (!validProcessId)
        {
            throw new NotFoundException($"process {processId} does not exist");
        }

        var context = processData.CreateManualProcessData(stepToTrigger, dimRepositories, () => $"processId {processId}");

        context.ScheduleProcessSteps(Enumerable.Repeat(stepToTrigger, 1));
        context.FinalizeProcessStep();
        await dimRepositories.SaveAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }
}
