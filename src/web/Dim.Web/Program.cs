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

using Dim.Clients.Api.Dim.DependencyInjection;
using Dim.Clients.Token;
using Dim.DbAccess.DependencyInjection;
using Dim.Web.Authentication;
using Dim.Web.Controllers;
using Dim.Web.Extensions;
using Microsoft.AspNetCore.Authentication;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Web;
using System.Text.Json.Serialization;

const string Version = "v1";

await WebApplicationBuildRunner
    .BuildAndRunWebApplicationAsync<Program>(args, "dim", Version, "dim",
        builder =>
        {
            builder.Services
                .AddTransient<IBasicAuthTokenService, BasicAuthTokenService>()
                .AddTransient<IClaimsTransformation, KeycloakClaimsTransformation>()
                .AddDimClient()
                .AddDim(builder.Configuration.GetSection("Dim"))
                .AddEndpointsApiExplorer()
                .AddDatabase(builder.Configuration)
                .ConfigureHttpJsonOptions(options =>
                {
                    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
                })
                .Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });
        },
        (app, env) =>
        {
            app.MapGroup("/api")
                .WithOpenApi()
                .MapDimApi();
        }).ConfigureAwait(ConfigureAwaitOptions.None);
