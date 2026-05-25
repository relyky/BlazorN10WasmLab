using BlazorN10WasmLab.Client.Services;
using BlazorN10WasmLab.Contracts;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

GrpcTypeModelSetup.Register();

var builder = WebAssemblyHostBuilder.CreateDefault(args);

var channel = GrpcChannel.ForAddress(
    builder.HostEnvironment.BaseAddress,
    new GrpcChannelOptions
    {
        HttpHandler = new GrpcWebHandler(GrpcWebMode.GrpcWebText, new HttpClientHandler())
    });

builder.Services.AddSingleton<IWeatherService>(new WeatherServiceClient(channel));
builder.Services.AddSingleton<IViewportBreakpoint, ViewportBreakpoint>();

await builder.Build().RunAsync();
