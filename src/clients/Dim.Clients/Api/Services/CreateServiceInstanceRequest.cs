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

namespace Dim.Clients.Api.Services;

public record CreateServiceInstanceRequest(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("service_offering_name")] string ServiceOfferingName,
    [property: JsonPropertyName("service_plan_name")] string ServicePlanName,
    [property: JsonPropertyName("parameters")] Dictionary<string, string> Parameters
);

public record CreateServiceInstanceResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name
);

public record CreateServiceBindingRequest(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("service_instance_id")] string ServiceInstanceId
);

public record CreateServiceBindingResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name
);

public record GetBindingResponse(
    [property: JsonPropertyName("items")] IEnumerable<BindingItem> Items
);

public record BindingItem(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("service_instance_id")] Guid ServiceInstanceId,
    [property: JsonPropertyName("credentials")] BindingCredentials Credentials
);

public record BindingCredentials(
    [property: JsonPropertyName("endpoints")] GetBindingEndpoints Endpoints,
    [property: JsonPropertyName("grant_type")] string GrantType,
    [property: JsonPropertyName("uaa")] GetBindingUaa Uaa
);

public record GetBindingUaa(
    [property: JsonPropertyName("clientid")] string ClientId,
    [property: JsonPropertyName("clientsecret")] string ClientSecret
);

public record GetBindingEndpoints(
    [property: JsonPropertyName("provisioning_service_url")] string ProvisioningServiceUrl,
    [property: JsonPropertyName("saas_registry_service_url")] string SaasRegistryServiceUrl
);
