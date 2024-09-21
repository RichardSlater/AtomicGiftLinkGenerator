// SPDX-License-Identifier: MIT
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace GiftLinkGenerator.AtomicAssets;

public class AtomicToolsGiftLink {
    public string ToolsContract { get; init; } = "atomictoolsx";
    public required string LinkId { get; init; }
    public required string Creator { get; init; }
    public LinkState State { get; init; }
    public required string PublicKey { get; init; }
    public required IEnumerable<AtomicAsset> Assets { get; init; }
}