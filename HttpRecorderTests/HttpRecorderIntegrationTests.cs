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
            HttpResponseMessage passthroughResponse = null;

            await ExecuteModeIterations(async (client, mode) =>
            {
                var response = await client.GetAsync(ApiController.JsonUri);

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
            });
        }

        [Fact]
        public async Task ItShouldGetJsonWithQueryString()
        {
            HttpResponseMessage passthroughResponse = null;
            var name = "Bar";

            await ExecuteModeIterations(async (client, mode) =>
            {
                var response = await client.GetAsync($"{ApiController.JsonUri}?name={name}");

                response.EnsureSuccessStatusCode();
                if (mode == HttpRecorderMode.Passthrough)
                {
                    passthroughResponse = response;
                    var result = await response.Content.ReadAsAsync<SampleModel>();
                    result.Name.Should().Be(name);
                }
                else
                {
                    response.Should().BeEquivalentTo(passthroughResponse);
                }
            });
        }

        [Fact]
        public async Task ItShouldPostJson()
        {
            var sampleModel = new SampleModel();
            HttpResponseMessage passthroughResponse = null;

            await ExecuteModeIterations(async (client, mode) =>
            {
                var response = await client.PostAsJsonAsync(ApiController.JsonUri, sampleModel);
                response.EnsureSuccessStatusCode();

                if (mode == HttpRecorderMode.Passthrough)
                {
                    passthroughResponse = response;
                    var result = await response.Content.ReadAsAsync<SampleModel>();
                    result.Name.Should().Be(sampleModel.Name);
                }
                else
                {
                    response.Should().BeEquivalentTo(passthroughResponse);
                }
            });
        }

        [Fact]
        public async Task ItShouldPostFormData()
        {
            var sampleModel = new SampleModel();
            HttpResponseMessage passthroughResponse = null;

            await ExecuteModeIterations(async (client, mode) =>
            {
                var formContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("name", sampleModel.Name),
                });

                var response = await client.PostAsync(ApiController.FormDataUri, formContent);
                response.EnsureSuccessStatusCode();
                if (mode == HttpRecorderMode.Passthrough)
                {
                    passthroughResponse = response;
                    var result = await response.Content.ReadAsAsync<SampleModel>();
                    result.Name.Should().Be(sampleModel.Name);
                }
                else
                {
                    response.Should().BeEquivalentTo(passthroughResponse);
                }
            });
        }

        [Fact]
        public async Task ItShouldExecuteMultipleRequestsInParallel()
        {
            const int Concurrency = 10;
            IList<HttpResponseMessage> passthroughResponses = null;

            await ExecuteModeIterations(async (client, mode) =>
            {
                var tasks = new List<Task<HttpResponseMessage>>();

                for (var i = 0; i < Concurrency; i++)
                {
                    tasks.Add(client.GetAsync($"{ApiController.JsonUri}?name={i}"));
                }

                var responses = await Task.WhenAll(tasks);

                if (mode == HttpRecorderMode.Passthrough)
                {
                    passthroughResponses = responses;
                    for (var i = 0; i < Concurrency; i++)
                    {
                        var response = responses[i];
                        response.EnsureSuccessStatusCode();
                        var result = await response.Content.ReadAsAsync<SampleModel>();
                        result.Name.Should().Be($"{i}");
                    }
                }
                else
                {
                    responses.Should().BeEquivalentTo(passthroughResponses);
                }
            });
            }

        private async Task ExecuteModeIterations(Func<HttpClient, HttpRecorderMode, Task> test)
        {
            var iterations = new[]
            {
                HttpRecorderMode.Passthrough,
                HttpRecorderMode.Record,
                HttpRecorderMode.Replay,
                HttpRecorderMode.Auto,
            };
            foreach (var mode in iterations)
            {
                var client = CreateHttpClient(mode);
                await test(client, mode);
            }
        }

        private HttpClient CreateHttpClient(HttpRecorderMode mode, [CallerMemberName] string testName = "")
            => new HttpClient(new HttpRecorderDelegatingHandler(testName, mode: mode))
            {
                BaseAddress = _fixture.ServerUri,
            };
    }
}
