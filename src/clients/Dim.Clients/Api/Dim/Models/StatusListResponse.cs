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

namespace Dim.Clients.Api.Dim.Models;

public record CreateStatusListRequest(
    [property: JsonPropertyName("payload")] CreateStatusListPaypload Payload
);

public record CreateStatusListPaypload(
    [property: JsonPropertyName("create")] CreateStatusList Create
);

public record CreateStatusList(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("length")] int Length
);

public record CreateStatusListResponse(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("revocationVc")] RevocationVc RevocationVc
);

public record RevocationVc(
    [property: JsonPropertyName("id")] string Id
);

public record StatusListListResponse(
    [property: JsonPropertyName("count")] int Count,
    [property: JsonPropertyName("data")] IEnumerable<StatusListResponse> Data
);

public record StatusListResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("statusListCredential")] string StatusListCredential,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("length")] int Length,
    [property: JsonPropertyName("remainingSpace")] int RemainingSpace
);
