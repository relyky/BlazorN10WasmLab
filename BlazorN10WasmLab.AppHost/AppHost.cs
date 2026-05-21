var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.BlazorN10WasmLab>("blazorn10wasmlab");

builder.Build().Run();
