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

using Dim.Entities.Entities;
using Dim.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Processes.Library.Concrete.Context;

namespace Dim.Entities;

public class DimDbContext(DbContextOptions<DimDbContext> options) :
    ProcessDbContext<Process, ProcessTypeId, ProcessStepTypeId>(options)
{
    public virtual DbSet<Tenant> Tenants { get; set; } = default!;
    public virtual DbSet<TechnicalUser> TechnicalUsers { get; set; } = default!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("Relational:Collation", "en_US.utf8");
        modelBuilder.HasDefaultSchema("dim");

        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Tenant>(t =>
            {
                t.HasOne(d => d.Process)
                    .WithMany(p => p.Tenants)
                    .HasForeignKey(d => d.ProcessId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

        modelBuilder.Entity<TechnicalUser>(tu =>
        {
            tu.HasOne(d => d.Process)
                .WithMany(p => p.TechnicalUsers)
                .HasForeignKey(d => d.ProcessId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            tu.HasOne(t => t.Tenant)
                .WithMany(t => t.TechnicalUsers)
                .HasForeignKey(t => t.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });
    }
}
