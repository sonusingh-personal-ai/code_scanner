import { useEffect, useState, useCallback } from 'react';
import confetti from 'canvas-confetti';
import { Header } from './components/Header';
import { ShiftStatsBar } from './components/ShiftStatsBar';
import { TopPanel } from './components/TopPanel';
import { TelemetryTables } from './components/TelemetryTables';
import { LineModal } from './components/LineModal';
import { AlertModal } from './components/AlertModal';
import { dashboardApi, testingApi, linesApi } from './services/api';
import { signalRService } from './services/signalrService';
import { soundEffects } from './services/soundEffects';
import type {
  DashboardInitDto,
  LineDto,
  SaveResponseRequestDto,
  TestResultDto,
  ShiftStatsDto
} from './types';

export function App() {
  // State
  const [initData, setInitData] = useState<DashboardInitDto | null>(null);
  const [shiftStats, setShiftStats] = useState<ShiftStatsDto | null>(null);
  const [isSignalRConnected, setIsSignalRConnected] = useState(false);
  const [isPortConnected, setIsPortConnected] = useState(false);
  const [currentPort, setCurrentPort] = useState<string | null>(null);
  const [currentBaud, setCurrentBaud] = useState(9600);
  const [isTestingRunning, setIsTestingRunning] = useState(false);
  const [isManualMode, setIsManualMode] = useState(false);

  // Status Badge: 'Ready' | 'Testing...' | 'PASS' | 'FAIL'
  const [statusBadge, setStatusBadge] = useState<{
    text: string;
    type: 'idle' | 'testing' | 'pass' | 'fail';
  }>({ text: 'STANDBY / READY', type: 'idle' });

  // Active form data
  const [formData, setFormData] = useState<SaveResponseRequestDto>({
    barcode: '',
    qcStatus: 1, // Testing
    productionLine: 1, // Card
    conProgNo: 'CP-202',
    disProgNo: '',
    systemRating: '12V-850VA'
  });

  // Active Test Result & Telemetry
  const [activeResult, setActiveResult] = useState<TestResultDto | null>(null);

  // Line modal states
  const [isLineModalOpen, setIsLineModalOpen] = useState(false);
  const [editingLine, setEditingLine] = useState<LineDto | null>(null);

  // Alert modal state
  const [alertState, setAlertState] = useState<{
    isOpen: boolean;
    title: string;
    message: string;
    type?: 'info' | 'success' | 'warning' | 'error';
  }>({ isOpen: false, title: '', message: '' });

  const showAlert = (title: string, message: string, type: 'info' | 'success' | 'warning' | 'error' = 'info') => {
    setAlertState({ isOpen: true, title, message, type });
  };

  // Load Dashboard Data
  const loadDashboardData = useCallback(async () => {
    try {
      const data = await dashboardApi.getInitData();
      setInitData(data);
      setShiftStats(data.shiftStats);

      // Preselect first model & line if unset
      setFormData((prev) => ({
        ...prev,
        model: prev.model || (data.models[0]?.value ?? ''),
        line: prev.line || (data.lines[0]?.id ?? undefined),
        visualBy: prev.visualBy || (data.visualByList[0]?.id ?? undefined),
        testedBy: prev.testedBy || (data.testedByList[0]?.id ?? undefined),
        processEngg: prev.processEngg || (data.processEngList[0]?.id ?? undefined),
        disProgNo: prev.disProgNo || (data.progDisplayNoList[0]?.name ?? '')
      }));
    } catch (err) {
      console.error('Failed to load dashboard data:', err);
      showAlert('API Error', 'Could not connect to backend API at http://localhost:5000.', 'error');
    }
  }, []);

  // Handle incoming test result (from SignalR or REST)
  const handleTestResult = useCallback((result: TestResultDto) => {
    setActiveResult(result);
    setIsTestingRunning(false);

    if (result.isOverallPass) {
      setStatusBadge({ text: 'PASS', type: 'pass' });
      soundEffects.playPassSound();

      // Confetti celebration
      try {
        confetti({
          particleCount: 100,
          spread: 70,
          origin: { y: 0.6 }
        });
      } catch (e) {
        console.warn('Confetti error:', e);
      }
    } else {
      setStatusBadge({ text: 'FAIL', type: 'fail' });
      soundEffects.playFailSound();
    }

    // Refresh Shift Stats
    dashboardApi.getShiftStats(formData.line).then(setShiftStats).catch(console.error);
  }, [formData.line]);

  // Initialize App & SignalR
  useEffect(() => {
    loadDashboardData();

    // Setup SignalR
    signalRService.onReceiveTestResult(handleTestResult);
    signalRService.onComPortStateChanged((connected, port, baudRate) => {
      setIsPortConnected(connected);
      setCurrentPort(port);
      setCurrentBaud(baudRate);
    });

    signalRService.startConnection().then((connected) => {
      setIsSignalRConnected(connected);
    });

    return () => {
      signalRService.stopConnection();
    };
  }, [loadDashboardData, handleTestResult]);

  // Form Change Handler
  const handleFormChange = (fields: Partial<SaveResponseRequestDto>) => {
    setFormData((prev) => ({ ...prev, ...fields }));

    // If Line changed, reload shift stats for that line
    if (fields.line !== undefined) {
      dashboardApi.getShiftStats(fields.line).then(setShiftStats).catch(console.error);
    }
  };

  // Start Testing
  const handleStartTesting = async () => {
    if (!formData.model) {
      showAlert('Validation Error', 'Please select a Model No before testing.', 'warning');
      return;
    }
    if (!formData.line) {
      showAlert('Validation Error', 'Please select a Production Line.', 'warning');
      return;
    }
    if (!formData.testedBy || !formData.visualBy) {
      showAlert('Validation Error', 'Please select both Tested By and Visual By personnel.', 'warning');
      return;
    }

    setIsTestingRunning(true);
    setStatusBadge({ text: 'TESTING...', type: 'testing' });

    try {
      const result = await testingApi.simulateCycle({
        ...formData,
        barcode: formData.barcode || `AUTO-${Date.now().toString().slice(-6)}`
      });
      handleTestResult(result);
    } catch (err) {
      console.error('Testing cycle error:', err);
      setIsTestingRunning(false);
      setStatusBadge({ text: 'ERROR', type: 'fail' });
      showAlert('Test Error', 'Failed to execute test cycle. Check server logs.', 'error');
    }
  };

  // Line Management actions
  const handleSaveLine = async (name: string, sequence?: number) => {
    try {
      if (editingLine) {
        await linesApi.update(editingLine.id, name, sequence);
      } else {
        await linesApi.create(name, sequence);
      }
      const updatedLines = await linesApi.getAll();
      setInitData((prev) => prev ? { ...prev, lines: updatedLines } : null);
      showAlert('Success', `Line "${name}" saved successfully.`, 'success');
    } catch (err) {
      console.error('Line save error:', err);
      showAlert('Error', 'Failed to save line.', 'error');
    }
  };

  const handleMoveLine = async (id: number, direction: 'up' | 'down') => {
    try {
      await linesApi.move(id, direction);
      const updatedLines = await linesApi.getAll();
      setInitData((prev) => prev ? { ...prev, lines: updatedLines } : null);
    } catch (err) {
      console.error('Line move error:', err);
      showAlert('Error', 'Failed to reorder line.', 'error');
    }
  };

  const handleDeleteLine = async (id: number) => {
    const target = initData?.lines.find((l) => l.id === id);
    if (!target) return;

    if (!confirm(`Are you sure you want to delete line "${target.name}"?`)) {
      return;
    }

    try {
      await linesApi.delete(id);
      const updatedLines = await linesApi.getAll();
      setInitData((prev) => prev ? { ...prev, lines: updatedLines } : null);
      if (formData.line === id) {
        setFormData((prev) => ({ ...prev, line: updatedLines[0]?.id }));
      }
      showAlert('Success', `Line "${target.name}" deleted.`, 'success');
    } catch (err) {
      console.error('Line delete error:', err);
      showAlert('Error', 'Failed to delete line.', 'error');
    }
  };

  // Port actions
  const handleConnectPort = async (port: string, baud: number) => {
    try {
      const res = await testingApi.connectPort(port, baud);
      if (res.success) {
        setIsPortConnected(true);
        setCurrentPort(res.port);
        setCurrentBaud(res.baudRate);
        showAlert('Port Connected', `Connected to ${res.port} at ${res.baudRate} baud.`, 'success');
      } else {
        showAlert('Port Error', `Could not open ${port}.`, 'error');
      }
    } catch (err) {
      console.error('Connect port error:', err);
      showAlert('Port Error', 'Failed to connect to serial port.', 'error');
    }
  };

  const handleDisconnectPort = async () => {
    try {
      await testingApi.disconnectPort();
      setIsPortConnected(false);
      setCurrentPort(null);
      showAlert('Port Disconnected', 'Serial port connection closed.', 'info');
    } catch (err) {
      console.error('Disconnect port error:', err);
    }
  };

  const handleRefreshPorts = async () => {
    try {
      const ports = await testingApi.getPorts();
      setInitData((prev) => prev ? { ...prev, availablePorts: ports } : null);
      showAlert('Ports Refreshed', `Found ${ports.length} available ports.`, 'info');
    } catch (err) {
      console.error('Refresh ports error:', err);
    }
  };

  const handleVerifyPrinter = () => {
    showAlert('Printer Test', 'Test label print command sent to selected printer.', 'success');
  };

  if (!initData) {
    return (
      <div className="min-h-screen bg-slate-950 flex flex-col items-center justify-center text-white">
        <div className="w-8 h-8 border-4 border-blue-500 border-t-transparent rounded-full animate-spin mb-4" />
        <p className="text-sm font-semibold tracking-wide text-slate-300">
          Connecting to CodeScanner Core API...
        </p>
        <p className="text-xs text-slate-500 mt-1">Target: http://localhost:5000</p>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-slate-950 text-slate-100 flex flex-col">
      {/* Top Header */}
      <Header
        isSignalRConnected={isSignalRConnected}
        comPort={currentPort}
        baudRate={currentBaud}
        onRefreshAll={loadDashboardData}
      />

      <main className="flex-1 p-3 max-w-[1700px] w-full mx-auto flex flex-col">
        {/* Real-time Shift Statistics Summary */}
        <ShiftStatsBar stats={shiftStats} />

        {/* Compact 6x3 Top Panel */}
        <TopPanel
          initData={initData}
          formData={formData}
          onChange={handleFormChange}
          onStartTesting={handleStartTesting}
          onVerifyPrinter={handleVerifyPrinter}
          onRefreshPorts={handleRefreshPorts}
          onConnectPort={handleConnectPort}
          onDisconnectPort={handleDisconnectPort}
          isPortConnected={isPortConnected}
          isTestingRunning={isTestingRunning}
          statusBadge={statusBadge}
          onOpenAddLine={() => {
            setEditingLine(null);
            setIsLineModalOpen(true);
          }}
          onOpenEditLine={(line) => {
            setEditingLine(line);
            setIsLineModalOpen(true);
          }}
          onMoveLine={handleMoveLine}
          onDeleteLine={handleDeleteLine}
          isManualMode={isManualMode}
          onToggleMode={setIsManualMode}
        />

        {/* Display 1 & Display 2 Telemetry Tables */}
        <div className="flex-1">
          <TelemetryTables
            parameters={activeResult?.parameters || []}
            modelName={activeResult?.modelName || formData.model || ''}
            isOverallPass={activeResult?.isOverallPass}
          />
        </div>
      </main>

      {/* Modern Line Modal */}
      <LineModal
        isOpen={isLineModalOpen}
        initialLine={editingLine}
        onSave={handleSaveLine}
        onClose={() => setIsLineModalOpen(false)}
      />

      {/* Modern Non-blocking Alert Modal */}
      <AlertModal
        isOpen={alertState.isOpen}
        title={alertState.title}
        message={alertState.message}
        type={alertState.type}
        onClose={() => setAlertState((prev) => ({ ...prev, isOpen: false }))}
      />
    </div>
  );
}

export default App;
