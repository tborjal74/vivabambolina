using indexApp.Components;
using indexApp.Features.AiVisualizer;
using indexApp.Features.AiVisualizer.Services;
using indexApp.Features.Shopify;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddOptions<AiVisualizerOptions>()
    .Bind(builder.Configuration.GetSection(AiVisualizerOptions.SectionName))
    .Validate(options => options.MaxUploadBytes > 0, "Visualizer uploads must allow at least one byte.")
    .Validate(options => options.AllowedImageContentTypes.Length > 0, "At least one image content type must be allowed.");

builder.Services.AddOptions<ShopifyOptions>()
    .Bind(builder.Configuration.GetSection(ShopifyOptions.SectionName));

builder.Services.AddScoped<IGownVisualizationService, StubGownVisualizationService>();

var app = builder.Build();

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

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<indexApp.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();
