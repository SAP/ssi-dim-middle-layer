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

public record ServicePlanResponse(
    [property: JsonPropertyName("resources")] IEnumerable<ServicePlanResources> Resources
);

public record ServicePlanResources(
    [property: JsonPropertyName("guid")] Guid Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("broker_catalog")] BrokerCatalog? BrokerCatalog
);

public record BrokerCatalog(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("metadata")] BrokerCatalogMetadata? BrokerCatalogMetadata
);

public record BrokerCatalogMetadata(
    [property: JsonPropertyName("auto_subscription")] AutoSupscription? AutoSubscription
);

public record AutoSupscription(
    [property: JsonPropertyName("app_name")] string? AppName
);

public record ServiceInstanceResponse(
    [property: JsonPropertyName("resources")] IEnumerable<ServiceInstanceResource> Resources
);

public record ServiceInstanceResource(
    [property: JsonPropertyName("guid")] Guid Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("relationships")] ServiceInstanceRelationship Relationships,
    [property: JsonPropertyName("last_operation")] LastOperation LastOperation
);

public record ServiceInstanceRelationship(
    [property: JsonPropertyName("space")] ServiceInstanceRelationshipSpace Space
);

public record LastOperation(
    [property: JsonPropertyName("state")] string State
);

public record ServiceInstanceRelationshipSpace(
    [property: JsonPropertyName("data")] DimData Data
);

public record CreateServiceCredentialBindingRequest(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("relationships")] ServiceCredentialRelationships Relationships
);

public record ServiceCredentialRelationships(
    [property: JsonPropertyName("service_instance")] DimServiceInstance ServiceInstance
);

public record DimServiceInstance(
    [property: JsonPropertyName("data")] DimData Data
);

public record CreateDimServiceInstance(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("relationships")] DimRelationships Relationships
);

public record DimRelationships(
    [property: JsonPropertyName("space")] DimSpace Space,
    [property: JsonPropertyName("service_plan")] DimServicePlan ServicePlan
);

public record DimServicePlan(
    [property: JsonPropertyName("data")] DimData Data
);

public record DimSpace(
    [property: JsonPropertyName("data")] DimData Data
);

public record DimData(
    [property: JsonPropertyName("guid")] Guid Id
);

public record ServiceCredentialBindingResponse(
    [property: JsonPropertyName("resources")] IEnumerable<ServiceCredentialBindingResource> Resources
);

public record ServiceCredentialBindingRelationships(
    [property: JsonPropertyName("service_instance")] ScbServiceInstnace ServiceInstance
);

public record ScbServiceInstnace(
    [property: JsonPropertyName("data")] DimData Data
);

public record ServiceCredentialBindingResource(
    [property: JsonPropertyName("guid")] Guid Id,
    [property: JsonPropertyName("relationships")] ServiceCredentialBindingRelationships Relationships
);

public record ServiceCredentialBindingDetailResponse(
    [property: JsonPropertyName("credentials")] Credentials Credentials
);

public record Credentials(
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("uaa")] Uaa Uaa
);

public record Uaa(
    [property: JsonPropertyName("clientid")] string ClientId,
    [property: JsonPropertyName("clientsecret")] string ClientSecret,
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("apiurl")] string ApiUrl
);
