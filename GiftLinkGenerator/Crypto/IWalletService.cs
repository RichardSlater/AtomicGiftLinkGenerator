// SPDX-License-Identifier: MIT

namespace GiftLinkGenerator.Crypto;

public interface IWalletService {
    Task AddAccount(string actor, string permission, string privateKey, string password);
    Task Remove(string actor, string permission);
    Task<string?> GetPrivateKey(string actor, string permission, string password);
    Task<bool> TestActor(string actor, string permission);
}