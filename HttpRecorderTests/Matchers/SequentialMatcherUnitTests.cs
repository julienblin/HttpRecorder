using System;
using System.Linq;
using System.Net.Http;
using FluentAssertions;
using HttpRecorder;
using HttpRecorder.Matchers;
using Newtonsoft.Json;
using Xunit;

namespace HttpRecorderTests.Matchers
{
    public class SequentialMatcherUnitTests
    {
        [Fact]
        public void ItShouldMatchInSequence()
        {
            var interaction = BuildInteraction(
                new HttpRequestMessage { RequestUri = new Uri("http://first") },
                new HttpRequestMessage { RequestUri = new Uri("http://second") });
            var request = new HttpRequestMessage();

            var matcher = SequentialMatcher.Match;

            var result = ((IRequestMatcher)matcher).Match(request, interaction);

            result.Response.RequestMessage.RequestUri.Should().BeEquivalentTo(new Uri("http://first"));
            interaction.Messages.Should().HaveCount(1);

            result = ((IRequestMatcher)matcher).Match(request, interaction);

            result.Should().NotBeNull();
            result.Response.RequestMessage.RequestUri.Should().BeEquivalentTo(new Uri("http://second"));
            interaction.Messages.Should().HaveCount(0);
        }

        [Fact]
        public void ItShouldMatchByHttpMethod()
        {
            var interaction = BuildInteraction(
                new HttpRequestMessage(),
                new HttpRequestMessage { RequestUri = new Uri("http://first"), Method = HttpMethod.Get },
                new HttpRequestMessage { RequestUri = new Uri("http://second"), Method = HttpMethod.Head });
            var request = new HttpRequestMessage { Method = HttpMethod.Head };

            var matcher = SequentialMatcher.Match
                .ByHttpMethod();

            var result = ((IRequestMatcher)matcher).Match(request, interaction);

            result.Should().NotBeNull();
            result.Response.RequestMessage.Method.Should().BeEquivalentTo(HttpMethod.Head);
            interaction.Messages.Should().HaveCount(2);
        }

        [Fact]
        public void ItShouldMatchByCompleteRequestUri()
        {
            var interaction = BuildInteraction(
                new HttpRequestMessage(),
                new HttpRequestMessage { RequestUri = new Uri("http://first?name=foo") },
                new HttpRequestMessage { RequestUri = new Uri("http://first?name=bar") });
            var request = new HttpRequestMessage { RequestUri = new Uri("http://first?name=bar") };

            var matcher = SequentialMatcher.Match
                .ByRequestUri();

            var result = ((IRequestMatcher)matcher).Match(request, interaction);

            result.Should().NotBeNull();
            result.Response.RequestMessage.RequestUri.Should().BeEquivalentTo(new Uri("http://first?name=bar"));
            interaction.Messages.Should().HaveCount(2);
        }

        [Fact]
        public void ItShouldMatchByPartialRequestUri()
        {
            var interaction = BuildInteraction(
                new HttpRequestMessage(),
                new HttpRequestMessage { RequestUri = new Uri("http://first?name=foo") },
                new HttpRequestMessage { RequestUri = new Uri("http://first?name=bar") });
            var request = new HttpRequestMessage { RequestUri = new Uri("http://first?name=bar") };

            var matcher = SequentialMatcher.Match
                .ByRequestUri(UriPartial.Path);

            var result = ((IRequestMatcher)matcher).Match(request, interaction);

            result.Should().NotBeNull();
            result.Response.RequestMessage.RequestUri.Should().BeEquivalentTo(new Uri("http://first?name=foo"));
            interaction.Messages.Should().HaveCount(2);
        }

        [Fact]
        public void ItShouldMatchByHeader()
        {
            var headerName = "If-None-Match";
            var firstRequest = new HttpRequestMessage();
            firstRequest.Headers.TryAddWithoutValidation(headerName, "first");
            var secondRequest = new HttpRequestMessage();
            secondRequest.Headers.TryAddWithoutValidation(headerName, "second");
            var interaction = BuildInteraction(new HttpRequestMessage(), firstRequest, secondRequest);
            var request = new HttpRequestMessage();
            request.Headers.TryAddWithoutValidation(headerName, "second");

            var matcher = SequentialMatcher.Match
                .ByHeader(headerName);

            var result = ((IRequestMatcher)matcher).Match(request, interaction);

            result.Should().NotBeNull();
            result.Response.RequestMessage.Headers.IfNoneMatch.ToString().Should().Be("second");
            interaction.Messages.Should().HaveCount(2);
        }

        [Fact]
        public void ItShouldMatchByContent()
        {
            var firstContent = new ByteArrayContent(new byte[] { 0, 1, 2, 3 });
            var secondContent = new ByteArrayContent(new byte[] { 3, 2, 1, 0 });
            var interaction = BuildInteraction(
                new HttpRequestMessage(),
                new HttpRequestMessage { Content = firstContent },
                new HttpRequestMessage { Content = secondContent });
            var request = new HttpRequestMessage { Content = secondContent };

            var matcher = SequentialMatcher.Match
                .ByContent();

            var result = ((IRequestMatcher)matcher).Match(request, interaction);

            result.Should().NotBeNull();
            result.Response.RequestMessage.Content.Should().BeEquivalentTo(secondContent);
            interaction.Messages.Should().HaveCount(2);
        }

        [Fact]
        public void ItShouldMatchByJsonContent()
        {
            var firstModel = new Model { Name = "first" };
            var secondModel = new Model { Name = "second" };
            var firstContent = new StringContent(JsonConvert.SerializeObject(firstModel));
            var secondContent = new StringContent(JsonConvert.SerializeObject(secondModel));

            var interaction = BuildInteraction(
                new HttpRequestMessage(),
                new HttpRequestMessage { Content = firstContent },
                new HttpRequestMessage { Content = secondContent });
            var request = new HttpRequestMessage { Content = secondContent };

            var matcher = SequentialMatcher.Match
                .ByJsonContent<Model>();

            var result = ((IRequestMatcher)matcher).Match(request, interaction);

            result.Should().NotBeNull();
            result.Response.RequestMessage.Content.Should().BeEquivalentTo(secondContent);
            interaction.Messages.Should().HaveCount(2);
        }

        private Interaction BuildInteraction(params HttpRequestMessage[] requests)
        {
            return new Interaction(
                "test",
                requests.Select(x => new InteractionMessage(
                    new HttpResponseMessage { RequestMessage = x },
                    new InteractionMessageTimings(DateTimeOffset.UtcNow, TimeSpan.MinValue))));
        }

        private class Model
        {
            public string Name { get; set; }

            public override bool Equals(object obj)
            {
                return Equals(obj as Model);
            }

            public bool Equals(Model other)
            {
                if (other == null)
                {
                    return false;
                }

                return string.Equals(Name, other.Name, StringComparison.InvariantCulture);
            }

            public override int GetHashCode() => Name == null ? 0 : Name.GetHashCode(StringComparison.InvariantCulture);
        }
    }
}
