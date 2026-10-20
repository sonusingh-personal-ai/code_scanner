using CodeScanner.API.Hubs;
using CodeScanner.API.Services;
using CodeScanner.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "CodeScanner Core Clean Architecture API", Version = "v1" });
});

// Clean Architecture: Infrastructure & Persistence
builder.Services.AddInfrastructureServices(builder.Configuration);

// Real-time SignalR Hub
builder.Services.AddSignalR();
builder.Services.AddHostedService<SerialTelemetryWorker>();

// CORS configuration for React frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment() || true)
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CodeScanner API v1");
    });
}

app.UseCors("AllowFrontend");

app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.MapHub<TestingHub>("/hub/testing");

app.Run();
