/********************************************************************************
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

using Dim.Clients.Extensions;
using Dim.Clients.Token;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.HttpClientExtensions;
using System.Net.Http.Json;
using System.Text.Json;

namespace Dim.Clients.Api.Dim;

public class DimClient(IBasicAuthTokenService basicAuthTokenService, IHttpClientFactory clientFactory)
    : IDimClient
{
    public async Task<CreateCompanyIdentityResponse> CreateCompanyIdentity(BasicAuthSettings dimBasicAuth, Guid tenantId, string hostingUrl, string baseUrl, bool isIssuer, CancellationToken cancellationToken)
    {
        var client = await basicAuthTokenService.GetBasicAuthorizedClient<DimClient>(dimBasicAuth, cancellationToken).ConfigureAwait(false);
        var data = new CreateCompanyIdentityRequest(new Payload(
            hostingUrl,
            new Network("web", "production"),
            [new Service($"dim:web:{tenantId}", "CredentialService", "https://dis-agent-prod.eu10.dim.cloud.sap/api/v1.0.0/iatp")],
            isIssuer ?
                [
                    new("SIGNING"),
                    new("SIGNING_VC")
                ] :
                new Key[]
                {
                    new("SIGNING")
                },
            "holder iatp"));
        var result = await client.PostAsJsonAsync($"{baseUrl}/api/v2.0.0/companyIdentities", data, JsonSerializerExtensions.Options, cancellationToken)
            .CatchingIntoServiceExceptionFor("create-company-identity", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE,
                async m =>
                {
                    var message = await m.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return (false, message);
                }).ConfigureAwait(false);
        try
        {
            var response = await result.Content
                .ReadFromJsonAsync<CreateCompanyIdentityResponse>(JsonSerializerExtensions.Options, cancellationToken)
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

    public async Task<JsonDocument> GetDidDocument(string url, CancellationToken cancellationToken)
    {
        var client = clientFactory.CreateClient("didDocumentDownload");
        using var result = await client.GetStreamAsync(url, cancellationToken).ConfigureAwait(false);
        var document = await JsonDocument.ParseAsync(result, cancellationToken: cancellationToken).ConfigureAwait(false);
        return document;
    }

    public async Task<string> CreateApplication(BasicAuthSettings dimAuth, string dimBaseUrl, string tenantName, CancellationToken cancellationToken)
    {
        var client = await basicAuthTokenService.GetBasicAuthorizedClient<DimClient>(dimAuth, cancellationToken).ConfigureAwait(false);
        var data = new CreateApplicationRequest(new ApplicationPayload(
            "catena-x-portal",
            $"Catena-X Portal MIW for {tenantName}",
            6));
        var result = await client.PostAsJsonAsync($"{dimBaseUrl}/api/v2.0.0/applications", data, JsonSerializerExtensions.Options, cancellationToken)
            .CatchingIntoServiceExceptionFor("create-application", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE,
                async m =>
                {
                    var message = await m.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return (false, message);
                }).ConfigureAwait(false);
        try
        {
            var response = await result.Content
                .ReadFromJsonAsync<CreateApplicationResponse>(JsonSerializerExtensions.Options, cancellationToken)
                .ConfigureAwait(false);
            if (response == null)
            {
                throw new ServiceException("Response was empty", true);
            }

            return response.Id;
        }
        catch (JsonException je)
        {
            throw new ServiceException(je.Message);
        }
    }

    public async Task<string> GetApplication(BasicAuthSettings dimAuth, string dimBaseUrl, string applicationId, CancellationToken cancellationToken)
    {
        var client = await basicAuthTokenService.GetBasicAuthorizedClient<DimClient>(dimAuth, cancellationToken).ConfigureAwait(false);
        var result = await client.GetAsync($"{dimBaseUrl}/api/v2.0.0/applications/{applicationId}", cancellationToken)
            .CatchingIntoServiceExceptionFor("get-application", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE,
                async m =>
                {
                    var message = await m.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return (false, message);
                }).ConfigureAwait(false);
        try
        {
            var response = await result.Content
                .ReadFromJsonAsync<ApplicationResponse>(JsonSerializerExtensions.Options, cancellationToken)
                .ConfigureAwait(false);
            if (response == null)
            {
                throw new ServiceException("Response must not be null");
            }

            return response.ApplicaitonKey;
        }
        catch (JsonException je)
        {
            throw new ServiceException(je.Message);
        }
    }

    public async Task AssignApplicationToCompany(BasicAuthSettings dimAuth, string dimBaseUrl, string applicationKey, Guid companyId, CancellationToken cancellationToken)
    {
        var client = await basicAuthTokenService.GetBasicAuthorizedClient<DimClient>(dimAuth, cancellationToken).ConfigureAwait(false);
        var data = new CompanyIdentityPatch(new ApplicationUpdates(Enumerable.Repeat(applicationKey, 1)));
        await client.PatchAsJsonAsync($"{dimBaseUrl}/api/v2.0.0/companyIdentities/{companyId}", data, JsonSerializerExtensions.Options, cancellationToken)
            .CatchingIntoServiceExceptionFor("assign-application", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE,
                async m =>
                {
                    var message = await m.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return (false, message);
                }).ConfigureAwait(false);
    }

    public async Task<string> GetStatusList(BasicAuthSettings dimAuth, string dimBaseUrl, Guid companyId, CancellationToken cancellationToken)
    {
        var client = await basicAuthTokenService.GetBasicAuthorizedClient<DimClient>(dimAuth, cancellationToken).ConfigureAwait(false);
        var result = await client.GetAsync($"{dimBaseUrl}/api/v2.0.0/companyIdentities/{companyId}/revocationLists", cancellationToken);
        try
        {
            var response = await result.Content
                .ReadFromJsonAsync<StatusListListResponse>(JsonSerializerExtensions.Options, cancellationToken)
                .ConfigureAwait(false);
            if (response == null)
            {
                throw new ServiceException("Response must not be null");
            }

            if (!response.Data.Any(x => x.RemainingSpace > 0))
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
        var client = await basicAuthTokenService.GetBasicAuthorizedClient<DimClient>(dimAuth, cancellationToken).ConfigureAwait(false);
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
                .ConfigureAwait(false);
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
