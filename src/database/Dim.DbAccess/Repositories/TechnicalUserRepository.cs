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

public class TechnicalUserRepository(DimDbContext dbContext)
    : ITechnicalUserRepository
{
    public void CreateTenantTechnicalUser(Guid tenantId, string technicalUserName, Guid externalId, Guid processId) =>
        dbContext.TechnicalUsers.Add(new TechnicalUser(Guid.NewGuid(), tenantId, externalId, technicalUserName, processId));

    public void AttachAndModifyTechnicalUser(Guid technicalUserId, Action<TechnicalUser>? initialize, Action<TechnicalUser> modify)
    {
        var technicalUser = new TechnicalUser(technicalUserId, Guid.Empty, Guid.Empty, null!, Guid.Empty);
        initialize?.Invoke(technicalUser);
        dbContext.TechnicalUsers.Attach(technicalUser);
        modify(technicalUser);
    }

    public Task<(bool Exists, Guid TechnicalUserId)> GetTenantDataForTechnicalUserProcessId(Guid processId) =>
        dbContext.TechnicalUsers
            .Where(x => x.ProcessId == processId)
            .Select(x => new ValueTuple<bool, Guid>(true, x.Id))
            .SingleOrDefaultAsync();

    public Task<(Guid ExternalId, WalletData WalletData)> GetTechnicalUserCallbackData(Guid technicalUserId) =>
        dbContext.TechnicalUsers
            .Where(x => x.Id == technicalUserId)
            .Select(x => new ValueTuple<Guid, WalletData>(
                x.ExternalId,
                new WalletData(
                    x.TokenAddress,
                    x.ClientId,
                    x.ClientSecret,
                    x.InitializationVector,
                    x.EncryptionMode)))
            .SingleOrDefaultAsync();

    public Task<(bool Exists, Guid TechnicalUserId, Guid ProcessId)> GetTechnicalUserForBpn(string bpn, string technicalUserName) =>
        dbContext.TechnicalUsers
            .Where(x => x.TechnicalUserName == technicalUserName && x.Tenant!.Bpn == bpn)
            .Select(x => new ValueTuple<bool, Guid, Guid>(true, x.Id, x.ProcessId))
            .SingleOrDefaultAsync();

    public void RemoveTechnicalUser(Guid technicalUserId) =>
        dbContext.TechnicalUsers
            .Remove(new TechnicalUser(technicalUserId, Guid.Empty, Guid.Empty, null!, Guid.Empty));

    public Task<(Guid? WalletId, string TechnicalUserName)> GetTechnicalUserNameAndWalletId(Guid technicalUserId) =>
        dbContext.TechnicalUsers
            .Where(x => x.Id == technicalUserId)
            .Select(x => new ValueTuple<Guid?, string>(x.Tenant!.WalletId, x.TechnicalUserName))
            .SingleOrDefaultAsync();

    public Task<Guid?> GetOperationIdForTechnicalUser(Guid technicalUserId) =>
        dbContext.TechnicalUsers
            .Where(x => x.Id == technicalUserId)
            .Select(x => x.OperationId)
            .SingleOrDefaultAsync();

    public Task<(Guid? OperationId, Guid ExternalId)> GetOperationAndExternalIdForTechnicalUser(Guid technicalUserId) =>
        dbContext.TechnicalUsers
            .Where(x => x.Id == technicalUserId)
            .Select(x => new ValueTuple<Guid?, Guid>(x.OperationId, x.ExternalId))
            .SingleOrDefaultAsync();

    public Task<(Guid? ServiceKeyId, Guid? WalletId)> GetServiceKeyAndWalletId(Guid technicalUserId) =>
        dbContext.TechnicalUsers
            .Where(x => x.Id == technicalUserId)
            .Select(x => new ValueTuple<Guid?, Guid?>(
                x.ServiceKeyId,
                x.Tenant!.WalletId))
            .SingleOrDefaultAsync();

    public Task<ProcessData?> GetTechnicalUserProcess(string bpn, string companyName, string technicalUserName) =>
        dbContext.TechnicalUsers
            .Where(x =>
                x.TechnicalUserName == technicalUserName &&
                x.Tenant!.Bpn == bpn &&
                x.Tenant!.CompanyName == companyName &&
                x.Process!.ProcessTypeId == ProcessTypeId.TECHNICAL_USER)
            .Select(x => new ProcessData(
                x.ProcessId,
                x.Process!.ProcessSteps.Select(ps => new ProcessStepData(
                    ps.ProcessStepTypeId,
                    ps.ProcessStepStatusId))))
            .SingleOrDefaultAsync();

    public Task<(Guid? WalletId, string TechnicalUserName)> GetWalletIdAndNameForTechnicalUser(Guid technicalUserId) =>
        dbContext.TechnicalUsers
            .Where(x => x.Id == technicalUserId)
            .Select(x => new ValueTuple<Guid?, string>(x.Tenant!.WalletId, x.TechnicalUserName))
            .SingleOrDefaultAsync();
}
