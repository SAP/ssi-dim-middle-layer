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
using Dim.Entities.Enums;

namespace Dim.DbAccess.Repositories;

/// <summary>
/// Repository for accessing and creating processSteps on persistence layer.
/// </summary>
public interface IProcessStepRepository
{
    Process CreateProcess(ProcessTypeId processTypeId);
    ProcessStep CreateProcessStep(ProcessStepTypeId processStepTypeId, ProcessStepStatusId processStepStatusId, Guid processId);
    IEnumerable<ProcessStep> CreateProcessStepRange(IEnumerable<(ProcessStepTypeId ProcessStepTypeId, ProcessStepStatusId ProcessStepStatusId, Guid ProcessId)> processStepTypeStatus);
    void AttachAndModifyProcessStep(Guid processStepId, Action<ProcessStep>? initialize, Action<ProcessStep> modify);
    void AttachAndModifyProcessSteps(IEnumerable<(Guid ProcessStepId, Action<ProcessStep>? Initialize, Action<ProcessStep> Modify)> processStepIdsInitializeModifyData);
    IAsyncEnumerable<Process> GetActiveProcesses(IEnumerable<ProcessTypeId> processTypeIds, IEnumerable<ProcessStepTypeId> processStepTypeIds, DateTimeOffset lockExpiryDate);
    IAsyncEnumerable<(Guid ProcessStepId, ProcessStepTypeId ProcessStepTypeId)> GetProcessStepData(Guid processId);
    Task<ProcessData?> GetWalletProcessForTenant(string bpn, string companyName);
    Task<ProcessData?> GetTechnicalUserProcess(string bpn, string companyName, string technicalUserName);
    Task<(bool ProcessExists, VerifyProcessData ProcessData)> IsValidProcess(Guid processId, ProcessTypeId processTypeId, IEnumerable<ProcessStepTypeId> processSetpTypeIds);
}
