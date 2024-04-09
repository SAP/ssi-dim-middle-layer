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

using System.Text.Json.Serialization;

namespace Dim.Clients.Api.SubAccounts;

public record CreateSubAccountRequest(
    [property: JsonPropertyName("betaEnabled")] bool BetaEnabled,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("displayName")] string DisplayName,
    [property: JsonPropertyName("labels")] Dictionary<string, IEnumerable<string>> Labels,
    [property: JsonPropertyName("origin")] string Origin,
    [property: JsonPropertyName("parentGUID")] Guid ParentId,
    [property: JsonPropertyName("region")] string Region,
    [property: JsonPropertyName("subaccountAdmins")] IEnumerable<string> SubaccountAdmins,
    [property: JsonPropertyName("subdomain")] string Subdomain,
    [property: JsonPropertyName("usedForProduction")] UsedForProduction UsedForProduction
);

public record CreateSubaccountResponse(
    [property: JsonPropertyName("guid")] Guid Id
);

public record ServiceManagementBindingResponse(
    [property: JsonPropertyName("items")] IEnumerable<ServiceManagementBindingItem> Items
);

public record ServiceManagementBindingItem(
    [property: JsonPropertyName("clientid")] string ClientId,
    [property: JsonPropertyName("clientsecret")] string ClientSecret,
    [property: JsonPropertyName("sm_url")] string SmUrl,
    [property: JsonPropertyName("url")] string Url
);
