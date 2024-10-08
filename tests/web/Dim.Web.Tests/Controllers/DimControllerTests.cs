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

using Dim.Web.Tests.Setup;
using System.Net;

namespace Dim.Web.Tests.Controllers;

public class DimControllerTests(IntegrationTestFactory factory) : IClassFixture<IntegrationTestFactory>
{
    private const string BaseUrl = "api/dim";
    private readonly HttpClient _client = factory.CreateClient();

    #region SetupDim

    [Fact]
    public async Task SetupDim_WithoutFilters_ReturnsExpected()
    {
        // Act
        var companyName = $"test-{DateTime.UtcNow.Ticks}";
        var bpn = $"BPN-{DateTime.UtcNow.Ticks}";
        var didDocumentLocation = $"https://example.org/did/{bpn}/did.json";
        var response = await _client.PostAsync($"{BaseUrl}/setup-dim?companyName={companyName}&bpn={bpn}&didDocumentLocation={didDocumentLocation}", null, CancellationToken.None);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Swagger

    [Fact]
    public async Task CheckSwagger_ReturnsExpected()
    {
        // Act
        var response = await _client.GetAsync($"{BaseUrl}/swagger/v1/swagger.json");

        // Assert
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion
}
