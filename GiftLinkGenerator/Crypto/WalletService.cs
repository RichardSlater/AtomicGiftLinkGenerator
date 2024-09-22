// SPDX-License-Identifier: MIT

using System.Text.Json.Nodes;
using Newtonsoft.Json;

namespace GiftLinkGenerator.Crypto;

public class WalletService(ICryptoService cryptoService) : IWalletService {
    private readonly string _walletFile = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        @".config\giftlinkgen_wallet.json");

    public async Task AddAccount(string actor, string permission, string privateKey, string password) {
        var wallets = await ReadWallet();

        var encryptedPrivateKey = cryptoService.Encrypt(privateKey, password);

        var wallet = new Wallet {
            Actor = actor,
            Permission = permission,
            PrivateKey = encryptedPrivateKey
        };

        if (wallets.Any(x => x.Actor == actor && x.Permission == permission)) {
            wallets = wallets.Where(x => x.Actor != actor && x.Permission != permission).ToList();
        }

        wallets.Add(wallet);

        await WriteWallet(wallets);
    }

    public async Task Remove(string actor, string permission) {
        var wallets = await ReadWallet();

        if (wallets.Any(x => x.Actor == actor && x.Permission == permission)) {
            wallets = wallets.Where(x => x.Actor != actor && x.Permission != permission).ToList();
        }

        await WriteWallet(wallets);
    }

    public async Task<string?> GetPrivateKey(string actor, string permission, string password) {
        var wallets = await ReadWallet();
        var wallet = wallets.SingleOrDefault(x => x.Actor == actor && x.Permission == permission);
        return wallet == null ? string.Empty : cryptoService.Decrypt(wallet.PrivateKey, password);
    }
    
    public async Task<bool> TestActor(string actor, string permission) {
        var wallets = await ReadWallet();
        return wallets.Any(x => x.Actor == actor && x.Permission == permission);
    }
    
    private async Task<List<Wallet>> ReadWallet() {
        if (!File.Exists(_walletFile)) {
            return [];
        }
        
        await using var walletText = File.OpenRead(_walletFile);
        var walletJson = await JsonNode.ParseAsync(walletText);
        walletText.Close();
        if (walletJson == null) {
            return new List<Wallet>();
        }

        return walletJson
            .AsArray()
            .Select(node => new Wallet {
                Actor = node!["Actor"]!.GetValue<string>(),
                Permission = node["Permission"]!.GetValue<string>(),
                PrivateKey = node["PrivateKey"]!.GetValue<string>(),
            })
            .ToList();
    }

    private async Task WriteWallet(List<Wallet> wallets) {
        var walletJson = JsonConvert.SerializeObject(wallets);
        var backupFile = $"{_walletFile}.bak";
        if (File.Exists(backupFile)) {
            File.Delete(backupFile);
        }

        if (File.Exists(_walletFile)) {
            File.Move(_walletFile, backupFile);
        }

        await File.WriteAllTextAsync(_walletFile, walletJson);
    }
}