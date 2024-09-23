// SPDX-License-Identifier: MIT

using CommandLine;
using GiftLinkGenerator;
using GiftLinkGenerator.AtomicAssets;
using GiftLinkGenerator.Crypto;
using GiftLinkGenerator.Wax;
using GiftLinkGenerator.Workers;
using Serilog;
using Serilog.Filters;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddUserSecrets<Program>();

// logging
builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

Log.Logger = new LoggerConfiguration()
    .Filter.ByExcluding(Matching.FromSource("System.Net.Http.HttpClient"))
    .Filter.ByExcluding(Matching.FromSource("Microsoft.Hosting.Lifetime"))
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

// extensions
builder.Services.AddHttpClient();

// services
builder.Services.Configure<AtomicAssetsOptions>(builder.Configuration.GetSection("AtomicAssets"));
builder.Services.Configure<WaxOptions>(builder.Configuration.GetSection("Wax"));
builder.Services.Configure<OutputOptions>(builder.Configuration.GetSection("Output"));
builder.Services.Configure<CryptoOptions>(builder.Configuration.GetSection("Crypto"));

// options
builder.Services.AddScoped<IAtomicAssetsFactory, AtomicAssetsFactory>();
builder.Services.AddScoped<IAtomicAssetsClient, AtomicAssetsClient>();
builder.Services.AddScoped<IAtomicToolsClient, AtomicToolsClient>();
builder.Services.AddScoped<ICryptoService, CryptoService>();
builder.Services.AddScoped<IWalletService, WalletService>();

// route commands
Parser.Default
    .ParseArguments<DefaultCommandLineOptions, CancelUnclaimedCommandLineOptions, AddWalletCommandLineOptions,
        DeleteWalletCommandLineOptions>(args)
    .WithParsed<DefaultCommandLineOptions>(options => {
        builder.Services.AddHostedService<BulkGenerateLinksWorker>();
        builder.Services.AddScoped<DefaultCommandLineOptions>(_ => options);
    })
    .WithParsed<CancelUnclaimedCommandLineOptions>(options => {
        builder.Services.AddHostedService<RemoveUnclaimedLinksWorker>();
        builder.Services.AddScoped<CancelUnclaimedCommandLineOptions>(_ => options);
    })
    .WithParsed<AddWalletCommandLineOptions>(options => {
        builder.Services.AddHostedService<AddWalletWorker>();
        builder.Services.AddScoped<AddWalletCommandLineOptions>(_ => options);
    })
    .WithParsed<DeleteWalletCommandLineOptions>(options => {
        builder.Services.AddHostedService<DeleteWalletWorker>();
        builder.Services.AddScoped<DeleteWalletCommandLineOptions>(_ => options);
    })
    .WithNotParsed(errors => {
        foreach (var error in errors) {
            Console.Error.WriteLine(error.ToString());
        }
    });

var host = builder.Build();
host.Run();