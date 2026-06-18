using BlazorN10WasmLab.Client.Pages;
using BlazorN10WasmLab.Components;
using BlazorN10WasmLab.Services;
using BlazorN10WasmLab.Services.Ingestion;
using BlazorN10WasmLab.Services.VectorStore;
using BlazorN10WasmLab.Contracts;
using OpenAI;
using OpenAI.Responses;
using ProtoBuf.Grpc.Server;
using System.ClientModel;

GrpcTypeModelSetup.Register();

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddCodeFirstGrpc();

// ---- RAG 後端服務（由 OpenAIChatLab 移轉）：OpenAI 用戶端 + 自管 sqlite-vec ----
// 金鑰與端點從設定與 user-secrets 讀取（缺值在啟動時拋例外）。
var openAIApiKey = builder.Configuration["AzureOpenAI:ApiKey"]
    ?? throw new InvalidOperationException("Missing configuration: AzureOpenAI:ApiKey.");
var openAIEndpoint = new Uri(builder.Configuration["AzureOpenAI:EndPoint"]
    ?? throw new InvalidOperationException("Missing configuration: AzureOpenAI:EndPoint."));

var apiKeyCredential = new ApiKeyCredential(openAIApiKey);
var openAIClient = new OpenAIClient(apiKeyCredential, new OpenAIClientOptions { Endpoint = openAIEndpoint });

#pragma warning disable OPENAI001 // OpenAI.Responses 型別標 [Experimental];本專案鎖定 2.11.0 直接使用。
var responsesClient = new ResponsesClient(apiKeyCredential, new ResponsesClientOptions { Endpoint = openAIEndpoint });
#pragma warning restore OPENAI001

var embeddingClient = openAIClient.GetEmbeddingClient("text-embedding-3-small");

var vectorStorePath = Path.Combine(AppContext.BaseDirectory, "vector-store.db");
var vectorStoreConnectionString = $"Data Source={vectorStorePath}";

builder.Services.AddSingleton(embeddingClient);
builder.Services.AddSingleton(new SqliteVecStore(vectorStoreConnectionString));
builder.Services.AddSingleton<DataIngestor>();
builder.Services.AddSingleton<SemanticSearch>();
builder.Services.AddKeyedSingleton("ingestion_directory", new DirectoryInfo(Path.Combine(builder.Environment.WebRootPath, "Data")));
builder.Services.AddSingleton(responsesClient);
builder.Services.AddSingleton<ChatResponder>();

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
