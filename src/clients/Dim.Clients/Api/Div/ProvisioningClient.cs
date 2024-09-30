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

using Dim.Clients.Api.Div.DependencyInjection;
using Dim.Clients.Api.Div.Models;
using Dim.Clients.Extensions;
using Dim.Clients.Token;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.HttpClientExtensions;
using System.Net.Http.Json;
using System.Text.Json;

namespace Dim.Clients.Api.Div;

public class ProvisioningClient(IBasicAuthTokenService basicAuthTokenService, IOptions<ProvisioningSettings> options)
    : IProvisioningClient
{
    private readonly ProvisioningSettings _settings = options.Value;

    public async Task<Guid> CreateOperation(Guid customerId, string customerName, string applicationName, string companyName, string didDocumentLocation, bool isIssuer, CancellationToken cancellationToken)
    {
        var data = new OperationCreationRequest(
            "provision",
            "customer-wallet",
            new OperationPayloadData(
                customerId.ToString(),
                customerName,
                "main",
                new WalletServiceParameter(
                    Enumerable.Repeat(new WalletApplication(
                        applicationName,
                        Enumerable.Repeat(new ApplicationCompany(
                            companyName,
                            didDocumentLocation,
                            [new ApplicationCompanyService("CredentialService", "https://dis-agent-prod.eu10.dim.cloud.sap/api/v1.0.0/iatp")],
                            isIssuer ?
                                [
                                    new("SIGNING"),
                                    new("SIGNING_VC")
                                ] :
                                new ApplicationCompanyKey[]
                                {
                                    new("SIGNING")
                                }
                        ), 1),
                        Enumerable.Empty<TrustedIssuer>()
                    ), 1)
                )
            )
        );
        var client = await basicAuthTokenService
            .GetBasicAuthorizedClient<ProvisioningClient>(_settings, cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);
        var result = await client.PostAsJsonAsync("/api/v1.0.0/operations", data, JsonSerializerExtensions.Options, cancellationToken)
            .CatchingIntoServiceExceptionFor("create-operation", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE,
                async response =>
                {
                    var content = await response.Content.ReadAsStringAsync().ConfigureAwait(ConfigureAwaitOptions.None);
                    return (false, content);
                })
            .ConfigureAwait(false);
        try
        {
            var response = await result.Content
                .ReadFromJsonAsync<OperationRequest>(JsonSerializerExtensions.Options, cancellationToken)
                .ConfigureAwait(ConfigureAwaitOptions.None);

            if (response == null)
            {
                throw new ServiceException("response should never be null here");
            }

            return response.OperationId;
        }
        catch (JsonException je)
        {
            throw new ServiceException(je.Message);
        }
    }

    public async Task<OperationResponse> GetOperation(Guid operationId, CancellationToken cancellationToken)
    {
        var client = await basicAuthTokenService
            .GetBasicAuthorizedClient<ProvisioningClient>(_settings, cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);
        var result = await client.GetAsync($"/api/v1.0.0/operations/{operationId}", cancellationToken)
            .CatchingIntoServiceExceptionFor("get-operation", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE,
                async response =>
                {
                    var content = await response.Content.ReadAsStringAsync().ConfigureAwait(ConfigureAwaitOptions.None);
                    return (false, content);
                }).ConfigureAwait(false);
        try
        {
            var response = await result.Content
                .ReadFromJsonAsync<OperationResponse>(JsonSerializerExtensions.Options, cancellationToken)
                .ConfigureAwait(ConfigureAwaitOptions.None);
            if (response == null)
            {
                throw new ServiceException("Response must not be null");
            }

            if (response.Status == OperationResponseStatus.failed)
            {
                throw new ServiceException($"Operation Creation failed with error: {response.Error}");
            }

            return response;
        }
        catch (JsonException je)
        {
            throw new ServiceException(je.Message);
        }
    }

    public async Task<Guid> CreateServiceKey(string technicalUserName, Guid walletId, CancellationToken cancellationToken)
    {
        var data = new ServiceKeyOperationCreationRequest(
            "customer-wallet-key",
            "create",
            new ServiceKeyCreationPayloadData(
                walletId,
                technicalUserName,
                new ServiceKeyWalletServiceParameter(["IatpOperations", "ReadCompanyIdentity", "ResolveDID"])
            )
        );
        var client = await basicAuthTokenService
            .GetBasicAuthorizedClient<ProvisioningClient>(_settings, cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);
        var result = await client.PostAsJsonAsync("/api/v1.0.0/operations", data, JsonSerializerExtensions.Options, cancellationToken)
            .CatchingIntoServiceExceptionFor("create-service-key", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE,
                async response =>
                {
                    var content = await response.Content.ReadAsStringAsync().ConfigureAwait(ConfigureAwaitOptions.None);
                    return (false, content);
                })
            .ConfigureAwait(false);
        try
        {
            var response = await result.Content
                .ReadFromJsonAsync<OperationRequest>(JsonSerializerExtensions.Options, cancellationToken)
                .ConfigureAwait(ConfigureAwaitOptions.None);

            if (response == null)
            {
                throw new ServiceException("response should never be null here");
            }

            return response.OperationId;
        }
        catch (JsonException je)
        {
            throw new ServiceException(je.Message);
        }
    }

    public async Task<Guid> GetServiceKey(string technicalUserName, Guid walletId, CancellationToken cancellationToken)
    {
        var client = await basicAuthTokenService
            .GetBasicAuthorizedClient<ProvisioningClient>(_settings, cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);
        var result = await client.GetAsync($"/api/v1.0.0/customerWallets?CustomerWalletId={walletId}", cancellationToken)
            .CatchingIntoServiceExceptionFor("get-service-key", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE,
                async response =>
                {
                    var content = await response.Content.ReadAsStringAsync().ConfigureAwait(ConfigureAwaitOptions.None);
                    return (false, content);
                }).ConfigureAwait(false);
        try
        {
            var response = await result.Content
                .ReadFromJsonAsync<CustomerWalletsResponse>(JsonSerializerExtensions.Options, cancellationToken)
                .ConfigureAwait(ConfigureAwaitOptions.None);
            if (response == null)
            {
                throw new ServiceException("Response must not be null");
            }

            var customers = response.Data.Where(x => x.Id == walletId);
            if (customers.Count() != 1)
            {
                throw new ServiceException($"Must have exactly one customer for wallet id {walletId}");
            }

            var serviceKey = customers.Single().ServiceKeys.Where(sk => sk.Name.Equals(technicalUserName, StringComparison.OrdinalIgnoreCase));
            if (serviceKey.Count() != 1)
            {
                throw new ServiceException($"Must have exactly one wallet and a service key with name {technicalUserName}");
            }

            return serviceKey.Single().Id;
        }
        catch (JsonException je)
        {
            throw new ServiceException(je.Message);
        }
    }

    public async Task<Guid?> DeleteServiceKey(Guid walletId, Guid serviceKeyId, CancellationToken cancellationToken)
    {
        var data = new ServiceKeyOperationDeletionRequest(
            "customer-wallet-key",
            "delete",
            new ServiceKeyDeletionPayloadData(
                serviceKeyId,
                walletId
            )
        );
        var client = await basicAuthTokenService
            .GetBasicAuthorizedClient<ProvisioningClient>(_settings, cancellationToken)
            .ConfigureAwait(ConfigureAwaitOptions.None);
        var result = await client.PostAsJsonAsync("/api/v1.0.0/operations", data, JsonSerializerExtensions.Options, cancellationToken)
            .CatchingIntoServiceExceptionFor("delete-service-key", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE,
                async response =>
                {
                    var content = await response.Content.ReadAsStringAsync().ConfigureAwait(ConfigureAwaitOptions.None);
                    return (false, content);
                })
            .ConfigureAwait(false);
        try
        {
            var response = await result.Content
                .ReadFromJsonAsync<OperationRequest>(JsonSerializerExtensions.Options, cancellationToken)
                .ConfigureAwait(ConfigureAwaitOptions.None);

            if (response == null)
            {
                throw new ServiceException("response should never be null here");
            }

            return response.OperationId;
        }
        catch (JsonException je)
        {
            throw new ServiceException(je.Message);
        }
    }
}
