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

using Dim.Entities.Entities;

namespace Dim.DbAccess.Repositories;

public interface ITenantRepository
{
    Tenant CreateTenant(string companyName, string bpn, string didDocumentLocation, bool isIssuer, Guid processId, Guid operatorId);
    Task<(bool Exists, Guid TenantId, string CompanyName, string Bpn)> GetTenantDataForProcessId(Guid processId);
    void AttachAndModifyTenant(Guid tenantId, Action<Tenant>? initialize, Action<Tenant> modify);
    Task<Guid?> GetSubAccountIdByTenantId(Guid tenantId);
    Task<(Guid? SubAccountId, string? ServiceInstanceId)> GetSubAccountAndServiceInstanceIdsByTenantId(Guid tenantId);
    Task<(Guid? SubAccountId, string? ServiceBindingName)> GetSubAccountIdAndServiceBindingNameByTenantId(Guid tenantId);
    Task<Guid?> GetSpaceId(Guid tenantId);
    Task<Guid?> GetDimInstanceId(Guid tenantId);
    Task<(string bpn, string? DownloadUrl, string? Did, Guid? DimInstanceId)> GetCallbackData(Guid tenantId);
    Task<(Guid? DimInstanceId, string HostingUrl, bool IsIssuer)> GetDimInstanceIdAndHostingUrl(Guid tenantId);
    Task<(string? ApplicationId, Guid? CompanyId, Guid? DimInstanceId, bool IsIssuer)> GetApplicationAndCompanyId(Guid tenantId);
    Task<(bool Exists, Guid? CompanyId, Guid? InstanceId)> GetCompanyAndInstanceIdForBpn(string bpn);
}
