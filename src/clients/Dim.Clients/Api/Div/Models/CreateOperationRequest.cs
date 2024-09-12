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

using Dim.Clients.Api.Dim;
using System.Text.Json.Serialization;

namespace Dim.Clients.Api.Div.Models;

public record OperationCreationRequest(
    [property: JsonPropertyName("action")] string Action,
    [property: JsonPropertyName("entity")] string Entity,
    [property: JsonPropertyName("payload")] OperationPayloadData Payload
);

public record OperationPayloadData(
    [property: JsonPropertyName("customerId")] string CustomerId,
    [property: JsonPropertyName("customerName")] string CustomerName,
    [property: JsonPropertyName("divWalletServiceName")] string WalletServiceName,
    [property: JsonPropertyName("divWalletServiceParameters")] WalletServiceParameter WalletServiceParameter
);

public record WalletServiceParameter(
    [property: JsonPropertyName("applications")] IEnumerable<WalletApplication> Applications
);

public record WalletApplication(
    [property: JsonPropertyName("application")] string Application,
    [property: JsonPropertyName("companies")] IEnumerable<ApplicationCompany> Companies,
    [property: JsonPropertyName("trustedIssuers")] IEnumerable<TrustedIssuer> TrustedIssuers
);

public record ApplicationCompany(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("hostingURL")] string HostingUrl,
    [property: JsonPropertyName("services")] IEnumerable<ApplicationCompanyService> Services,
    [property: JsonPropertyName("keys")] IEnumerable<ApplicationCompanyKey> Keys
);

public record ApplicationCompanyService(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("serviceEndpoint")] string ServiceEndpoint
);

public record ApplicationCompanyKey(
    [property: JsonPropertyName("type")] string Type
);

public record TrustedIssuer(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("did")] string Did,
    [property: JsonPropertyName("ignoreMissingHashlist")] bool IgnoreMissingHashlist
);

public record CreateOperationRequest(
    [property: JsonPropertyName("operationId")] Guid OperationId
);
