// SPDX-License-Identifier: MIT

using GiftLinkGenerator.Crypto;

namespace GiftLinkGenerator;

public class DeleteWalletWorker(
    ILogger<DeleteWalletWorker> logger,
    IWalletService walletService,
    DeleteWalletCommandLineOptions commandLineOptions
    ) : BackgroundService {
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandLineOptions.Actor);
        ArgumentException.ThrowIfNullOrWhiteSpace(commandLineOptions.Permission);
        
        await walletService.Remove(commandLineOptions.Actor, commandLineOptions.Permission);
        
        logger.LogInformation("All done, press Ctrl+C / Cmd+C to exit.");
    }
}