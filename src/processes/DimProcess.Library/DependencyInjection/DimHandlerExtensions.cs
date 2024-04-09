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

using Dim.Clients.Api.Cf.DependencyInjection;
using Dim.Clients.Api.Dim.DependencyInjection;
using Dim.Clients.Api.Entitlements.DependencyInjection;
using Dim.Clients.Api.Provisioning.DependencyInjection;
using Dim.Clients.Api.Services.DependencyInjection;
using Dim.Clients.Api.SubAccounts.DependencyInjection;
using Dim.Clients.Api.Subscriptions.DependencyInjection;
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
            .AddSubAccountClient(config.GetSection("SubAccount"))
            .AddEntitlementClient(config.GetSection("Entitlement"))
            .AddServiceClient()
            .AddSubscriptionClient()
            .AddProvisioningClient()
            .AddCfClient(config.GetSection("Cf"))
            .AddDimClient()
            .AddCallbackClient(config.GetSection("Callback"));

        return services;
    }
}
