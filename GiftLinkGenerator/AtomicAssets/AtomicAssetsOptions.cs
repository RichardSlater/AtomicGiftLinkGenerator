// SPDX-License-Identifier: MIT

namespace GiftLinkGenerator.AtomicAssets;

public sealed class AtomicAssetsOptions {
    public string BaseUri { get; init; } = string.Empty;
    public string AtomicToolsBaseUri { get; init; } = string.Empty;
    public int TemplateId { get; init; }
    public AtomicEndpoints Endpoints { get; init; } = new();
}

public class AtomicEndpoints {
    public AtomicMarketEndpoints AtomicMarket { get; init; } = new();
    public AtomicAssetsEndpoints AtomicAssets { get; init; } = new();
    public AtomicToolsEndpoints AtomicTools { get; init; } = new();
}

public class AtomicAssetsEndpoints {
    public string Assets { get; init; } = string.Empty;
}

public class AtomicMarketEndpoints {
    public string Assets { get; init; } = string.Empty;
}

public class AtomicToolsEndpoints {
    public string Links { get; init; } = string.Empty;
    public string TradingLinks { get; init; } = string.Empty;
}