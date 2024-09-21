// SPDX-License-Identifier: MIT

using Microsoft.Extensions.Options;

namespace GiftLinkGenerator.AtomicAssets;

public class AtomicAssetsClient(
    IHttpClientFactory clientFactory,
    IAtomicAssetsFactory atomicAssetsFactory,
    IOptions<AtomicAssetsOptions> options,
    ILogger<AtomicAssetsClient> logger)
    : AtomicAssetHttpClientBase<AtomicAssetsClient>(clientFactory, logger), IAtomicAssetsClient {
    private readonly AtomicAssetsOptions _options = options.Value;

    public async Task<IEnumerable<AtomicAsset>> GetAccountAssets(int templateId, string owner, int limit = 50,
        CancellationToken cancellationToken = new()) {
        ArgumentException.ThrowIfNullOrWhiteSpace(owner, nameof(owner));
        var uri = BuildAccountAssetUri(templateId, owner);
        var nodeArray = await HttpFetchPageWalkInternal(uri, limit, cancellationToken);
        return atomicAssetsFactory.CreateAtomicAssets(nodeArray);
    }

    public async Task<IEnumerable<AtomicToolsGiftLink>> GetAccountLinks(string creator, LinkState[] states,
        DateTimeOffset after = default, int limit = 50,
        CancellationToken cancellationToken = new()) {
        ArgumentException.ThrowIfNullOrWhiteSpace(creator, nameof(creator));
        var uri = BuildLinksUri(creator, states, after);
        var nodeArray = await HttpFetchPageWalkInternal(uri, limit, cancellationToken);
        return atomicAssetsFactory.CreateLinks(nodeArray);
    }

    private Uri BuildAccountAssetUri(int templateId, string owner) {
        var resourceUri = $"{_options.BaseUri}{_options.Endpoints.AtomicAssets.Assets}";
        Dictionary<string, string> parameters = new() {
            { "template_id", templateId.ToString() },
            { "owner", owner }
        };
        var builder = new UriBuilder(resourceUri) {
            Query = FormatQueryString(parameters)
        };
        return builder.Uri;
    }

    private Uri BuildLinksUri(string creator, LinkState[] states, DateTimeOffset after = default) {
        var resourceUri = $"{_options.BaseUri}{_options.Endpoints.AtomicTools.Links}";
        Dictionary<string, string> parameters = new() {
            { "creator", creator },
            { "state", string.Join(",", states.Select(x => ((int)x).ToString())) },
        };

        if (after != default) parameters.Add("after", after.ToUnixTimeMilliseconds().ToString());

        var builder = new UriBuilder(resourceUri) {
            Query = FormatQueryString(parameters)
        };
        return builder.Uri;
    }
}