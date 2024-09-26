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
using Dim.Migrations.Seeder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Seeding;
using Testcontainers.PostgreSql;
using Xunit;
using Xunit.Extensions.AssemblyFixture;

[assembly: TestFramework(AssemblyFixtureFramework.TypeName, AssemblyFixtureFramework.AssemblyName)]
namespace Dim.DbAccess.Tests.Setup;

public class TestDbFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithDatabase("test_db")
        .WithImage("postgres")
        .WithCleanUp(true)
        .WithName(Guid.NewGuid().ToString())
        .Build();

    /// <summary>
    /// Foreach test a new DimDbContext will be created and filled with the custom seeding data. 
    /// </summary>
    /// <remarks>
    /// In this method the migrations don't need to get executed since they are already on the testcontainer.
    /// Because of that the EnsureCreatedAsync is enough.
    /// </remarks>
    /// <param name="seedActions">Additional data for the database</param>
    /// <returns>Returns the created DimDbContext</returns>
    public async Task<DimDbContext> GetDbContext(params Action<DimDbContext>[] seedActions)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DimDbContext>();

        optionsBuilder.UseNpgsql(
            _container.GetConnectionString(),
            x => x.MigrationsAssembly(typeof(_120).Assembly.GetName().Name)
                .MigrationsHistoryTable("__efmigrations_history_dim")
        );
        var context = new DimDbContext(optionsBuilder.Options);
        await context.Database.EnsureCreatedAsync();
        foreach (var seedAction in seedActions)
        {
            seedAction.Invoke(context);
        }

        await context.SaveChangesAsync();
        return context;
    }

    /// <summary>
    /// This method is used to initially setup the database and run all migrations
    /// </summary>
    public async Task InitializeAsync()
    {
        await _container.StartAsync()
            ;

        var optionsBuilder = new DbContextOptionsBuilder<DimDbContext>();

        optionsBuilder.UseNpgsql(
            _container.GetConnectionString(),
            x => x.MigrationsAssembly(typeof(_120).Assembly.GetName().Name)
                .MigrationsHistoryTable("__efmigrations_history_dim")
        );
        var context = new DimDbContext(optionsBuilder.Options);
        await context.Database.MigrateAsync();

        var seederOptions = Options.Create(new SeederSettings
        {
            TestDataEnvironments = Enumerable.Repeat("unittests", 1),
            DataPaths = new[] { "Seeder/Data" }
        });
        var insertSeeder = new BatchInsertSeeder(context,
            LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<BatchInsertSeeder>(),
            seederOptions);
        await insertSeeder.ExecuteAsync(CancellationToken.None);
    }

    /// <inheritdoc />
    public async Task DisposeAsync() => await _container.DisposeAsync();
}
