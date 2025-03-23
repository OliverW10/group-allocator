using GroupAllocator;
using GroupAllocator.Database;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});
builder.Services.AddAuthentication().AddCookie();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
// TODO: work out proper migrations setup for dev and prod
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("MainDb")));
builder.Services.AddControllers();
builder.Services.RegisterApplicationServices();
var app = builder.Build();
app.UseCors("AllowAllOrigins");
app.UseAuthentication();

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
