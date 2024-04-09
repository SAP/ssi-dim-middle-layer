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

using Dim.Clients.Api.SubAccounts;
using Dim.Clients.Extensions;
using Dim.Clients.Token;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.HttpClientExtensions;
using System.Net.Http.Json;
using System.Text.Json;

namespace Dim.Clients.Api.Services;

public class ServiceClient : IServiceClient
{
    private readonly IBasicAuthTokenService _basicAuthTokenService;

    public ServiceClient(IBasicAuthTokenService basicAuthTokenService)
    {
        _basicAuthTokenService = basicAuthTokenService;
    }

    public async Task<CreateServiceInstanceResponse> CreateServiceInstance(ServiceManagementBindingItem saBinding, CancellationToken cancellationToken)
    {
        var serviceAuth = new BasicAuthSettings
        {
            TokenAddress = $"{saBinding.Url}/oauth/token",
            ClientId = saBinding.ClientId,
            ClientSecret = saBinding.ClientSecret
        };
        var client = await _basicAuthTokenService.GetBasicAuthorizedClient<ServiceClient>(serviceAuth, cancellationToken).ConfigureAwait(false);
        var directory = new CreateServiceInstanceRequest(
            "cis-local-instance",
            "cis",
            "local",
            new Dictionary<string, string>
            {
                { "grantType", "clientCredentials" }
            }
        );

        var result = await client.PostAsJsonAsync($"{saBinding.SmUrl}/v1/service_instances?async=false", directory, JsonSerializerExtensions.Options, cancellationToken)
            .CatchingIntoServiceExceptionFor("create-service-instance", HttpAsyncResponseMessageExtension.RecoverOptions.ALLWAYS).ConfigureAwait(false);
        try
        {
            var response = await result.Content
                .ReadFromJsonAsync<CreateServiceInstanceResponse>(JsonSerializerExtensions.Options, cancellationToken)
                .ConfigureAwait(false);
            if (response == null)
            {
                throw new ServiceException("Response was empty", true);
            }

            return response;
        }
        catch (JsonException je)
        {
            throw new ServiceException(je.Message);
        }
    }

    public async Task<CreateServiceBindingResponse> CreateServiceBinding(ServiceManagementBindingItem saBinding, string serviceInstanceId, CancellationToken cancellationToken)
    {
        var serviceAuth = new BasicAuthSettings
        {
            TokenAddress = $"{saBinding.Url}/oauth/token",
            ClientId = saBinding.ClientId,
            ClientSecret = saBinding.ClientSecret
        };
        var client = await _basicAuthTokenService.GetBasicAuthorizedClient<ServiceClient>(serviceAuth, cancellationToken).ConfigureAwait(false);
        var data = new CreateServiceBindingRequest(
            "cis-local-binding",
            serviceInstanceId
        );

        var result = await client.PostAsJsonAsync($"{saBinding.SmUrl}/v1/service_bindings?async=false", data, JsonSerializerExtensions.Options, cancellationToken)
            .CatchingIntoServiceExceptionFor("create-service-binding", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE).ConfigureAwait(false);
        try
        {
            var response = await result.Content
                .ReadFromJsonAsync<CreateServiceBindingResponse>(JsonSerializerExtensions.Options, cancellationToken)
                .ConfigureAwait(false);
            if (response == null)
            {
                throw new ServiceException("Response was empty", true);
            }

            return response;
        }
        catch (JsonException je)
        {
            throw new ServiceException(je.Message);
        }
    }

    public async Task<BindingItem> GetServiceBinding(ServiceManagementBindingItem saBinding, string serviceBindingName, CancellationToken cancellationToken)
    {
        var serviceAuth = new BasicAuthSettings
        {
            TokenAddress = $"{saBinding.Url}/oauth/token",
            ClientId = saBinding.ClientId,
            ClientSecret = saBinding.ClientSecret
        };
        var client = await _basicAuthTokenService.GetBasicAuthorizedClient<ServiceClient>(serviceAuth, cancellationToken).ConfigureAwait(false);
        var result = await client.GetAsync($"{saBinding.SmUrl}/v1/service_bindings?fieldQuery=name eq '{serviceBindingName}'", cancellationToken)
            .CatchingIntoServiceExceptionFor("get-service-binding", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE).ConfigureAwait(false);
        try
        {
            var response = await result.Content
                .ReadFromJsonAsync<GetBindingResponse>(JsonSerializerExtensions.Options, cancellationToken)
                .ConfigureAwait(false);
            if (response == null)
            {
                throw new ServiceException("Response was empty", true);
            }

            if (response.Items.Count() != 1)
            {
                throw new ServiceException($"There must be exactly one binding for {serviceBindingName}");
            }

            return response.Items.Single();
        }
        catch (JsonException je)
        {
            throw new ServiceException(je.Message);
        }
    }
}
