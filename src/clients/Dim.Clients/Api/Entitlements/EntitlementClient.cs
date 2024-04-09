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
using Org.Eclipse.TractusX.Portal.Backend.Framework.HttpClientExtensions;
using System.Net.Http.Json;

namespace Dim.Clients.Api.Entitlements;

public class EntitlementClient : IEntitlementClient
{
    private readonly IBasicAuthTokenService _basicAuthTokenService;

    public EntitlementClient(IBasicAuthTokenService basicAuthTokenService)
    {
        _basicAuthTokenService = basicAuthTokenService;
    }

    public async Task AssignEntitlements(BasicAuthSettings basicAuthSettings, Guid subAccountId, CancellationToken cancellationToken)
    {
        var client = await _basicAuthTokenService.GetBasicAuthorizedClient<EntitlementClient>(basicAuthSettings, cancellationToken).ConfigureAwait(false);
        var data = new CreateSubAccountRequest(
                new List<SubaccountServicePlan>
                {
                    new(Enumerable.Repeat(new AssignmentInfo(true, null, subAccountId), 1), "cis", "local"),
                    new(Enumerable.Repeat(new AssignmentInfo(true, null, subAccountId), 1), "decentralized-identity-management-app", "standard"),
                    new(Enumerable.Repeat(new AssignmentInfo(null, 1, subAccountId), 1), "decentralized-identity-management", "standard"),
                    new(Enumerable.Repeat(new AssignmentInfo(true, null, subAccountId), 1), "auditlog-viewer", "free")
                }
            );

        await client.PutAsJsonAsync("/entitlements/v1/subaccountServicePlans", data, JsonSerializerExtensions.Options, cancellationToken)
            .CatchingIntoServiceExceptionFor("assign-entitlements", HttpAsyncResponseMessageExtension.RecoverOptions.INFRASTRUCTURE).ConfigureAwait(false);
    }
}
