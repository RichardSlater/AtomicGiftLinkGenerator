// SPDX-License-Identifier: MIT
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace GiftLinkGenerator.AtomicAssets;

public record AtomicToolsLinkRecord {
    public required string AssetId { get; init; }
    public required string Name { get; init; }
    public required string GiftPublicKey { get; init; }
    public required string GiftPrivateKey { get; init; }
    public string? LinkId { get; set; }
    public string TransactionId { get; set; } = "Unknown";
    public LinkStatus Status { get; set; } = LinkStatus.Failed;
    public DateTimeOffset Created { get; set; }
    public Uri? GiftLinkUri { get; set; }
}