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

using Dim.Clients.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Dim.Clients.Api.Div.DependencyInjection;

public static class ProvisioningClientServiceExtensions
{
    public static IServiceCollection AddProvisioningClient(this IServiceCollection services, IConfigurationSection section)
    {
        services.AddOptions<ProvisioningSettings>()
            .Bind(section)
            .ValidateOnStart()
            .ValidateDataAnnotations();

        var sp = services.BuildServiceProvider();
        var settings = sp.GetRequiredService<IOptions<ProvisioningSettings>>();
        services
            .AddCustomHttpClientWithAuthentication<ProvisioningClient>(settings.Value.BaseUrl, settings.Value.TokenAddress)
            .AddTransient<IProvisioningClient, ProvisioningClient>();

        return services;
    }
}
