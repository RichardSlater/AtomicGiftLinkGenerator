// SPDX-License-Identifier: MIT

using GetPass;
using GiftLinkGenerator.AtomicAssets;
using GiftLinkGenerator.Crypto;
using GiftLinkGenerator.Wax;
using Microsoft.Extensions.Options;

namespace GiftLinkGenerator;

public class AddWalletWorker(
    ILogger<AddWalletWorker> logger,
    IWalletService walletService,
    AddWalletCommandLineOptions commandLineOptions
    ) : BackgroundService {
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandLineOptions.Actor);
        ArgumentException.ThrowIfNullOrWhiteSpace(commandLineOptions.Permission);
        
        var privateKey = ConsolePasswordReader.Read("Enter the private key, and press enter:");
        var password = ConsolePasswordReader.Read("Enter the Password key, and press enter:");
        await walletService.AddAccount(commandLineOptions.Actor, commandLineOptions.Permission, privateKey, password);
        logger.LogInformation("All done, press Ctrl+C / Cmd+C to exit.");
    }
}