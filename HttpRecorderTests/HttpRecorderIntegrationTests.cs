using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using HttpRecorder;
using HttpRecorderTests.Server;
using Xunit;

namespace HttpRecorderTests
{
    [Collection(ServerCollection.Name)]
    public class HttpRecorderIntegrationTests
    {
        private readonly ServerFixture _fixture;

        public HttpRecorderIntegrationTests(ServerFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory]
        [InlineData(HttpRecorderMode.Passthrough)]
        public async Task ItShouldGetJson(HttpRecorderMode mode)
        {
            var client = CreateHttpClient(mode);

            var response = await client.GetAsync(ApiController.JsonUri);

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsAsync<SampleModel>();
            result.Name.Should().Be(SampleModel.DefaultName);
        }

        [Theory]
        [InlineData(HttpRecorderMode.Passthrough)]
        public async Task ItShouldGetJsonWithQueryString(HttpRecorderMode mode)
        {
            var client = CreateHttpClient(mode);
            var name = "Bar";

            var response = await client.GetAsync($"{ApiController.JsonUri}?name={name}");

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsAsync<SampleModel>();
            result.Name.Should().Be(name);
        }

        [Theory]
        [InlineData(HttpRecorderMode.Passthrough)]
        public async Task ItShouldPostJson(HttpRecorderMode mode)
        {
            var client = CreateHttpClient(mode);
            var sampleModel = new SampleModel();

            var response = await client.PostAsJsonAsync(ApiController.JsonUri, sampleModel);

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsAsync<SampleModel>();
            result.Name.Should().Be(sampleModel.Name);
        }

        [Theory]
        [InlineData(HttpRecorderMode.Passthrough)]
        public async Task ItShouldPostFormData(HttpRecorderMode mode)
        {
            var client = CreateHttpClient(mode);
            var sampleModel = new SampleModel();

            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("name", sampleModel.Name),
            });

            var response = await client.PostAsync(ApiController.FormDataUri, formContent);

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsAsync<SampleModel>();
            result.Name.Should().Be(sampleModel.Name);
        }

        [Theory]
        [InlineData(HttpRecorderMode.Passthrough)]
        public async Task ItShouldExecuteMultipleRequestsInParallel(HttpRecorderMode mode)
        {
            const int Concurrency = 10;
            var client = CreateHttpClient(mode);
            var tasks = new List<Task<HttpResponseMessage>>();

            for (var i = 0; i < Concurrency; i++)
            {
                tasks.Add(client.GetAsync($"{ApiController.JsonUri}?name={i}"));
            }

            var responses = await Task.WhenAll(tasks);

            for (var i = 0; i < Concurrency; i++)
            {
                var response = responses[i];
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsAsync<SampleModel>();
                result.Name.Should().Be($"{i}");
            }
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
