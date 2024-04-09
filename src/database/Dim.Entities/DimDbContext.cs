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

using Dim.Entities.Entities;
using Dim.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace Dim.Entities;

public class DimDbContext : DbContext
{
    protected DimDbContext()
    {
    }

    public DimDbContext(DbContextOptions<DimDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Process> Processes { get; set; } = default!;
    public virtual DbSet<ProcessStep> ProcessSteps { get; set; } = default!;
    public virtual DbSet<ProcessStepStatus> ProcessStepStatuses { get; set; } = default!;
    public virtual DbSet<ProcessStepType> ProcessStepTypes { get; set; } = default!;
    public virtual DbSet<ProcessType> ProcessTypes { get; set; } = default!;
    public virtual DbSet<Tenant> Tenants { get; set; } = default!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSnakeCaseNamingConvention();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("Relational:Collation", "en_US.utf8");
        modelBuilder.HasDefaultSchema("dim");

        modelBuilder.Entity<Process>()
            .HasOne(d => d.ProcessType)
            .WithMany(p => p.Processes)
            .HasForeignKey(d => d.ProcessTypeId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        modelBuilder.Entity<ProcessStep>()
            .HasOne(d => d.Process)
            .WithMany(p => p.ProcessSteps)
            .HasForeignKey(d => d.ProcessId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        modelBuilder.Entity<ProcessType>()
            .HasData(
                Enum.GetValues(typeof(ProcessTypeId))
                    .Cast<ProcessTypeId>()
                    .Select(e => new ProcessType(e))
            );

        modelBuilder.Entity<ProcessStepStatus>()
            .HasData(
                Enum.GetValues(typeof(ProcessStepStatusId))
                    .Cast<ProcessStepStatusId>()
                    .Select(e => new ProcessStepStatus(e))
            );

        modelBuilder.Entity<ProcessStepType>()
            .HasData(
                Enum.GetValues(typeof(ProcessStepTypeId))
                    .Cast<ProcessStepTypeId>()
                    .Select(e => new ProcessStepType(e))
            );

        modelBuilder.Entity<Tenant>()
            .HasOne(d => d.Process)
            .WithMany(p => p.Tenants)
            .HasForeignKey(d => d.ProcessId)
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}
