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

namespace Dim.Entities.Enums;

public enum ProcessStepTypeId
{
    // Setup Dim Process
    CREATE_SUBACCOUNT = 1,
    CREATE_SERVICEMANAGER_BINDINGS = 2,
    ASSIGN_ENTITLEMENTS = 3,
    CREATE_SERVICE_INSTANCE = 4,
    CREATE_SERVICE_BINDING = 5,
    SUBSCRIBE_APPLICATION = 6,
    CREATE_CLOUD_FOUNDRY_ENVIRONMENT = 7,
    CREATE_CLOUD_FOUNDRY_SPACE = 8,
    ADD_SPACE_MANAGER_ROLE = 9,
    ADD_SPACE_DEVELOPER_ROLE = 10,
    CREATE_DIM_SERVICE_INSTANCE = 11,
    CREATE_SERVICE_INSTANCE_BINDING = 12,
    GET_DIM_DETAILS = 13,
    CREATE_APPLICATION = 14,
    CREATE_COMPANY_IDENTITY = 15,
    ASSIGN_COMPANY_APPLICATION = 16,
    CREATE_STATUS_LIST = 17,
    SEND_CALLBACK = 18
}
