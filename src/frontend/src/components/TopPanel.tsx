import React from 'react';
import {
  Play,
  RotateCw,
  Printer,
  Plus,
  ArrowUp,
  ArrowDown,
  Edit2,
  Trash2,
  CheckCircle,
  AlertCircle
} from 'lucide-react';
import type {
  DashboardInitDto,
  LineDto,
  SaveResponseRequestDto
} from '../types';

interface TopPanelProps {
  initData: DashboardInitDto;
  formData: SaveResponseRequestDto;
  onChange: (fields: Partial<SaveResponseRequestDto>) => void;
  onStartTesting: () => void;
  onVerifyPrinter: () => void;
  onRefreshPorts: () => void;
  onConnectPort: (port: string, baud: number) => void;
  onDisconnectPort: () => void;
  isPortConnected: boolean;
  isTestingRunning: boolean;
  statusBadge: {
    text: string;
    type: 'idle' | 'testing' | 'pass' | 'fail';
  };
  onOpenAddLine: () => void;
  onOpenEditLine: (line: LineDto) => void;
  onMoveLine: (id: number, dir: 'up' | 'down') => void;
  onDeleteLine: (id: number) => void;
  isManualMode: boolean;
  onToggleMode: (manual: boolean) => void;
}

export const TopPanel: React.FC<TopPanelProps> = ({
  initData,
  formData,
  onChange,
  onStartTesting,
  onVerifyPrinter,
  onRefreshPorts,
  onConnectPort,
  onDisconnectPort,
  isPortConnected,
  isTestingRunning,
  statusBadge,
  onOpenAddLine,
  onOpenEditLine,
  onMoveLine,
  onDeleteLine,
  isManualMode,
  onToggleMode
}) => {
  const isAssembly = formData.productionLine === 2;

  // Selected Line
  const selectedLineObj = initData.lines.find((l) => l.id === formData.line);

  return (
    <div className="bg-slate-900 border border-slate-800 rounded-lg p-3 shadow-md text-xs mb-3">
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-6 gap-3">
        {/* COLUMN 1 */}
        <div className="space-y-2">
          {/* Date & Time */}
          <div>
            <label className="block text-[11px] font-medium text-slate-400 mb-1">
              Date / Time
            </label>
            <div className="grid grid-cols-2 gap-1.5 font-mono">
              <input
                type="text"
                readOnly
                value={new Date().toLocaleDateString('en-GB')}
                className="w-full bg-slate-800/80 border border-slate-700 rounded px-2 py-1 text-slate-300 text-center"
              />
              <input
                type="text"
                readOnly
                value={new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                className="w-full bg-slate-800/80 border border-slate-700 rounded px-2 py-1 text-slate-300 text-center"
              />
            </div>
          </div>

          {/* Display Prog. No. */}
          <div>
            <label className="block text-[11px] font-medium text-slate-400 mb-1">
              Display Prog. No.
            </label>
            <select
              value={formData.disProgNo || ''}
              onChange={(e) => onChange({ disProgNo: e.target.value })}
              className="w-full bg-slate-800 border border-slate-700 rounded px-2 py-1 text-white focus:outline-none focus:ring-1 focus:ring-blue-500"
            >
              <option value="">Select Display Prog. No.</option>
              {initData.progDisplayNoList.map((m) => (
                <option key={m.id} value={m.name}>
                  {m.name}
                </option>
              ))}
            </select>
          </div>

          {/* Status Badge */}
          <div>
            <label className="block text-[11px] font-medium text-slate-400 mb-1">
              Status Badge
            </label>
            <div
              className={`w-full py-1.5 px-3 rounded text-center font-bold font-mono tracking-wide flex items-center justify-center gap-1.5 transition-all duration-200 ${
                statusBadge.type === 'pass'
                  ? 'bg-emerald-500 text-slate-950 shadow-md shadow-emerald-500/20'
                  : statusBadge.type === 'fail'
                  ? 'bg-rose-600 text-white shadow-md shadow-rose-600/20'
                  : statusBadge.type === 'testing'
                  ? 'bg-amber-500 text-slate-950 animate-pulse'
                  : 'bg-slate-800 border border-slate-700 text-slate-300'
              }`}
            >
              {statusBadge.type === 'pass' && <CheckCircle className="w-3.5 h-3.5" />}
              {statusBadge.type === 'fail' && <AlertCircle className="w-3.5 h-3.5" />}
              {statusBadge.text}
            </div>
          </div>
        </div>

        {/* COLUMN 2 */}
        <div className="space-y-2">
          {/* Testing Stage */}
          <div>
            <label className="block text-[11px] font-medium text-slate-400 mb-1">
              Testing Stage <span className="text-rose-400">*</span>
            </label>
            <select
              value={formData.qcStatus ?? 1}
              onChange={(e) => onChange({ qcStatus: parseInt(e.target.value, 10) })}
              className="w-full bg-slate-800 border border-slate-700 rounded px-2 py-1 text-white focus:outline-none focus:ring-1 focus:ring-blue-500"
            >
              <option value={1}>Testing</option>
              <option value={2}>QC</option>
              <option value={3}>OQC</option>
            </select>
          </div>

          {/* Control Prog. No. */}
          <div>
            <label className="block text-[11px] font-medium text-slate-400 mb-1">
              Control Prog. No.
            </label>
            <input
              type="text"
              readOnly
              value={formData.conProgNo || 'CP-202'}
              className="w-full bg-slate-800/80 border border-slate-700 rounded px-2 py-1 text-slate-300"
            />
          </div>

          {/* Model No */}
          <div>
            <label className="block text-[11px] font-medium text-slate-400 mb-1">
              Model No <span className="text-rose-400">*</span>
            </label>
            <select
              value={formData.model || ''}
              onChange={(e) => onChange({ model: e.target.value })}
              className="w-full bg-slate-800 border border-slate-700 rounded px-2 py-1 text-white font-medium focus:outline-none focus:ring-1 focus:ring-blue-500"
            >
              <option value="">Select Model</option>
              {initData.models.map((m) => (
                <option key={m.id} value={m.value}>
                  {m.name} ({m.value})
                </option>
              ))}
            </select>
          </div>
        </div>

        {/* COLUMN 3 */}
        <div className="space-y-2">
          {/* Tested By */}
          <div>
            <label className="block text-[11px] font-medium text-slate-400 mb-1">
              Tested By <span className="text-rose-400">*</span>
            </label>
            <select
              value={formData.testedBy || ''}
              onChange={(e) => onChange({ testedBy: e.target.value ? parseInt(e.target.value, 10) : undefined })}
              className="w-full bg-slate-800 border border-slate-700 rounded px-2 py-1 text-white focus:outline-none focus:ring-1 focus:ring-blue-500"
            >
              <option value="">Select Tested By</option>
              {initData.testedByList.map((m) => (
                <option key={m.id} value={m.id}>
                  {m.name}
                </option>
              ))}
            </select>
          </div>

          {/* Visual By */}
          <div>
            <label className="block text-[11px] font-medium text-slate-400 mb-1">
              Visual By <span className="text-rose-400">*</span>
            </label>
            <select
              value={formData.visualBy || ''}
              onChange={(e) => onChange({ visualBy: e.target.value ? parseInt(e.target.value, 10) : undefined })}
              className="w-full bg-slate-800 border border-slate-700 rounded px-2 py-1 text-white focus:outline-none focus:ring-1 focus:ring-blue-500"
            >
              <option value="">Select Visual By</option>
              {initData.visualByList.map((m) => (
                <option key={m.id} value={m.id}>
                  {m.name}
                </option>
              ))}
            </select>
          </div>

          {/* Sys Rating */}
          <div>
            <label className="block text-[11px] font-medium text-slate-400 mb-1">
              Sys Rating
            </label>
            <input
              type="text"
              readOnly
              value={formData.systemRating || '12V-850VA'}
              className="w-full bg-slate-800/80 border border-slate-700 rounded px-2 py-1 text-slate-300"
            />
          </div>
        </div>

        {/* COLUMN 4 */}
        <div className="space-y-2">
          {/* Production Line */}
          <div>
            <label className="block text-[11px] font-medium text-slate-400 mb-1">
              Production Line <span className="text-rose-400">*</span>
            </label>
            <select
              value={formData.productionLine ?? 1}
              onChange={(e) => onChange({ productionLine: parseInt(e.target.value, 10) })}
              className="w-full bg-slate-800 border border-slate-700 rounded px-2 py-1 text-white font-medium focus:outline-none focus:ring-1 focus:ring-blue-500"
            >
              <option value={1}>Card</option>
              <option value={2}>Assembly</option>
            </select>
          </div>

          {/* Process Engg */}
          <div>
            <label className="block text-[11px] font-medium text-slate-400 mb-1">
              Process Engg.
            </label>
            <select
              value={formData.processEngg || ''}
              onChange={(e) => onChange({ processEngg: e.target.value ? parseInt(e.target.value, 10) : undefined })}
              className="w-full bg-slate-800 border border-slate-700 rounded px-2 py-1 text-white focus:outline-none focus:ring-1 focus:ring-blue-500"
            >
              <option value="">Select Process Engg</option>
              {initData.processEngList.map((m) => (
                <option key={m.id} value={m.id}>
                  {m.name}
                </option>
              ))}
            </select>
          </div>

          {/* Sys Sr. No. */}
          <div>
            <label className="block text-[11px] font-medium text-slate-400 mb-1">
              Sys Sr. No. {isManualMode ? '(Manual)' : '(Automatic)'}
            </label>
            <input
              type="text"
              disabled={!isManualMode}
              value={formData.barcode || ''}
              onChange={(e) => onChange({ barcode: e.target.value })}
              placeholder={isManualMode ? 'Enter Barcode...' : 'Auto via Serial/Scanner'}
              className={`w-full border rounded px-2 py-1 text-white focus:outline-none focus:ring-1 focus:ring-blue-500 ${
                isManualMode ? 'bg-slate-800 border-blue-500/50' : 'bg-slate-800/80 border-slate-700 text-slate-400'
              }`}
            />
          </div>
        </div>

        {/* COLUMN 5 */}
        <div className="space-y-2">
          {/* Card / Assembly Serial Number (Dynamic label!) */}
          <div>
            <label className="block text-[11px] font-medium text-slate-400 mb-1">
              {isAssembly ? 'Assembly Serial Number' : 'Card Serial Number'}
            </label>
            <input
              type="text"
              value={formData.serialCardNo || ''}
              onChange={(e) => onChange({ serialCardNo: e.target.value })}
              placeholder="e.g. SN-098234"
              className="w-full bg-slate-800 border border-slate-700 rounded px-2 py-1 text-white focus:outline-none focus:ring-1 focus:ring-blue-500"
            />
          </div>

          {/* Printer List + Verify button */}
          <div>
            <label className="block text-[11px] font-medium text-slate-400 mb-1">
              Printer List
            </label>
            <div className="flex space-x-1">
              <select className="flex-1 bg-slate-800 border border-slate-700 rounded px-2 py-1 text-white truncate focus:outline-none focus:ring-1 focus:ring-blue-500">
                <option value="">Select Printer</option>
                {initData.availablePrinters.map((p, idx) => (
                  <option key={idx} value={p}>
                    {p}
                  </option>
                ))}
              </select>
              <button
                type="button"
                onClick={onVerifyPrinter}
                title="Verify Printer Connection"
                className="px-2 py-1 bg-slate-800 hover:bg-slate-700 border border-slate-700 rounded text-slate-300 hover:text-white transition-colors"
              >
                <Printer className="w-3.5 h-3.5" />
              </button>
            </div>
          </div>

          {/* Mode Switch + Start Testing Button */}
          <div className="pt-1 flex items-center space-x-1.5">
            <button
              type="button"
              onClick={() => onToggleMode(!isManualMode)}
              className={`flex-1 py-1.5 px-2 rounded font-semibold text-center transition-colors border ${
                isManualMode
                  ? 'bg-amber-500/20 text-amber-300 border-amber-500/40'
                  : 'bg-slate-800 text-slate-300 border-slate-700 hover:bg-slate-700'
              }`}
            >
              {isManualMode ? 'Manual' : 'Automatic'}
            </button>

            <button
              type="button"
              disabled={isTestingRunning}
              onClick={onStartTesting}
              className="flex-1 py-1.5 px-2 bg-blue-600 hover:bg-blue-500 disabled:bg-blue-900 text-white rounded font-bold shadow-md flex items-center justify-center gap-1 transition-colors"
            >
              <Play className="w-3 h-3 fill-current" />
              Start
            </button>
          </div>
        </div>

        {/* COLUMN 6 */}
        <div className="space-y-2">
          {/* COM Port & Baud Rate side-by-side */}
          <div>
            <div className="flex items-center justify-between mb-1">
              <label className="text-[11px] font-medium text-slate-400">
                COM Port & Baud
              </label>
              <button
                type="button"
                onClick={onRefreshPorts}
                title="Refresh COM Ports"
                className="text-blue-400 hover:text-blue-300"
              >
                <RotateCw className="w-3 h-3" />
              </button>
            </div>
            <div className="grid grid-cols-2 gap-1.5">
              <select
                id="com_port_select"
                defaultValue={initData.availablePorts[0] || 'COM_SIMULATOR'}
                className="w-full bg-slate-800 border border-slate-700 rounded px-1.5 py-1 text-white focus:outline-none focus:ring-1 focus:ring-blue-500 text-[11px]"
              >
                {initData.availablePorts.map((p, idx) => (
                  <option key={idx} value={p}>
                    {p}
                  </option>
                ))}
              </select>
              <select
                id="baud_rate_select"
                defaultValue="9600"
                className="w-full bg-slate-800 border border-slate-700 rounded px-1.5 py-1 text-white focus:outline-none focus:ring-1 focus:ring-blue-500 text-[11px]"
              >
                <option value="9600">9600</option>
                <option value="19200">19200</option>
                <option value="38400">38400</option>
                <option value="115200">115200</option>
              </select>
            </div>
            <button
              type="button"
              onClick={() => {
                const portSel = document.getElementById('com_port_select') as HTMLSelectElement;
                const baudSel = document.getElementById('baud_rate_select') as HTMLSelectElement;
                if (isPortConnected) {
                  onDisconnectPort();
                } else {
                  onConnectPort(portSel.value, parseInt(baudSel.value, 10));
                }
              }}
              className={`w-full mt-1 py-0.5 px-2 rounded text-[10px] font-bold border transition-colors ${
                isPortConnected
                  ? 'bg-emerald-500/20 text-emerald-300 border-emerald-500/40 hover:bg-rose-500/20 hover:text-rose-300 hover:border-rose-500/40'
                  : 'bg-slate-800 text-slate-300 border-slate-700 hover:bg-slate-700'
              }`}
            >
              {isPortConnected ? 'Connected (Click to Disconnect)' : 'Connect Port'}
            </button>
          </div>

          {/* Line Selector with Inline Management */}
          <div>
            <div className="flex items-center justify-between mb-1">
              <label className="text-[11px] font-medium text-slate-400">
                Production Line
              </label>
              <button
                type="button"
                onClick={onOpenAddLine}
                title="Add New Line"
                className="text-emerald-400 hover:text-emerald-300 flex items-center gap-0.5 text-[10px]"
              >
                <Plus className="w-3 h-3" /> New
              </button>
            </div>
            <div className="flex space-x-1">
              <select
                value={formData.line || ''}
                onChange={(e) => onChange({ line: e.target.value ? parseInt(e.target.value, 10) : undefined })}
                className="flex-1 bg-slate-800 border border-slate-700 rounded px-2 py-1 text-white focus:outline-none focus:ring-1 focus:ring-blue-500"
              >
                <option value="">-- Select Line --</option>
                {initData.lines.map((l) => (
                  <option key={l.id} value={l.id}>
                    #{l.sequence} - {l.name}
                  </option>
                ))}
              </select>

              {/* Inline Action buttons for selected line */}
              {selectedLineObj && (
                <div className="flex items-center space-x-0.5">
                  <button
                    type="button"
                    onClick={() => onMoveLine(selectedLineObj.id, 'up')}
                    title="Move Up"
                    className="p-1 bg-slate-800 hover:bg-slate-700 text-slate-400 hover:text-white rounded border border-slate-700"
                  >
                    <ArrowUp className="w-3 h-3" />
                  </button>
                  <button
                    type="button"
                    onClick={() => onMoveLine(selectedLineObj.id, 'down')}
                    title="Move Down"
                    className="p-1 bg-slate-800 hover:bg-slate-700 text-slate-400 hover:text-white rounded border border-slate-700"
                  >
                    <ArrowDown className="w-3 h-3" />
                  </button>
                  <button
                    type="button"
                    onClick={() => onOpenEditLine(selectedLineObj)}
                    title="Edit Line"
                    className="p-1 bg-slate-800 hover:bg-slate-700 text-blue-400 hover:text-blue-300 rounded border border-slate-700"
                  >
                    <Edit2 className="w-3 h-3" />
                  </button>
                  <button
                    type="button"
                    onClick={() => onDeleteLine(selectedLineObj.id)}
                    title="Delete Line"
                    className="p-1 bg-slate-800 hover:bg-slate-700 text-rose-400 hover:text-rose-300 rounded border border-slate-700"
                  >
                    <Trash2 className="w-3 h-3" />
                  </button>
                </div>
              )}
            </div>
          </div>

          {/* Assembly: Printer Model Selector */}
          {isAssembly && (
            <div>
              <label className="block text-[11px] font-medium text-slate-400 mb-1">
                Printer Model
              </label>
              <select
                value={formData.printerModelId || ''}
                onChange={(e) => onChange({ printerModelId: e.target.value ? parseInt(e.target.value, 10) : undefined })}
                className="w-full bg-slate-800 border border-slate-700 rounded px-2 py-1 text-white focus:outline-none focus:ring-1 focus:ring-blue-500"
              >
                <option value="">Select Printer Model</option>
                {initData.printerModels.map((pm) => (
                  <option key={pm.id} value={pm.id}>
                    {pm.name}
                  </option>
                ))}
              </select>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
