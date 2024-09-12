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
    public partial class AddDivProvisioning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 12);

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

            migrationBuilder.DropColumn(
                name: "space_id",
                schema: "dim",
                table: "tenants");

            migrationBuilder.RenameColumn(
                name: "sub_account_id",
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.RenameColumn(
                name: "token_address",
                schema: "dim",
                table: "tenants",
                newName: "service_instance_id");

            migrationBuilder.RenameColumn(
                name: "operation_id",
                schema: "dim",
                table: "tenants",
                newName: "sub_account_id");

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

            migrationBuilder.AddColumn<Guid>(
                name: "space_id",
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

            migrationBuilder.InsertData(
                schema: "dim",
                table: "process_step_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
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
        }
    }
}
