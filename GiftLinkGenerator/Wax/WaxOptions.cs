// SPDX-License-Identifier: MIT

using EosSharp.Core;
using EosSharp.Core.Providers;

namespace GiftLinkGenerator.Wax;

public class WaxOptions {
    public string Account { get; init; } = string.Empty;
    public string Actor { get; init; } = string.Empty;
    public string Permission { get; init; } = string.Empty;
    public string GiftContract { get; init; } = string.Empty;
    public string AtomicAssetsContract { get; init; } = string.Empty;
    public string BaseUri { get; init; } = string.Empty;
    public string ChainId { get; init; } = string.Empty;
    public int ExpireSeconds { get; init; } = 120;
    public string? PrivateKey { get; init; } = string.Empty;

    public string GiftMemo { get; init; } = string.Empty;

    public EosConfigurator GetConfigurator(string? privateKey) {
        return new EosConfigurator {
            HttpEndpoint = BaseUri,
            ChainId = ChainId,
            ExpireSeconds = ExpireSeconds,
            SignProvider = new DefaultSignProvider(privateKey)
        };
    }
}