using System;
using System.Collections.Specialized;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Scryfall.Api
{
    public class ScryfallApiError : Exception
    {
        public Error Error { get; }

        public ScryfallApiError(Error error)
        {
            Error = error;
        }
    }

    public partial class ApiClient
    {
        private readonly HttpClient _client;
        private readonly JsonSerializer _serializer;

        public ApiClient(HttpClient client, JsonSerializer serializer)
        {
            _client = client;
            _serializer = serializer;
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        private async Task<T> SendRequest<T>(HttpMethod method, string path, NameValueCollection query)
        {
            var requestUri = new UriBuilder(_baseUri) {Path = path, Query = query.ToString()}.Uri;
            using (var request = new HttpRequestMessage {Method = method, RequestUri = requestUri})
            using (var response = await _client.SendAsync(request))
            {
                if (!response.IsSuccessStatusCode)
                {
                    throw new ScryfallApiError(await ReadContentAsJson<Error>(response));
                }

                return await ReadContentAsJson<T>(response);
            }
        }

        private async Task<T> ReadContentAsJson<T>(HttpResponseMessage response)
        {
            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var streamReader = new StreamReader(stream))
            using (var reader = new JsonTextReader(streamReader))
                return _serializer.Deserialize<T>(reader);
        }
    }
}