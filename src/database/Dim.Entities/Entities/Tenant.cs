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

namespace Dim.Entities.Entities;

public class Tenant
{
    public Tenant(Guid id, string companyName, string bpn, string didDocumentLocation, bool isIssuer, Guid processId, Guid operatorId)
    {
        Id = id;
        CompanyName = companyName;
        Bpn = bpn;
        DidDocumentLocation = didDocumentLocation;
        IsIssuer = isIssuer;
        ProcessId = processId;
        OperatorId = operatorId;
    }

    public Guid Id { get; set; }
    public string CompanyName { get; set; }
    public string Bpn { get; set; }

    public string DidDocumentLocation { get; set; }

    public bool IsIssuer { get; set; }

    public Guid ProcessId { get; set; }

    public Guid? SubAccountId { get; set; }

    public string? ServiceInstanceId { get; set; }

    public string? ServiceBindingName { get; set; }

    public Guid? SpaceId { get; set; }

    public Guid? DimInstanceId { get; set; }
    public string? DidDownloadUrl { get; set; }
    public string? Did { get; set; }
    public string? ApplicationId { get; set; }
    public Guid? CompanyId { get; set; }
    public string? ApplicationKey { get; set; }
    public Guid OperatorId { get; set; }
    public virtual Process? Process { get; set; }
}
