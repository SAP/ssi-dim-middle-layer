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

using Dim.Clients.Api.Dim.Models;
using Dim.Clients.Extensions;
using Dim.Clients.Token;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.HttpClientExtensions;
using System.Net.Http.Json;
using System.Text.Json;
using System.Web;

namespace Dim.Clients.Api.Dim;

public class DimClient(IBasicAuthTokenService basicAuthTokenService)
    : IDimClient
{
    public async Task<CompanyData> GetCompanyData(BasicAuthSettings dimAuth, string dimBaseUrl, string tenantName, string application, CancellationToken cancellationToken)
    {
        var client = await basicAuthTokenService.GetBasicAuthorizedClient<DimClient>(dimAuth, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        var filterString = $"name eq {tenantName} and application eq {application}";
        var result = await client.GetAsync($"{dimBaseUrl}/api/v2.0.0/companyIdentities?$filter={HttpUtility.UrlEncode(filterString)}", cancellationToken);
        try
        {
            var response = await result.Content
                .ReadFromJsonAsync<CompanyIdentitiesResponse>(JsonSerializerExtensions.Options, cancellationToken)
                .ConfigureAwait(false);
            if (response?.Data == null || response.Data.Count() != 1)
            {
                throw new ConflictException("There is no matching company");
            }

            return response.Data.Single();
        }
        catch (JsonException je)
        {
            throw new ServiceException(je.Message);
        }
    }

    public async Task<string> GetStatusList(BasicAuthSettings dimAuth, string dimBaseUrl, Guid companyId, CancellationToken cancellationToken)
    {
        var client = await basicAuthTokenService.GetBasicAuthorizedClient<DimClient>(dimAuth, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        var result = await client.GetAsync($"{dimBaseUrl}/api/v2.0.0/companyIdentities/{companyId}/revocationLists", cancellationToken);
        try
        {
            var response = await result.Content
                .ReadFromJsonAsync<StatusListListResponse>(JsonSerializerExtensions.Options, cancellationToken)
                .ConfigureAwait(ConfigureAwaitOptions.None);
            if (response?.Data == null || !response.Data.Any(x => x.RemainingSpace > 0))
            {
                throw new ConflictException("There is no status list with remaining space, please create a new one.");
            }

            return response.Data.First(x => x.RemainingSpace > 0).StatusListCredential;
        }
        catch (JsonException je)
        {
            throw new ServiceException(je.Message);
        }
    }

    public async Task<string> CreateStatusList(BasicAuthSettings dimAuth, string dimBaseUrl, Guid companyId, CancellationToken cancellationToken)
    {
        var client = await basicAuthTokenService.GetBasicAuthorizedClient<DimClient>(dimAuth, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        var data = new CreateStatusListRequest(new CreateStatusListPaypload(new CreateStatusList("StatusList2021", DateTimeOffset.UtcNow.ToString("yyyyMMdd"), "New revocation list", 2097152)));
        var result = await client.PostAsJsonAsync($"{dimBaseUrl}/api/v2.0.0/companyIdentities/{companyId}/revocationLists", data, JsonSerializerExtensions.Options, cancellationToken)
            .CatchingIntoServiceExceptionFor("assign-application", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE,
                async m =>
                {
                    var message = await m.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return (false, message);
                }).ConfigureAwait(false);
        try
        {
            var response = await result.Content
                .ReadFromJsonAsync<CreateStatusListResponse>(JsonSerializerExtensions.Options, cancellationToken)
                .ConfigureAwait(ConfigureAwaitOptions.None);
            if (response == null)
            {
                throw new ServiceException("Response must not be null");
            }

            return response.RevocationVc.Id;
        }
        catch (JsonException je)
        {
            throw new ServiceException(je.Message);
        }
    }
}
