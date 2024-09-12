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

using Dim.Clients.Api.Dim;
using Dim.Clients.Token;
using Dim.DbAccess;
using Dim.DbAccess.Extensions;
using Dim.DbAccess.Repositories;
using Dim.Entities.Enums;
using Dim.Web.ErrorHandling;
using Dim.Web.Models;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Models.Configuration;
using System.Text.RegularExpressions;

namespace Dim.Web.BusinessLogic;

public class DimBusinessLogic(
    IDimRepositories dimRepositories,
    IDimClient dimClient,
    IOptions<DimSettings> options)
    : IDimBusinessLogic
{
    private static readonly Regex TenantName = new(@"(?<=[^\w-])|(?<=[^-])[\W_]+|(?<=[^-])$", RegexOptions.Compiled, new TimeSpan(0, 0, 0, 1));
    private readonly DimSettings _settings = options.Value;

    public async Task StartSetupDim(string companyName, string bpn, string didDocumentLocation, bool isIssuer)
    {
        var tenant = TenantName.Replace(companyName, string.Empty).TrimStart('-').TrimEnd('-').ToLower();
        if (await dimRepositories.GetInstance<ITenantRepository>().IsTenantExisting(companyName, bpn).ConfigureAwait(ConfigureAwaitOptions.None))
        {
            throw ConflictException.Create(DimErrors.TENANT_ALREADY_EXISTS, new ErrorParameter[] { new("companyName", companyName), new("bpn", bpn) });
        }

        var processStepRepository = dimRepositories.GetInstance<IProcessStepRepository>();
        var processId = processStepRepository.CreateProcess(ProcessTypeId.SETUP_DIM).Id;
        processStepRepository.CreateProcessStep(ProcessStepTypeId.CREATE_WALLET, ProcessStepStatusId.TODO, processId);

        dimRepositories.GetInstance<ITenantRepository>().CreateTenant(tenant, bpn, didDocumentLocation, isIssuer, processId, _settings.OperatorId);

        await dimRepositories.SaveAsync().ConfigureAwait(false);
    }

    public async Task<string> GetStatusList(string bpn, CancellationToken cancellationToken)
    {
        var (exists, companyId, baseUrl, walletData) = await dimRepositories.GetInstance<ITenantRepository>().GetCompanyAndWalletDataForBpn(bpn).ConfigureAwait(false);
        var (tokenAddress, clientId, clientSecret, initializationVector, encryptionMode) = walletData.ValidateData();
        if (!exists)
        {
            throw NotFoundException.Create(DimErrors.NO_COMPANY_FOR_BPN, new ErrorParameter[] { new("bpn", bpn) });
        }

        if (companyId is null)
        {
            throw ConflictException.Create(DimErrors.NO_COMPANY_ID_SET);
        }

        if (baseUrl is null)
        {
            throw ConflictException.Create(DimErrors.NO_BASE_URL_SET);
        }

        var cryptoHelper = _settings.EncryptionConfigs.GetCryptoHelper(encryptionMode);
        var secret = cryptoHelper.Decrypt(clientSecret, initializationVector);

        var dimAuth = new BasicAuthSettings
        {
            TokenAddress = $"{tokenAddress}/oauth/token",
            ClientId = clientId,
            ClientSecret = secret
        };
        return await dimClient.GetStatusList(dimAuth, baseUrl, companyId.Value, cancellationToken).ConfigureAwait(false);
    }

    public async Task<string> CreateStatusList(string bpn, CancellationToken cancellationToken)
    {
        var (exists, companyId, baseUrl, walletData) = await dimRepositories.GetInstance<ITenantRepository>().GetCompanyAndWalletDataForBpn(bpn).ConfigureAwait(false);
        var (tokenAddress, clientId, clientSecret, initializationVector, encryptionMode) = walletData.ValidateData();
        if (!exists)
        {
            throw NotFoundException.Create(DimErrors.NO_COMPANY_FOR_BPN, new ErrorParameter[] { new("bpn", bpn) });
        }

        if (companyId is null)
        {
            throw ConflictException.Create(DimErrors.NO_COMPANY_ID_SET);
        }

        if (baseUrl is null)
        {
            throw ConflictException.Create(DimErrors.NO_BASE_URL_SET);
        }

        var cryptoHelper = _settings.EncryptionConfigs.GetCryptoHelper(encryptionMode);
        var secret = cryptoHelper.Decrypt(clientSecret, initializationVector);

        var dimAuth = new BasicAuthSettings
        {
            TokenAddress = $"{tokenAddress}/oauth/token",
            ClientId = clientId,
            ClientSecret = secret
        };
        return await dimClient.CreateStatusList(dimAuth, baseUrl, companyId.Value, cancellationToken).ConfigureAwait(false);
    }

    public async Task CreateTechnicalUser(string bpn, TechnicalUserData technicalUserData)
    {
        var (exists, tenantId) = await dimRepositories.GetInstance<ITenantRepository>().GetTenantForBpn(bpn).ConfigureAwait(false);

        if (!exists)
        {
            throw NotFoundException.Create(DimErrors.NO_COMPANY_FOR_BPN, new ErrorParameter[] { new("bpn", bpn) });
        }

        var processStepRepository = dimRepositories.GetInstance<IProcessStepRepository>();
        var processId = processStepRepository.CreateProcess(ProcessTypeId.TECHNICAL_USER).Id;
        processStepRepository.CreateProcessStep(ProcessStepTypeId.CREATE_TECHNICAL_USER, ProcessStepStatusId.TODO, processId);

        dimRepositories.GetInstance<ITenantRepository>().CreateTenantTechnicalUser(tenantId, technicalUserData.Name, technicalUserData.ExternalId, processId);

        await dimRepositories.SaveAsync().ConfigureAwait(false);
    }

    public async Task DeleteTechnicalUser(string bpn, TechnicalUserData technicalUserData)
    {
        var (exists, technicalUserId, processId) = await dimRepositories.GetInstance<ITenantRepository>().GetTechnicalUserForBpn(bpn, technicalUserData.Name).ConfigureAwait(false);
        if (!exists)
        {
            throw NotFoundException.Create(DimErrors.NO_TECHNICAL_USER_FOUND, new ErrorParameter[] { new("bpn", bpn) });
        }

        var processStepRepository = dimRepositories.GetInstance<IProcessStepRepository>();
        processStepRepository.CreateProcessStep(ProcessStepTypeId.DELETE_TECHNICAL_USER, ProcessStepStatusId.TODO, processId);

        dimRepositories.GetInstance<ITenantRepository>().AttachAndModifyTechnicalUser(technicalUserId,
            t =>
            {
                t.ExternalId = Guid.Empty;
                t.ProcessId = processId;
            },
            t =>
            {
                t.ExternalId = technicalUserData.ExternalId;
                t.ProcessId = processId;
            });

        await dimRepositories.SaveAsync().ConfigureAwait(false);
    }
}
