import React from 'react';
import { Award, CheckCircle, XCircle, TrendingUp, Calendar } from 'lucide-react';
import type { ShiftStatsDto } from '../types';

interface ShiftStatsBarProps {
  stats: ShiftStatsDto | null;
}

export const ShiftStatsBar: React.FC<ShiftStatsBarProps> = ({ stats }) => {
  if (!stats) return null;

  return (
    <div className="bg-slate-800/80 border border-slate-700/80 rounded-lg p-2.5 shadow-sm grid grid-cols-2 md:grid-cols-5 gap-3 text-xs mb-3">
      {/* Shift & Date */}
      <div className="flex items-center space-x-2.5 px-3 py-1.5 bg-slate-900/60 rounded border border-slate-800">
        <Calendar className="w-4 h-4 text-sky-400 shrink-0" />
        <div>
          <div className="text-[10px] uppercase font-bold text-slate-400 tracking-wider">Shift & Date</div>
          <div className="font-semibold text-white truncate">{stats.currentShift} · {stats.date}</div>
        </div>
      </div>

      {/* Total Tested */}
      <div className="flex items-center space-x-2.5 px-3 py-1.5 bg-slate-900/60 rounded border border-slate-800">
        <Award className="w-4 h-4 text-indigo-400 shrink-0" />
        <div>
          <div className="text-[10px] uppercase font-bold text-slate-400 tracking-wider">Total Tested</div>
          <div className="text-base font-bold text-white font-mono">{stats.totalTested}</div>
        </div>
      </div>

      {/* Passed */}
      <div className="flex items-center space-x-2.5 px-3 py-1.5 bg-emerald-950/40 rounded border border-emerald-800/50">
        <CheckCircle className="w-4 h-4 text-emerald-400 shrink-0" />
        <div>
          <div className="text-[10px] uppercase font-bold text-emerald-400 tracking-wider">Passed</div>
          <div className="text-base font-bold text-emerald-300 font-mono">{stats.passCount}</div>
        </div>
      </div>

      {/* Failed */}
      <div className="flex items-center space-x-2.5 px-3 py-1.5 bg-rose-950/40 rounded border border-rose-800/50">
        <XCircle className="w-4 h-4 text-rose-400 shrink-0" />
        <div>
          <div className="text-[10px] uppercase font-bold text-rose-400 tracking-wider">Failed</div>
          <div className="text-base font-bold text-rose-300 font-mono">{stats.failCount}</div>
        </div>
      </div>

      {/* Yield % */}
      <div className="flex items-center space-x-2.5 px-3 py-1.5 bg-slate-900/60 rounded border border-slate-800">
        <TrendingUp className="w-4 h-4 text-amber-400 shrink-0" />
        <div className="w-full">
          <div className="flex justify-between items-center">
            <span className="text-[10px] uppercase font-bold text-slate-400 tracking-wider">Yield</span>
            <span className="font-bold text-amber-300 font-mono">{stats.yieldPercentage}%</span>
          </div>
          <div className="w-full bg-slate-700 h-1.5 rounded-full mt-1 overflow-hidden">
            <div
              className={`h-full transition-all duration-500 ${
                stats.yieldPercentage >= 95 ? 'bg-emerald-500' : stats.yieldPercentage >= 85 ? 'bg-amber-500' : 'bg-rose-500'
              }`}
              style={{ width: `${Math.min(100, Math.max(0, stats.yieldPercentage))}%` }}
            />
          </div>
        </div>
      </div>
    </div>
  );
};
