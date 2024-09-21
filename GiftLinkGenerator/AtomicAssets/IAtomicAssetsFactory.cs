// SPDX-License-Identifier: MIT
using System.Text.Json.Nodes;

namespace GiftLinkGenerator.AtomicAssets;

public interface IAtomicAssetsFactory {
    AtomicAsset CreateAtomicAsset(JsonNode node);
    IEnumerable<AtomicAsset> CreateAtomicAssets(IEnumerable<JsonNode> nodes);
    AtomicToolsGiftLink CreateLink(JsonNode node);
    IEnumerable<AtomicToolsGiftLink> CreateLinks(IEnumerable<JsonNode> nodes);
}