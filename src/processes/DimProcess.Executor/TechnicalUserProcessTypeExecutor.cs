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
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Worker.Library;
using System.Collections.Immutable;

namespace DimProcess.Executor;

public class TechnicalUserProcessTypeExecutor(
    IDimRepositories dimRepositories,
    ITechnicalUserProcessHandler technicalUserProcessHandler)
    : IProcessTypeExecutor<ProcessTypeId, ProcessStepTypeId>
{
    private readonly IEnumerable<ProcessStepTypeId> _executableProcessSteps = ImmutableArray.Create(
        ProcessStepTypeId.CREATE_TECHNICAL_USER,
        ProcessStepTypeId.GET_TECHNICAL_USER_DATA,
        ProcessStepTypeId.GET_TECHNICAL_USER_SERVICE_KEY,
        ProcessStepTypeId.SEND_TECHNICAL_USER_CREATION_CALLBACK,
        ProcessStepTypeId.DELETE_TECHNICAL_USER,
        ProcessStepTypeId.SEND_TECHNICAL_USER_DELETION_CALLBACK);

    private Guid _technicalUserId;

    public ProcessTypeId GetProcessTypeId() => ProcessTypeId.TECHNICAL_USER;
    public bool IsExecutableStepTypeId(ProcessStepTypeId processStepTypeId) => _executableProcessSteps.Contains(processStepTypeId);
    public IEnumerable<ProcessStepTypeId> GetExecutableStepTypeIds() => _executableProcessSteps;
    public ValueTask<bool> IsLockRequested(ProcessStepTypeId processStepTypeId) => new(false);

    public async ValueTask<IProcessTypeExecutor<ProcessTypeId, ProcessStepTypeId>.InitializationResult> InitializeProcess(Guid processId, IEnumerable<ProcessStepTypeId> processStepTypeIds)
    {
        var (exists, technicalUserId) = await dimRepositories.GetInstance<ITechnicalUserRepository>().GetTenantDataForTechnicalUserProcessId(processId).ConfigureAwait(ConfigureAwaitOptions.None);
        if (!exists)
        {
            throw new NotFoundException($"process {processId} does not exist or is not associated with an technical user");
        }

        _technicalUserId = technicalUserId;
        return new IProcessTypeExecutor<ProcessTypeId, ProcessStepTypeId>.InitializationResult(false, null);
    }

    public async ValueTask<IProcessTypeExecutor<ProcessTypeId, ProcessStepTypeId>.StepExecutionResult> ExecuteProcessStep(ProcessStepTypeId processStepTypeId, IEnumerable<ProcessStepTypeId> processStepTypeIds, CancellationToken cancellationToken)
    {
        if (_technicalUserId == Guid.Empty)
        {
            throw new UnexpectedConditionException("technicalUserId should never be empty here");
        }

        IEnumerable<ProcessStepTypeId>? nextStepTypeIds;
        ProcessStepStatusId stepStatusId;
        bool modified;
        string? processMessage;

        try
        {
            (nextStepTypeIds, stepStatusId, modified, processMessage) = processStepTypeId switch
            {
                ProcessStepTypeId.CREATE_TECHNICAL_USER => await technicalUserProcessHandler.CreateServiceInstanceBindings(_technicalUserId, cancellationToken)
                    .ConfigureAwait(ConfigureAwaitOptions.None),
                ProcessStepTypeId.GET_TECHNICAL_USER_DATA => await technicalUserProcessHandler.GetTechnicalUserData(_technicalUserId, cancellationToken)
                    .ConfigureAwait(ConfigureAwaitOptions.None),
                ProcessStepTypeId.GET_TECHNICAL_USER_SERVICE_KEY => await technicalUserProcessHandler.GetTechnicalUserServiceKey(_technicalUserId, cancellationToken)
                    .ConfigureAwait(ConfigureAwaitOptions.None),
                ProcessStepTypeId.SEND_TECHNICAL_USER_CREATION_CALLBACK => await technicalUserProcessHandler.SendCreateCallback(_technicalUserId, cancellationToken)
                    .ConfigureAwait(ConfigureAwaitOptions.None),
                ProcessStepTypeId.DELETE_TECHNICAL_USER => await technicalUserProcessHandler.DeleteServiceInstanceBindings(_technicalUserId, cancellationToken)
                    .ConfigureAwait(ConfigureAwaitOptions.None),
                ProcessStepTypeId.SEND_TECHNICAL_USER_DELETION_CALLBACK => await technicalUserProcessHandler.SendDeleteCallback(_technicalUserId, cancellationToken)
                    .ConfigureAwait(ConfigureAwaitOptions.None),
                _ => (null, ProcessStepStatusId.TODO, false, null)
            };
        }
        catch (Exception ex) when (ex is not SystemException)
        {
            (stepStatusId, processMessage, nextStepTypeIds) = ProcessError(ex, processStepTypeId);
            modified = true;
        }

        return new IProcessTypeExecutor<ProcessTypeId, ProcessStepTypeId>.StepExecutionResult(modified, stepStatusId, nextStepTypeIds, null, processMessage);
    }

    private static (ProcessStepStatusId StatusId, string? ProcessMessage, IEnumerable<ProcessStepTypeId>? nextSteps) ProcessError(Exception ex, ProcessStepTypeId processStepTypeId)
    {
        return ex switch
        {
            ServiceException { IsRecoverable: true } => (ProcessStepStatusId.TODO, ex.Message, null),
            _ => (ProcessStepStatusId.FAILED, ex.Message, Enumerable.Repeat(processStepTypeId.GetRetriggerStep(ProcessTypeId.TECHNICAL_USER), 1))
        };
    }
}
