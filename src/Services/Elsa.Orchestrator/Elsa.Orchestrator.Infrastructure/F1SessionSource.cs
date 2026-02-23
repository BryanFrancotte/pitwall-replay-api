using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elsa.Orchestrator.Infrastructure
{
    public sealed class F1SessionSource
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public F1SessionSource(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _baseUrl = config["F1:BaseUrl"] ?? "https://livetiming.formula1.com";
        }

        public async Task<string> GetInfoSessionJsonAsync(string sessionPath, CancellationToken cancellationToken)
        {
            var url = $"{_baseUrl}/{sessionPath}";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
    }
}
