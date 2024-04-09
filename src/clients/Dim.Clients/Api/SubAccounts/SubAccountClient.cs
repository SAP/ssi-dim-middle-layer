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

using Dim.Clients.Extensions;
using Dim.Clients.Token;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.HttpClientExtensions;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Dim.Clients.Api.SubAccounts;

public class SubAccountClient : ISubAccountClient
{
    private static readonly Regex TenantName = new(@"(?<=[^\w-])|(?<=[^-])[\W_]+|(?<=[^-])$", RegexOptions.Compiled, new TimeSpan(0, 0, 0, 1));
    private readonly IBasicAuthTokenService _basicAuthTokenService;

    public SubAccountClient(IBasicAuthTokenService basicAuthTokenService)
    {
        _basicAuthTokenService = basicAuthTokenService;
    }

    public async Task<Guid> CreateSubaccount(BasicAuthSettings basicAuthSettings, string adminMail, string tenantName, Guid directoryId, CancellationToken cancellationToken)
    {
        var subdomain = TenantName.Replace(tenantName, "-").ToLower().TrimStart('-').TrimEnd('-');
        var client = await _basicAuthTokenService.GetBasicAuthorizedClient<SubAccountClient>(basicAuthSettings, cancellationToken).ConfigureAwait(false);
        var directory = new CreateSubAccountRequest(
            false,
            $"CX customer sub-account {tenantName}",
            tenantName,
            new Dictionary<string, IEnumerable<string>>
            {
                { "cloud_management_service", new[] { "Created by API - Don't change it" } },
                { "tenantName", new[] { tenantName } }
            },
            "API",
            directoryId,
            "eu10",
            Enumerable.Repeat(adminMail, 1),
            subdomain,
            UsedForProduction.USED_FOR_PRODUCTION
        );

        var result = await client.PostAsJsonAsync("/accounts/v1/subaccounts", directory, JsonSerializerExtensions.Options, cancellationToken)
            .CatchingIntoServiceExceptionFor("create-subaccount", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE, async message =>
            {
                var errorMessage = await message.Content.ReadAsStringAsync().ConfigureAwait(false);
                return new(false, errorMessage);
            }).ConfigureAwait(false);
        try
        {
            var response = await result.Content
                .ReadFromJsonAsync<CreateSubaccountResponse>(JsonSerializerExtensions.Options, cancellationToken)
                .ConfigureAwait(false);

            if (response == null)
            {
                throw new ServiceException("Response must not be null");
            }

            return response.Id;
        }
        catch (JsonException je)
        {
            throw new ServiceException(je.Message);
        }
    }

    public async Task CreateServiceManagerBindings(BasicAuthSettings basicAuthSettings, Guid subAccountId, CancellationToken cancellationToken)
    {
        var client = await _basicAuthTokenService.GetBasicAuthorizedClient<SubAccountClient>(basicAuthSettings, cancellationToken).ConfigureAwait(false);
        var data = new
        {
            name = "accessServiceManager"
        };

        await client.PostAsJsonAsync($"/accounts/v2/subaccounts/{subAccountId}/serviceManagerBindings", data, JsonSerializerExtensions.Options, cancellationToken)
            .CatchingIntoServiceExceptionFor("create-subaccount", HttpAsyncResponseMessageExtension.RecoverOptions.ALLWAYS).ConfigureAwait(false);
    }

    public async Task<ServiceManagementBindingItem> GetServiceManagerBindings(BasicAuthSettings basicAuthSettings, Guid subAccountId, CancellationToken cancellationToken)
    {
        var client = await _basicAuthTokenService.GetBasicAuthorizedClient<SubAccountClient>(basicAuthSettings, cancellationToken).ConfigureAwait(false);

        var result = await client.GetAsync($"/accounts/v2/subaccounts/{subAccountId}/serviceManagerBindings", cancellationToken)
            .CatchingIntoServiceExceptionFor("create-subaccount", HttpAsyncResponseMessageExtension.RecoverOptions.ALLWAYS).ConfigureAwait(false);
        try
        {
            var response = await result.Content
                .ReadFromJsonAsync<ServiceManagementBindingResponse>(JsonSerializerExtensions.Options, cancellationToken)
                .ConfigureAwait(false);

            if (response == null || response.Items.Count() != 1)
            {
                throw new ServiceException("Response must not be null and contain exactly 1 item");
            }

            return response.Items.Single();
        }
        catch (JsonException je)
        {
            throw new ServiceException(je.Message);
        }
    }
}
