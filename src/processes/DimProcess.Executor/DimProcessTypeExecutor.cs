/********************************************************************************
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

public class DimProcessTypeExecutor : IProcessTypeExecutor
{
    private readonly IDimRepositories _dimRepositories;
    private readonly IDimProcessHandler _dimProcessHandler;

    private readonly IEnumerable<ProcessStepTypeId> _executableProcessSteps = ImmutableArray.Create(
        ProcessStepTypeId.CREATE_SUBACCOUNT,
        ProcessStepTypeId.CREATE_SERVICEMANAGER_BINDINGS,
        ProcessStepTypeId.ASSIGN_ENTITLEMENTS,
        ProcessStepTypeId.CREATE_SERVICE_INSTANCE,
        ProcessStepTypeId.CREATE_SERVICE_BINDING,
        ProcessStepTypeId.SUBSCRIBE_APPLICATION,
        ProcessStepTypeId.CREATE_CLOUD_FOUNDRY_ENVIRONMENT,
        ProcessStepTypeId.CREATE_CLOUD_FOUNDRY_SPACE,
        ProcessStepTypeId.ADD_SPACE_MANAGER_ROLE,
        ProcessStepTypeId.ADD_SPACE_DEVELOPER_ROLE,
        ProcessStepTypeId.CREATE_DIM_SERVICE_INSTANCE,
        ProcessStepTypeId.CREATE_SERVICE_INSTANCE_BINDING,
        ProcessStepTypeId.GET_DIM_DETAILS,
        ProcessStepTypeId.CREATE_APPLICATION,
        ProcessStepTypeId.CREATE_COMPANY_IDENTITY,
        ProcessStepTypeId.CREATE_STATUS_LIST,
        ProcessStepTypeId.ASSIGN_COMPANY_APPLICATION,
        ProcessStepTypeId.SEND_CALLBACK);

    private Guid _tenantId;
    private string? _tenantName;

    public DimProcessTypeExecutor(
        IDimRepositories dimRepositories,
        IDimProcessHandler dimProcessHandler)
    {
        _dimRepositories = dimRepositories;
        _dimProcessHandler = dimProcessHandler;
    }

    public ProcessTypeId GetProcessTypeId() => ProcessTypeId.SETUP_DIM;
    public bool IsExecutableStepTypeId(ProcessStepTypeId processStepTypeId) => _executableProcessSteps.Contains(processStepTypeId);
    public IEnumerable<ProcessStepTypeId> GetExecutableStepTypeIds() => _executableProcessSteps;
    public ValueTask<bool> IsLockRequested(ProcessStepTypeId processStepTypeId) => new(false);

    public async ValueTask<IProcessTypeExecutor.InitializationResult> InitializeProcess(Guid processId, IEnumerable<ProcessStepTypeId> processStepTypeIds)
    {
        var (exists, tenantId, companyName, bpn) = await _dimRepositories.GetInstance<ITenantRepository>().GetTenantDataForProcessId(processId).ConfigureAwait(false);
        if (!exists)
        {
            throw new NotFoundException($"process {processId} does not exist or is not associated with an tenant");
        }

        _tenantId = tenantId;
        _tenantName = $"{bpn}_{companyName}";
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
                ProcessStepTypeId.CREATE_SUBACCOUNT => await _dimProcessHandler.CreateSubaccount(_tenantId, _tenantName, cancellationToken)
                    .ConfigureAwait(false),
                ProcessStepTypeId.CREATE_SERVICEMANAGER_BINDINGS => await _dimProcessHandler.CreateServiceManagerBindings(_tenantId, cancellationToken)
                    .ConfigureAwait(false),
                ProcessStepTypeId.ASSIGN_ENTITLEMENTS => await _dimProcessHandler.AssignEntitlements(_tenantId, cancellationToken)
                    .ConfigureAwait(false),
                ProcessStepTypeId.CREATE_SERVICE_INSTANCE => await _dimProcessHandler.CreateServiceInstance(_tenantId, cancellationToken)
                    .ConfigureAwait(false),
                ProcessStepTypeId.CREATE_SERVICE_BINDING => await _dimProcessHandler.CreateServiceBindings(_tenantId, cancellationToken)
                    .ConfigureAwait(false),
                ProcessStepTypeId.SUBSCRIBE_APPLICATION => await _dimProcessHandler.SubscribeApplication(_tenantId, cancellationToken)
                    .ConfigureAwait(false),
                ProcessStepTypeId.CREATE_CLOUD_FOUNDRY_ENVIRONMENT => await _dimProcessHandler.CreateCloudFoundryEnvironment(_tenantId, _tenantName, cancellationToken)
                    .ConfigureAwait(false),
                ProcessStepTypeId.CREATE_CLOUD_FOUNDRY_SPACE => await _dimProcessHandler.CreateCloudFoundrySpace(_tenantId, _tenantName, cancellationToken)
                    .ConfigureAwait(false),
                ProcessStepTypeId.ADD_SPACE_MANAGER_ROLE => await _dimProcessHandler.AddSpaceManagerRole(_tenantId, cancellationToken)
                    .ConfigureAwait(false),
                ProcessStepTypeId.ADD_SPACE_DEVELOPER_ROLE => await _dimProcessHandler.AddSpaceDeveloperRole(_tenantId, cancellationToken)
                    .ConfigureAwait(false),
                ProcessStepTypeId.CREATE_DIM_SERVICE_INSTANCE => await _dimProcessHandler.CreateDimServiceInstance(_tenantName, _tenantId, cancellationToken)
                    .ConfigureAwait(false),
                ProcessStepTypeId.CREATE_SERVICE_INSTANCE_BINDING => await _dimProcessHandler.CreateServiceInstanceBindings(_tenantName, _tenantId, cancellationToken)
                    .ConfigureAwait(false),
                ProcessStepTypeId.GET_DIM_DETAILS => await _dimProcessHandler.GetDimDetails(_tenantName, _tenantId, cancellationToken)
                    .ConfigureAwait(false),
                ProcessStepTypeId.CREATE_APPLICATION => await _dimProcessHandler.CreateApplication(_tenantName, _tenantId, cancellationToken)
                    .ConfigureAwait(false),
                ProcessStepTypeId.CREATE_COMPANY_IDENTITY => await _dimProcessHandler.CreateCompanyIdentity(_tenantId, _tenantName, cancellationToken)
                    .ConfigureAwait(false),
                ProcessStepTypeId.ASSIGN_COMPANY_APPLICATION => await _dimProcessHandler.AssignCompanyApplication(_tenantId, cancellationToken)
                    .ConfigureAwait(false),
                ProcessStepTypeId.CREATE_STATUS_LIST => await _dimProcessHandler.CreateStatusList(_tenantId, cancellationToken)
                    .ConfigureAwait(false),
                ProcessStepTypeId.SEND_CALLBACK => await _dimProcessHandler.SendCallback(_tenantId, cancellationToken)
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
