using Inventory.Api.Endpoints;
using Inventory.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInventoryApiDocumentation();
builder.Services.AddInventoryApplication();
builder.Services.AddInventoryInfrastructure(builder.Configuration, builder.Environment.ContentRootPath);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapOpenApi();

app.MapHealthEndpoints();
app.MapProductEndpoints();
app.MapInventoryEndpoints();
app.MapKpiEndpoints();
app.MapImportEndpoints();

app.Run();
