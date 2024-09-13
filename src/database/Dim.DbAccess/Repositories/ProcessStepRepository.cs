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
using ProcessTypeId = Dim.Entities.Enums.ProcessTypeId;

namespace Dim.DbAccess.Repositories;

public class ProcessStepRepository(DimDbContext dbContext)
    : IProcessStepRepository
{
    public Process CreateProcess(ProcessTypeId processTypeId) =>
        dbContext.Add(new Process(Guid.NewGuid(), processTypeId, Guid.NewGuid())).Entity;

    public ProcessStep CreateProcessStep(ProcessStepTypeId processStepTypeId, ProcessStepStatusId processStepStatusId, Guid processId) =>
        dbContext.Add(new ProcessStep(Guid.NewGuid(), processStepTypeId, processStepStatusId, processId, DateTimeOffset.UtcNow)).Entity;

    public IEnumerable<ProcessStep> CreateProcessStepRange(IEnumerable<(ProcessStepTypeId ProcessStepTypeId, ProcessStepStatusId ProcessStepStatusId, Guid ProcessId)> processStepTypeStatus)
    {
        var processSteps = processStepTypeStatus.Select(x => new ProcessStep(Guid.NewGuid(), x.ProcessStepTypeId, x.ProcessStepStatusId, x.ProcessId, DateTimeOffset.UtcNow)).ToList();
        dbContext.AddRange(processSteps);
        return processSteps;
    }

    public void AttachAndModifyProcessStep(Guid processStepId, Action<ProcessStep>? initialize, Action<ProcessStep> modify)
    {
        var step = new ProcessStep(processStepId, default, default, Guid.Empty, default);
        initialize?.Invoke(step);
        dbContext.Attach(step);
        step.DateLastChanged = DateTimeOffset.UtcNow;
        modify(step);
    }

    public void AttachAndModifyProcessSteps(IEnumerable<(Guid ProcessStepId, Action<ProcessStep>? Initialize, Action<ProcessStep> Modify)> processStepIdsInitializeModifyData)
    {
        var stepModifyData = processStepIdsInitializeModifyData.Select(data =>
            {
                var step = new ProcessStep(data.ProcessStepId, default, default, Guid.Empty, default);
                data.Initialize?.Invoke(step);
                return (Step: step, data.Modify);
            }).ToList();
        dbContext.AttachRange(stepModifyData.Select(data => data.Step));
        stepModifyData.ForEach(data =>
            {
                data.Step.DateLastChanged = DateTimeOffset.UtcNow;
                data.Modify(data.Step);
            });
    }

    public IAsyncEnumerable<Process> GetActiveProcesses(IEnumerable<ProcessTypeId> processTypeIds, IEnumerable<ProcessStepTypeId> processStepTypeIds, DateTimeOffset lockExpiryDate) =>
        dbContext.Processes
            .AsNoTracking()
            .Where(process =>
                processTypeIds.Contains(process.ProcessTypeId) &&
                process.ProcessSteps.Any(step => processStepTypeIds.Contains(step.ProcessStepTypeId) && step.ProcessStepStatusId == ProcessStepStatusId.TODO) &&
                (process.LockExpiryDate == null || process.LockExpiryDate < lockExpiryDate))
            .AsAsyncEnumerable();

    public IAsyncEnumerable<(Guid ProcessStepId, ProcessStepTypeId ProcessStepTypeId)> GetProcessStepData(Guid processId) =>
        dbContext.ProcessSteps
            .AsNoTracking()
            .Where(step =>
                step.ProcessId == processId &&
                step.ProcessStepStatusId == ProcessStepStatusId.TODO)
            .OrderBy(step => step.ProcessStepTypeId)
            .Select(step =>
                new ValueTuple<Guid, ProcessStepTypeId>(
                    step.Id,
                    step.ProcessStepTypeId))
            .AsAsyncEnumerable();

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

    public Task<(bool ProcessExists, VerifyProcessData ProcessData)> IsValidProcess(Guid processId, ProcessTypeId processTypeId, IEnumerable<ProcessStepTypeId> processStepTypeIds) =>
        dbContext.Processes
            .AsNoTracking()
            .Where(x => x.Id == processId && x.ProcessTypeId == processTypeId)
            .Select(x => new ValueTuple<bool, VerifyProcessData>(
                true,
                new VerifyProcessData(
                    x,
                    x.ProcessSteps
                        .Where(step =>
                            processStepTypeIds.Contains(step.ProcessStepTypeId) &&
                            step.ProcessStepStatusId == ProcessStepStatusId.TODO))
            ))
            .SingleOrDefaultAsync();
}
