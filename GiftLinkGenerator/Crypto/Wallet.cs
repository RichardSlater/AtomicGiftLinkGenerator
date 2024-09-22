// SPDX-License-Identifier: MIT

namespace GiftLinkGenerator.Crypto;

public class Wallet {
    public required string Actor { get; set; }
    public required string Permission { get; set; }
    public required string PrivateKey { get; set; }
}