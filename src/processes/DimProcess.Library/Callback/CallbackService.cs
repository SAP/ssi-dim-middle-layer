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

using Dim.Clients.Api.Cf;
using Dim.Clients.Extensions;
using Dim.Clients.Token;
using DimProcess.Library.Callback.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Token;
using System.Net.Http.Json;
using System.Text.Json;

namespace DimProcess.Library.Callback;

public class CallbackService(ITokenService tokenService, IOptions<CallbackSettings> options)
    : ICallbackService
{
    private readonly CallbackSettings _settings = options.Value;

    public async Task SendCallback(string bpn, ServiceCredentialBindingDetailResponse dimDetails, JsonDocument didDocument, string did, CancellationToken cancellationToken)
    {
        var httpClient = await tokenService.GetAuthorizedClient<CallbackService>(_settings, cancellationToken)
            .ConfigureAwait(false);
        var data = new CallbackDataModel(
            did,
            didDocument,
            new AuthenticationDetail(
                dimDetails.Credentials.Uaa.Url,
                dimDetails.Credentials.Uaa.ClientId,
                dimDetails.Credentials.Uaa.ClientSecret)
        );
        await httpClient.PostAsJsonAsync($"/api/administration/registration/dim/{bpn}", data, JsonSerializerExtensions.Options, cancellationToken).ConfigureAwait(false);
    }

    public async Task SendTechnicalUserCallback(Guid externalId, string tokenAddress, string clientId, string clientSecret, CancellationToken cancellationToken)
    {
        var httpClient = await tokenService.GetAuthorizedClient<CallbackService>(_settings, cancellationToken)
            .ConfigureAwait(false);
        var data = new AuthenticationDetail(
            tokenAddress,
            clientId,
            clientSecret);
        await httpClient.PostAsJsonAsync($"/api/adminstration/serviceAccount/callback/{externalId}", data, JsonSerializerExtensions.Options, cancellationToken).ConfigureAwait(false);
    }
}
