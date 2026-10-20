import * as signalR from '@microsoft/signalr';
import type { TestResultDto, SaveResponseRequestDto } from '../types';

export class SignalRService {
  private hubConnection: signalR.HubConnection | null = null;
  private onTestResultCallback: ((result: TestResultDto) => void) | null = null;
  private onComPortStateCallback: ((connected: boolean, port: string | null, baudRate: number, msg?: string) => void) | null = null;

  public async startConnection(url = 'http://localhost:5000/hub/testing'): Promise<boolean> {
    try {
      this.hubConnection = new signalR.HubConnectionBuilder()
        .withUrl(url, {
          skipNegotiation: false,
          transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling
        })
        .withAutomaticReconnect([0, 2000, 5000, 10000])
        .configureLogging(signalR.LogLevel.Information)
        .build();

      this.hubConnection.on('ReceiveTestResult', (result: TestResultDto) => {
        if (this.onTestResultCallback) {
          this.onTestResultCallback(result);
        }
      });

      this.hubConnection.on('ComPortStateChanged', (connected: boolean, port: string | null, baudRate: number, msg?: string) => {
        if (this.onComPortStateCallback) {
          this.onComPortStateCallback(connected, port, baudRate, msg);
        }
      });

      await this.hubConnection.start();
      console.log('SignalR connected successfully.');
      return true;
    } catch (err) {
      console.warn('SignalR initial connection failed, will retry or operate in REST fallback:', err);
      return false;
    }
  }

  public onReceiveTestResult(callback: (result: TestResultDto) => void) {
    this.onTestResultCallback = callback;
  }

  public onComPortStateChanged(callback: (connected: boolean, port: string | null, baudRate: number, msg?: string) => void) {
    this.onComPortStateCallback = callback;
  }

  public async triggerTest(request: SaveResponseRequestDto): Promise<TestResultDto | null> {
    if (this.hubConnection && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      return await this.hubConnection.invoke<TestResultDto>('TriggerTest', request);
    }
    return null;
  }

  public async stopConnection(): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.stop();
    }
  }
}

export const signalRService = new SignalRService();
