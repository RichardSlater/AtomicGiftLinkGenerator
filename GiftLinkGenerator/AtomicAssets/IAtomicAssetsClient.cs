// SPDX-License-Identifier: MIT

namespace GiftLinkGenerator.AtomicAssets;

public interface IAtomicAssetsClient {
    Task<IEnumerable<AtomicAsset>> GetAccountAssets(int templateId, string owner, int limit = 50,
        CancellationToken cancellationToken = new());

    Task<IEnumerable<AtomicToolsGiftLink>> GetAccountLinks(string creator, LinkState[] states,
        DateTimeOffset after = default, int limit = 50,
        CancellationToken cancellationToken = new());
}