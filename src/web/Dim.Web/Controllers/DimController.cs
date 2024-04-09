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

using Dim.Web.BusinessLogic;
using Dim.Web.Extensions;
using Dim.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dim.Web.Controllers;

/// <summary>
/// Creates a new instance of <see cref="DimController"/>
/// </summary>
public static class DimController
{
    public static RouteGroupBuilder MapDimApi(this RouteGroupBuilder group)
    {
        var policyHub = group.MapGroup("/dim");

        policyHub.MapPost("setup-dim", ([FromQuery] string companyName, [FromQuery] string bpn, [FromQuery] string didDocumentLocation, IDimBusinessLogic dimBusinessLogic) => dimBusinessLogic.StartSetupDim(companyName, bpn, didDocumentLocation, false))
            .WithSwaggerDescription("Gets the keys for the attributes",
                "Example: Post: api/dim/setup-dim",
                "the name of the company",
                "bpn of the wallets company",
                "The did document location")
            .RequireAuthorization(r => r.RequireRole("setup_wallet"))
            .Produces(StatusCodes.Status201Created);

        policyHub.MapPost("setup-issuer", ([FromQuery] string companyName, [FromQuery] string bpn, [FromQuery] string didDocumentLocation, IDimBusinessLogic dimBusinessLogic) => dimBusinessLogic.StartSetupDim(companyName, bpn, didDocumentLocation, true))
            .WithSwaggerDescription("Gets the keys for the attributes",
                "Example: Post: api/dim/setup-issuer",
                "the name of the company",
                "bpn of the wallets company",
                "The did document location")
            .RequireAuthorization(r => r.RequireRole("setup_wallet"))
            .Produces(StatusCodes.Status201Created);

        policyHub.MapGet("status-list", ([FromQuery] string bpn, CancellationToken cancellationToken, [FromServices] IDimBusinessLogic dimBusinessLogic) => dimBusinessLogic.GetStatusList(bpn, cancellationToken))
            .WithSwaggerDescription("Gets the status list for the given company",
                "Example: GET: api/dim/status-list/{bpn}",
                "id of the dim company")
            .RequireAuthorization(r => r.RequireRole("view_status_list"))
            .Produces(StatusCodes.Status200OK, responseType: typeof(string), contentType: Constants.JsonContentType);

        policyHub.MapPost("status-list", ([FromQuery] string bpn, CancellationToken cancellationToken, [FromServices] IDimBusinessLogic dimBusinessLogic) => dimBusinessLogic.CreateStatusList(bpn, cancellationToken))
            .WithSwaggerDescription("Creates a status list for the given company",
                "Example: Post: api/dim/status-list/{bpn}",
                "bpn of the company")
            .RequireAuthorization(r => r.RequireRole("create_status_list"))
            .Produces(StatusCodes.Status200OK, responseType: typeof(string), contentType: Constants.JsonContentType);

        return group;
    }
}
