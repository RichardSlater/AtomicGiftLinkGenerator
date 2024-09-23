// SPDX-License-Identifier: MIT

using GiftLinkGenerator.Crypto;

namespace GiftLinkGenerator.Workers;

public class DeleteWalletWorker(
    ILogger<DeleteWalletWorker> logger,
    IWalletService walletService,
    DeleteWalletCommandLineOptions commandLineOptions,
    IHostApplicationLifetime applicationLifetime
) : BackgroundService {
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandLineOptions.Actor);
        ArgumentException.ThrowIfNullOrWhiteSpace(commandLineOptions.Permission);
        
        await Task.Delay(TimeSpan.FromSeconds(0.5), stoppingToken);

        if (!await walletService.TestActor(commandLineOptions.Actor, commandLineOptions.Permission)) {
            logger.LogWarning("Actor `{actor}` with permission `{permission}` does not exist",
                commandLineOptions.Actor, commandLineOptions.Permission);
            applicationLifetime.StopApplication();
            return;
        }

        logger.LogInformation("Removing wallet for `{actor}` with permission `{permission}`",
            commandLineOptions.Actor, commandLineOptions.Permission);
        await walletService.Remove(commandLineOptions.Actor, commandLineOptions.Permission);

        applicationLifetime.StopApplication();
    }
}