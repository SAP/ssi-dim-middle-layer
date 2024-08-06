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

using Dim.Entities.Enums;

namespace Dim.Entities.Extensions;

public static class ProcessStepTypeIdExtensions
{
    public static ProcessStepTypeId GetWalletRetriggerStep(this ProcessStepTypeId processStepTypeId) =>
        processStepTypeId switch
        {
            ProcessStepTypeId.SEND_CALLBACK => ProcessStepTypeId.RETRIGGER_SEND_CALLBACK,
            ProcessStepTypeId.CREATE_SUBACCOUNT => ProcessStepTypeId.RETRIGGER_CREATE_SUBACCOUNT,
            ProcessStepTypeId.CREATE_SERVICEMANAGER_BINDINGS => ProcessStepTypeId.RETRIGGER_CREATE_SERVICEMANAGER_BINDINGS,
            ProcessStepTypeId.ASSIGN_ENTITLEMENTS => ProcessStepTypeId.RETRIGGER_ASSIGN_ENTITLEMENTS,
            ProcessStepTypeId.CREATE_SERVICE_INSTANCE => ProcessStepTypeId.RETRIGGER_CREATE_SERVICE_INSTANCE,
            ProcessStepTypeId.CREATE_SERVICE_BINDING => ProcessStepTypeId.RETRIGGER_CREATE_SERVICE_BINDING,
            ProcessStepTypeId.SUBSCRIBE_APPLICATION => ProcessStepTypeId.RETRIGGER_SUBSCRIBE_APPLICATION,
            ProcessStepTypeId.CREATE_CLOUD_FOUNDRY_ENVIRONMENT => ProcessStepTypeId.RETRIGGER_CREATE_CLOUD_FOUNDRY_ENVIRONMENT,
            ProcessStepTypeId.CREATE_CLOUD_FOUNDRY_SPACE => ProcessStepTypeId.RETRIGGER_CREATE_CLOUD_FOUNDRY_SPACE,
            ProcessStepTypeId.ADD_SPACE_MANAGER_ROLE => ProcessStepTypeId.RETRIGGER_ADD_SPACE_MANAGER_ROLE,
            ProcessStepTypeId.ADD_SPACE_DEVELOPER_ROLE => ProcessStepTypeId.RETRIGGER_ADD_SPACE_DEVELOPER_ROLE,
            ProcessStepTypeId.CREATE_DIM_SERVICE_INSTANCE => ProcessStepTypeId.RETRIGGER_CREATE_DIM_SERVICE_INSTANCE,
            ProcessStepTypeId.CREATE_SERVICE_INSTANCE_BINDING => ProcessStepTypeId.RETRIGGER_CREATE_SERVICE_INSTANCE_BINDING,
            ProcessStepTypeId.GET_DIM_DETAILS => ProcessStepTypeId.RETRIGGER_GET_DIM_DETAILS,
            ProcessStepTypeId.CREATE_APPLICATION => ProcessStepTypeId.RETRIGGER_CREATE_APPLICATION,
            ProcessStepTypeId.CREATE_COMPANY_IDENTITY => ProcessStepTypeId.RETRIGGER_CREATE_COMPANY_IDENTITY,
            ProcessStepTypeId.ASSIGN_COMPANY_APPLICATION => ProcessStepTypeId.RETRIGGER_ASSIGN_COMPANY_APPLICATION,
            ProcessStepTypeId.CREATE_STATUS_LIST => ProcessStepTypeId.RETRIGGER_CREATE_STATUS_LIST,
            _ => throw new ArgumentOutOfRangeException(nameof(processStepTypeId), processStepTypeId, null)
        };

