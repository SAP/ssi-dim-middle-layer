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

using Dim.Entities;
using Dim.Migrations.Migrations;
using Dim.Web.BusinessLogic;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Logging;
using System.Text.Json.Serialization;
using Testcontainers.PostgreSql;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Dim.Web.Tests.Setup;

public class IntegrationTestFactory : WebApplicationFactory<DimBusinessLogic>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithDatabase("test_db")
        .WithImage("postgres")
        .WithCleanUp(true)
        .WithName(Guid.NewGuid().ToString())
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var projectDir = Directory.GetCurrentDirectory();
        var configPath = Path.Combine(projectDir, "appsettings.IntegrationTests.json");

        var config = new ConfigurationBuilder().AddJsonFile(configPath, true).Build();
        builder.UseConfiguration(config);
        builder.ConfigureTestServices(services =>
        {
            services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
            services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<DimDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<DimDbContext>(options =>
            {
                options.UseNpgsql(_container.GetConnectionString(),
                        x => x.MigrationsAssembly(typeof(Initial).Assembly.GetName().Name)
                            .MigrationsHistoryTable("__efmigrations_history_dim"));
            });
            services.AddSingleton<IPolicyEvaluator, FakePolicyEvaluator>();
        });
    }

    /// <inheritdoc />
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.AddLogging();
        var host = base.CreateHost(builder);

        var optionsBuilder = new DbContextOptionsBuilder<DimDbContext>();

        optionsBuilder.UseNpgsql(
            _container.GetConnectionString(),
            x => x.MigrationsAssembly(typeof(Initial).Assembly.GetName().Name)
                .MigrationsHistoryTable("__efmigrations_history_dim", "public")
        );
        var context = new DimDbContext(optionsBuilder.Options);
        context.Database.Migrate();

        return host;
    }

    public async Task InitializeAsync() => await _container.StartAsync();

    public new async Task DisposeAsync() => await _container.DisposeAsync();
}
