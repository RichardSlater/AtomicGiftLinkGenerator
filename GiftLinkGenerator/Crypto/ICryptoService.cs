// SPDX-License-Identifier: MIT

namespace GiftLinkGenerator.Crypto;

public interface ICryptoService {
    string Encrypt(string plaintext, string password);
    string? Decrypt(string ciphertext, string password);
}