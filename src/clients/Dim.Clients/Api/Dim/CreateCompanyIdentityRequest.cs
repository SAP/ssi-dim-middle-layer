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

namespace Dim.Clients.Api.Dim;

public record CreateCompanyIdentityRequest(
    [property: JsonPropertyName("payload")] Payload Payload
);

public record Payload(
    [property: JsonPropertyName("hostingUrl")] string HostingUrl,
    [property: JsonPropertyName("bootstrap")] Bootstrap Bootstrap,
    [property: JsonPropertyName("keys")] IEnumerable<Key> Keys
);

public record Service(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("type")] string Type
);

public record Bootstrap(
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("protocols")] IEnumerable<string> Protocols
);

public record Key(
    [property: JsonPropertyName("type")] string Type
);

public record Network(
    [property: JsonPropertyName("didMethod")] string DidMethod,
    [property: JsonPropertyName("type")] string Type
);

public record CreateCompanyIdentityResponse(
    [property: JsonPropertyName("did")] string Did,
    [property: JsonPropertyName("companyId")] Guid CompanyId,
    [property: JsonPropertyName("downloadURL")] string DownloadUrl
);
