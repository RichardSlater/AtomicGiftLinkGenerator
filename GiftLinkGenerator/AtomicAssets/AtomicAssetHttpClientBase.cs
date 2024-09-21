// SPDX-License-Identifier: MIT

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Web;

namespace GiftLinkGenerator.AtomicAssets;

public abstract class AtomicAssetHttpClientBase<T>(IHttpClientFactory clientFactory, ILogger<T> logger) {
    private readonly JsonDocumentOptions _jsonDocumentOptions = new();
    private readonly JsonNodeOptions _jsonNodeOptions = new();

    protected async Task<IEnumerable<JsonNode>> HttpFetchPageWalkInternal(Uri uri, int limit = 50,
        CancellationToken cancellationToken = default) {
        var page = 1;
        var results = new List<JsonNode>();

        while (true) {
            var paginatedUri = new Uri($"{uri.ToString()}{GeneratePaginationQuery(page, limit)}");
            var result = await HttpFetchInternal(paginatedUri, cancellationToken);
            if (result["data"]!.AsArray().Count == 0) return results.AsEnumerable();

            results.AddRange(result["data"]!.AsArray().ToArray().OfType<JsonNode>());
            page++;
        }
    }

    private async Task<JsonNode> HttpFetchInternal(Uri uri, CancellationToken cancellationToken = default) {
        var client = clientFactory.CreateClient("AtomicAssets");
        var request = new HttpRequestMessage(HttpMethod.Get, uri);
        InvalidOperationException? exception = null;

        for (var tries = 0; tries < 5; tries++) {
            if (tries > 0) {
                var delay = (Math.Pow(5, tries) - 1) / 10;
                logger.LogWarning("A request to {url} encountered a transient failure, delaying for {delay} seconds.",
                    uri, delay);
                await Task.Delay(TimeSpan.FromSeconds(delay), cancellationToken);
            }

            var response = await client.SendAsync(request, cancellationToken);

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStreamAsync(cancellationToken);
            var node = await JsonNode.ParseAsync(content, _jsonNodeOptions, _jsonDocumentOptions, cancellationToken);

            if (node == null) {
                exception = new InvalidOperationException($"The call to {uri} succeeded, but no result was returned.");
                logger.LogWarning(exception.Message);
            }

            if (node!["success"] == null || node["data"] == null) {
                exception = new InvalidOperationException(
                    $"The call to {uri} succeeded, but the results did not contain the expected payload. Expected: {{ \"success\": true, \"data\": [ ... ] }}");
                logger.LogWarning(exception.Message);
            }

            if (!node["success"]!.GetValue<bool>()) {
                exception = new InvalidOperationException(
                    $"The call to {uri} succeeded, but the payload indicates failure.");
                logger.LogWarning(exception.Message);
            }

            return node;
        }

        if (exception is not null) {
            throw exception;
        }

        throw new InvalidOperationException("The call exceeded the retry limit, however no exception was recorded.");
    }

    private string GeneratePaginationQuery(int page, int limit) {
        Dictionary<string, string> paginationParameters = new() {
            { "page", page.ToString() },
            { "limit", limit.ToString() }
        };

        return $"&{FormatQueryString(paginationParameters)}";
    }

    protected string FormatQueryString(Dictionary<string, string> parameters) {
        var formattedParams = parameters
            .Select(kvp => $"{kvp.Key}={HttpUtility.UrlEncode(kvp.Value.ToString())}");
        return string.Join("&", formattedParams);
    }
}