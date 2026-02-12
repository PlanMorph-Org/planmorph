using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PlanMorph.Admin.Services;
using PlanMorph.Core.Entities;
using PlanMorph.Core.Interfaces;
using PlanMorph.Infrastructure.Data;
using PlanMorph.Infrastructure.Repositories;
using PlanMorph.Infrastructure.Utilities;

EnvLoader.Load();
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Keep AddDbContext for Identity (it requires scoped context)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add DbContextFactory for Blazor Server (avoids concurrency issues)
// Use a pooled factory for better performance
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")),
    ServiceLifetime.Scoped);

// Add Identity
builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Register UnitOfWork and Repository
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));


// Register Admin Services
builder.Services.AddHttpClient<ApiClient>();
builder.Services.AddScoped<AuthStateService>();
builder.Services.AddScoped<Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage.ProtectedLocalStorage>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.UseStaticFiles();
app.MapRazorComponents<PlanMorph.Admin.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();
