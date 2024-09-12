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

using Dim.DbAccess.Repositories;
using Dim.Entities;
using Microsoft.EntityFrameworkCore;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using System.Collections.Immutable;

namespace Dim.DbAccess;

public class DimRepositories(DimDbContext dbContext)
    : IDimRepositories
{
    private static readonly IReadOnlyDictionary<Type, Func<DimDbContext, object>> Types = new Dictionary<Type, Func<DimDbContext, object>> {
        { typeof(IProcessStepRepository), context => new ProcessStepRepository(context) },
        { typeof(ITenantRepository), context => new TenantRepository(context) }
    }.ToImmutableDictionary();

    public RepositoryType GetInstance<RepositoryType>()
    {
        object? repository = default;

        if (Types.TryGetValue(typeof(RepositoryType), out var createFunc))
        {
            repository = createFunc(dbContext);
        }

        return (RepositoryType)(repository ?? throw new ArgumentException($"unexpected type {typeof(RepositoryType).Name}", nameof(RepositoryType)));
    }

    /// <inheritdoc />
    public TEntity Attach<TEntity>(TEntity entity, Action<TEntity>? setOptionalParameters = null) where TEntity : class
    {
        var attachedEntity = dbContext.Attach(entity).Entity;
        setOptionalParameters?.Invoke(attachedEntity);

        return attachedEntity;
    }

    public Task<int> SaveAsync()
    {
        try
        {
            return dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException e)
        {
            throw new ConflictException("while processing a concurrent update was saved to the database (reason could also be data to be deleted is no longer existing)", e);
        }
    }

    public void Clear() => dbContext.ChangeTracker.Clear();
}
