// SPDX-License-Identifier: MIT

namespace GiftLinkGenerator.AtomicAssets;

public interface IAtomicToolsClient {
    Task<AtomicToolsLinkRecord> AnnounceDeposit(AtomicAsset asset);
    Uri BuildGiftLinkUri(string linkId, string linkPrivateKey);
    Task<bool> CancelLink(AtomicToolsGiftLink atomicGiftLink);
}