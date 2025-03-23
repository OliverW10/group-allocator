using GroupAllocator;
using GroupAllocator.Database;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.None;
});
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
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
    using (var scope = app.Services.CreateScope())
    {
        scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.EnsureCreated();
    }
}

app.MapControllers();

app.Run();
