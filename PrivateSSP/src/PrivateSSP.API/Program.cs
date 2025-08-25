using PrivateSSP.Core.Interfaces;
using PrivateSSP.Services.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add HTTP client
builder.Services.AddHttpClient();

// Add configuration
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

// Register services
builder.Services.AddScoped<IDecryptionService, DecryptionService>();
builder.Services.AddScoped<ISmsQueueService, SmsQueueService>();
builder.Services.AddScoped<ISmsProviderService, SmsProviderService>();
builder.Services.AddScoped<IWebhookService, WebhookService>();
builder.Services.AddScoped<SmsProcessingService>();

// Add logging
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.Run();
