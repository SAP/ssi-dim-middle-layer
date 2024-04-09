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

public record AddSpaceRoleToUserRequest(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("relationships")] SpaceRoleRelationship Relationship
);

public record SpaceRoleRelationship(
    [property: JsonPropertyName("user")] RelationshipUser User,
    [property: JsonPropertyName("space")] SpaceRoleSpace Space
);

public record RelationshipUser(
    [property: JsonPropertyName("data")] UserData Data
);

public record UserData(
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("origin")] string Origin
);

public record SpaceRoleSpace(
    [property: JsonPropertyName("data")] SpaceRoleData Data
);

public record SpaceRoleData(
    [property: JsonPropertyName("guid")] Guid Id
);
