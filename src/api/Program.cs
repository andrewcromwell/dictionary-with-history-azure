using LookupWord_Api.Services;
using LookupWord_Api.Services.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Azure.Identity;

var credential = new DefaultAzureCredential();

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration(config => 
        config.AddAzureKeyVault(new Uri(Environment.GetEnvironmentVariable("AZURE_KEY_VAULT_ENDPOINT")!), credential))
    .ConfigureServices((config, services) =>
    {
        services.AddScoped<ILookupService, LookupService>();
        services.AddScoped<IDictionaryService, WiktionaryService>();
    })
    .Build();

host.Run();
