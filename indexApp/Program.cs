using indexApp.Components;
using indexApp.Features.Auth;
using indexApp.Features.Auth.Data;
using indexApp.Features.AiVisualizer;
using indexApp.Features.AiVisualizer.Services;
using indexApp.Features.Shopify;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();

builder.Services.AddOptions<AiVisualizerOptions>()
    .Bind(builder.Configuration.GetSection(AiVisualizerOptions.SectionName))
    .Validate(options => options.MaxUploadBytes > 0, "Visualizer uploads must allow at least one byte.")
    .Validate(options => options.AllowedImageContentTypes.Length > 0, "At least one image content type must be allowed.");

builder.Services.AddOptions<ShopifyOptions>()
    .Bind(builder.Configuration.GetSection(ShopifyOptions.SectionName));

builder.Services.AddOptions<AuthOptions>()
    .Bind(builder.Configuration.GetSection(AuthOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.Jwt.SigningKey), "Auth:Jwt:SigningKey is required.")
    .Validate(options => options.Jwt.SigningKey.Length >= 32, "Auth:Jwt:SigningKey must be at least 32 characters.")
    .Validate(options => options.Jwt.ExpirationMinutes > 0, "Auth:Jwt:ExpirationMinutes must be positive.");

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var authOptions = builder.Configuration.GetSection(AuthOptions.SectionName).Get<AuthOptions>() ?? new AuthOptions();
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOptions.Jwt.SigningKey));

var authenticationBuilder = builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = AdminSessionAuth.Scheme;
        options.DefaultChallengeScheme = AdminSessionAuth.Scheme;
    })
    .AddCookie(AdminSessionAuth.Scheme, AdminSessionAuth.ConfigureCookie)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = authOptions.Jwt.Issuer,
            ValidAudience = authOptions.Jwt.Audience,
            IssuerSigningKey = signingKey
        };
    });

if (authOptions.OAuth.IsConfigured)
{
    authenticationBuilder
        .AddCookie(AdminSessionAuth.ExternalScheme)
        .AddOAuth("OAuth", options =>
        {
            options.SignInScheme = AdminSessionAuth.ExternalScheme;
            options.ClientId = authOptions.OAuth.ClientId;
            options.ClientSecret = authOptions.OAuth.ClientSecret;
            options.AuthorizationEndpoint = authOptions.OAuth.AuthorizationEndpoint;
            options.TokenEndpoint = authOptions.OAuth.TokenEndpoint;
            options.UserInformationEndpoint = authOptions.OAuth.UserInformationEndpoint;
            options.CallbackPath = authOptions.OAuth.CallbackPath;
        });
}
else
{
    authenticationBuilder.AddCookie(AdminSessionAuth.ExternalScheme);
}

builder.Services.AddAuthorization();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AuthSeeder>();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddScoped<IGownVisualizationService, StubGownVisualizationService>();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    try
    {
        await scope.ServiceProvider.GetRequiredService<AuthSeeder>().SeedAsync();
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Admin auth seed skipped because the database is not available.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapAuthEndpoints();
app.MapRazorComponents<indexApp.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();
