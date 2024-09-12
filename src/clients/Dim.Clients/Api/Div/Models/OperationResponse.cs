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

using System.Text.Json.Serialization;

namespace Dim.Clients.Api.Div.Models;

public record OperationResponse(
    [property: JsonPropertyName("operationId")] Guid OperationId,
    [property: JsonPropertyName("status")] OperationResponseStatus Status,
    [property: JsonPropertyName("createdAt")] DateTimeOffset? CreatedAt,
    [property: JsonPropertyName("error")] string? Error,
    [property: JsonPropertyName("data")] OperationResponseData? Data
);

public record OperationResponseData(
    [property: JsonPropertyName("customerWalletId")] Guid CustomerWalletId,
    [property: JsonPropertyName("customerId")] string CustomerId,
    [property: JsonPropertyName("customerName")] string CustomerName,
    [property: JsonPropertyName("serviceKey")] ServiceKey ServiceKey
);

public record ServiceKey(
    [property: JsonPropertyName("uaa")] ServiceUaa Uaa,
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("vendor")] string Vendor
);

public record ServiceUaa(
    [property: JsonPropertyName("apiurl")] string ApiUrl,
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("clientid")] string ClientId,
    [property: JsonPropertyName("clientsecret")] string ClientSecret
);

public enum OperationResponseStatus
{
    pending = 1,
    failed = 2,
    completed = 3
}
