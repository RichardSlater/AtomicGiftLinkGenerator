# Atomic Tools Gift Link Generator

[![CI/CD](https://github.com/RichardSlater/AtomicGiftLinkGenerator/actions/workflows/dotnet.yml/badge.svg)](https://github.com/RichardSlater/AtomicGiftLinkGenerator/actions/workflows/dotnet.yml)

Atomic Tools Gift Link Generator (`giftlinkgen`) is a simple tool to generate AtomicHub Gift Links that allow a NFT to be gifted to someone, these links can be generated in bulk then used in giveaways on Discord or Twitch.

> [!NOTE]
> This is a fairly simple project built to help someone automate a mundane task, it is not designed to be comprehensive or complete. You are however welcome to use it, steal code from it, train an AI with the code or submit pull requests.

## Installation

Binaries are available for Windows, Mac and Linux from the [Releases](https://github.com/RichardSlater/AtomicGiftLinkGenerator/releases) there is no installer. You can download the ZIP or tar.gz for your platform and expand. Ensure that you change the version number in the commands below and that you have [.NET 8](https://dotnet.microsoft.com/en-us/download) installed.

### Windows

> [!NOTE]
> Use a recent version of Powershell (pwsh) **not** Windows Powershell or `cmd`.

```powershell
Invoke-WebRequest -Uri https://github.com/RichardSlater/AtomicGiftLinkGenerator/releases/download/v0.2.20/AtomicGiftLinkGenerator_0.2.20-win-x64.zip -OutFile AtomicGiftLinkGenerator_0.2.20-win-x64.zip
Expand-Archive ./AtomicGiftLinkGenerator_0.2.20-win-x64.zip
cd ./AtomicGiftLinkGenerator_0.2.20-win-x64
./giftlinkgen generate
```

### Mac / Linux

```bash
mkdir atomicgiftlink && cd atomicgiftlink
wget https://github.com/RichardSlater/AtomicGiftLinkGenerator/releases/download/v0.2.20/AtomicGiftLinkGenerator_0.2.20-linux-x64.tar.gz
tar -xvzf ./AtomicGiftLinkGenerator_0.2.20-linux-x64.tar.gz
k
./giftlinkgen generate
```

## Usage

There are four verbs, allowing gift links to be created and removed plus a very basic wallet to allow a private key to be saved locally encrypted with a password:

- `generate`: the default, thus can be executed with `giftlinkgen` will read data from `appsettings.json` and attempt to generate a gift link for each NFT matching the Template ID in the account. A CSV file will be created in `~/` containing all of the Gift Links along with the Asset ID and other attributes.
  - `--limit`, `-l`: limit the number of Gift Links created in one go using the `--limit n` parameter where `n` is an number, the only practical limit is the amount of CPU, NET and RAM the account has access to.
  - `--template`, `-t`: override the template specified in `appsettings.json`
- `cancel-unclaimed`: by default will cancel all unclaimed links on the account, use this command with care.
  - `--linkId`, `-l`: cancel a specific link
- `add-wallet`: accepts two required options, `--actor` and `--permission`, and will provide an interactive prompt to enter the private key and a password to encrypt the private key with.
- `delete-wallet`: accepts two required options, `--actor` and `--permission`, this will delete a saved wallet.

> [!WARNING]
> There is no way to re-do a cancelled gift-link, it will invalidate any links already sent out, however it does allow you to reclaim the RAM spent on unclaimed links.

> [!NOTE]
> Wallets are stored as a JSON file in `~/.config/giftlinkgen_wallet.json`, each time the file is written a backup is created. Individual Private Keys are encrypted using AES and a key generated from the password using PBKDF2.

## Tool Configuration

The vast majority of the settings in `appsettings.json` are configured as required to work on WAX, change these if you need to change chains, contracts, etc.:

- **`$.Wax.Account`** - the name of the account that contains the NFTs that you want to create into gift-links.
- **`$.Wax.Actor`** - **normally** the same as the Account, however if using account-based permissions it is different. (Advanced)
- **`$.Wax.Permission`** - the permission within the Actor's account, if not using a custom permission it is `active`.
- **`$.Wax.BaseUri`** and **`$.AtomicAssets.BaseUri`** - the WAX RPC API / AtomicAssets API, for WAX I recommend using EOS USA as the usage is generous, there is an exponential backoff-delay mechanism built in, beware of RPC and AtomicAsset API servers with very low rate limits.
- **`$.Wax.GiftMemo`** - the memo you want to be displayed in the gift-link.
- **`$.Output.OutputFile`** - where to spit a CSV file out, by default it will be `~/Desktop/gift-links.csv`

> [!NOTE]
> The other settings in the file are useful for when switching between chains, however changing them are likely to break things due to subtle differences in the API versions between EOS and WAX in particular.

## WAX / EOS Configuration

> [!IMPORTANT]
> If your private key is exposed, then bad actors / malicious actors can and will steal from your account. It's highly recommended to run this tool with an account that only has assets you wish to give away and with a key that has the minimum privileges.
> 
> For the default behavior, i.e. generating gift links, this means `atomicassets::transfer` and `atomictoolsx::announcelink`. For `cancel-unclaimed` then only `atomictoolsx::cancellink` is required. Your use of this tool is entirely at your own risk.

You can configure custom permissions using [WaxBlock](https://waxblock.io/wallet/permissions), I have previously [blogged about doing this](https://peakd.com/wax/@scetrov/using-wax-permissions-to-create-a-custom-warsaken-claim-only-keypair).

### Secret Configuration

This tool requires access to your private key, I assume by this point you have read the above important information about private keys.

#### Interactive Mode

First save your private key in the wallet store:

```
./giftlinkgen add-wallet --actor tip.scetrov --permission tips
```

> [!NOTE]
> Keys should be in the original EOS format, if you need to convert them then Anchor Wallet has a converter in the Tools menu.

You will be promoted for a private key and a password.

Second make sure `appsettings.json` contains the same actor/account and permission which should be in the same folders as `giftlinkgen`, you may also want to update the `GiftMemo`, `OutputFile` and `TemplateId`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "Wax": {
    "Account": "tip.scetrov",
    "Actor": "tip.scetrov",
    "GiftContract": "atomictoolsx",
    "AtomicAssetsContract": "atomicassets",
    "Permission": "tips",
    "BaseUri": "https://wax.eosusa.io",
    "ChainId": "1064487b3cd1a897ce03ae5b6a865651747e2e152090f99c1d19d44e01aea5a4",
    "ExpireSeconds": 120,
    "GiftMemo": "FROM SCETROV WITH LOVE"
  },
  "Output": {
    "OutputFile": "~/gift-links.csv"
  },
  "Crypto": {
    "HashAlgorithm": "SHA256",
    "Iterations": 10000,
    "Salt": "I1&!6nMG"
  },
  "AtomicAssets": {
    "BaseUri": "https://wax.eosusa.io",
    "AtomicToolsBaseUri": "https://wax.atomichub.io",
    "TemplateId": 713712,
    "Endpoints": {
      "AtomicMarket": {
        "Assets": "/atomicmarket/v1/assets"
      },
      "AtomicAssets": {
        "Assets": "/atomicassets/v1/assets"
      },
      "AtomicTools": {
        "Links": "/atomictools/v1/links",
        "TradingLinks": "/trading/link"
      }
    }
  }
}
```
Finally run `./giftlinkgen generate` or simply:

```
./giftlinkgen
```

#### Headless Mode

> [!WARNING]
> Only file-system access controls protect this file, it is **NOT** encrypted.

You can put your private key in this file if you don't want to enter your password each time you can put the private key in the following location:

**Windows**: 
```
~\AppData\Roaming\Microsoft\UserSecrets\dotnet-GiftLinkGenerator-6b9a105e-f82f-4234-9a06-c389d47b2891\secrets.json
```

Linux:Mac:
```
~/.microsoft/usersecrets/6b9a105e-f82f-4234-9a06-c389d47b2891/secrets.json
```

The format of the file is as follows:

```json
{
  "Wax": {
    "PrivateKey": "5...rest of your private key replaces this entire string."
  }
}
```
> [!NOTE]
> Keys should be in the original EOS format, if you need to convert them then Anchor Wallet has a converter in the Tools menu.

## Examples

### Generate five Gift Links using a specific template ID

```
./giftlinkgen generate --limit 5 --template 713714
```

This will generate a CSV file containing five gift links, for the template `713714` (Warsaken Loot 5,000)

## Issues / Support

If you have issues or you find bugs, please report them via [Issues](https://github.com/RichardSlater/AtomicGiftLinkGenerator/issues).

## Attribution

- Application Icon [by Howieart](https://www.svgrepo.com/author/Howieart/) (License: CC-BY)
- [Eos-Sharp-2.0](https://github.com/Saltant/Eos-Sharp-2.0) has been invaluable (License: MIT)
