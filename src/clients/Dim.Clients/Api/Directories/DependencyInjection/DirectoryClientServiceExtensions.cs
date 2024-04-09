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

using Dim.Clients.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Dim.Clients.Api.Directories.DependencyInjection;

public static class DirectoryClientServiceExtensions
{
    public static IServiceCollection AddDirectoryClient(this IServiceCollection services, IConfigurationSection section)
    {
        services.AddOptions<DirectorySettings>()
            .Bind(section)
            .ValidateOnStart();

        var sp = services.BuildServiceProvider();
        var settings = sp.GetRequiredService<IOptions<DirectorySettings>>();
        services
            .AddCustomHttpClientWithAuthentication<DirectoryClient>(settings.Value.BaseUrl, settings.Value.TokenAddress)
            .AddTransient<IDirectoryClient, DirectoryClient>();

        return services;
    }
}
