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

using Dim.Entities.Enums;
using Org.Eclipse.TractusX.Portal.Backend.Framework.DBAccess;
using System.ComponentModel.DataAnnotations;

namespace Dim.Entities.Entities;

public class Process : ILockableEntity
{
    private Process()
    {
        ProcessSteps = new HashSet<ProcessStep>();
        Tenants = new HashSet<Tenant>();
    }

    public Process(Guid id, ProcessTypeId processTypeId, Guid version) : this()
    {
        Id = id;
        ProcessTypeId = processTypeId;
        Version = version;
    }

    public Guid Id { get; private set; }

    public ProcessTypeId ProcessTypeId { get; set; }

    public DateTimeOffset? LockExpiryDate { get; set; }

    [ConcurrencyCheck]
    public Guid Version { get; set; }

    // Navigation properties
    public virtual ProcessType? ProcessType { get; set; }
    public virtual ICollection<ProcessStep> ProcessSteps { get; private set; }
    public virtual ICollection<Tenant> Tenants { get; private set; }
}
