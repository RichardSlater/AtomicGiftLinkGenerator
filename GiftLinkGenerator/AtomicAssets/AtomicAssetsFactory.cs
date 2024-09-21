// SPDX-License-Identifier: MIT

using System.Text.Json.Nodes;

namespace GiftLinkGenerator.AtomicAssets;

public class AtomicAssetsFactory : IAtomicAssetsFactory {
    private const string MissingData = "Missing Data";
    private const string AssetId = "asset_id";
    private const string Mint = "mint";
    private const string Template = "template";
    private const string TemplateId = "template_id";
    private const string Name = "name";
    private const string IsTransferable = "is_transferable";
    private const string Owner = "owner";

    public AtomicAsset CreateAtomicAsset(JsonNode node) {
        AtomicAsset atomicAsset = new() {
            AssetId = node[AssetId]?.GetValue<string>() ?? MissingData,
            Owner = node[Owner]?.GetValue<string>() ?? MissingData,
            Name = node[Name]?.GetValue<string>() ?? MissingData
        };

        if (long.TryParse(node[Mint]?.GetValue<string>(), out var mint)) atomicAsset.Mint = mint;

        if (int.TryParse(node[Template]?[TemplateId]?.GetValue<string>(), out var templateId))
            atomicAsset.TemplateId = templateId;

        atomicAsset.IsTransferable = node[IsTransferable]?.GetValue<bool>() ?? false;

        return atomicAsset;
    }

    public IEnumerable<AtomicAsset> CreateAtomicAssets(IEnumerable<JsonNode> nodes) {
        return nodes.Select(CreateAtomicAsset);
    }

    public AtomicToolsGiftLink CreateLink(JsonNode node) {
        AtomicToolsGiftLink atomicAsset = new() {
            Assets = CreateAtomicAssets(node["assets"]!.AsArray().ToArray()!),
            Creator = node["creator"]?.GetValue<string>() ?? MissingData,
            LinkId = node["link_id"]?.GetValue<string>() ?? MissingData,
            PublicKey = node["public_key"]?.GetValue<string>() ?? MissingData,
            State = (LinkState)node["state"]?.GetValue<int>()!,
            ToolsContract = node["tools_contract"]?.GetValue<string>() ?? MissingData
        };

        return atomicAsset;
    }

    public IEnumerable<AtomicToolsGiftLink> CreateLinks(IEnumerable<JsonNode> nodes) {
        return nodes.Select(CreateLink);
    }
}