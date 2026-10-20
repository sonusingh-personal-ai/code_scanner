using CodeScanner.API.Hubs;
using CodeScanner.Application.DTOs;
using CodeScanner.Application.Services;
using Microsoft.AspNetCore.SignalR;

namespace CodeScanner.API.Services;

public class SerialTelemetryWorker : IHostedService
{
    private readonly ISerialPortService _serialPortService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IHubContext<TestingHub> _hubContext;
    private readonly ILogger<SerialTelemetryWorker> _logger;

    public SerialTelemetryWorker(
        ISerialPortService serialPortService,
        IServiceProvider serviceProvider,
        IHubContext<TestingHub> hubContext,
        ILogger<SerialTelemetryWorker> logger)
    {
        _serialPortService = serialPortService;
        _serviceProvider = serviceProvider;
        _hubContext = hubContext;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _serialPortService.DataReceived += OnSerialDataReceived;
        _serialPortService.ConnectionStateChanged += OnConnectionStateChanged;
        _logger.LogInformation("SerialTelemetryWorker started and attached to serial port events");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _serialPortService.DataReceived -= OnSerialDataReceived;
        _serialPortService.ConnectionStateChanged -= OnConnectionStateChanged;
        return Task.CompletedTask;
    }

    private async void OnSerialDataReceived(string rawTelemetry)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var testingService = scope.ServiceProvider.GetRequiredService<ITestingService>();

            var context = new SaveResponseRequestDto
            {
                RawTelemetry = rawTelemetry
            };

            var result = await testingService.ProcessRawTelemetryAsync(rawTelemetry, context);
            await _hubContext.Clients.All.SendAsync("ReceiveTestResult", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing incoming serial telemetry packet");
        }
    }

    private async void OnConnectionStateChanged(bool isConnected, string message)
    {
        try
        {
            await _hubContext.Clients.All.SendAsync("ComPortStateChanged", isConnected, _serialPortService.CurrentPort, _serialPortService.CurrentBaudRate, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying clients of serial port connection change");
        }
    }
}
