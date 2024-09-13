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
using System.Text.Json;

namespace DimProcess.Library;

public class DimProcessHandler(
    IDimRepositories dimRepositories,
    IProvisioningClient provisioningClient,
    IDimClient dimClient,
    ICallbackService callbackService,
    IHttpClientFactory httpClientFactory,
    IOptions<DimHandlerSettings> options)
    : IDimProcessHandler
{
    private readonly DimHandlerSettings _settings = options.Value;

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> CreateWallet(Guid tenantId, string tenantName, CancellationToken cancellationToken)
    {
        var tenantRepository = dimRepositories.GetInstance<ITenantRepository>();
        var (isIssuer, didDocumentLocation) = await tenantRepository.GetHostingUrlAndIsIssuer(tenantId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (didDocumentLocation == null)
        {
            throw new UnexpectedConditionException("DidDocumentLocation must always be set");
        }

        var operationId = await provisioningClient.CreateOperation(tenantId, tenantName, _settings.ApplicationName, tenantName, didDocumentLocation, isIssuer, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        tenantRepository.AttachAndModifyTenant(tenantId, t => t.OperationId = null, t => t.OperationId = operationId);

        return new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(
            Enumerable.Repeat(ProcessStepTypeId.CHECK_OPERATION, 1),
            ProcessStepStatusId.DONE,
            false,
            null);
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> CheckOperation(Guid tenantId, CancellationToken cancellationToken)
    {
        var tenantRepository = dimRepositories.GetInstance<ITenantRepository>();
        var operationId = await tenantRepository.GetOperationId(tenantId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (operationId == null)
        {
            throw new UnexpectedConditionException("OperationId must always be set");
        }

        var response = await provisioningClient.GetOperation(operationId.Value, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        if (response.Status == OperationResponseStatus.pending)
        {
            return new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(
                null,
                ProcessStepStatusId.TODO,
                false,
                null);
        }

        if (response is { Status: OperationResponseStatus.completed, Data: null })
        {
            throw new UnexpectedConditionException($"Data should never be null when in status {OperationResponseStatus.completed}");
        }

        var serviceKey = response.Data!.ServiceKey;

        var cryptoHelper = _settings.EncryptionConfigs.GetCryptoHelper(_settings.EncryptionConfigIndex);
        var (secret, initializationVector) = cryptoHelper.Encrypt(serviceKey.Uaa.ClientSecret);

        tenantRepository.AttachAndModifyTenant(tenantId,
            t =>
            {
                t.TokenAddress = null;
                t.BaseUrl = null;
                t.ClientId = null;
                t.ClientSecret = null;
                t.WalletId = null;
            },
            t =>
            {
                t.WalletId = response.Data.CustomerWalletId;
                t.TokenAddress = serviceKey.Uaa.Url;
                t.BaseUrl = serviceKey.Url;
                t.ClientId = serviceKey.Uaa.ClientId;
                t.ClientSecret = secret;
                t.InitializationVector = initializationVector;
                t.EncryptionMode = _settings.EncryptionConfigIndex;
            });

        return new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(
            Enumerable.Repeat(ProcessStepTypeId.GET_COMPANY, 1),
            ProcessStepStatusId.DONE,
            false,
            null);
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> GetCompany(Guid tenantId, string tenantName, CancellationToken cancellationToken)
    {
        var tenantRepository = dimRepositories.GetInstance<ITenantRepository>();
        var (baseUrl, walletData) = await tenantRepository.GetCompanyRequestData(tenantId).ConfigureAwait(ConfigureAwaitOptions.None);
        var (tokenAddress, clientId, clientSecret, initializationVector, encryptionMode) = walletData.ValidateData();

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new UnexpectedConditionException("BaseAddress must not be null");
        }

        var cryptoHelper = _settings.EncryptionConfigs.GetCryptoHelper(encryptionMode);
        var secret = cryptoHelper.Decrypt(clientSecret, initializationVector);

        var dimAuth = new BasicAuthSettings
        {
            TokenAddress = $"{tokenAddress}/oauth/token",
            ClientId = clientId,
            ClientSecret = secret
        };
        var companyData = await dimClient.GetCompanyData(dimAuth, baseUrl, tenantName, _settings.ApplicationName, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);

        tenantRepository.AttachAndModifyTenant(tenantId, t =>
        {
            t.DidDownloadUrl = null;
            t.CompanyId = null;
        },
        t =>
        {
            t.DidDownloadUrl = companyData.DownloadUrl;
            t.CompanyId = companyData.CompanyId;
        });

        return new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(
            Enumerable.Repeat(ProcessStepTypeId.GET_DID_DOCUMENT, 1),
            ProcessStepStatusId.DONE,
            false,
            null);
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> GetDidDocument(Guid tenantId, CancellationToken cancellationToken)
    {
        var tenantRepository = dimRepositories.GetInstance<ITenantRepository>();
        var (downloadUrl, isIssuer) = await tenantRepository.GetDownloadUrlAndIsIssuer(tenantId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (string.IsNullOrWhiteSpace(downloadUrl))
        {
            throw new UnexpectedConditionException("DownloadUrl must not be null");
        }

        var didDocument = await GetDidDocument(downloadUrl, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        var did = didDocument.RootElement.GetProperty("id").GetString();

        tenantRepository.AttachAndModifyTenant(tenantId, t =>
            {
                t.Did = null;
            },
            t =>
            {
                t.Did = did;
            });

        return new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(
            Enumerable.Repeat(isIssuer ? ProcessStepTypeId.CREATE_STATUS_LIST : ProcessStepTypeId.SEND_CALLBACK, 1),
            ProcessStepStatusId.DONE,
            false,
            null);
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> CreateStatusList(Guid tenantId, CancellationToken cancellationToken)
    {
        var (companyId, baseUrl, walletData) = await dimRepositories.GetInstance<ITenantRepository>().GetStatusListCreationData(tenantId).ConfigureAwait(ConfigureAwaitOptions.None);
        var (tokenAddress, clientId, clientSecret, initializationVector, encryptionMode) = walletData.ValidateData();

        if (companyId == null)
        {
            throw new UnexpectedConditionException("CompanyId must not be null");
        }

        if (baseUrl == null)
        {
            throw new UnexpectedConditionException("BaseUrl must not be null");
        }

        var cryptoHelper = _settings.EncryptionConfigs.GetCryptoHelper(encryptionMode);
        var secret = cryptoHelper.Decrypt(clientSecret, initializationVector);

        var dimAuth = new BasicAuthSettings
        {
            TokenAddress = $"{tokenAddress}/oauth/token",
            ClientId = clientId,
            ClientSecret = secret
        };
        await dimClient.CreateStatusList(dimAuth, baseUrl, companyId.Value, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);

        return new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(
            Enumerable.Repeat(ProcessStepTypeId.SEND_CALLBACK, 1),
            ProcessStepStatusId.DONE,
            false,
            null);
    }

    public async Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> SendCallback(Guid tenantId, CancellationToken cancellationToken)
    {
        var (bpn, baseUrl, walletData, did, downloadUrl) = await dimRepositories.GetInstance<ITenantRepository>().GetCallbackData(tenantId).ConfigureAwait(ConfigureAwaitOptions.None);
        var (tokenAddress, clientId, clientSecret, initializationVector, encryptionMode) = walletData.ValidateData();
        if (baseUrl == null)
        {
            throw new UnexpectedConditionException("BaseUrl must always be set");
        }

        if (did == null)
        {
            throw new UnexpectedConditionException("Did must always be set");
        }

        if (downloadUrl == null)
        {
            throw new UnexpectedConditionException("DownloadUrl must always be set");
        }

        var cryptoHelper = _settings.EncryptionConfigs.GetCryptoHelper(encryptionMode);
        var secret = cryptoHelper.Decrypt(clientSecret, initializationVector);

        var didDocument = await GetDidDocument(downloadUrl, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        var uaa = new AuthenticationDetail(tokenAddress, clientId, secret);
        await callbackService.SendCallback(bpn, uaa, didDocument, did, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);

        return new ValueTuple<IEnumerable<ProcessStepTypeId>?, ProcessStepStatusId, bool, string?>(
            null,
            ProcessStepStatusId.DONE,
            false,
            null);
    }

    private async Task<JsonDocument> GetDidDocument(string downloadUrl, CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient("didDocumentDownload");
        using var result = await client.GetStreamAsync(downloadUrl, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        return await JsonDocument.ParseAsync(result, cancellationToken: cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
    }
}