    public static ProcessStepTypeId GetWalletStepForRetrigger(this ProcessStepTypeId processStepTypeId) =>
        processStepTypeId switch
        {
            ProcessStepTypeId.RETRIGGER_SEND_CALLBACK => ProcessStepTypeId.SEND_CALLBACK,
            ProcessStepTypeId.RETRIGGER_CREATE_SUBACCOUNT => ProcessStepTypeId.CREATE_SUBACCOUNT,
            ProcessStepTypeId.RETRIGGER_CREATE_SERVICEMANAGER_BINDINGS => ProcessStepTypeId.CREATE_SERVICEMANAGER_BINDINGS,
            ProcessStepTypeId.RETRIGGER_ASSIGN_ENTITLEMENTS => ProcessStepTypeId.ASSIGN_ENTITLEMENTS,
            ProcessStepTypeId.RETRIGGER_CREATE_SERVICE_INSTANCE => ProcessStepTypeId.CREATE_SERVICE_INSTANCE,
            ProcessStepTypeId.RETRIGGER_CREATE_SERVICE_BINDING => ProcessStepTypeId.CREATE_SERVICE_BINDING,
            ProcessStepTypeId.RETRIGGER_SUBSCRIBE_APPLICATION => ProcessStepTypeId.SUBSCRIBE_APPLICATION,
            ProcessStepTypeId.RETRIGGER_CREATE_CLOUD_FOUNDRY_ENVIRONMENT => ProcessStepTypeId.CREATE_CLOUD_FOUNDRY_ENVIRONMENT,
            ProcessStepTypeId.RETRIGGER_CREATE_CLOUD_FOUNDRY_SPACE => ProcessStepTypeId.CREATE_CLOUD_FOUNDRY_SPACE,
            ProcessStepTypeId.RETRIGGER_ADD_SPACE_MANAGER_ROLE => ProcessStepTypeId.ADD_SPACE_MANAGER_ROLE,
            ProcessStepTypeId.RETRIGGER_ADD_SPACE_DEVELOPER_ROLE => ProcessStepTypeId.ADD_SPACE_DEVELOPER_ROLE,
            ProcessStepTypeId.RETRIGGER_CREATE_DIM_SERVICE_INSTANCE => ProcessStepTypeId.CREATE_DIM_SERVICE_INSTANCE,
            ProcessStepTypeId.RETRIGGER_CREATE_SERVICE_INSTANCE_BINDING => ProcessStepTypeId.CREATE_SERVICE_INSTANCE_BINDING,
            ProcessStepTypeId.RETRIGGER_GET_DIM_DETAILS => ProcessStepTypeId.GET_DIM_DETAILS,
            ProcessStepTypeId.RETRIGGER_CREATE_APPLICATION => ProcessStepTypeId.CREATE_APPLICATION,
            ProcessStepTypeId.RETRIGGER_CREATE_COMPANY_IDENTITY => ProcessStepTypeId.CREATE_COMPANY_IDENTITY,
            ProcessStepTypeId.RETRIGGER_ASSIGN_COMPANY_APPLICATION => ProcessStepTypeId.ASSIGN_COMPANY_APPLICATION,
            ProcessStepTypeId.RETRIGGER_CREATE_STATUS_LIST => ProcessStepTypeId.CREATE_STATUS_LIST,
            _ => throw new ArgumentOutOfRangeException(nameof(processStepTypeId), processStepTypeId, null)
        };

    public static ProcessStepTypeId GetTechnicalRetriggerStep(this ProcessStepTypeId processStepTypeId) =>
        processStepTypeId switch
        {
            ProcessStepTypeId.CREATE_TECHNICAL_USER => ProcessStepTypeId.RETRIGGER_CREATE_TECHNICAL_USER,
            ProcessStepTypeId.GET_TECHNICAL_USER_DATA => ProcessStepTypeId.RETRIGGER_GET_TECHNICAL_USER_DATA,
            ProcessStepTypeId.SEND_TECHNICAL_USER_CREATION_CALLBACK => ProcessStepTypeId.RETRIGGER_SEND_TECHNICAL_USER_CREATION_CALLBACK,
            ProcessStepTypeId.DELETE_TECHNICAL_USER => ProcessStepTypeId.RETRIGGER_DELETE_TECHNICAL_USER,
            ProcessStepTypeId.SEND_TECHNICAL_USER_DELETION_CALLBACK => ProcessStepTypeId.RETRIGGER_SEND_TECHNICAL_USER_DELETION_CALLBACK,
            _ => throw new ArgumentOutOfRangeException(nameof(processStepTypeId), processStepTypeId, null)
        };

    public static ProcessStepTypeId GetTechnicalStepForRetrigger(this ProcessStepTypeId processStepTypeId) =>
        processStepTypeId switch
        {
            ProcessStepTypeId.RETRIGGER_CREATE_TECHNICAL_USER => ProcessStepTypeId.CREATE_TECHNICAL_USER,
            ProcessStepTypeId.RETRIGGER_GET_TECHNICAL_USER_DATA => ProcessStepTypeId.GET_TECHNICAL_USER_DATA,
            ProcessStepTypeId.RETRIGGER_SEND_TECHNICAL_USER_CREATION_CALLBACK => ProcessStepTypeId.SEND_TECHNICAL_USER_CREATION_CALLBACK,
            ProcessStepTypeId.RETRIGGER_DELETE_TECHNICAL_USER => ProcessStepTypeId.DELETE_TECHNICAL_USER,
            ProcessStepTypeId.RETRIGGER_SEND_TECHNICAL_USER_DELETION_CALLBACK => ProcessStepTypeId.SEND_TECHNICAL_USER_DELETION_CALLBACK,
            _ => throw new ArgumentOutOfRangeException(nameof(processStepTypeId), processStepTypeId, null)
        };
}
