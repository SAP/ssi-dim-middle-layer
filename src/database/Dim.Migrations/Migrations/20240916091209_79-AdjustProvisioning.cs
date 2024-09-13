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
using System;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Dim.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class _79AdjustProvisioning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 18);

            migrationBuilder.DropColumn(
                name: "application_id",
                schema: "dim",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "dim_instance_id",
                schema: "dim",
                table: "tenants");

            migrationBuilder.RenameColumn(
                name: "sub_account_id",
                schema: "dim",
                table: "tenants",
                newName: "wallet_id");

            migrationBuilder.RenameColumn(
                name: "space_id",
                schema: "dim",
                table: "tenants",
                newName: "operation_id");

            migrationBuilder.RenameColumn(
                name: "service_instance_id",
                schema: "dim",
                table: "tenants",
                newName: "token_address");

            migrationBuilder.RenameColumn(
                name: "service_binding_name",
                schema: "dim",
                table: "tenants",
                newName: "client_id");

            migrationBuilder.RenameColumn(
                name: "application_key",
                schema: "dim",
                table: "tenants",
                newName: "base_url");

            migrationBuilder.AddColumn<byte[]>(
                name: "client_secret",
                schema: "dim",
                table: "tenants",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "encryption_mode",
                schema: "dim",
                table: "tenants",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "initialization_vector",
                schema: "dim",
                table: "tenants",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "operation_id",
                schema: "dim",
                table: "technical_users",
                type: "uuid",
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 1,
                column: "label",
                value: "CREATE_WALLET");

            migrationBuilder.UpdateData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 2,
                column: "label",
                value: "CHECK_OPERATION");

            migrationBuilder.UpdateData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 3,
                column: "label",
                value: "GET_COMPANY");

            migrationBuilder.UpdateData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 4,
                column: "label",
                value: "GET_DID_DOCUMENT");

            migrationBuilder.UpdateData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 5,
                column: "label",
                value: "CREATE_STATUS_LIST");

            migrationBuilder.UpdateData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 6,
                column: "label",
                value: "SEND_CALLBACK");

            migrationBuilder.UpdateData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 7,
                column: "label",
                value: "RETRIGGER_CREATE_WALLET");

            migrationBuilder.UpdateData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 8,
                column: "label",
                value: "RETRIGGER_CHECK_OPERATION");

            migrationBuilder.UpdateData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 9,
                column: "label",
                value: "RETRIGGER_GET_COMPANY");

            migrationBuilder.UpdateData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 10,
                column: "label",
                value: "RETRIGGER_GET_DID_DOCUMENT");

            migrationBuilder.UpdateData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 11,
                column: "label",
                value: "RETRIGGER_CREATE_STATUS_LIST");

            migrationBuilder.UpdateData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 12,
                column: "label",
                value: "RETRIGGER_SEND_CALLBACK");

            migrationBuilder.InsertData(
                schema: "dim",
                table: "process_step_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 103, "RETRIGGER_CREATE_TECHNICAL_USER" },
                    { 104, "RETRIGGER_GET_TECHNICAL_USER_DATA" },
                    { 105, "RETRIGGER_SEND_TECHNICAL_USER_CREATION_CALLBACK" },
                    { 202, "RETRIGGER_DELETE_TECHNICAL_USER" },
                    { 203, "RETRIGGER_SEND_TECHNICAL_USER_DELETION_CALLBACK" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 103);

            migrationBuilder.DeleteData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 104);

            migrationBuilder.DeleteData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 105);

            migrationBuilder.DeleteData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 202);

            migrationBuilder.DeleteData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 203);

            migrationBuilder.DropColumn(
                name: "client_secret",
                schema: "dim",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "encryption_mode",
                schema: "dim",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "initialization_vector",
                schema: "dim",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "operation_id",
                schema: "dim",
                table: "technical_users");

            migrationBuilder.RenameColumn(
                name: "wallet_id",
                schema: "dim",
                table: "tenants",
                newName: "sub_account_id");

            migrationBuilder.RenameColumn(
                name: "token_address",
                schema: "dim",
                table: "tenants",
                newName: "service_instance_id");

            migrationBuilder.RenameColumn(
                name: "operation_id",
                schema: "dim",
                table: "tenants",
                newName: "space_id");

            migrationBuilder.RenameColumn(
                name: "client_id",
                schema: "dim",
                table: "tenants",
                newName: "service_binding_name");

            migrationBuilder.RenameColumn(
                name: "base_url",
                schema: "dim",
                table: "tenants",
                newName: "application_key");

            migrationBuilder.AddColumn<string>(
                name: "application_id",
                schema: "dim",
                table: "tenants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "dim_instance_id",
                schema: "dim",
                table: "tenants",
                type: "uuid",
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 1,
                column: "label",
                value: "CREATE_SUBACCOUNT");

            migrationBuilder.UpdateData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 2,
                column: "label",
                value: "CREATE_SERVICEMANAGER_BINDINGS");

            migrationBuilder.UpdateData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 3,
                column: "label",
                value: "ASSIGN_ENTITLEMENTS");

            migrationBuilder.UpdateData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 4,
                column: "label",
                value: "CREATE_SERVICE_INSTANCE");

            migrationBuilder.UpdateData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 5,
                column: "label",
                value: "CREATE_SERVICE_BINDING");

            migrationBuilder.UpdateData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 6,
                column: "label",
                value: "SUBSCRIBE_APPLICATION");

            migrationBuilder.UpdateData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 7,
                column: "label",
                value: "CREATE_CLOUD_FOUNDRY_ENVIRONMENT");

            migrationBuilder.UpdateData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 8,
                column: "label",
                value: "CREATE_CLOUD_FOUNDRY_SPACE");

            migrationBuilder.UpdateData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 9,
                column: "label",
                value: "ADD_SPACE_MANAGER_ROLE");

            migrationBuilder.UpdateData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 10,
                column: "label",
                value: "ADD_SPACE_DEVELOPER_ROLE");

            migrationBuilder.UpdateData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 11,
                column: "label",
                value: "CREATE_DIM_SERVICE_INSTANCE");

            migrationBuilder.UpdateData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 12,
                column: "label",
                value: "CREATE_SERVICE_INSTANCE_BINDING");

            migrationBuilder.InsertData(
                schema: "dim",
                table: "process_step_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 13, "GET_DIM_DETAILS" },
                    { 14, "CREATE_APPLICATION" },
                    { 15, "CREATE_COMPANY_IDENTITY" },
                    { 16, "ASSIGN_COMPANY_APPLICATION" },
                    { 17, "CREATE_STATUS_LIST" },
                    { 18, "SEND_CALLBACK" }
                });
        }
    }
}
