using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HttpRecorder;
using Xunit;

namespace Sample
{
    public class SampleIntegrationTests
    {
        [Fact]
        public async Task ItShould()
        {
            // Initialize the HttpClient with the recorded file
            // stored in a fixture repository.
            var client = CreateHttpClient();

            // Performs HttpClient operations.
            // The interaction is recorded if there are no record,
            // or replayed if there are
            // (without actually hitting the target API).
            // Fixture is recorded in the SampleIntegrationTestsFixtures\ItShould.har file.
            var response = await client.GetAsync("api/user");

            // Performs assertions.
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private HttpClient CreateHttpClient(
            [CallerMemberName] string testName = "",
            [CallerFilePath] string filePath = "")
        {
            // The location of the file where the interaction is recorded.
            // We use the C# CallerMemberName/CallerFilePath attributes to
            // automatically set an appropriate path based on the test case.
            var interactionFilePath = Path.Join(
                Path.GetDirectoryName(filePath),
                $"{Path.GetFileNameWithoutExtension(filePath)}Fixtures",
                testName);

            // Initialize the HttpClient with HttpRecorderDelegatingHandler, which
            // records and replays the interactions.
            return new HttpClient(new HttpRecorderDelegatingHandler(interactionFilePath))
            {
                BaseAddress = new Uri("https://reqres.in/"),
            };
        }
    }
}
