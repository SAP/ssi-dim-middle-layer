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

using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Dim.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dim");

            migrationBuilder.CreateTable(
                name: "process_step_statuses",
                schema: "dim",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_process_step_statuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "process_step_types",
                schema: "dim",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_process_step_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "process_types",
                schema: "dim",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_process_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "processes",
                schema: "dim",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    process_type_id = table.Column<int>(type: "integer", nullable: false),
                    lock_expiry_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_processes", x => x.id);
                    table.ForeignKey(
                        name: "fk_processes_process_types_process_type_id",
                        column: x => x.process_type_id,
                        principalSchema: "dim",
                        principalTable: "process_types",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "process_steps",
                schema: "dim",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    process_step_type_id = table.Column<int>(type: "integer", nullable: false),
                    process_step_status_id = table.Column<int>(type: "integer", nullable: false),
                    process_id = table.Column<Guid>(type: "uuid", nullable: false),
                    date_created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    date_last_changed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    message = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_process_steps", x => x.id);
                    table.ForeignKey(
                        name: "fk_process_steps_process_step_statuses_process_step_status_id",
                        column: x => x.process_step_status_id,
                        principalSchema: "dim",
                        principalTable: "process_step_statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_process_steps_process_step_types_process_step_type_id",
                        column: x => x.process_step_type_id,
                        principalSchema: "dim",
                        principalTable: "process_step_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_process_steps_processes_process_id",
                        column: x => x.process_id,
                        principalSchema: "dim",
                        principalTable: "processes",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "tenants",
                schema: "dim",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_name = table.Column<string>(type: "text", nullable: false),
                    bpn = table.Column<string>(type: "text", nullable: false),
                    did_document_location = table.Column<string>(type: "text", nullable: false),
                    is_issuer = table.Column<bool>(type: "boolean", nullable: false),
                    process_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sub_account_id = table.Column<Guid>(type: "uuid", nullable: true),
                    service_instance_id = table.Column<string>(type: "text", nullable: true),
                    service_binding_name = table.Column<string>(type: "text", nullable: true),
                    space_id = table.Column<Guid>(type: "uuid", nullable: true),
                    dim_instance_id = table.Column<Guid>(type: "uuid", nullable: true),
                    did_download_url = table.Column<string>(type: "text", nullable: true),
                    did = table.Column<string>(type: "text", nullable: true),
                    application_id = table.Column<string>(type: "text", nullable: true),
                    company_id = table.Column<Guid>(type: "uuid", nullable: true),
                    application_key = table.Column<string>(type: "text", nullable: true),
                    operator_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tenants", x => x.id);
                    table.ForeignKey(
                        name: "fk_tenants_processes_process_id",
                        column: x => x.process_id,
                        principalSchema: "dim",
                        principalTable: "processes",
                        principalColumn: "id");
                });

            migrationBuilder.InsertData(
                schema: "dim",
                table: "process_step_statuses",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "TODO" },
                    { 2, "DONE" },
                    { 3, "SKIPPED" },
                    { 4, "FAILED" },
                    { 5, "DUPLICATE" }
                });

            migrationBuilder.InsertData(
                schema: "dim",
                table: "process_step_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 1, "CREATE_SUBACCOUNT" },
                    { 2, "CREATE_SERVICEMANAGER_BINDINGS" },
                    { 3, "ASSIGN_ENTITLEMENTS" },
                    { 4, "CREATE_SERVICE_INSTANCE" },
                    { 5, "CREATE_SERVICE_BINDING" },
                    { 6, "SUBSCRIBE_APPLICATION" },
                    { 7, "CREATE_CLOUD_FOUNDRY_ENVIRONMENT" },
                    { 8, "CREATE_CLOUD_FOUNDRY_SPACE" },
                    { 9, "ADD_SPACE_MANAGER_ROLE" },
                    { 10, "ADD_SPACE_DEVELOPER_ROLE" },
                    { 11, "CREATE_DIM_SERVICE_INSTANCE" },
                    { 12, "CREATE_SERVICE_INSTANCE_BINDING" },
                    { 13, "GET_DIM_DETAILS" },
                    { 14, "CREATE_APPLICATION" },
                    { 15, "CREATE_COMPANY_IDENTITY" },
                    { 16, "ASSIGN_COMPANY_APPLICATION" },
                    { 17, "CREATE_STATUS_LIST" },
                    { 18, "SEND_CALLBACK" }
                });

            migrationBuilder.InsertData(
                schema: "dim",
                table: "process_types",
                columns: new[] { "id", "label" },
                values: new object[] { 1, "SETUP_DIM" });

            migrationBuilder.CreateIndex(
                name: "ix_process_steps_process_id",
                schema: "dim",
                table: "process_steps",
                column: "process_id");

            migrationBuilder.CreateIndex(
                name: "ix_process_steps_process_step_status_id",
                schema: "dim",
                table: "process_steps",
                column: "process_step_status_id");

            migrationBuilder.CreateIndex(
                name: "ix_process_steps_process_step_type_id",
                schema: "dim",
                table: "process_steps",
                column: "process_step_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_processes_process_type_id",
                schema: "dim",
                table: "processes",
                column: "process_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_tenants_process_id",
                schema: "dim",
                table: "tenants",
                column: "process_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "process_steps",
                schema: "dim");

            migrationBuilder.DropTable(
                name: "tenants",
                schema: "dim");

            migrationBuilder.DropTable(
                name: "process_step_statuses",
                schema: "dim");

            migrationBuilder.DropTable(
                name: "process_step_types",
                schema: "dim");

            migrationBuilder.DropTable(
                name: "processes",
                schema: "dim");

            migrationBuilder.DropTable(
                name: "process_types",
                schema: "dim");
        }
    }
}
