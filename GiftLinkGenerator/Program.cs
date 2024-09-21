// SPDX-License-Identifier: MIT

using GiftLinkGenerator;
using GiftLinkGenerator.AtomicAssets;
using GiftLinkGenerator.Wax;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddUserSecrets<Program>();

if (args.Length == 0) builder.Services.AddHostedService<BulkGenerateLinksWorker>();

if (args.Contains("cancel-unclaimed")) builder.Services.AddHostedService<RemoveUnclaimedLinksWorker>();

builder.Services.AddHttpClient();
builder.Services.Configure<AtomicAssetsOptions>(builder.Configuration.GetSection("AtomicAssets"));
builder.Services.Configure<WaxOptions>(builder.Configuration.GetSection("Wax"));
builder.Services.Configure<OutputOptions>(builder.Configuration.GetSection("Output"));
builder.Services.AddScoped<IAtomicAssetsFactory, AtomicAssetsFactory>();
builder.Services.AddScoped<IAtomicAssetsClient, AtomicAssetsClient>();
builder.Services.AddScoped<IAtomicToolsClient, AtomicToolsClient>();
var host = builder.Build();
host.Run();