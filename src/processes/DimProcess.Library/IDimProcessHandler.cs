/********************************************************************************
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

using Dim.Entities.Enums;

namespace DimProcess.Library;

public interface IDimProcessHandler
{
    Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> CreateSubaccount(Guid tenantId, string tenantName, CancellationToken cancellationToken);
    Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> CreateServiceManagerBindings(Guid tenantId, CancellationToken cancellationToken);
    Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> AssignEntitlements(Guid tenantId, CancellationToken cancellationToken);
    Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> CreateServiceInstance(Guid tenantId, CancellationToken cancellationToken);
    Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> CreateServiceBindings(Guid tenantId, CancellationToken cancellationToken);
    Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> SubscribeApplication(Guid tenantId, CancellationToken cancellationToken);
    Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> CreateCloudFoundryEnvironment(Guid tenantId, string tenantName, CancellationToken cancellationToken);
    Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> CreateCloudFoundrySpace(Guid tenantId, string tenantName, CancellationToken cancellationToken);
    Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> AddSpaceManagerRole(Guid tenantId, CancellationToken cancellationToken);
    Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> AddSpaceDeveloperRole(Guid tenantId, CancellationToken cancellationToken);
    Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> CreateDimServiceInstance(string tenantName, Guid tenantId, CancellationToken cancellationToken);
    Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> CreateServiceInstanceBindings(string tenantName, Guid tenantId, CancellationToken cancellationToken);
    Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> CreateApplication(string tenantName, Guid tenantId, CancellationToken cancellationToken);
    Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> GetDimDetails(string tenantName, Guid tenantId, CancellationToken cancellationToken);
    Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> CreateCompanyIdentity(Guid tenantId, string tenantName, CancellationToken cancellationToken);
    Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> CreateStatusList(Guid tenantId, CancellationToken cancellationToken);
    Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> AssignCompanyApplication(Guid tenantId, CancellationToken cancellationToken);
    Task<(IEnumerable<ProcessStepTypeId>? nextStepTypeIds, ProcessStepStatusId stepStatusId, bool modified, string? processMessage)> SendCallback(Guid tenantId, CancellationToken cancellationToken);
}
