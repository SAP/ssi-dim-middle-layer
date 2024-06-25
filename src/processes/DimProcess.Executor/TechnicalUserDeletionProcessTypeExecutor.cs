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
using DimProcess.Library;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Processes.Worker.Library;
using System.Collections.Immutable;

namespace DimProcess.Executor;

public class TechnicalUserDeletionProcessTypeExecutor(
    IDimRepositories dimRepositories,
    ITechnicalUserDeletionProcessHandler technicalUserDeletionProcessHandler)
    : IProcessTypeExecutor
{
    private readonly IEnumerable<ProcessStepTypeId> _executableProcessSteps = ImmutableArray.Create(
        ProcessStepTypeId.DELETE_TECHNICAL_USER,
        ProcessStepTypeId.SEND_TECHNICAL_USER_DELETION_CALLBACK);

    private Guid _technicalUserId;
    private string? _tenantName;

    public ProcessTypeId GetProcessTypeId() => ProcessTypeId.DELETE_TECHNICAL_USER;
    public bool IsExecutableStepTypeId(ProcessStepTypeId processStepTypeId) => _executableProcessSteps.Contains(processStepTypeId);
    public IEnumerable<ProcessStepTypeId> GetExecutableStepTypeIds() => _executableProcessSteps;
    public ValueTask<bool> IsLockRequested(ProcessStepTypeId processStepTypeId) => new(false);

    public async ValueTask<IProcessTypeExecutor.InitializationResult> InitializeProcess(Guid processId, IEnumerable<ProcessStepTypeId> processStepTypeIds)
    {
        var (exists, technicalUserId, companyName, bpn) = await dimRepositories.GetInstance<ITenantRepository>().GetTenantDataForTechnicalUserProcessId(processId).ConfigureAwait(false);
        if (!exists)
        {
            throw new NotFoundException($"process {processId} does not exist or is not associated with an technical user");
        }

        _technicalUserId = technicalUserId;
        _tenantName = $"{bpn}_{companyName}";
        return new IProcessTypeExecutor.InitializationResult(false, null);
    }

    public async ValueTask<IProcessTypeExecutor.StepExecutionResult> ExecuteProcessStep(ProcessStepTypeId processStepTypeId, IEnumerable<ProcessStepTypeId> processStepTypeIds, CancellationToken cancellationToken)
    {
        if (_technicalUserId == Guid.Empty || _tenantName is null)
        {
            throw new UnexpectedConditionException("technicalUserId and tenantName should never be empty here");
        }

        IEnumerable<ProcessStepTypeId>? nextStepTypeIds;
        ProcessStepStatusId stepStatusId;
        bool modified;
        string? processMessage;

        try
        {
            (nextStepTypeIds, stepStatusId, modified, processMessage) = processStepTypeId switch
            {
                ProcessStepTypeId.DELETE_TECHNICAL_USER => await technicalUserDeletionProcessHandler.DeleteServiceInstanceBindings(_tenantName, _technicalUserId, cancellationToken)
                    .ConfigureAwait(false),
                ProcessStepTypeId.SEND_TECHNICAL_USER_CREATION_CALLBACK => await technicalUserDeletionProcessHandler.SendCallback(_technicalUserId, cancellationToken)
                    .ConfigureAwait(false),
                _ => (null, ProcessStepStatusId.TODO, false, null)
            };
        }
        catch (Exception ex) when (ex is not SystemException)
        {
            (stepStatusId, processMessage, nextStepTypeIds) = ProcessError(ex);
            modified = true;
        }

        return new IProcessTypeExecutor.StepExecutionResult(modified, stepStatusId, nextStepTypeIds, null, processMessage);
    }

    private static (ProcessStepStatusId StatusId, string? ProcessMessage, IEnumerable<ProcessStepTypeId>? nextSteps) ProcessError(Exception ex)
    {
        return ex switch
        {
            ServiceException { IsRecoverable: true } => (ProcessStepStatusId.TODO, ex.Message, null),
            _ => (ProcessStepStatusId.FAILED, ex.Message, null)
        };
    }
}
