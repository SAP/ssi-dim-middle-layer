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

using Dim.DbAccess.Models;
using Dim.Entities.Entities;

namespace Dim.DbAccess.Repositories;

public interface ITenantRepository
{
    Tenant CreateTenant(string companyName, string bpn, string didDocumentLocation, bool isIssuer, Guid processId, Guid operatorId);
    Task<(bool Exists, Guid TenantId, string CompanyName, string Bpn)> GetTenantDataForProcessId(Guid processId);
    void AttachAndModifyTenant(Guid tenantId, Action<Tenant>? initialize, Action<Tenant> modify);
    Task<(bool IsIssuer, string? HostingUrl)> GetHostingUrlAndIsIssuer(Guid tenantId);
    Task<(bool Exists, Guid TenantId)> GetTenantForBpn(string bpn);
    void CreateTenantTechnicalUser(Guid tenantId, string technicalUserName, Guid externalId, Guid processId);
    void AttachAndModifyTechnicalUser(Guid technicalUserId, Action<TechnicalUser>? initialize, Action<TechnicalUser> modify);
    Task<(bool Exists, Guid TechnicalUserId, string CompanyName, string Bpn)> GetTenantDataForTechnicalUserProcessId(Guid processId);
    Task<(Guid ExternalId, WalletData WalletData)> GetTechnicalUserCallbackData(Guid technicalUserId);
    Task<(bool Exists, Guid TechnicalUserId, Guid ProcessId)> GetTechnicalUserForBpn(string bpn, string technicalUserName);
    Task<Guid> GetExternalIdForTechnicalUser(Guid technicalUserId);
    void RemoveTechnicalUser(Guid technicalUserId);
    Task<bool> IsTenantExisting(string companyName, string bpn);
    Task<string> GetTenantBpn(Guid tenantId);
    Task<Guid?> GetOperationId(Guid tenantId);
    Task<(string? BaseUrl, WalletData WalletData)> GetCompanyRequestData(Guid tenantId);
    Task<(bool Exists, Guid? CompanyId, string? BaseUrl, WalletData WalletData)> GetCompanyAndWalletDataForBpn(string bpn);
    Task<(Guid? CompanyId, string? BaseUrl, WalletData WalletData)> GetStatusListCreationData(Guid tenantId);
    Task<(string Bpn, string? BaseUrl, WalletData WalletData, string? Did, string? DownloadUrl)> GetCallbackData(Guid tenantId);
    Task<(string? DownloadUrl, bool IsIssuer)> GetDownloadUrlAndIsIssuer(Guid tenantId);
}
