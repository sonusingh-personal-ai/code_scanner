using CodeScanner.API.Hubs;
using CodeScanner.Application.DTOs;
using CodeScanner.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace CodeScanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestingController : ControllerBase
{
    private readonly ITestingService _testingService;
    private readonly ISerialPortService _serialPortService;
    private readonly IHubContext<TestingHub> _hubContext;

    public TestingController(
        ITestingService testingService,
        ISerialPortService serialPortService,
        IHubContext<TestingHub> hubContext)
    {
        _testingService = testingService;
        _serialPortService = serialPortService;
        _hubContext = hubContext;
    }

    [HttpPost("process")]
    public async Task<ActionResult<TestResultDto>> ProcessTelemetry([FromBody] SaveResponseRequestDto request)
    {
        var result = await _testingService.ProcessRawTelemetryAsync(request.RawTelemetry ?? string.Empty, request);
        await _hubContext.Clients.All.SendAsync("ReceiveTestResult", result);
        return Ok(result);
    }

    [HttpPost("simulate")]
    public async Task<ActionResult<TestResultDto>> SimulateCycle([FromBody] SaveResponseRequestDto request)
    {
        var result = await _testingService.SimulateTestCycleAsync(request);
        await _hubContext.Clients.All.SendAsync("ReceiveTestResult", result);
        return Ok(result);
    }

    [HttpGet("ports")]
    public ActionResult<List<string>> GetPorts()
    {
        var ports = _serialPortService.GetAvailablePorts();
        return Ok(ports);
    }

    [HttpPost("connect-port")]
    public async Task<ActionResult> ConnectPort([FromBody] PortConnectionRequestDto request)
    {
        bool success = _serialPortService.Connect(request.PortName, request.BaudRate);
        await _hubContext.Clients.All.SendAsync("ComPortStateChanged", success, _serialPortService.CurrentPort, _serialPortService.CurrentBaudRate, success ? "Connected" : "Connection failed");
        return Ok(new { success, port = _serialPortService.CurrentPort, baudRate = _serialPortService.CurrentBaudRate });
    }

    [HttpPost("disconnect-port")]
    public async Task<ActionResult> DisconnectPort()
    {
        _serialPortService.Disconnect();
        await _hubContext.Clients.All.SendAsync("ComPortStateChanged", false, null, 0, "Disconnected");
        return Ok(new { success = true });
    }
}
