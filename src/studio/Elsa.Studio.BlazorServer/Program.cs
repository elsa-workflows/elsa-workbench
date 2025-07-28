using Elsa.Studio.BlazorServer;
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
using Elsa.Studio.Host.Server;
using Elsa.Studio.Login.Extensions;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var services = builder.Services;

services.AddRazorPages();
services.AddServerSideBlazor(options =>
{
    // Register the root components.
    // V2 activity wrapper by default.
    options.RootComponents.RegisterCustomElsaStudioElements();
    
    // To use V1 activity wrapper layout, specify the V1 component instead:
    //options.RootComponents.RegisterCustomElsaStudioElements(typeof(Elsa.Studio.Workflows.Designer.Components.ActivityWrappers.V1.EmbeddedActivityWrapper));
    
    options.RootComponents.MaxJSRootComponents = 1000;
});

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
        options.SupportedCultures = new[] { options?.DefaultCulture ?? new LocalizationOptions().DefaultCulture }
            .Concat(options?.SupportedCultures.Where(culture => culture != options.DefaultCulture) ?? []) .ToArray();
    }
};

services.AddCore();
services.AddShell(options => configuration.GetSection("Shell").Bind(options));
services.AddRemoteBackend(backendApiConfig);
services.AddLoginModule();
services.UseElsaIdentity();
services.AddDashboardModule();
services.AddWorkflowsModule();
services.AddLocalizationModule(localizationConfig);
services.AddTranslations();
services.AddAgentsModule(backendApiConfig);
services.AddScoped<ITimeZoneProvider, LocalTimeZoneProvider>();

// Uncomment for V1 designer theme (default is V2).
// services.Configure<DesignerOptions>(options =>
// {
//     options.DesignerCssClass = "elsa-flowchart-diagram-designer-v1";
//     options.GraphSettings.Grid.Type = "mesh";
// });

// Configure SignalR.
services.AddSignalR(options => options.MaximumReceiveMessageSize = 5 * 1024 * 1000);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseResponseCompression();
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