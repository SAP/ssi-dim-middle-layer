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

using Dim.DbAccess;
using Dim.DbAccess.Repositories;
using Dim.Entities.Enums;
using Dim.Entities.Extensions;
using DimProcess.Library;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Processes.Worker.Library;
using System.Collections.Immutable;

namespace DimProcess.Executor;

public class DimProcessTypeExecutor(
    IDimRepositories dimRepositories,
    IDimProcessHandler dimProcessHandler)
    : IProcessTypeExecutor
{
    private readonly IEnumerable<ProcessStepTypeId> _executableProcessSteps = ImmutableArray.Create(
        ProcessStepTypeId.CREATE_WALLET,
        ProcessStepTypeId.CHECK_OPERATION,
        ProcessStepTypeId.GET_COMPANY,
        ProcessStepTypeId.GET_DID_DOCUMENT,
        ProcessStepTypeId.CREATE_STATUS_LIST,
        ProcessStepTypeId.SEND_CALLBACK);

    private Guid _tenantId;
    private string? _tenantName;

    public ProcessTypeId GetProcessTypeId() => ProcessTypeId.SETUP_DIM;
    public bool IsExecutableStepTypeId(ProcessStepTypeId processStepTypeId) => _executableProcessSteps.Contains(processStepTypeId);
    public IEnumerable<ProcessStepTypeId> GetExecutableStepTypeIds() => _executableProcessSteps;
    public ValueTask<bool> IsLockRequested(ProcessStepTypeId processStepTypeId) => new(false);

    public async ValueTask<IProcessTypeExecutor.InitializationResult> InitializeProcess(Guid processId, IEnumerable<ProcessStepTypeId> processStepTypeIds)
    {
        var (exists, tenantId, companyName, bpn) = await dimRepositories.GetInstance<ITenantRepository>().GetTenantDataForProcessId(processId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (!exists)
        {
            throw new NotFoundException($"process {processId} does not exist or is not associated with an tenant");
        }

        _tenantId = tenantId;
        _tenantName = $"{bpn}{companyName}";
        return new IProcessTypeExecutor.InitializationResult(false, null);
    }

    public async ValueTask<IProcessTypeExecutor.StepExecutionResult> ExecuteProcessStep(ProcessStepTypeId processStepTypeId, IEnumerable<ProcessStepTypeId> processStepTypeIds, CancellationToken cancellationToken)
    {
        if (_tenantId == Guid.Empty || _tenantName is null)
        {
            throw new UnexpectedConditionException("tenantId and tenantName should never be empty here");
        }

        IEnumerable<ProcessStepTypeId>? nextStepTypeIds;
        ProcessStepStatusId stepStatusId;
        bool modified;
        string? processMessage;

        try
        {
            (nextStepTypeIds, stepStatusId, modified, processMessage) = processStepTypeId switch
            {
                ProcessStepTypeId.CREATE_WALLET => await dimProcessHandler.CreateWallet(_tenantId, _tenantName, cancellationToken)
                    .ConfigureAwait(ConfigureAwaitOptions.None),
                ProcessStepTypeId.CHECK_OPERATION => await dimProcessHandler.CheckOperation(_tenantId, cancellationToken)
                    .ConfigureAwait(ConfigureAwaitOptions.None),
                ProcessStepTypeId.GET_COMPANY => await dimProcessHandler.GetCompany(_tenantId, _tenantName, cancellationToken)
                    .ConfigureAwait(ConfigureAwaitOptions.None),
                ProcessStepTypeId.GET_DID_DOCUMENT => await dimProcessHandler.GetDidDocument(_tenantId, cancellationToken)
                    .ConfigureAwait(ConfigureAwaitOptions.None),
                ProcessStepTypeId.CREATE_STATUS_LIST => await dimProcessHandler.CreateStatusList(_tenantId, cancellationToken)
                    .ConfigureAwait(ConfigureAwaitOptions.None),
                ProcessStepTypeId.SEND_CALLBACK => await dimProcessHandler.SendCallback(_tenantId, cancellationToken)
                    .ConfigureAwait(ConfigureAwaitOptions.None),
                _ => (null, ProcessStepStatusId.TODO, false, null)
            };
        }
        catch (Exception ex) when (ex is not SystemException)
        {
            (stepStatusId, processMessage, nextStepTypeIds) = ProcessError(ex, processStepTypeId);
            modified = true;
        }

        return new IProcessTypeExecutor.StepExecutionResult(modified, stepStatusId, nextStepTypeIds, null, processMessage);
    }

    private static (ProcessStepStatusId StatusId, string? ProcessMessage, IEnumerable<ProcessStepTypeId>? nextSteps) ProcessError(Exception ex, ProcessStepTypeId processStepTypeId)
    {
        return ex switch
        {
            ServiceException { IsRecoverable: true } => (ProcessStepStatusId.TODO, ex.Message, null),
            _ => (ProcessStepStatusId.FAILED, ex.Message, Enumerable.Repeat(processStepTypeId.GetRetriggerStep(ProcessTypeId.SETUP_DIM), 1))
        };
    }
}
