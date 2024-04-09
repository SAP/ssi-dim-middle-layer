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

namespace Dim.Clients.Api.Cf;

public record CreateSpaceRequest(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("relationships")] SpaceRelationship Relationship
);

public record SpaceRelationship(
    [property: JsonPropertyName("organization")] SpaceOrganization Organization
);

public record SpaceOrganization(
    [property: JsonPropertyName("data")] SpaceRelationshipData Data
);

public record SpaceRelationshipData(
    [property: JsonPropertyName("guid")] Guid Id
);

public record CreateSpaceResponse(
    [property: JsonPropertyName("guid")] Guid Id
);

public record GetEnvironmentsResponse(
    [property: JsonPropertyName("resources")] IEnumerable<EnvironmentResource> Resources
);

public record EnvironmentResource(
    [property: JsonPropertyName("guid")] Guid EnvironmentId,
    [property: JsonPropertyName("name")] string Name
);
