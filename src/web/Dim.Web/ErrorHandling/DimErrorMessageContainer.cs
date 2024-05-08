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

using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling.Service;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Dim.Web.ErrorHandling;

[ExcludeFromCodeCoverage]
public class DimErrorMessageContainer : IErrorMessageContainer
{
    private static readonly IReadOnlyDictionary<int, string> _messageContainer = new Dictionary<DimErrors, string> {
        { DimErrors.NO_COMPANY_FOR_BPN, "No Tenant found for Bpn {bpn}" },
        { DimErrors.NO_COMPANY_ID_SET, "No Company Id set" },
        { DimErrors.NO_INSTANCE_ID_SET, "No Instnace Id set" },
    }.ToImmutableDictionary(x => (int)x.Key, x => x.Value);

    public Type Type { get => typeof(DimErrors); }
    public IReadOnlyDictionary<int, string> MessageContainer { get => _messageContainer; }
}

public enum DimErrors
{
    NO_COMPANY_FOR_BPN,
    NO_COMPANY_ID_SET,
    NO_INSTANCE_ID_SET
}
