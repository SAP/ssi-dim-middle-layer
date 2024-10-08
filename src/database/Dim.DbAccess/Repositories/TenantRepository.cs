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
using Dim.Entities;
using Dim.Entities.Entities;
using Dim.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace Dim.DbAccess.Repositories;

public class TenantRepository(DimDbContext dbContext)
    : ITenantRepository
{
    public Tenant CreateTenant(string companyName, string bpn, string didDocumentLocation, bool isIssuer, Guid processId, Guid operatorId) =>
        dbContext.Tenants.Add(new Tenant(Guid.NewGuid(), companyName, bpn, didDocumentLocation, isIssuer, processId, operatorId)).Entity;

    public Task<(bool Exists, Guid TenantId, string CompanyName, string Bpn)> GetTenantDataForProcessId(Guid processId) =>
        dbContext.Tenants
            .Where(x => x.ProcessId == processId)
            .Select(x => new ValueTuple<bool, Guid, string, string>(true, x.Id, x.CompanyName, x.Bpn))
            .SingleOrDefaultAsync();

    public void AttachAndModifyTenant(Guid tenantId, Action<Tenant>? initialize, Action<Tenant> modify)
    {
        var tenant = new Tenant(tenantId, null!, null!, null!, default, Guid.Empty, Guid.Empty);
        initialize?.Invoke(tenant);
        dbContext.Tenants.Attach(tenant);
        modify(tenant);
    }

    public Task<(bool IsIssuer, string? HostingUrl)> GetHostingUrlAndIsIssuer(Guid tenantId)
        => dbContext.Tenants
            .Where(x => x.Id == tenantId)
            .Select(x => new ValueTuple<bool, string?>(x.IsIssuer, x.DidDocumentLocation))
            .SingleOrDefaultAsync();

    public Task<(bool Exists, Guid TenantId)> GetTenantForBpn(string bpn) =>
        dbContext.Tenants.Where(x => x.Bpn == bpn)
            .Select(x => new ValueTuple<bool, Guid>(true, x.Id))
            .SingleOrDefaultAsync();

    public Task<bool> IsTenantExisting(string companyName, string bpn) =>
        dbContext.Tenants
            .AnyAsync(x => x.CompanyName == companyName && x.Bpn == bpn);

    public Task<Guid?> GetOperationId(Guid tenantId) =>
        dbContext.Tenants
            .Where(x => x.Id == tenantId)
            .Select(x => x.OperationId)
            .SingleOrDefaultAsync();

    public Task<(string? BaseUrl, WalletData WalletData)> GetCompanyRequestData(Guid tenantId) =>
        dbContext.Tenants
            .Where(x => x.Id == tenantId)
            .Select(x => new ValueTuple<string?, WalletData>(
                x.BaseUrl,
                new WalletData(
                    x.TokenAddress,
                    x.ClientId,
                    x.ClientSecret,
                    x.InitializationVector,
                    x.EncryptionMode
                )))
            .SingleOrDefaultAsync();

    public Task<(bool Exists, Guid? CompanyId, string? BaseUrl, WalletData WalletData)> GetCompanyAndWalletDataForBpn(string bpn) =>
        dbContext.Tenants
            .Where(x => x.Bpn == bpn)
            .Select(x => new ValueTuple<bool, Guid?, string?, WalletData>(
                true,
                x.CompanyId,
                x.BaseUrl,
                new WalletData(
                    x.TokenAddress,
                    x.ClientId,
                    x.ClientSecret,
                    x.InitializationVector,
                    x.EncryptionMode
                )))
            .SingleOrDefaultAsync();

    public Task<(Guid? CompanyId, string? BaseUrl, WalletData WalletData)> GetStatusListCreationData(Guid tenantId) =>
        dbContext.Tenants
            .Where(x => x.Id == tenantId)
            .Select(x => new ValueTuple<Guid?, string?, WalletData>(
                x.CompanyId,
                x.BaseUrl,
                new WalletData(
                    x.TokenAddress,
                    x.ClientId,
                    x.ClientSecret,
                    x.InitializationVector,
                    x.EncryptionMode
                )))
            .SingleOrDefaultAsync();

    public Task<(string Bpn, string? BaseUrl, WalletData WalletData, string? Did, string? DownloadUrl)> GetCallbackData(Guid tenantId) =>
        dbContext.Tenants
            .Where(x => x.Id == tenantId)
            .Select(x => new ValueTuple<string, string?, WalletData, string?, string?>(
                x.Bpn,
                x.BaseUrl,
                new WalletData(
                    x.TokenAddress,
                    x.ClientId,
                    x.ClientSecret,
                    x.InitializationVector,
                    x.EncryptionMode
                ),
                x.Did,
                x.DidDownloadUrl))
            .SingleOrDefaultAsync();

    public Task<(string? DownloadUrl, bool IsIssuer)> GetDownloadUrlAndIsIssuer(Guid tenantId) =>
        dbContext.Tenants
            .Where(x => x.Id == tenantId)
            .Select(x => new ValueTuple<string?, bool>(x.DidDownloadUrl, x.IsIssuer))
            .SingleOrDefaultAsync();

    public Task<ProcessData?> GetWalletProcessForTenant(string bpn, string companyName) =>
        dbContext.Tenants
            .Where(x =>
                x.Bpn == bpn &&
                x.CompanyName == companyName &&
                x.Process!.ProcessTypeId == ProcessTypeId.SETUP_DIM)
            .Select(x => new ProcessData(
                x.ProcessId,
                x.Process!.ProcessSteps.Select(ps => new ProcessStepData(
                    ps.ProcessStepTypeId,
                    ps.ProcessStepStatusId))))
            .SingleOrDefaultAsync();
}
