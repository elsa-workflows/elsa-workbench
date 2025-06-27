using Elsa.Studio.Core.BlazorServer.Extensions;
using Elsa.Studio.Dashboard.Extensions;
using Elsa.Studio.Extensions;
using Elsa.Studio.Localization.Time;
using Elsa.Studio.Localization.Time.Providers;
using Elsa.Studio.Login.BlazorServer.Extensions;
using Elsa.Studio.Login.HttpMessageHandlers;
using Elsa.Studio.Models;
using Elsa.Studio.Shell.Extensions;
using Elsa.Studio.Workflows.Extensions;
using Elsa.Studio.Workflows.Designer.Extensions;
using Elsa.Studio.Localization.BlazorServer.Extensions;
using Elsa.Studio.Localization.Models;
using Elsa.Studio.Localization.Options;
using Elsa.Studio.Translations;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Elsa.Studio.Branding;
using Elsa.Studio.Contracts;
using Elsa.Studio.Login.Extensions;
using Elsa.Studio.Services;
using Elsa.Studio.Workbench;

// Build the host.
var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var services = builder.Services;

// Register Razor services.
services.AddRazorPages();
services.AddServerSideBlazor(options =>
{
    options.RootComponents.RegisterCustomElsaStudioElements();
    options.RootComponents.MaxJSRootComponents = 1000;
});

// Register shell services and modules.
var backendApiConfig = new BackendApiConfig
{
    ConfigureBackendOptions = options => configuration.GetSection("Backend").Bind(options),
    ConfigureHttpClientBuilder = options =>
    {
        options.AuthenticationHandler = typeof(AuthenticatingApiHttpMessageHandler);
        options.ConfigureHttpClient = (_, client) =>
        {
            // Set a long time out to simplify debugging both Elsa Studio and the Elsa Server backend.
            client.Timeout = TimeSpan.FromHours(1);
        };
    },
};

var localizationConfig = new LocalizationConfig
{
    ConfigureLocalizationOptions = options =>
    {
        configuration.GetSection(LocalizationOptions.LocalizationSection).Bind(options);
        options.SupportedCultures = new[] { options.DefaultCulture ?? new LocalizationOptions().DefaultCulture }
            .Concat(options?.SupportedCultures.Where(culture => culture != options?.DefaultCulture) ?? []).ToArray();
    }
};

services.AddScoped<IBrandingProvider, StudioBrandingProvider>();
services.AddCore().Replace(new(typeof(IBrandingProvider), typeof(StudioBrandingProvider), ServiceLifetime.Scoped));
services.AddScoped<IClientInformationProvider, StaticClientInformationProvider>();
services.AddShell(options => configuration.GetSection("Shell").Bind(options));
services.AddRemoteBackend(backendApiConfig);
services.AddLoginModule();

var identityProvider = configuration.GetValue<string>("Authentication:Provider");

switch (identityProvider)
{
    case "Elsa":
        services.UseElsaIdentity();
        break;
    case "OAuth2":
        services.UseOAuth2(options => configuration.GetSection("Authentication:Providers:OAuth2").Bind(options));
        break;
}

services.AddDashboardModule();
services.AddWorkflowsModule();
services.AddLocalizationModule(localizationConfig);
services.AddTranslations();
services.AddAgentsModule(backendApiConfig);

// Replace some services with other implementations.
services.AddScoped<ITimeZoneProvider, LocalTimeZoneProvider>();

// Build the application.
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseResponseCompression();

    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseElsaLocalization();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.Run();