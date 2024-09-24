// SPDX-License-Identifier: MIT

using GetPass;
using GiftLinkGenerator.Crypto;

namespace GiftLinkGenerator.Workers;

public class AddWalletWorker(
    ILogger<AddWalletWorker> logger,
    IWalletService walletService,
    AddWalletCommandLineOptions commandLineOptions,
    IHostApplicationLifetime applicationLifetime
) : BackgroundService {
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandLineOptions.Actor);
        ArgumentException.ThrowIfNullOrWhiteSpace(commandLineOptions.Permission);

        await Task.Delay(TimeSpan.FromSeconds(0.5), stoppingToken);

        logger.LogInformation("Adding wallet for {actor} with permission name {permission}", commandLineOptions.Actor,
            commandLineOptions.Permission);
        
        var privateKey = ConsolePasswordReader.Read("Enter the private key, and press enter:");
        var password = ConsolePasswordReader.Read("Enter the password, and press enter:");
        await walletService.AddAccount(commandLineOptions.Actor, commandLineOptions.Permission, privateKey, password);
        
        logger.LogInformation("Success!");
        
        applicationLifetime.StopApplication();
    }
}