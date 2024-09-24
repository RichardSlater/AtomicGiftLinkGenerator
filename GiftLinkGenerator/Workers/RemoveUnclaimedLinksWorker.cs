// SPDX-License-Identifier: MIT

using GiftLinkGenerator.AtomicAssets;
using GiftLinkGenerator.Wax;
using Microsoft.Extensions.Options;

namespace GiftLinkGenerator.Workers;

public class RemoveUnclaimedLinksWorker(
    ILogger<RemoveUnclaimedLinksWorker> logger,
    IAtomicAssetsClient atomicAssetsClient,
    IAtomicToolsClient atomicToolsClient,
    IOptions<WaxOptions> waxOptions,
    CancelUnclaimedCommandLineOptions commandLineOptions,
    IHostApplicationLifetime applicationLifetime
    ) : BackgroundService {
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        if (logger.IsEnabled(LogLevel.Information)) {
            logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        }
        
        await Task.Delay(TimeSpan.FromSeconds(0.5), stoppingToken);

        var owner = waxOptions.Value.Account;
        var links = await atomicAssetsClient.GetAccountLinks(owner, [LinkState.Created, LinkState.Waiting],
            cancellationToken: stoppingToken);

        var filteredGiftLinks = commandLineOptions.LinkId > 0
            ? links.Where(x => x.LinkId == commandLineOptions.LinkId.ToString()).ToArray()
            : links as AtomicToolsGiftLink[] ?? links.ToArray();
        
        var dirty = false;
        foreach (var link in filteredGiftLinks) {
            logger.LogInformation("Removing unclaimed link: #{link} (Asset ID(s): {assetIds})", link.LinkId,
                string.Join(", ", link.Assets.Select(x => x.AssetId)));
            var result = await atomicToolsClient.CancelLink(link);
            if (!result) dirty = true;
        }

        if (dirty) logger.LogWarning("One or more gift links could not be cancelled.");

        applicationLifetime.StopApplication();
    }
}