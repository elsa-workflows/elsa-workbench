using System.Text.Encodings.Web;
using Elsa.Agents;
using Elsa.Email.Models;
using Elsa.Extensions;
using Elsa.Persistence.EFCore.Extensions;
using Elsa.Persistence.EFCore.Modules.Identity;
using Elsa.Persistence.EFCore.Modules.Management;
using Elsa.Persistence.EFCore.Modules.Runtime;
using Elsa.Persistence.EFCore.MySql.Services;
using Elsa.Persistence.EFCore.Oracle.Services;
using Elsa.Persistence.EFCore.PostgreSql.Services;
using Elsa.Persistence.EFCore.Sqlite.Services;
using Elsa.Persistence.EFCore.SqlServer.Services;
using Elsa.Server.Agents.Web.AI.Plugins;
using Elsa.Workflows.Runtime.Distributed.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;
var sqliteConnectionString = configuration.GetConnectionString("Sqlite");
var sqlServerConnectionString = configuration.GetConnectionString("SqlServer")!;
var postgresConnectionString = configuration.GetConnectionString("Postgres")!;
var mySqlConnectionString = configuration.GetConnectionString("MySql")!;
var oracleConnectionString = configuration.GetConnectionString("Oracle")!;
var identitySection = configuration.GetSection("Identity");
var identityTokenSection = identitySection.GetSection("Tokens");

// Add Elsa services.
services
    .AddElsa(elsa =>
    {
        elsa
            .AddActivitiesFrom<Program>()
            .AddWorkflowsFrom<Program>()
            .UseFluentStorageProvider()
            .UseIdentity(identity =>
            {
                identity.UseEntityFrameworkCore(ef => ef.UseSqlite(sqliteConnectionString));
                identity.TokenOptions = options => identityTokenSection.Bind(options);
                identity.UseAdminUserProvider();
            })
            .UseDefaultAuthentication(auth => auth.UseAdminApiKey())
            .UseWorkflows()
            .UseWorkflowManagement(management =>
            {
                management.UseEntityFrameworkCore(ef => ef.UsePostgreSql(postgresConnectionString));
                management.UseWorkflowReferenceFinder<PostgreSqlWorkflowReferenceQuery>();
                management.UseCache();
            })
            .UseWorkflowRuntime(runtime =>
            {
                runtime.UseEntityFrameworkCore(ef => ef.UseSqlite(sqliteConnectionString));
                runtime.UseDistributedRuntime();
                runtime.UseCache();
            })
            .UseScheduling(scheduling => scheduling.UseQuartzScheduler())
            .UseWorkflowsApi()
            .UseCSharp()
            .UseJavaScript(options =>
            {
                options.AllowClrAccess = true;
                options.ConfigureEngine(engine => engine.RegisterType(typeof(EmailAttachment)));
            })
            .UseLiquid(liquid => liquid.FluidOptions = options => options.Encoder = HtmlEncoder.Default)
            .UseHttp(http =>
            {
                http.ConfigureHttpOptions = options => configuration.GetSection("Http").Bind(options);
                http.UseCache();
            })
            .UseOpenTelemetry(otel => otel.UseNewRootActivityForRemoteParent = true);

        elsa.UseQuartz(quartz =>
        {
            quartz.UseSqlite(sqliteConnectionString);
        });

        elsa.UseMassTransit();
        elsa.UseDistributedCache(distributedCaching => distributedCaching.UseMassTransit());
        elsa.UseAgents();
        elsa.UseAgentActivities();
        elsa.UseAgentPersistence(persistence => persistence.UseEntityFrameworkCore(ef => ef.UseSqlite(sqliteConnectionString)));
        elsa.UseAgentsApi();
        elsa.UseEmail(email => email.ConfigureOptions = options => configuration.GetSection("Smtp").Bind(options));
        elsa.AddVariableTypeAndAlias<EmailAttachment>(nameof(EmailAttachment), "Email");
        elsa.AddFastEndpointsAssembly<Program>();
    });

services.AddPluginProvider<CreditScorePluginProvider>();
services.AddPluginProvider<EmailPluginProvider>();
services.AddPluginProvider<CustomerPluginProvider>();

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