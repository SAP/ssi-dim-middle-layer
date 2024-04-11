/********************************************************************************
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

using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Dim.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class _101 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "technical_users",
                schema: "dim",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_id = table.Column<Guid>(type: "uuid", nullable: false),
                    technical_user_name = table.Column<string>(type: "text", nullable: false),
                    token_address = table.Column<string>(type: "text", nullable: true),
                    client_id = table.Column<string>(type: "text", nullable: true),
                    client_secret = table.Column<byte[]>(type: "bytea", nullable: true),
                    initialization_vector = table.Column<byte[]>(type: "bytea", nullable: true),
                    encryption_mode = table.Column<int>(type: "integer", nullable: true),
                    process_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_technical_users", x => x.id);
                    table.ForeignKey(
                        name: "fk_technical_users_processes_process_id",
                        column: x => x.process_id,
                        principalSchema: "dim",
                        principalTable: "processes",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_technical_users_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "dim",
                        principalTable: "tenants",
                        principalColumn: "id");
                });

            migrationBuilder.InsertData(
                schema: "dim",
                table: "process_step_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 100, "CREATE_TECHNICAL_USER" },
                    { 101, "GET_TECHNICAL_USER_DATA" },
                    { 102, "SEND_TECHNICAL_USER_CALLBACK" }
                });

            migrationBuilder.InsertData(
                schema: "dim",
                table: "process_types",
                columns: new[] { "id", "label" },
                values: new object[] { 2, "CREATE_TECHNICAL_USER" });

            migrationBuilder.CreateIndex(
                name: "ix_technical_users_process_id",
                schema: "dim",
                table: "technical_users",
                column: "process_id");

            migrationBuilder.CreateIndex(
                name: "ix_technical_users_tenant_id",
                schema: "dim",
                table: "technical_users",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "technical_users",
                schema: "dim");

            migrationBuilder.DeleteData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 100);

            migrationBuilder.DeleteData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 101);

            migrationBuilder.DeleteData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 102);

            migrationBuilder.DeleteData(
                schema: "dim",
                table: "process_types",
                keyColumn: "id",
                keyValue: 2);
        }
    }
}
