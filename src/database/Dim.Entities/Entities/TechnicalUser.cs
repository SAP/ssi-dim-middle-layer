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

namespace Dim.Entities.Entities;

public class TechnicalUser(
    Guid id,
    Guid tenantId,
    Guid externalId,
    string technicalUserName,
    Guid processId)
{
    public Guid Id { get; set; } = id;
    public Guid TenantId { get; set; } = tenantId;
    public Guid ExternalId { get; set; } = externalId;
    public string TechnicalUserName { get; set; } = technicalUserName;
    public Guid? OperationId { get; set; }
    public string? TokenAddress { get; set; }
    public string? ClientId { get; set; }
    public byte[]? ClientSecret { get; set; }
    public byte[]? InitializationVector { get; set; }
    public int? EncryptionMode { get; set; }
    public Guid? ServiceKeyId { get; set; }
    public Guid ProcessId { get; set; } = processId;
    public virtual Tenant? Tenant { get; set; }
    public virtual Process? Process { get; set; }
}
