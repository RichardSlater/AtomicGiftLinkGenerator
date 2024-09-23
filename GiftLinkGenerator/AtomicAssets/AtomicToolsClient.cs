// SPDX-License-Identifier: MIT

using EosSharp;
using EosSharp.Core.Api.v1;
using EosSharp.Core.Exceptions;
using EosSharp.Core.Helpers;
using GetPass;
using GiftLinkGenerator.Crypto;
using GiftLinkGenerator.Exceptions;
using GiftLinkGenerator.Wax;
using Microsoft.Extensions.Options;
using Action = EosSharp.Core.Api.v1.Action;

namespace GiftLinkGenerator.AtomicAssets;

public class AtomicToolsClient(
    ILogger<AtomicToolsClient> logger,
    IWalletService walletService,
    IOptions<WaxOptions> waxOptions,
    IOptions<AtomicAssetsOptions> atomicAssetsOptions,
    IHttpClientFactory httpClientFactory
    ) : AtomicAssetHttpClientBase<AtomicToolsClient>(httpClientFactory, logger), IAtomicToolsClient {
    private readonly AtomicAssetsOptions _atomicAssetsOptions = atomicAssetsOptions.Value;
    private readonly ILogger<AtomicToolsClient> _logger = logger;
    private string? _privateKey;

    public async Task<AtomicToolsLinkRecord> AnnounceDeposit(AtomicAsset asset) {
        var wax = await GetWaxConfigurator();

        var keyPair = CryptoHelper.GenerateKeyPair();
        var atomicGiftLink = new AtomicToolsLinkRecord {
            AssetId = asset.AssetId,
            Name = asset.Name,
            GiftPublicKey = keyPair.PublicKey,
            GiftPrivateKey = keyPair.PrivateKey
        };

        var actions = new List<Action> {
            GetAnnounceLinkAction(atomicGiftLink),
            GetTransferAction(atomicGiftLink)
        };

        try {
            var result = await wax.CreateTransaction(new Transaction { actions = actions });
            _logger.LogInformation("Transaction broadcast: {txnId}", result);

            atomicGiftLink.TransactionId = result;
            atomicGiftLink.Status = LinkStatus.Announced;
        }
        catch (ApiErrorException ex) {
            atomicGiftLink.Status = LinkStatus.Failed;

            foreach (var error in ex.error.details) {
                switch (error.method) {
                    case "validate_account_cpu_usage_estimate":
                    case "validate_cpu_usage_to_bill":
                    case "validate_account_cpu_usage":
                    case "validate_ram_usage":
                        _logger.LogCritical("{message}: {file}:{line} ({method})", error.message, error.file, error.line_number,
                            error.method);
                        throw new OutOfResourcesException($"{error.message}: {error.file}:{error.line_number} ({error.method})");
                    default:
                        _logger.LogError("{message}: {file}:{line} ({method})", error.message, error.file, error.line_number,
                            error.method);
                        break;
                }
            }
        }
        catch (ApiException ex) {
            _logger.LogError("An API exception was thrown while cancelling the link: {content}", ex.Content);
            atomicGiftLink.Status = LinkStatus.Failed;
        }

        return atomicGiftLink;
    }

    private async Task<Eos> GetWaxConfigurator() {
        var options = waxOptions.Value;
        var pk = await GetPrivateKey(options);
        return new(waxOptions.Value.GetConfigurator(pk));
    }

    private async Task<string> GetPrivateKey(WaxOptions options) {
        if (_privateKey is not null) {
            return _privateKey;
        } 
        
        if (string.IsNullOrWhiteSpace(options.PrivateKey)) {
            if (!await walletService.TestActor(options.Actor, options.Permission)) {
                _logger.LogError("Private key was not available from either configuration nor the wallet.");
                throw new InvalidOperationException();
            }

            var password = ConsolePasswordReader.Read("Enter password to unlock wallet:");
            _privateKey = await walletService.GetPrivateKey(options.Actor, options.Permission, password);

            if (_privateKey is not null) return _privateKey;
            
            _logger.LogError("Failed to get private key");
            throw new InvalidOperationException();
        }

        _privateKey = options.PrivateKey;
        return _privateKey;
    }

    public async Task<bool> CancelLink(AtomicToolsGiftLink atomicGiftLink) {
        var wax = await GetWaxConfigurator();

        var actions = new List<Action> {
            GetCancelLink(atomicGiftLink)
        };

        try {
            var result = await wax.CreateTransaction(new Transaction { actions = actions });
            _logger.LogInformation("Transaction broadcast: {txnId}", result);
            return true;
        }
        catch (ApiErrorException ex) {
            foreach (var error in ex.error.details)
                _logger.LogError("{message}: {file}:{line} ({method})", error.message, error.file, error.line_number,
                    error.method);

            return false;
        }
        catch (ApiException ex) {
            _logger.LogError("An API exception was thrown while cancelling the link: {content}", ex.Content);
            return false;
        }
    }

    public Uri BuildGiftLinkUri(string linkId, string linkPrivateKey) {
        var resourceUri =
            $"{_atomicAssetsOptions.AtomicToolsBaseUri}{_atomicAssetsOptions.Endpoints.AtomicTools.TradingLinks}/{linkId}";
        Dictionary<string, string> parameters = new() {
            { "key", linkPrivateKey }
        };
        var builder = new UriBuilder(resourceUri) {
            Query = FormatQueryString(parameters)
        };
        return builder.Uri;
    }

    private Action GetAnnounceLinkAction(AtomicToolsLinkRecord atomicToolsLinkRecord) {
        return new Action {
            account = waxOptions.Value.GiftContract,
            name = "announcelink",
            authorization = [
                GetPermission()
            ],
            data = new Dictionary<string, object> {
                { "creator", waxOptions.Value.Account },
                { "key", atomicToolsLinkRecord.GiftPublicKey },
                { "asset_ids", new[] { atomicToolsLinkRecord.AssetId } },
                { "memo", waxOptions.Value.GiftMemo }
            }
        };
    }

    private Action GetCancelLink(AtomicToolsGiftLink giftLink) {
        return new Action {
            account = waxOptions.Value.GiftContract,
            name = "cancellink",
            authorization = [
                GetPermission()
            ],
            data = new Dictionary<string, object> {
                { "link_id", giftLink.LinkId }
            }
        };
    }

    private Action GetTransferAction(AtomicToolsLinkRecord atomicToolsLinkRecord) {
        return new Action {
            account = waxOptions.Value.AtomicAssetsContract,
            name = "transfer",
            authorization = [
                GetPermission()
            ],
            data = new Dictionary<string, object> {
                { "from", waxOptions.Value.Account },
                { "to", waxOptions.Value.GiftContract },
                { "asset_ids", new[] { atomicToolsLinkRecord.AssetId } },
                { "memo", "link" }
            }
        };
    }

    private PermissionLevel GetPermission() {
        return new PermissionLevel { actor = waxOptions.Value.Actor, permission = waxOptions.Value.Permission };
    }
}