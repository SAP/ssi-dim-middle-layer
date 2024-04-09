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

using Dim.Entities;
using Dim.Entities.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dim.DbAccess.Repositories;

public class TenantRepository : ITenantRepository
{
    private readonly DimDbContext _context;

    public TenantRepository(DimDbContext context)
    {
        _context = context;
    }

    public Tenant CreateTenant(string companyName, string bpn, string didDocumentLocation, bool isIssuer, Guid processId, Guid operatorId) =>
        _context.Tenants.Add(new Tenant(Guid.NewGuid(), companyName, bpn, didDocumentLocation, isIssuer, processId, operatorId)).Entity;

    public Task<(bool Exists, Guid TenantId, string CompanyName, string Bpn)> GetTenantDataForProcessId(Guid processId) =>
        _context.Tenants
            .Where(x => x.ProcessId == processId)
            .Select(x => new ValueTuple<bool, Guid, string, string>(true, x.Id, x.CompanyName, x.Bpn))
            .SingleOrDefaultAsync();

    public void AttachAndModifyTenant(Guid tenantId, Action<Tenant>? initialize, Action<Tenant> modify)
    {
        var tenant = new Tenant(tenantId, null!, null!, null!, default, Guid.Empty, Guid.Empty);
        initialize?.Invoke(tenant);
        _context.Tenants.Attach(tenant);
        modify(tenant);
    }

    public Task<Guid?> GetSubAccountIdByTenantId(Guid tenantId)
        => _context.Tenants
            .Where(x => x.Id == tenantId)
            .Select(x => x.SubAccountId)
            .SingleOrDefaultAsync();

    public Task<(Guid? SubAccountId, string? ServiceInstanceId)> GetSubAccountAndServiceInstanceIdsByTenantId(Guid tenantId)
        => _context.Tenants
            .Where(x => x.Id == tenantId)
            .Select(x => new ValueTuple<Guid?, string?>(x.SubAccountId, x.ServiceInstanceId))
            .SingleOrDefaultAsync();

    public Task<(Guid? SubAccountId, string? ServiceBindingName)> GetSubAccountIdAndServiceBindingNameByTenantId(Guid tenantId)
        => _context.Tenants
            .Where(x => x.Id == tenantId)
            .Select(x => new ValueTuple<Guid?, string?>(x.SubAccountId, x.ServiceBindingName))
            .SingleOrDefaultAsync();

    public Task<Guid?> GetSpaceId(Guid tenantId)
        => _context.Tenants
            .Where(x => x.Id == tenantId)
            .Select(x => x.SpaceId)
            .SingleOrDefaultAsync();

    public Task<Guid?> GetDimInstanceId(Guid tenantId)
        => _context.Tenants
            .Where(x => x.Id == tenantId)
            .Select(x => x.DimInstanceId)
            .SingleOrDefaultAsync();

    public Task<(string bpn, string? DownloadUrl, string? Did, Guid? DimInstanceId)> GetCallbackData(Guid tenantId)
        => _context.Tenants
            .Where(x => x.Id == tenantId)
            .Select(x => new ValueTuple<string, string?, string?, Guid?>(x.Bpn, x.DidDownloadUrl, x.Did, x.DimInstanceId))
            .SingleOrDefaultAsync();

    public Task<(Guid? DimInstanceId, string HostingUrl, bool IsIssuer)> GetDimInstanceIdAndHostingUrl(Guid tenantId)
        => _context.Tenants
            .Where(x => x.Id == tenantId)
            .Select(x => new ValueTuple<Guid?, string, bool>(x.DimInstanceId, x.DidDocumentLocation, x.IsIssuer))
            .SingleOrDefaultAsync();

    public Task<(string? ApplicationId, Guid? CompanyId, Guid? DimInstanceId, bool IsIssuer)> GetApplicationAndCompanyId(Guid tenantId) =>
        _context.Tenants
            .Where(x => x.Id == tenantId)
            .Select(x => new ValueTuple<string?, Guid?, Guid?, bool>(
                x.ApplicationId,
                x.CompanyId,
                x.DimInstanceId,
                x.IsIssuer))
            .SingleOrDefaultAsync();

    public Task<(bool Exists, Guid? CompanyId, Guid? InstanceId)> GetCompanyAndInstanceIdForBpn(string bpn) =>
        _context.Tenants.Where(x => x.Bpn == bpn)
            .Select(x => new ValueTuple<bool, Guid?, Guid?>(true, x.CompanyId, x.DimInstanceId))
            .SingleOrDefaultAsync();
}
