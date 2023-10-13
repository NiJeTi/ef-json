using System.Text.Json;
using App.Database;
using App.Database.Entities;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
    options.ConfigureEndpointDefaults(listenOptions => listenOptions.Protocols = HttpProtocols.Http2);
});
builder.Services.AddHttpsRedirection(options => options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect);

builder.Services.AddDbContext<DatabaseContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("Database"));
});

var app = builder.Build();

app.UseHttpsRedirection();

var jsonId = Guid.Empty;
app.Map("/", async (DatabaseContext db, CancellationToken cancellationToken) =>
{
    await FillDatabase(db, cancellationToken);
    return await db.Set<Entity>().ToListAsync(cancellationToken);
});
app.Map("/grouped", (DatabaseContext db, CancellationToken cancellationToken) =>
{
    return db
        .Set<Entity>()
        .GroupBy(x => (Guid?)x.Json.RootElement.GetProperty(jsonId.ToString()).GetGuid())
        .Select(g => new { g.Key, Count = g.Count(), Items = g.ToList() })
        .ToListAsync(cancellationToken);
});

app.Run();
return;

async Task FillDatabase(DatabaseContext db, CancellationToken cancellationToken)
{
    await db.Set<Entity>().ExecuteDeleteAsync();

    jsonId = Guid.NewGuid();
    
    for (var i = 0; i < 3; i++)
    {
        var json = new Dictionary<Guid, Guid?>
        {
            [jsonId] = i == 0 ? null : Guid.NewGuid()
        };
        var entity = new Entity
        {
            Json = JsonSerializer.SerializeToDocument(json)
        };
        db.Add(entity);
    }

    await db.SaveChangesAsync(cancellationToken);
}