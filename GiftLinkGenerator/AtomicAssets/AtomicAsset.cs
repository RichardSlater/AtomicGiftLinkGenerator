// SPDX-License-Identifier: MIT
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace GiftLinkGenerator.AtomicAssets;

public class AtomicAsset {
    public required string AssetId { get; init; }
    public required string Owner { get; init; }
    public bool IsTransferable { get; set; }
    public int TemplateId { get; set; }
    public required string Name { get; init; }
    public long Mint { get; set; }
}