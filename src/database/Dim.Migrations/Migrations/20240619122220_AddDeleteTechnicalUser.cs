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
    public partial class AddDeleteTechnicalUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 102,
                column: "label",
                value: "SEND_TECHNICAL_USER_CREATION_CALLBACK");

            migrationBuilder.InsertData(
                schema: "dim",
                table: "process_step_types",
                columns: new[] { "id", "label" },
                values: new object[,]
                {
                    { 200, "DELETE_TECHNICAL_USER" },
                    { 201, "SEND_TECHNICAL_USER_DELETION_CALLBACK" }
                });

            migrationBuilder.InsertData(
                schema: "dim",
                table: "process_types",
                columns: new[] { "id", "label" },
                values: new object[] { 3, "DELETE_TECHNICAL_USER" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 200);

            migrationBuilder.DeleteData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 201);

            migrationBuilder.DeleteData(
                schema: "dim",
                table: "process_types",
                keyColumn: "id",
                keyValue: 3);

            migrationBuilder.UpdateData(
                schema: "dim",
                table: "process_step_types",
                keyColumn: "id",
                keyValue: 102,
                column: "label",
                value: "SEND_TECHNICAL_USER_CALLBACK");
        }
    }
}
