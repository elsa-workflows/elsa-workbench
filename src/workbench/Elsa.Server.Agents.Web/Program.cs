using System.Text.Encodings.Web;
using Elsa.Agents;
using Elsa.Extensions;
using Elsa.Persistence.EFCore.Extensions;
using Elsa.Persistence.EFCore.Modules.Identity;
using Elsa.Persistence.EFCore.Modules.Management;
using Elsa.Persistence.EFCore.Modules.Runtime;
using Elsa.Workflows.Runtime.Distributed.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;
var identitySection = configuration.GetSection("Identity");
var identityTokenSection = identitySection.GetSection("Tokens");

services
    .AddElsa(elsa =>
    {
        elsa
            .AddActivitiesFrom<Program>()
            .AddWorkflowsFrom<Program>()
            .UseFluentStorageProvider()
            .UseIdentity(identity =>
            {
                identity.UseEntityFrameworkCore(ef => ef.UseSqlite());
                identity.TokenOptions = options => identityTokenSection.Bind(options);
                identity.UseAdminUserProvider();
            })
            .UseDefaultAuthentication(auth => auth.UseAdminApiKey())
            .UseWorkflows()
            .UseWorkflowManagement(management =>
            {
                management.UseEntityFrameworkCore(ef => ef.UseSqlite());
                management.UseCache();
            })
            .UseWorkflowRuntime(runtime =>
            {
                runtime.UseEntityFrameworkCore(ef => ef.UseSqlite());
                runtime.UseDistributedRuntime();
                runtime.UseCache();
            })
            .UseScheduling(scheduling => scheduling.UseQuartzScheduler())
            .UseWorkflowsApi()
            .UseCSharp()
            .UseJavaScript(options => options.AllowClrAccess = true)
            .UseLiquid(liquid => liquid.FluidOptions = options => options.Encoder = HtmlEncoder.Default)
            .UseHttp(http =>
            {
                http.ConfigureHttpOptions = options => configuration.GetSection("Http").Bind(options);
                http.UseCache();
            })
            .UseOpenTelemetry(otel => otel.UseNewRootActivityForRemoteParent = true);

        elsa.UseQuartz(quartz =>
        {
            quartz.UseSqlite();
        });

        elsa.UseMassTransit();
        elsa.UseDistributedCache(distributedCaching => distributedCaching.UseMassTransit());
        elsa.UseAgents();
        elsa.UseAgentPersistence(persistence => persistence.UseEntityFrameworkCore(ef => ef.UseSqlite()));
        elsa.UseAgentActivities();
        elsa.UseAgentsApi();
        elsa.AddFastEndpointsAssembly<Program>();
    });

services.AddHealthChecks();
services.AddControllers();
services.AddCors(cors => cors.AddDefaultPolicy(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin().WithExposedHeaders("*")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();

app.UseCors();
app.MapHealthChecks("/");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseWorkflowsApi();
app.UseJsonSerializationErrorHandler();
app.UseWorkflows();
app.MapControllers();

await app.RunAsync();