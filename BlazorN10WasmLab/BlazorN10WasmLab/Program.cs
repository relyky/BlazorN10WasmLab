using BlazorN10WasmLab.Client.Pages;
using BlazorN10WasmLab.Components;
using BlazorN10WasmLab.Services;
using BlazorN10WasmLab.Contracts;
using ProtoBuf.Grpc.Server;

GrpcTypeModelSetup.Register();

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddCodeFirstGrpc();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorN10WasmLab.Client._Imports).Assembly);

app.MapGrpcService<WeatherService>().EnableGrpcWeb();

app.Run();
