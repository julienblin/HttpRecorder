using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        [Fact]
        public async Task ItShouldGetJson()
        {
            var iterations = new[]
            {
                HttpRecorderMode.Passthrough,
                HttpRecorderMode.Record,
                HttpRecorderMode.Replay,
                HttpRecorderMode.Auto,
            };
            var responses = new List<HttpResponseMessage>();
            HttpResponseMessage passthroughResponse = null;
            foreach (var mode in iterations)
            {
                var(client, file) = CreateHttpClient(mode);

                var response = await client.GetAsync(ApiController.JsonUri);
                responses.Add(response);

                response.EnsureSuccessStatusCode();
                if (mode == HttpRecorderMode.Passthrough)
                {
                    passthroughResponse = response;
                    var result = await response.Content.ReadAsAsync<SampleModel>();
                    result.Name.Should().Be(SampleModel.DefaultName);
                }
                else
                {
                    response.Should().BeEquivalentTo(passthroughResponse);
                }
            }
        }

        [Theory]
        [InlineData(HttpRecorderMode.Passthrough)]
        public async Task ItShouldGetJsonWithQueryString(HttpRecorderMode mode)
        {
            var(client, file) = CreateHttpClient(mode);
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
            var(client, file) = CreateHttpClient(mode);
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
            var(client, file) = CreateHttpClient(mode);
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
            var(client, file) = CreateHttpClient(mode);
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

        private(HttpClient client, string testName) CreateHttpClient(HttpRecorderMode mode, [CallerMemberName] string testName = "")
        {
            return (
                new HttpClient(new HttpRecorderDelegatingHandler(testName, mode: mode))
                {
                    BaseAddress = _fixture.ServerUri,
                },
                testName);
        }
    }
}
