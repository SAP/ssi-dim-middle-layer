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
using Org.Eclipse.TractusX.Portal.Backend.Framework.HttpClientExtensions;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Dim.Clients.Token;

public class BasicAuthTokenService : IBasicAuthTokenService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public BasicAuthTokenService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<HttpClient> GetBasicAuthorizedClient<T>(BasicAuthSettings settings, CancellationToken cancellationToken)
    {
        var tokenParameters = new GetBasicTokenSettings(
            $"{typeof(T).Name}Auth",
            settings.ClientId,
            settings.ClientSecret,
            settings.TokenAddress);

        var token = await this.GetBasicTokenAsync(tokenParameters, cancellationToken).ConfigureAwait(false);

        var httpClient = _httpClientFactory.CreateClient(typeof(T).Name);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return httpClient;
    }

    public async Task<HttpClient> GetBasicAuthorizedLegacyClient<T>(BasicAuthSettings settings, CancellationToken cancellationToken)
    {
        var tokenParameters = new GetBasicTokenSettings(
            $"{typeof(T).Name}Auth",
            settings.ClientId,
            settings.ClientSecret,
            settings.TokenAddress);

        var token = await this.GetBasicLegacyToken(tokenParameters, cancellationToken).ConfigureAwait(false);

        var httpClient = _httpClientFactory.CreateClient(typeof(T).Name);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return httpClient;
    }

    private async Task<string?> GetBasicTokenAsync(GetBasicTokenSettings settings, CancellationToken cancellationToken)
    {
        var formParameters = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" }
        };
        var content = new FormUrlEncodedContent(formParameters);
        var authClient = _httpClientFactory.CreateClient(settings.HttpClientName);
        var authenticationString = $"{settings.ClientId}:{settings.ClientSecret}";
        var base64String = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(authenticationString));

        authClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64String);

        var response = await authClient.PostAsync(settings.TokenAddress, content, cancellationToken)
            .CatchingIntoServiceExceptionFor("token-post", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE).ConfigureAwait(false);

        var responseObject = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonSerializerExtensions.Options, cancellationToken).ConfigureAwait(false);
        return responseObject?.AccessToken;
    }

    private async Task<string?> GetBasicLegacyToken(GetBasicTokenSettings settings, CancellationToken cancellationToken)
    {
        var formParameters = new Dictionary<string, string>
        {
            { "username", settings.ClientId },
            { "password", settings.ClientSecret },
            { "client_id", "cf" },
            { "grant_type", "password" },
            { "response_type", "token" }
        };
        var content = new FormUrlEncodedContent(formParameters);
        var authClient = _httpClientFactory.CreateClient(settings.HttpClientName);
        var base64String = Convert.ToBase64String("cf:"u8.ToArray());
        authClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64String);

        var response = await authClient.PostAsync(settings.TokenAddress, content, cancellationToken)
            .CatchingIntoServiceExceptionFor("token-post", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE).ConfigureAwait(false);

        var responseObject = await response.Content.ReadFromJsonAsync<LegacyAuthResponse>(JsonSerializerExtensions.Options, cancellationToken).ConfigureAwait(false);
        return responseObject?.AccessToken;
    }
}
