using ApiCubosExamen.Data;
using ApiCubosExamen.Helpers;
using ApiCubosExamen.Repositories;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddAzureClients(factory =>
{
    factory.AddSecretClient(builder.Configuration.GetSection("KeyVault"));
});
SecretClient clienteSecreto = builder.Services.BuildServiceProvider().GetService<SecretClient>();
KeyVaultSecret sqlconnectionsecret = await clienteSecreto.GetSecretAsync("secret-sql");
string connectionString = sqlconnectionsecret.Value;

builder.Services.AddDbContext<ContextCubos>(options => options.UseSqlServer(connectionString));

builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTodo", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddTransient<RepositoryCubos>();

KeyVaultSecret secretoAes = await clienteSecreto.GetSecretAsync("secret-token-key");
HelperCifrado.Initialize(secretoAes.Value);

HelperActionOAuthService helper = new HelperActionOAuthService(builder.Configuration, clienteSecreto);
builder.Services.AddSingleton<HelperActionOAuthService>(helper);
builder.Services.AddAuthentication(helper.GetAuthenticationSchema()).AddJwtBearer(helper.GetJWtBearerOptions());

string storageCubos = builder.Configuration.GetValue<string>("AzureKeys:StorageAccount");
BlobServiceClient blobServiceClient = new BlobServiceClient(storageCubos);
builder.Services.AddTransient<BlobServiceClient>(x => blobServiceClient);
builder.Services.AddTransient<ServiceStorageBlobs>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
}
app.MapOpenApi();
app.MapScalarApiReference();
app.MapGet("/", context =>
{
    context.Response.Redirect("/scalar");
    return Task.CompletedTask;
});
app.UseCors("PermitirTodo");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
