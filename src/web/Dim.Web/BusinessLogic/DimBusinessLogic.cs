/********************************************************************************
 * Copyright (c) 2024 BMW Group AG
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
using Dim.DbAccess.Repositories;
using Dim.Entities.Enums;
using Dim.Web.ErrorHandling;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;

namespace Dim.Web.BusinessLogic;

public class DimBusinessLogic : IDimBusinessLogic
{
    private readonly IDimRepositories _dimRepositories;
    private readonly ICfClient _cfClient;
    private readonly IDimClient _dimClient;
    private readonly DimSettings _settings;

    public DimBusinessLogic(IDimRepositories dimRepositories, ICfClient cfClient, IDimClient dimClient, IOptions<DimSettings> options)
    {
        _dimRepositories = dimRepositories;
        _cfClient = cfClient;
        _dimClient = dimClient;
        _settings = options.Value;
    }

    public async Task StartSetupDim(string companyName, string bpn, string didDocumentLocation, bool isIssuer)
    {
        var processStepRepository = _dimRepositories.GetInstance<IProcessStepRepository>();
        var processId = processStepRepository.CreateProcess(ProcessTypeId.SETUP_DIM).Id;
        processStepRepository.CreateProcessStep(ProcessStepTypeId.CREATE_SUBACCOUNT, ProcessStepStatusId.TODO, processId);

        _dimRepositories.GetInstance<ITenantRepository>().CreateTenant(companyName, bpn, didDocumentLocation, isIssuer, processId, _settings.OperatorId);

        await _dimRepositories.SaveAsync().ConfigureAwait(false);
    }

    public async Task<string> GetStatusList(string bpn, CancellationToken cancellationToken)
    {
        var (exists, companyId, instanceId) = await _dimRepositories.GetInstance<ITenantRepository>().GetCompanyAndInstanceIdForBpn(bpn).ConfigureAwait(false);
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

        var dimDetails = await _cfClient.GetServiceBindingDetails(instanceId.Value, cancellationToken).ConfigureAwait(false);
        var dimAuth = new BasicAuthSettings
        {
            TokenAddress = $"{dimDetails.Credentials.Uaa.Url}/oauth/token",
            ClientId = dimDetails.Credentials.Uaa.ClientId,
            ClientSecret = dimDetails.Credentials.Uaa.ClientSecret
        };
        var dimBaseUrl = dimDetails.Credentials.Url;
        return await _dimClient.GetStatusList(dimAuth, dimBaseUrl, companyId.Value, cancellationToken).ConfigureAwait(false);
    }

    public async Task<string> CreateStatusList(string bpn, CancellationToken cancellationToken)
    {
        var (exists, companyId, instanceId) = await _dimRepositories.GetInstance<ITenantRepository>().GetCompanyAndInstanceIdForBpn(bpn).ConfigureAwait(false);
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

        var dimDetails = await _cfClient.GetServiceBindingDetails(instanceId.Value, cancellationToken).ConfigureAwait(false);
        var dimAuth = new BasicAuthSettings
        {
            TokenAddress = $"{dimDetails.Credentials.Uaa.Url}/oauth/token",
            ClientId = dimDetails.Credentials.Uaa.ClientId,
            ClientSecret = dimDetails.Credentials.Uaa.ClientSecret
        };
        var dimBaseUrl = dimDetails.Credentials.Url;
        return await _dimClient.CreateStatusList(dimAuth, dimBaseUrl, companyId.Value, cancellationToken).ConfigureAwait(false);
    }
}
