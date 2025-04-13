using GroupAllocator;
using GroupAllocator.Database;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        policy =>
        {
            policy
                .AllowCredentials()
                .WithOrigins("http://localhost:5173", "https://localhost:5173", "https://group-allocator.pages.dev", "https://*.group-allocator.pages.dev")
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

Func<RedirectContext<CookieAuthenticationOptions>, Task> unauthorizedHandler = ctx =>
{
    ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
    return Task.CompletedTask;
};

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.None;
    options.Events = new CookieAuthenticationEvents()
    {
        OnRedirectToLogin = unauthorizedHandler,
        OnRedirectToAccessDenied = unauthorizedHandler
    };
});

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireClaim("admin", "True"));
});
// TODO: work out proper migrations setup for dev and prod
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("MainDb")));
builder.Services.AddControllers();
builder.Services.RegisterApplicationServices();
var app = builder.Build();
app.UseCors("AllowAllOrigins");
app.UseAuthentication();
app.UseCookiePolicy();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// TODO: Development can probably be removed from this
if (app.Configuration.GetValue<bool>("DbReset") || app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureDeleted();
    db.Database.EnsureCreated();
}

app.MapControllers();

app.Run();
