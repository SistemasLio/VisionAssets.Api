using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using VisionAssets.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthorization();

builder.Services.AddSingleton<InventorySnapshotStore>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapPost(
        "/v1/inventory-snapshots",
        async Task<IResult> (
            HttpContext http,
            InventorySnapshotStore store,
            CancellationToken cancellationToken) =>
        {
            http.Request.EnableBuffering();
            using var reader = new StreamReader(http.Request.Body, leaveOpen: true);
            var body = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(body))
                return TypedResults.BadRequest(new { error = "Corpo JSON vazio." });

            try
            {
                JsonDocument.Parse(body);
            }
            catch (JsonException)
            {
                return TypedResults.BadRequest(new { error = "JSON inválido." });
            }

            var idem = http.Request.Headers["Idempotency-Key"].ToString();
            if (string.IsNullOrEmpty(idem))
                idem = null;

            var snapshot = store.Accept(body, idem);
            return TypedResults.Ok(
                new
                {
                    correlation_id = snapshot.CorrelationId,
                    received_at_utc = snapshot.ReceivedAtUtc,
                });
        })
    .RequireAuthorization()
    .WithOpenApi()
    .WithName("PostInventorySnapshot");

app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
    .WithOpenApi()
    .WithName("Health")
    .AllowAnonymous();

app.Run();
