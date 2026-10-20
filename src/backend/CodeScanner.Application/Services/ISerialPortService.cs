namespace CodeScanner.Application.Services;

public interface ISerialPortService
{
    List<string> GetAvailablePorts();
    bool Connect(string portName, int baudRate);
    void Disconnect();
    bool IsConnected { get; }
    string? CurrentPort { get; }
    int CurrentBaudRate { get; }

    event Action<string>? DataReceived;
    event Action<bool, string>? ConnectionStateChanged;
}
