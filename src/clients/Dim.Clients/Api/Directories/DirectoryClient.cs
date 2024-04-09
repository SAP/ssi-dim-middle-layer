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

using Dim.Clients.Api.Directories.DependencyInjection;
using Dim.Clients.Extensions;
using Dim.Clients.Token;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.HttpClientExtensions;
using System.Net.Http.Json;
using System.Text.Json;

namespace Dim.Clients.Api.Directories;

public class DirectoryClient : IDirectoryClient
{
    private readonly DirectorySettings _settings;
    private readonly IBasicAuthTokenService _basicAuthTokenService;

    public DirectoryClient(IBasicAuthTokenService basicAuthTokenService, IOptions<DirectorySettings> settings)
    {
        _basicAuthTokenService = basicAuthTokenService;
        _settings = settings.Value;
    }

    public async Task<Guid> CreateDirectory(string description, string bpnl, Guid parentId, CancellationToken cancellationToken)
    {
        var client = await _basicAuthTokenService.GetBasicAuthorizedClient<DirectoryClient>(_settings, cancellationToken).ConfigureAwait(false);
        var directory = new DirectoryRequest(
            description,
            Enumerable.Repeat("phil.schneider@digitalnativesolutions.de", 1),
            bpnl,
            new Dictionary<string, IEnumerable<string>>()
            {
                { "cloud_management_service", new[] { "Created by API - Don't change it" } }
            }
        );

        var result = await client.PostAsJsonAsync($"/accounts/v1/directories?parentId={parentId}", directory, JsonSerializerExtensions.Options, cancellationToken)
            .CatchingIntoServiceExceptionFor("create-directory", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE).ConfigureAwait(false);
        try
        {
            var response = await result.Content
                .ReadFromJsonAsync<DirectoryResponse>(JsonSerializerExtensions.Options, cancellationToken)
                .ConfigureAwait(false);

            if (response == null)
            {
                throw new ServiceException("Directory response must not be null");
            }

            return response.Id;
        }
        catch (JsonException je)
        {
            throw new ServiceException(je.Message);
        }
    }
}
