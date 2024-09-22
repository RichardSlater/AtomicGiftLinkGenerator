// SPDX-License-Identifier: MIT

namespace GiftLinkGenerator.Crypto;

public class CryptoOptions {
    public string HashAlgorithm { get; set; } = "SHA256";
    public string? Salt { get; set; }
    public int Iterations { get; set; } = 10000;
}