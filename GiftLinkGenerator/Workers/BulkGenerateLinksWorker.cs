// SPDX-License-Identifier: MIT

using System.Globalization;
using CsvHelper;
using GiftLinkGenerator.AtomicAssets;
using GiftLinkGenerator.Exceptions;
using GiftLinkGenerator.Wax;
using Microsoft.Extensions.Options;

namespace GiftLinkGenerator.Workers;

public class BulkGenerateLinksWorker(
    ILogger<BulkGenerateLinksWorker> logger,
    IAtomicAssetsClient atomicAssetsClient,
    IAtomicToolsClient atomicToolsClient,
    IOptions<AtomicAssetsOptions> atomicAssetsOptions,
    IOptions<WaxOptions> waxOptions,
    IOptions<OutputOptions> outputOptions,
    DefaultCommandLineOptions commandLineOptions,
    IHostApplicationLifetime applicationLifetime
) : BackgroundService {
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        if (logger.IsEnabled(LogLevel.Information)) {
            logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        }

        if (commandLineOptions.Limit < 0) {
            logger.LogError("Invalid --limit, it should be greater than 0");
            applicationLifetime.StopApplication();
            return;
        }
        
        await Task.Delay(TimeSpan.FromSeconds(0.5), stoppingToken);

        var templateId = commandLineOptions.TemplateId > 0
            ? commandLineOptions.TemplateId
            : atomicAssetsOptions.Value.TemplateId;

        var owner = waxOptions.Value.Account;
        var accountAssets =
            await atomicAssetsClient.GetAccountAssets(templateId, owner, cancellationToken: stoppingToken);
        var startedAt =
            DateTimeOffset.UtcNow - TimeSpan.FromMinutes(5); // offset by 5m to account for any possible skew
        var linkRecords = new List<AtomicToolsLinkRecord>();

        var limit = commandLineOptions.Limit > 0 ? commandLineOptions.Limit : int.MaxValue;

        var atomicAssets = accountAssets as AtomicAsset[] ?? accountAssets.ToArray();

        if (!atomicAssets.Any()) {
            logger.LogError("No assets matching {template} found in {account}", templateId, owner);
            applicationLifetime.StopApplication();
            return;
        }
        
        logger.LogInformation("{n} NFTs matching {template} found in {account}", atomicAssets.Length, templateId, owner);

        foreach (var asset in atomicAssets.Take(limit)) {
            logger.LogInformation("Processing template Asset# {asset}, Template: {template}: \"{name}\" #{mint}",
                asset.AssetId, asset.TemplateId, asset.Name, asset.Mint);

            try {
                var result = await atomicToolsClient.AnnounceDeposit(asset);
                linkRecords.Add(result);
            }
            catch (OutOfResourcesException) {
                logger.LogError(
                    "Account out of resources, can not meaningfully continue broadcasting transactions. Skipping remaining assets and generating links for broadcast transactions.");
                break;
            }
        }

        logger.LogInformation("Pausing for 10 seconds to wait for indexer to run.");
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        var links = await atomicAssetsClient.GetAccountLinks(owner, [LinkState.Created], startedAt, limit: 50,
            cancellationToken: stoppingToken);
        var giftLinks = links as AtomicToolsGiftLink[] ?? links.ToArray();

        foreach (var linkRecord in linkRecords) {
            if (linkRecord.Status != LinkStatus.Announced) {
                logger.LogWarning("The assetId ({assetId}) is in a failed state.", linkRecord.AssetId);
                continue;
            }

            if (!giftLinks.Any(gl => gl.Assets.Any(a => a.AssetId == linkRecord.AssetId))) {
                logger.LogWarning("The assetId ({assetId}) was not found in the list of links, skipping.",
                    linkRecord.AssetId);
                continue; // skip results that don't have links applied to them;
            }

            var link = giftLinks.Single(gl => gl.Assets.Any(a => a.AssetId == linkRecord.AssetId));

            linkRecord.LinkId = link.LinkId;
            linkRecord.Status = LinkStatus.Success;

            logger.LogInformation("Asset ID#{assetId} has a Link Id {linkId}, generating private link.",
                linkRecord.AssetId, link.LinkId);

            linkRecord.GiftLinkUri = atomicToolsClient.BuildGiftLinkUri(linkRecord.LinkId, linkRecord.GiftPrivateKey);
            linkRecord.Created = DateTimeOffset.UtcNow;
        }

        var fullPath = Path.GetFullPath(outputOptions.Value.OutputFile
            .Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile))
            .Replace("//", "/"));

        if (File.Exists(fullPath)) {
            var backupFile = fullPath + $"_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.old";
            logger.LogWarning("Output file already exists, backing up old file as {oldFile}", backupFile);
            File.Move(fullPath, backupFile);
        }

        logger.LogInformation("Generation completed, writing to output file: {output}", fullPath);

        await using (var writer = new StreamWriter(fullPath))
        await using (var csv = new CsvWriter(writer, outputOptions.Value.RespectCulture ? CultureInfo.CurrentCulture : CultureInfo.InvariantCulture)) {
            await csv.WriteRecordsAsync(linkRecords, stoppingToken);
        }

        await Task.Delay(1000, stoppingToken);

        applicationLifetime.StopApplication();
    }
}