using Elsa.Http.Webhooks.Persistence.EFCore;
using Elsa.Persistence.EFCore.Abstractions;
using Elsa.Persistence.EFCore.Extensions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Elsa.Server.Web.Data.Webhooks;

[UsedImplicitly]
public class WebhooksDbContextFactory : DesignTimeDbContextFactoryBase<WebhooksDbContext>
{
    protected override void ConfigureBuilder(DbContextOptionsBuilder<WebhooksDbContext> builder, string connectionString)
    {
        builder.UseElsaSqlite(GetType().Assembly, connectionString);
    }
}

