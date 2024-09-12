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

namespace Dim.Entities.Enums;

public enum ProcessStepTypeId
{
    // Setup Dim Process
    CREATE_WALLET = 1,
    CHECK_OPERATION = 2,
    GET_COMPANY = 3,
    GET_DID_DOCUMENT = 4,
    CREATE_STATUS_LIST = 5,
    SEND_CALLBACK = 6,

    // Create Technical User
    CREATE_TECHNICAL_USER = 100,
    GET_TECHNICAL_USER_DATA = 101,
    SEND_TECHNICAL_USER_CREATION_CALLBACK = 102,

    // Delete Technical User
    DELETE_TECHNICAL_USER = 200,
    SEND_TECHNICAL_USER_DELETION_CALLBACK = 201
}
