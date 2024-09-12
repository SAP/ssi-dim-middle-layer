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

using Dim.Clients.Api.Div.Models;
using Dim.Tests.Shared;
using DimProcess.Library.Callback;
using DimProcess.Library.Callback.DependencyInjection;
using Microsoft.Extensions.Options;
using Org.Eclipse.TractusX.Portal.Backend.Framework.ErrorHandling;
using Org.Eclipse.TractusX.Portal.Backend.Framework.Token;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace DimProcess.Library.Tests;

public class CallbackServiceTests
{
    #region Initialization

    private readonly ITokenService _tokenService;
    private readonly IFixture _fixture;
    private readonly IOptions<CallbackSettings> _options;

    public CallbackServiceTests()
    {
        _fixture = new Fixture().Customize(new AutoFakeItEasyCustomization { ConfigureMembers = true });
        _fixture.ConfigureFixture();

        _options = Options.Create(new CallbackSettings
        {
            Password = "passWord",
            Scope = "test",
            Username = "user@name",
            BaseAddress = "https://base.address.com",
            ClientId = "CatenaX",
            ClientSecret = "pass@Secret",
            GrantType = "cred",
            TokenAddress = "https://key.cloak.com",
        });
        _fixture.Inject(_options);
        _tokenService = A.Fake<ITokenService>();
    }

    #endregion

    #region SendCallback

    [Fact]
    public async Task SendCallback_WithValidData_DoesNotThrowException()
    {
        // Arrange
        var httpMessageHandlerMock =
            new HttpMessageHandlerMock(HttpStatusCode.OK);
        using var httpClient = new HttpClient(httpMessageHandlerMock);
        httpClient.BaseAddress = new Uri("https://base.address.com");
        A.CallTo(() => _tokenService.GetAuthorizedClient<CallbackService>(_options.Value, A<CancellationToken>._))
            .Returns(httpClient);
        var sut = new CallbackService(_tokenService, _options);

        // Act
        await sut.SendCallback("BPNL00001TEST", _fixture.Create<AuthenticationDetail>(), _fixture.Create<JsonDocument>(), "did:web:test123", CancellationToken.None);

        // Assert
        httpMessageHandlerMock.RequestMessage.Should().Match<HttpRequestMessage>(x =>
            x.Content is JsonContent &&
            (x.Content as JsonContent)!.ObjectType == typeof(CallbackDataModel) &&
            ((x.Content as JsonContent)!.Value as CallbackDataModel)!.Did == "did:web:test123"
        );
    }

    [Fact]
    public async Task SendCallback_WithInvalidData_ThrowsServiceException()
    {
        // Arrange
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.BadRequest);
        using var httpClient = new HttpClient(httpMessageHandlerMock);
        httpClient.BaseAddress = new Uri("https://base.address.com");
        A.CallTo(() => _tokenService.GetAuthorizedClient<CallbackService>(_options.Value, A<CancellationToken>._)).Returns(httpClient);
        var sut = new CallbackService(_tokenService, _options);

        // Act
        async Task Act() => await sut.SendCallback("BPNL00001TEST", _fixture.Create<AuthenticationDetail>(), _fixture.Create<JsonDocument>(), "did:web:test123", CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);
        ex.Message.Should().Contain("call to external system send-callback failed with statuscode");
    }

    #endregion

    #region SendTechnicalUserCallback

    [Fact]
    public async Task SendTechnicalUserCallback_WithValidData_DoesNotThrowException()
    {
        // Arrange
        var httpMessageHandlerMock =
            new HttpMessageHandlerMock(HttpStatusCode.OK);
        using var httpClient = new HttpClient(httpMessageHandlerMock);
        httpClient.BaseAddress = new Uri("https://base.address.com");
        A.CallTo(() => _tokenService.GetAuthorizedClient<CallbackService>(_options.Value, A<CancellationToken>._))
            .Returns(httpClient);
        var sut = new CallbackService(_tokenService, _options);

        // Act
        await sut.SendTechnicalUserCallback(Guid.NewGuid(), "https://example.org/token", "cl1", "test123", CancellationToken.None);

        // Assert
        httpMessageHandlerMock.RequestMessage.Should().Match<HttpRequestMessage>(x =>
            x.Content is JsonContent &&
            (x.Content as JsonContent)!.ObjectType == typeof(AuthenticationDetail) &&
            ((x.Content as JsonContent)!.Value as AuthenticationDetail)!.ClientId == "cl1" &&
            ((x.Content as JsonContent)!.Value as AuthenticationDetail)!.ClientSecret == "test123"
        );
    }

    [Fact]
    public async Task SendTechnicalUserCallback_WithInvalidData_ThrowsServiceException()
    {
        // Arrange
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.BadRequest);
        using var httpClient = new HttpClient(httpMessageHandlerMock);
        httpClient.BaseAddress = new Uri("https://base.address.com");
        A.CallTo(() => _tokenService.GetAuthorizedClient<CallbackService>(_options.Value, A<CancellationToken>._)).Returns(httpClient);
        var sut = new CallbackService(_tokenService, _options);

        // Act
        async Task Act() => await sut.SendTechnicalUserCallback(Guid.NewGuid(), "https://example.org/token", "cl1", "test123", CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);
        ex.Message.Should().Contain("call to external system send-technical-user-callback failed with statuscode");
    }

    #endregion

    #region SendTechnicalUserDeletionCallback

    [Fact]
    public async Task SendTechnicalUserDeletionCallback_WithValidData_DoesNotThrowException()
    {
        // Arrange
        var externalId = Guid.NewGuid();
        var httpMessageHandlerMock =
            new HttpMessageHandlerMock(HttpStatusCode.OK);
        using var httpClient = new HttpClient(httpMessageHandlerMock);
        httpClient.BaseAddress = new Uri("https://base.address.com");
        A.CallTo(() => _tokenService.GetAuthorizedClient<CallbackService>(_options.Value, A<CancellationToken>._))
            .Returns(httpClient);
        var sut = new CallbackService(_tokenService, _options);

        // Act
        await sut.SendTechnicalUserDeletionCallback(externalId, CancellationToken.None);

        // Assert
        httpMessageHandlerMock.RequestMessage.Should().Match<HttpRequestMessage>(x =>
            x.RequestUri!.AbsoluteUri.Contains($"{externalId}/delete")
        );
    }

    [Fact]
    public async Task SendTechnicalUserDeletionCallback_WithInvalidData_ThrowsServiceException()
    {
        // Arrange
        var httpMessageHandlerMock = new HttpMessageHandlerMock(HttpStatusCode.BadRequest);
        using var httpClient = new HttpClient(httpMessageHandlerMock);
        httpClient.BaseAddress = new Uri("https://base.address.com");
        A.CallTo(() => _tokenService.GetAuthorizedClient<CallbackService>(_options.Value, A<CancellationToken>._)).Returns(httpClient);
        var sut = new CallbackService(_tokenService, _options);

        // Act
        async Task Act() => await sut.SendTechnicalUserDeletionCallback(Guid.NewGuid(), CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<ServiceException>(Act);
        ex.Message.Should().Contain("call to external system send-technical-user-deletion-callback failed with statuscode");
    }

    #endregion
}
