/*
 * TEST TYPE: Functional Test — Test Infrastructure (Factory)
 *
 * WHAT IS THIS FILE?
 *   TaskApiFactory is not a test itself. It is the shared setup used by every
 *   functional test in this project. It boots the real application and swaps
 *   out only what is necessary to make it runnable in a test environment.
 *
 * HOW DOES IT WORK?
 *   WebApplicationFactory<Program> starts your real Program.cs — the same DI
 *   registrations, middleware pipeline, filters, and routing — entirely in
 *   memory. Tests get an HttpClient that sends requests directly into that
 *   in-process server, so no network port is needed.
 *
 * WHAT IS SWAPPED OUT?
 *   The file-based SQLite database (registered in Program.cs) is replaced with
 *   a SQLite in-memory connection. The real SQLite provider is kept so that SQL
 *   semantics (constraints, ordering, etc.) still apply — only the storage
 *   medium changes from disk to RAM.
 *
 * WHY A PERSISTENT CONNECTION?
 *   SQLite in-memory databases are tied to a single connection. If that
 *   connection closed between HTTP requests, the database would be wiped.
 *   Keeping _connection open for the lifetime of the factory ensures all
 *   requests within a test class see the same data.
 *
 * WHY EnsureCreated() IN CreateHost()?
 *   The in-memory database starts empty. EnsureCreated() applies the EF Core
 *   model (creates the Tasks table) once, before any test runs.
 */

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TaskFlow.Api.Data;

// WebApplicationFactory boots your real Program.cs, but lets you swap things out
public class TaskApiFactory : WebApplicationFactory<Program>
{
    // A single open SQLite in-memory connection shared across all requests.
    // Using :memory: with an open connection keeps the database alive for the
    // lifetime of the factory. If the connection closed between requests, the
    // in-memory data would be wiped.
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();

        builder.ConfigureServices(services =>
        {
            // Remove the file-based SQLite registration from Program.cs
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Re-register with the same provider (SQLite) but pointed at our
            // open in-memory connection — no provider conflict, real SQL semantics
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(_connection));
        });
    }

    // Create the schema after the host (and DI container) is fully built
    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();

        return host;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) _connection.Dispose();
    }
}
