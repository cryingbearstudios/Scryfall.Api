using System;
using System.Collections.Specialized;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Scryfall.Api
{
    public partial class ApiClient
    {
        private const int RateLimitDelayMilliseconds = 50;
        private Task _rateLimitingTask = Task.FromResult(true);

        private readonly HttpClient _client;
        private readonly JsonSerializer _serializer;

        public ApiClient(HttpClient client, JsonSerializer serializer)
        {
            _client = client;
            _serializer = serializer;
        }

        public void Dispose() => _client.Dispose();

        private async Task<T> SendRequest<T>(HttpMethod method, string path, NameValueCollection query)
        {
            await _rateLimitingTask.ConfigureAwait(false);
            var requestUri = new UriBuilder(_baseUri) {Path = path, Query = query.ToString()}.Uri;
            var request = new HttpRequestMessage {Method = method, RequestUri = requestUri};
            var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            _rateLimitingTask = Task.Delay(RateLimitDelayMilliseconds);

            if (response.IsSuccessStatusCode)
                return await ReadContentAsJson<T>(response).ConfigureAwait(false);

            var scryfallError = await ReadContentAsJson<Error>(response).ConfigureAwait(false);
            throw scryfallError;
        }

        private async Task<T> ReadContentAsJson<T>(HttpResponseMessage response)
        {
            var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using (var reader = new JsonTextReader(new StreamReader(stream)))
                return _serializer.Deserialize<T>(reader);
        }
    }
}