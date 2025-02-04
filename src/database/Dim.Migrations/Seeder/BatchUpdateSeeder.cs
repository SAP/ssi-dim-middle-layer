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
public class BatchUpdateSeeder(DimDbContext context, ILogger<BatchInsertSeeder> logger, IOptions<SeederSettings> options)
    : ICustomSeeder
{
    private readonly SeederSettings _settings = options.Value;

    /// <inheritdoc />
    public int Order => 2;

    /// <inheritdoc />
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (!_settings.DataPaths.Any())
        {
            logger.LogInformation("There a no data paths configured, therefore the {SeederName} will be skipped", nameof(BatchUpdateSeeder));
            return;
        }

        logger.LogInformation("Start BaseEntityBatch Seeder");
        await SeedTable<Tenant>("tenants",
            x => x.Id,
            x => x.dataEntity.CompanyName != x.dbEntity.CompanyName || x.dataEntity.Bpn != x.dbEntity.Bpn || x.dataEntity.DidDocumentLocation != x.dbEntity.DidDocumentLocation,
            (dbEntry, entry) =>
            {
                dbEntry.Bpn = entry.Bpn;
                dbEntry.CompanyName = entry.CompanyName;
                dbEntry.DidDocumentLocation = entry.DidDocumentLocation;
            }, cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(ConfigureAwaitOptions.None);
        logger.LogInformation("Finished BaseEntityBatch Seeder");
    }

    private async Task SeedTable<T>(string fileName, Func<T, object> keySelector, Func<(T dataEntity, T dbEntity), bool> whereClause, Action<T, T> updateEntries, CancellationToken cancellationToken) where T : class
    {
        logger.LogInformation("Start seeding {Filename}", fileName);
        var additionalEnvironments = _settings.TestDataEnvironments ?? Enumerable.Empty<string>();
        var data = await SeederHelper.GetSeedData<T>(logger, fileName, _settings.DataPaths, cancellationToken, additionalEnvironments.ToArray()).ConfigureAwait(ConfigureAwaitOptions.None);
        logger.LogInformation("Found {ElementCount} data", data.Count);
        if (data.Any())
        {
            var typeName = typeof(T).Name;
            var entriesForUpdate = data
                .Join(context.Set<T>(), keySelector, keySelector, (dataEntry, dbEntry) => (DataEntry: dataEntry, DbEntry: dbEntry))
                .Where(whereClause.Invoke)
                .ToList();
            if (entriesForUpdate.Any())
            {
                logger.LogInformation("Started to Update {EntryCount} entries of {TableName}", entriesForUpdate.Count, typeName);
                foreach (var entry in entriesForUpdate)
                {
                    updateEntries.Invoke(entry.DbEntry, entry.DataEntry);
                }
                logger.LogInformation("Updated {TableName}", typeName);
            }
        }
    }
}
