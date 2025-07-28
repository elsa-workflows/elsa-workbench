using System.Text.Encodings.Web;
using Elsa.Expressions.Helpers;
using Elsa.Extensions;
using Elsa.Persistence.EFCore.Extensions;
using Elsa.Persistence.EFCore.Modules.Management;
using Elsa.Persistence.EFCore.Modules.Runtime;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// ReSharper disable RedundantAssignment
ObjectConverter.StrictMode = true;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

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
                identity.TokenOptions = options => options.SigningKey = "stone tree moon leaf dagger frog";
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
                runtime.UseCache();
            })
            .UseScheduling(scheduling => scheduling.UseQuartzScheduler())
            .UseWorkflowsApi()
            .UseCSharp()
            .UseJavaScript(options => options.AllowClrAccess = true)
            .UseLiquid(liquid => liquid.FluidOptions = options => options.Encoder = HtmlEncoder.Default)
            .UseHttp(http => http.UseCache())
            .UseQuartz(quartz => quartz.UseSqlite());
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