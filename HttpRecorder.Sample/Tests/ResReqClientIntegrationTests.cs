using System;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FluentAssertions;
using HttpRecorder.Matchers;
using HttpRecorder.Sample.Client;
using Xunit;

namespace HttpRecorder.Sample.Tests
{
    /// <summary>
    /// This class demonstrates how to test <see cref="ReqResClient"/> using <see cref="HttpRecorderDelegatingHandler"/>.
    /// </summary>
    public class ResReqClientIntegrationTests
    {
        /// <summary>
        /// This test sample shows the default behavior of the <see cref="HttpRecorderDelegatingHandler"/>
        /// </summary>
        [Fact]
        public async Task DemonstrateStandardConfiguration()
        {
            var request = new EmailPasswordRequest
            {
                Email = "john.doe@example.com",
                Password = "Passw0rd1"
            };
            var client = new ReqResClient(CreateHttpClient());

            var result = await client.Register(request);

            result.Token.Should().NotBeNullOrEmpty();
        }

        /// <summary>
        /// This test sample shows the <see cref="HttpRecorderMode.Passthrough"/> behavior of the <see cref="HttpRecorderDelegatingHandler"/>.
        /// Requests are not recorded and nothing is changed from standard behavior.
        /// </summary>
        [Fact]
        public async Task DemonstratePassthrough()
        {
            var request = new EmailPasswordRequest
            {
                Email = "john.doe@example.com",
                Password = "Passw0rd1"
            };
            var client = new ReqResClient(CreateHttpClient(mode: HttpRecorderMode.Passthrough));

            var result = await client.Register(request);

            result.Token.Should().NotBeNullOrEmpty();
        }

        /// <summary>
        /// This test sample shows custom matching.
        /// It forces replay-only mode and only match the host of the request, and match it multiple times.
        /// Open the DemonstrateCustomMatching.har file to see the stubbed requests.
        ///
        /// It also shows how you can craft interactions by hand, as the DemonstrateCustomMatching.har file is very minimal, but it still works.
        /// </summary>
        [Fact]
        public async Task DemonstrateCustomMatching()
        {
            var request = new EmailPasswordRequest
            {
                Email = "john.doe@example.com",
                Password = "Passw0rd1"
            };
            var client = new ReqResClient(
                CreateHttpClient(
                    mode: HttpRecorderMode.Replay,
                    matcher: RulesMatcher.MatchMultiple.ByRequestUri(UriPartial.Authority)));

            // First time
            var result = await client.Register(request);
            result.Token.Should().NotBeNullOrEmpty();

            // Second time - this works because of the MatchMultiple configuration.
            result = await client.Register(request);
            result.Token.Should().NotBeNullOrEmpty();
        }

        private HttpClient CreateHttpClient(
            [CallerMemberName] string testName = "",
            [CallerFilePath] string filePath = "",
            HttpRecorderMode mode = HttpRecorderMode.Auto,
            IRequestMatcher matcher = null)
        {
            var interactionName = Path.Join(
                Path.GetDirectoryName(filePath),
                $"{Path.GetFileNameWithoutExtension(filePath)}Fixtures",
                testName);

            return new HttpClient(new HttpRecorderDelegatingHandler(interactionName, mode: mode, matcher: matcher))
            {
                BaseAddress = new Uri("https://reqres.in/"),
            };
        }
    }
}
