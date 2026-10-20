import React from 'react';
import { CheckCircle2, AlertTriangle, HelpCircle } from 'lucide-react';
import type { TelemetryParamDto } from '../types';

interface TelemetryTablesProps {
  parameters: TelemetryParamDto[];
  modelName: string;
  isOverallPass?: boolean;
}

export const TelemetryTables: React.FC<TelemetryTablesProps> = ({
  parameters,
  modelName,
  isOverallPass
}) => {
  if (!parameters || parameters.length === 0) {
    return (
      <div className="bg-slate-900 border border-slate-800 rounded-lg p-8 text-center text-slate-400">
        <p className="text-sm font-medium">No active test telemetry data.</p>
        <p className="text-xs text-slate-500 mt-1">
          Select a Model and click "Start Testing" or trigger a scan from the testing jig.
        </p>
      </div>
    );
  }

  // Split into two halves: Display 1 and Display 2
  const midPoint = Math.ceil(parameters.length / 2);
  const display1 = parameters.slice(0, midPoint);
  const display2 = parameters.slice(midPoint);

  const renderTable = (items: TelemetryParamDto[], title: string) => (
    <div className="flex-1 bg-slate-900/90 border border-slate-800 rounded-lg overflow-hidden shadow-sm flex flex-col">
      <div className="bg-slate-800/80 px-3 py-1.5 border-b border-slate-700/80 flex items-center justify-between">
        <span className="text-xs font-bold uppercase tracking-wider text-slate-200">
          {title}
        </span>
        <span className="text-[11px] font-mono text-slate-400">
          {items.length} Parameters
        </span>
      </div>

      <div className="overflow-x-auto">
        <table className="w-full text-left border-collapse text-xs">
          <thead>
            <tr className="border-b border-slate-800 bg-slate-950/40 text-[10px] uppercase font-bold text-slate-400">
              <th className="py-1.5 px-3">#</th>
              <th className="py-1.5 px-3">Parameter Name</th>
              <th className="py-1.5 px-3 text-center">Display</th>
              <th className="py-1.5 px-3 text-center">Actual</th>
              <th className="py-1.5 px-3 text-center">Status</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-800/60 font-mono">
            {items.map((item, index) => {
              const isPass = item.isPass || item.status.toUpperCase() === 'OK';
              const isFail = item.status.toUpperCase() === 'FAIL';
              return (
                <tr
                  key={`${item.parameter}-${index}`}
                  className={`hover:bg-slate-800/40 transition-colors ${
                    isFail ? 'bg-rose-950/20' : ''
                  }`}
                >
                  <td className="py-1.5 px-3 text-slate-500 text-[11px]">
                    {index + 1}
                  </td>
                  <td className="py-1.5 px-3 font-sans font-medium text-slate-200">
                    {item.parameter}
                  </td>
                  <td className="py-1.5 px-3 text-center text-slate-300">
                    {item.display}
                  </td>
                  <td className="py-1.5 px-3 text-center text-slate-300">
                    {item.actual}
                  </td>
                  <td className="py-1.5 px-3 text-center">
                    {isPass ? (
                      <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded text-[10px] font-bold bg-emerald-500/20 text-emerald-300 border border-emerald-500/30">
                        <CheckCircle2 className="w-3 h-3" /> OK
                      </span>
                    ) : isFail ? (
                      <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded text-[10px] font-bold bg-rose-500/20 text-rose-300 border border-rose-500/30">
                        <AlertTriangle className="w-3 h-3" /> FAIL
                      </span>
                    ) : (
                      <span className="inline-flex items-center gap-1 px-2 py-0.5 rounded text-[10px] font-bold bg-slate-700/40 text-slate-400 border border-slate-700">
                        <HelpCircle className="w-3 h-3" /> {item.status || 'N/A'}
                      </span>
                    )}
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
    </div>
  );

  return (
    <div className="space-y-2">
      <div className="flex items-center justify-between px-1 text-xs">
        <span className="text-slate-400 font-medium">
          Active Model: <strong className="text-white font-mono">{modelName || 'Standard'}</strong>
        </span>
        {isOverallPass !== undefined && (
          <span
            className={`px-2.5 py-0.5 rounded-full font-bold text-xs ${
              isOverallPass
                ? 'bg-emerald-500 text-slate-950 shadow-emerald-500/30 shadow'
                : 'bg-rose-600 text-white shadow-rose-600/30 shadow'
            }`}
          >
            {isOverallPass ? 'CYCLE VERIFIED: PASS' : 'CYCLE ALERT: FAIL DETECTED'}
          </span>
        )}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-3">
        {renderTable(display1, 'Display 1 (Primary Telemetry)')}
        {renderTable(display2, 'Display 2 (Secondary Telemetry)')}
      </div>
    </div>
  );
};
