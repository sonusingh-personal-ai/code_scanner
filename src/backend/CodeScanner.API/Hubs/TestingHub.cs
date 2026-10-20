using CodeScanner.Application.DTOs;
using CodeScanner.Application.Services;
using Microsoft.AspNetCore.SignalR;

namespace CodeScanner.API.Hubs;

public class TestingHub : Hub
{
    private readonly ITestingService _testingService;
    private readonly ISerialPortService _serialPortService;
    private readonly ILogger<TestingHub> _logger;

    public TestingHub(
        ITestingService testingService,
        ISerialPortService serialPortService,
        ILogger<TestingHub> logger)
    {
        _testingService = testingService;
        _serialPortService = serialPortService;
        _logger = logger;
    }

    public async Task<TestResultDto> TriggerTest(SaveResponseRequestDto request)
    {
        _logger.LogInformation("SignalR: TriggerTest received for model {Model}, barcode {Barcode}", request.Model, request.Barcode);
        var result = await _testingService.SimulateTestCycleAsync(request);
        await Clients.All.SendAsync("ReceiveTestResult", result);
        return result;
    }

    public async Task<bool> ConnectComPort(string portName, int baudRate)
    {
        bool success = _serialPortService.Connect(portName, baudRate);
        await Clients.All.SendAsync("ComPortStateChanged", success, _serialPortService.CurrentPort, _serialPortService.CurrentBaudRate);
        return success;
    }

    public async Task DisconnectComPort()
    {
        _serialPortService.Disconnect();
        await Clients.All.SendAsync("ComPortStateChanged", false, null, 0);
    }
}
