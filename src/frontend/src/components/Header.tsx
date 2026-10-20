import React, { useEffect, useState } from 'react';
import { Activity, Radio, Cpu, RefreshCw } from 'lucide-react';

interface HeaderProps {
  isSignalRConnected: boolean;
  comPort: string | null;
  baudRate: number;
  onRefreshAll: () => void;
}

export const Header: React.FC<HeaderProps> = ({
  isSignalRConnected,
  comPort,
  baudRate,
  onRefreshAll
}) => {
  const [currentTime, setCurrentTime] = useState<string>('');

  useEffect(() => {
    const updateTime = () => {
      const now = new Date();
      setCurrentTime(now.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', second: '2-digit' }));
    };
    updateTime();
    const interval = setInterval(updateTime, 1000);
    return () => clearInterval(interval);
  }, []);

  return (
    <header className="bg-slate-900 border-b border-slate-800 text-white px-4 py-2 flex flex-wrap items-center justify-between shadow-md">
      <div className="flex items-center space-x-3">
        <div className="p-1.5 bg-blue-600 rounded-md flex items-center justify-center shadow">
          <Activity className="w-5 h-5 text-white" />
        </div>
        <div>
          <h1 className="text-base font-bold tracking-wide flex items-center gap-2">
            UTL Industrial CodeScanner
            <span className="text-xs font-semibold px-2 py-0.5 rounded bg-blue-500/20 text-blue-300 border border-blue-500/30">
              .NET Core + React
            </span>
          </h1>
          <p className="text-xs text-slate-400">Barcode & Inverter Testing Telemetry Platform</p>
        </div>
      </div>

      <div className="flex items-center space-x-3 text-xs">
        {/* SignalR Live Pill */}
        <div className="flex items-center space-x-1.5 px-2.5 py-1 rounded-full bg-slate-800 border border-slate-700">
          <span className={`w-2 h-2 rounded-full ${isSignalRConnected ? 'bg-emerald-400 animate-pulse' : 'bg-rose-500'}`} />
          <span className="text-slate-300">
            {isSignalRConnected ? 'Live Telemetry Active' : 'Offline / Polling'}
          </span>
        </div>

        {/* COM Port Pill */}
        <div className="flex items-center space-x-1.5 px-2.5 py-1 rounded bg-slate-800 border border-slate-700">
          <Radio className="w-3.5 h-3.5 text-blue-400" />
          <span className="text-slate-300">
            {comPort ? `${comPort} @ ${baudRate}` : 'COM Disconnected'}
          </span>
        </div>

        {/* Live Clock */}
        <div className="flex items-center space-x-1.5 px-2.5 py-1 rounded bg-slate-800 text-slate-300 font-mono">
          <Cpu className="w-3.5 h-3.5 text-amber-400" />
          <span>{currentTime}</span>
        </div>

        {/* Refresh button */}
        <button
          onClick={onRefreshAll}
          title="Reload system metadata"
          className="p-1.5 rounded hover:bg-slate-800 text-slate-300 hover:text-white transition-colors"
        >
          <RefreshCw className="w-4 h-4" />
        </button>
      </div>
    </header>
  );
};
