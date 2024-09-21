// SPDX-License-Identifier: MIT
// ReSharper disable UnusedMember.Global

namespace GiftLinkGenerator.AtomicAssets;

public enum LinkState : uint {
    Waiting = 0, // Link created but items were not transferred yet
    Created = 1, // Link is pending
    Cancelled = 2, // Creator canceled link
    Claimed = 3 // Link was claimed
}