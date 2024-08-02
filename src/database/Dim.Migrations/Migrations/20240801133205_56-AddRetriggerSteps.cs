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

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Dim.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class _56AddRetriggerSteps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_tenants_process_id",
                schema: "dim",
                table: "tenants");

            migrationBuilder.DropIndex(
                name: "ix_technical_users_process_id",
                schema: "dim",
                table: "technical_users");

            migrationBuilder.InsertData(
                schema: "dim",
                table: "process_step_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 19, "RETRIGGER_CREATE_SUBACCOUNT" },
                    { 20, "RETRIGGER_CREATE_SERVICEMANAGER_BINDINGS" },
                    { 21, "RETRIGGER_ASSIGN_ENTITLEMENTS" },
                    { 22, "RETRIGGER_CREATE_SERVICE_INSTANCE" },
                    { 23, "RETRIGGER_CREATE_SERVICE_BINDING" },
                    { 24, "RETRIGGER_SUBSCRIBE_APPLICATION" },
                    { 25, "RETRIGGER_CREATE_CLOUD_FOUNDRY_ENVIRONMENT" },
                    { 26, "RETRIGGER_CREATE_CLOUD_FOUNDRY_SPACE" },
                    { 27, "RETRIGGER_ADD_SPACE_MANAGER_ROLE" },
                    { 28, "RETRIGGER_ADD_SPACE_DEVELOPER_ROLE" },
                    { 29, "RETRIGGER_CREATE_DIM_SERVICE_INSTANCE" },
                    { 30, "RETRIGGER_CREATE_SERVICE_INSTANCE_BINDING" },
                    { 31, "RETRIGGER_GET_DIM_DETAILS" },
                    { 32, "RETRIGGER_CREATE_APPLICATION" },
                    { 33, "RETRIGGER_CREATE_COMPANY_IDENTITY" },
                    { 34, "RETRIGGER_ASSIGN_COMPANY_APPLICATION" },
                    { 35, "RETRIGGER_CREATE_STATUS_LIST" },
                    { 36, "RETRIGGER_SEND_CALLBACK" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_tenants_process_id",
                schema: "dim",
                table: "tenants",
                column: "process_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_technical_users_process_id",
                schema: "dim",
                table: "technical_users",
                column: "process_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_tenants_process_id",
                schema: "dim",
                table: "tenants");

            migrationBuilder.DropIndex(
                name: "ix_technical_users_process_id",
                schema: "dim",
                table: "technical_users");

            migrationBuilder.DeleteData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 36);

            migrationBuilder.CreateIndex(
                name: "ix_tenants_process_id",
                schema: "dim",
                table: "tenants",
                column: "process_id");

            migrationBuilder.CreateIndex(
                name: "ix_technical_users_process_id",
                schema: "dim",
                table: "technical_users",
                column: "process_id");
        }
    }
}
