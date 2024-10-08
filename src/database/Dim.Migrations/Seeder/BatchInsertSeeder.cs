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
using Dim.Entities.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Seeding;

namespace Dim.Migrations.Seeder;

/// <summary>
/// Seeder to seed the base entities (those with an id as primary key)
/// </summary>
public class BatchInsertSeeder(DimDbContext context, ILogger<BatchInsertSeeder> logger, IOptions<SeederSettings> options)
    : ICustomSeeder
{
    private readonly SeederSettings _settings = options.Value;

    /// <inheritdoc />
    public int Order => 1;

    /// <inheritdoc />
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (!_settings.DataPaths.Any())
        {
            logger.LogInformation("There a no data paths configured, therefore the {SeederName} will be skipped", nameof(BatchInsertSeeder));
            return;
        }

        logger.LogInformation("Start BaseEntityBatch Seeder");
        await SeedTable<Tenant>("tenants", x => x.Id, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        await SeedTable<ProcessStep>("process_steps", x => x.Id, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        await SeedTable<Process>("processes", x => x.Id, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        await SeedTable<TechnicalUser>("technical_users", x => x.Id, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        logger.LogInformation("Finished BaseEntityBatch Seeder");
    }

    private async Task SeedTable<T>(string fileName, Func<T, object> keySelector, CancellationToken cancellationToken) where T : class
    {
        logger.LogInformation("Start seeding {Filename}", fileName);
        var additionalEnvironments = _settings.TestDataEnvironments ?? Enumerable.Empty<string>();
        var data = await SeederHelper.GetSeedData<T>(logger, fileName, _settings.DataPaths, cancellationToken, additionalEnvironments.ToArray()).ConfigureAwait(ConfigureAwaitOptions.None);
        logger.LogInformation("Found {ElementCount} data", data.Count);
        if (data.Any())
        {
            var typeName = typeof(T).Name;
            logger.LogInformation("Started to Seed {TableName}", typeName);
            data = data.GroupJoin(context.Set<T>(), keySelector, keySelector, (d, dbEntry) => new { d, dbEntry })
                .SelectMany(t => t.dbEntry.DefaultIfEmpty(), (t, x) => new { t, x })
                .Where(t => t.x == null)
                .Select(t => t.t.d).ToList();
            await context.Set<T>().AddRangeAsync(data, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
            logger.LogInformation("Seeded {TableName}", typeName);
        }
    }
}
