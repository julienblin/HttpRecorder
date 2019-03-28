using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using HttpRecorder;
using HttpRecorder.Repositories;
using HttpRecorderTests.Server;
using Moq;
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

        [Fact]
        public async Task ItShouldGetBinary()
        {
            HttpResponseMessage passthroughResponse = null;
            var expectedBinaryContent = await File.ReadAllBytesAsync(typeof(ApiController).Assembly.Location);

            await ExecuteModeIterations(async (client, mode) =>
            {
                var response = await client.GetAsync(ApiController.BinaryUri);

                response.EnsureSuccessStatusCode();
                if (mode == HttpRecorderMode.Passthrough)
                {
                    passthroughResponse = response;
                    var result = await response.Content.ReadAsByteArrayAsync();
                    result.Should().BeEquivalentTo(expectedBinaryContent);
                }
                else
                {
                    response.Should().BeEquivalentTo(passthroughResponse);
                }
            });
        }

        [Fact]
        public async Task ItShouldThrowIfDoesNotFindFile()
        {
            const string TestFile = "unknown.file";
            var client = CreateHttpClient(HttpRecorderMode.Replay, TestFile);

            Func<Task> act = async () => await client.GetAsync(ApiController.JsonUri);

            act.Should().Throw<HttpRecorderException>()
                .WithMessage($"*{TestFile}*");
        }

        [Fact]
        public async Task ItShouldThrowIfFileIsCorrupted()
        {
            var file = typeof(HttpRecorderIntegrationTests).Assembly.Location;
            var client = CreateHttpClient(HttpRecorderMode.Replay, file);

            Func<Task> act = async () => await client.GetAsync(ApiController.JsonUri);

            act.Should().Throw<HttpRecorderException>()
                .WithMessage($"*{file}*");
        }

        [Fact]
        public async Task ItShouldThrowIfNoRequestCanBeMatched()
        {
            var repositoryMock = new Mock<IInteractionRepository>();
            repositoryMock.Setup(x => x.ExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            repositoryMock.Setup(x => x.LoadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns<string, CancellationToken>((interactionName, _) => Task.FromResult(new Interaction(interactionName)));

            var client = CreateHttpClient(HttpRecorderMode.Replay, repository: repositoryMock.Object);

            Func<Task> act = async () => await client.GetAsync(ApiController.JsonUri);

            act.Should().Throw<HttpRecorderException>()
                .WithMessage($"*{ApiController.JsonUri}*");
        }

        [Theory]
        [InlineData(202)]
        [InlineData(301)]
        [InlineData(303)]
        [InlineData(404)]
        [InlineData(500)]
        [InlineData(502)]
        public async Task ItShouldGetStatus(int statusCode)
        {
            HttpResponseMessage passthroughResponse = null;

            await ExecuteModeIterations(async (client, mode) =>
            {
                var response = await client.GetAsync($"{ApiController.StatusCodeUri}?statusCode={statusCode}");
                response.StatusCode.Should().Be(statusCode);
                if (mode == HttpRecorderMode.Passthrough)
                {
                    passthroughResponse = response;
                }
                else
                {
                    response.Should().BeEquivalentTo(passthroughResponse);
                }
            });
        }

        private async Task ExecuteModeIterations(Func<HttpClient, HttpRecorderMode, Task> test, [CallerMemberName] string testName = "")
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
                var client = CreateHttpClient(mode, testName);
                await test(client, mode);
            }
        }

        private HttpClient CreateHttpClient(
            HttpRecorderMode mode,
            [CallerMemberName] string testName = "",
            IInteractionRepository repository = null)
            => new HttpClient(new HttpRecorderDelegatingHandler(testName, mode: mode, repository: repository))
            {
                BaseAddress = _fixture.ServerUri,
            };
    }
}
