using CodeScanner.Application.DTOs;

namespace CodeScanner.Application.Services;

public interface ITestingService
{
    Task<TestResultDto> ProcessRawTelemetryAsync(string rawTelemetry, SaveResponseRequestDto context);
    Task<TestResultDto> SimulateTestCycleAsync(SaveResponseRequestDto context);
}
