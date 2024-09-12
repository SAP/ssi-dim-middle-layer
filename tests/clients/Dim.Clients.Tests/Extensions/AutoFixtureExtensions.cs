using AutoFixture;
using Dim.Clients.Token;
using FakeItEasy;

namespace Dim.Clients.Tests.Extensions;

public static class AutoFixtureExtensions
{
    public static void ConfigureTokenServiceFixture<T>(this IFixture fixture, HttpResponseMessage httpResponseMessage, Action<HttpRequestMessage?>? setMessage = null)
    {
        var messageHandler = A.Fake<HttpMessageHandler>();
        A.CallTo(messageHandler) // mock protected method
            .Where(x => x.Method.Name == "SendAsync")
            .WithReturnType<Task<HttpResponseMessage>>()
            .ReturnsLazily(call =>
            {
                var message = call.Arguments.Get<HttpRequestMessage>(0);
                setMessage?.Invoke(message);
                return Task.FromResult(httpResponseMessage);
            });
        var httpClient = new HttpClient(messageHandler) { BaseAddress = new Uri("https://example.com/path/test/") };
        fixture.Inject(httpClient);

        var tokenService = fixture.Freeze<Fake<IBasicAuthTokenService>>();
        A.CallTo(() => tokenService.FakedObject.GetBasicAuthorizedClient<T>(A<BasicAuthSettings>._, A<CancellationToken>._)).Returns(httpClient);
    }
}
