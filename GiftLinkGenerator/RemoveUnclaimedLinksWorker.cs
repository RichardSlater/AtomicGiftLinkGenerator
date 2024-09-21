// SPDX-License-Identifier: MIT

using GiftLinkGenerator.AtomicAssets;
using GiftLinkGenerator.Wax;
using Microsoft.Extensions.Options;

namespace GiftLinkGenerator;

public class RemoveUnclaimedLinksWorker(
    ILogger<RemoveUnclaimedLinksWorker> logger,
    IAtomicAssetsClient atomicAssetsClient,
    IAtomicToolsClient atomicToolsClient,
    IOptions<WaxOptions> waxOptions) : BackgroundService {
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        if (logger.IsEnabled(LogLevel.Information))
            logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

        var owner = waxOptions.Value.Account;
        var links = await atomicAssetsClient.GetAccountLinks(owner, [LinkState.Created, LinkState.Waiting],
            cancellationToken: stoppingToken);
        var giftLinks = links as AtomicToolsGiftLink[] ?? links.ToArray();

        var dirty = false;
        foreach (var link in giftLinks) {
            logger.LogInformation("Removing unclaimed link: {link} ({assetIds})", link.LinkId,
                string.Join(", ", link.Assets.Select(x => x.AssetId)));
            var result = await atomicToolsClient.CancelLink(link);
            if (!result) dirty = true;
        }

        if (dirty) logger.LogWarning("One or more gift links could not be cancelled.");

        logger.LogInformation("All done, press Ctrl+C / Cmd+C to exit.");
    }
}