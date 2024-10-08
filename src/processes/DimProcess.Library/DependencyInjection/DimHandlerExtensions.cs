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
using Dim.Clients.Api.Div.DependencyInjection;
using Dim.Clients.Token;
using DimProcess.Library.Callback.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DimProcess.Library.DependencyInjection;

public static class DimHandlerExtensions
{
    public static IServiceCollection AddDimProcessHandler(this IServiceCollection services, IConfiguration config)
    {
        services.AddOptions<DimHandlerSettings>()
            .Bind(config.GetSection("Dim"))
            .ValidateOnStart();

        services
            .AddTransient<IBasicAuthTokenService, BasicAuthTokenService>()
            .AddTransient<IDimProcessHandler, DimProcessHandler>()
            .AddProvisioningClient(config.GetSection("Provisioning"))
            .AddDimClient()
            .AddCallbackClient(config.GetSection("Callback"));

        return services;
    }

    public static IServiceCollection AddTechnicalUserProcessHandler(this IServiceCollection services, IConfiguration config)
    {
        services.AddOptions<TechnicalUserSettings>()
            .Bind(config.GetSection("TechnicalUserCreation"))
            .ValidateOnStart();

        services
            .AddTransient<IBasicAuthTokenService, BasicAuthTokenService>()
            .AddTransient<ITechnicalUserProcessHandler, TechnicalUserProcessHandler>()
            .AddProvisioningClient(config.GetSection("Provisioning"))
            .AddCallbackClient(config.GetSection("Callback"));

        return services;
    }
}
