using GroupAllocator;
using GroupAllocator.Controllers;
using GroupAllocator.Database;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Stripe;
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
				.WithOrigins(
					"http://localhost:5173",
					"https://localhost:5173",
					"https://group-allocator.pages.dev",
					"https://*.group-allocator.pages.dev",
					"https://oliverw10.github.io",
					"https://group-allocator.vercel.app",
					"https://app.group-allocator.com"
				)
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
	options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
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
	options.AddPolicy("AdminOnly", policy => policy.RequireClaim(AuthRolesConstants.AdminClaimName, true.ToString()));
	options.AddPolicy("TeacherOnly", policy => policy.RequireClaim(AuthRolesConstants.RoleClaimName, AuthRolesConstants.Teacher));
	options.AddPolicy("StudentOnly", policy => policy.RequireClaim(AuthRolesConstants.RoleClaimName, AuthRolesConstants.Student));
});
var dbConnectionString = builder.Configuration.GetConnectionString("MainDb");
builder.Services.AddDbContext<ApplicationDbContext>(options => options
	.UseNpgsql(dbConnectionString)
// .ConfigureWarnings(w => w.Throw(RelationalEventId.MultipleCollectionIncludeWarning))
);
Console.WriteLine($"Using database: '{dbConnectionString}'");
builder.Services.RegisterApplicationServices();
var stripeKey = builder.Configuration.GetValue<string>("Stripe:SecretKey");
if (stripeKey == null){
	Console.ForegroundColor = ConsoleColor.Red;
	Console.WriteLine("Stripe secret key not found");
	Console.ResetColor();
}
StripeConfiguration.ApiKey = stripeKey;

if (!builder.Environment.IsDevelopment())
{
	builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
}

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

app.MapControllers();

app.Run();
