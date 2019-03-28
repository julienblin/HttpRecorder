using System;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;

namespace HttpRecorder.Sample.Client
{
    /// <summary>
    /// This sample client uses the publicly available API at https://reqres.in/.
    /// Notice how the constructor takes the <see cref="HttpClient"/> as a dependency.
    /// </summary>
    public class ReqResClient
    {
        private static readonly MediaTypeFormatter _jsonFormatter = new JsonMediaTypeFormatter
        {
            SerializerSettings = new Newtonsoft.Json.JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }
        };

        private readonly HttpClient _httpClient;

        public ReqResClient(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<TokenResponse> Register(EmailPasswordRequest request)
        {
            var response = await _httpClient.PostAsync("api/register", request, _jsonFormatter);
            return await response.Content.ReadAsAsync<TokenResponse>();
        }
    }
}
