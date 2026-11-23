using Api.Endpoints;
using Api.Infrastructure;
using Api.Validation;
using Core;
using Core.Abstractions;
using FluentValidation;
using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddValidatorsFromAssemblyContaining<SaveBookRequestValidator>();

var raw = builder.Configuration.GetConnectionString("AppDb") ?? "Data Source=data/app.db";
var csb = new SqliteConnectionStringBuilder(raw);
if (!Path.IsPathRooted(csb.DataSource))
{
    var full = Path.Combine(builder.Environment.ContentRootPath, csb.DataSource);
    Directory.CreateDirectory(Path.GetDirectoryName(full)!);
    csb.DataSource = full;
}

builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite(csb.ConnectionString));
builder.Services.AddScoped<ICatalogStore, EfCatalogStore>();
builder.Services.AddScoped<IAsyncCatalogStore, EfCatalogStore>();

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins(builder.Configuration["Cors:WebOrigin"] ?? "http://localhost:5090")
     .AllowAnyMethod()
     .AllowAnyHeader()));

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseCors();

if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();

        if (!db.Books.Any())
        {
            var store = scope.ServiceProvider.GetRequiredService<ICatalogStore>();
            foreach (var book in SampleData.Books())
            {
                store.Add(book);
            }
        }
    }

    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapBooks();

app.Run();

public partial class Program { }
