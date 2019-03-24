using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FluentAssertions;
using HttpRecorder;
using HttpRecorderTests.Server;
using Xunit;

namespace HttpRecorderTests
{
    [Collection(ServerCollection.Name)]
    public class HttpRecorderDelegatingHandlerTests
    {
        private readonly ServerFixture _fixture;

        public HttpRecorderDelegatingHandlerTests(ServerFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task ItShouldGetJsonInPassthrough()
        {
            var client = CreateHttpClient(HttpRecorderMode.Passthrough);

            var response = await client.GetAsync(ApiController.GetJsonUri);

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsAsync<JsonModel>();
            result.Name.Should().Be(new JsonModel().Name);
        }

        private HttpClient CreateHttpClient(HttpRecorderMode mode, [CallerMemberName] string testName = "")
        {
            return new HttpClient(new HttpRecorderDelegatingHandler(testName, mode: mode))
            {
                BaseAddress = _fixture.ServerUri,
            };
        }
    }
}
