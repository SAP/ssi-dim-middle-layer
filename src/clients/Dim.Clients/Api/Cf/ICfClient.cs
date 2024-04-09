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

namespace Dim.Clients.Api.Cf;

public interface ICfClient
{
    Task<Guid> CreateCloudFoundrySpace(string tenantName, CancellationToken cancellationToken);
    Task AddSpaceRoleToUser(string type, string user, Guid spaceId, CancellationToken cancellationToken);
    Task<Guid> GetServicePlan(string servicePlanName, string servicePlanType, CancellationToken cancellationToken);
    Task<Guid> GetSpace(string tenantName, CancellationToken cancellationToken);
    Task CreateDimServiceInstance(string tenantName, Guid spaceId, Guid servicePlanId, CancellationToken cancellationToken);
    Task CreateServiceInstanceBindings(string tenantName, Guid spaceId, CancellationToken cancellationToken);
    Task<Guid> GetServiceBinding(string tenantName, Guid spaceId, string bindingName, CancellationToken cancellationToken);
    Task<ServiceCredentialBindingDetailResponse> GetServiceBindingDetails(Guid id, CancellationToken cancellationToken);
}
