using CommandLine;
// ReSharper disable ClassNeverInstantiated.Global

namespace GiftLinkGenerator;

[Verb("generate", isDefault: true, HelpText = "Broadcasts links to the chain then generates gift links using public key cryptography.")]
public class DefaultCommandLineOptions {
    [Option('l', "limit", Required = false, HelpText = "Maximum number of links to generate.")]
    public int Limit { get; set; }
    
    [Option('t', "template", Required = false, HelpText = "The template to generate the gift links.")]
    public int TemplateId { get; set; }
}

[Verb("cancel-unclaimed", HelpText = "Cancels all unclaimed links.")]
public class CancelUnclaimedCommandLineOptions {
    [Option('i', "linkId", Required = false, HelpText = "The link ID to generate.")]
    public int LinkId { get; set; }
}

[Verb("add-wallet", HelpText = "Add private keys within encrypted wallet.")]
public class AddWalletCommandLineOptions {
    [Option('a', "actor", Required = true, HelpText = "The actor to add.")]
    public string? Actor { get; set; } = default;

    [Option('p', "permission", Required = true, HelpText = "The permission to add.")]
    public string? Permission { get; set; } = default;
}

[Verb("delete-wallet", HelpText = "Remove private keys for wallet.")]
public class DeleteWalletCommandLineOptions {
    [Option('a', "actor", Required = true, HelpText = "The actor to remove.")]
    public string? Actor { get; set; } = default;

    [Option('p', "permission", Required = true, HelpText = "The permission to remove.")]
    public string? Permission { get; set; } = default;
}