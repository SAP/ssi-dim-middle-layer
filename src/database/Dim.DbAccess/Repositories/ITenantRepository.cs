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
    Task<(bool Exists, Guid TenantId)> GetTenantForBpn(string bpn);
    void CreateTenantTechnicalUser(Guid tenantId, string technicalUserName, Guid externalId, Guid processId);
    void AttachAndModifyTechnicalUser(Guid technicalUserId, Action<TechnicalUser>? initialize, Action<TechnicalUser> modify);
    Task<(bool Exists, Guid TechnicalUserId, string CompanyName, string Bpn)> GetTenantDataForTechnicalUserProcessId(Guid processId);
    Task<(Guid? spaceId, string technicalUserName)> GetSpaceIdAndTechnicalUserName(Guid technicalUserId);
    Task<(Guid ExternalId, string? TokenAddress, string? ClientId, byte[]? ClientSecret, byte[]? InitializationVector, int? EncryptionMode)> GetTechnicalUserCallbackData(Guid technicalUserId);
    Task<(Guid? DimInstanceId, Guid? CompanyId)> GetDimInstanceIdAndDid(Guid tenantId);
    Task<(bool Exists, Guid TechnicalUserId, Guid ProcessId)> GetTechnicalUserForBpn(string bpn, string technicalUserName);
    Task<Guid> GetExternalIdForTechnicalUser(Guid technicalUserId);
    void RemoveTechnicalUser(Guid technicalUserId);
    Task<bool> IsTenantExisting(string companyName, string bpn);
}
