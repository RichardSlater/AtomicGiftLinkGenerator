# Atomic Tools Gift Link Generator

> [!NOTE]
> This is a fairly simple project built to help someone automate a mundane task, it is not designed to be comprehensive or complete. You are however welcome to use it, steal code from it, train an AI with the code or submit pull requests.

## Usage

There are only two modes, the first is executing `giftlinkgen.exe` with no parameters which will read all the information from `appsettings.json` in the same folder as the executable.

The second mode must be used with care, and thus you must execute it with the command line parameter `giftlinkgen.exe cancel-unclaimed` which will cancel **all** gift links for that account regardless of the template ID *including any which have been sent but not claimed*. This is mostly useful for testing or to clean out an account.

> [!WARNING]
> There is no way to re-do a cancelled gift-link, it will invalidate any link already sent out as they will be published with new key pairs. You can however issue a new gift-link for the same asset.

## Tool Configuration

The vast majority of the settings in `appsettings.json` are configured as required to work on WAX, change these if you need to change behavior:

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

This tool requires access to your secret key, I assume by this point you have read the above important information about secret keys.

You can put your private key in this file:

```
~\AppData\Roaming\Microsoft\UserSecrets\dotnet-GiftLinkGenerator-6b9a105e-f82f-4234-9a06-c389d47b2891\secrets.json
```

> [!WARNING]
> Only file-system access controls protect this file, it is **NOT** encrypted.

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


## Attribution

Application Icon [by Howieart](https://www.svgrepo.com/author/Howieart/) (License: CC-BY)
[Eos-Sharp-2.0](https://github.com/Saltant/Eos-Sharp-2.0) has been invaluable (License: MIT)