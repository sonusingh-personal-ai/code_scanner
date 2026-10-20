import React, { useState, useEffect } from 'react';
import { X, Layers } from 'lucide-react';
import type { LineDto } from '../types';

interface LineModalProps {
  isOpen: boolean;
  initialLine?: LineDto | null;
  onSave: (name: string, sequence?: number) => void;
  onClose: () => void;
}

export const LineModal: React.FC<LineModalProps> = ({
  isOpen,
  initialLine,
  onSave,
  onClose
}) => {
  const [name, setName] = useState('');
  const [sequence, setSequence] = useState<number | undefined>(undefined);

  useEffect(() => {
    if (initialLine) {
      setName(initialLine.name);
      setSequence(initialLine.sequence);
    } else {
      setName('');
      setSequence(undefined);
    }
  }, [initialLine, isOpen]);

  if (!isOpen) return null;

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim()) return;
    onSave(name.trim(), sequence);
    onClose();
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm p-4">
      <div className="bg-slate-900 border border-slate-700 rounded-lg shadow-xl max-w-sm w-full p-5 relative animate-in fade-in duration-150">
        <button
          onClick={onClose}
          className="absolute top-3 right-3 text-slate-400 hover:text-white"
        >
          <X className="w-4 h-4" />
        </button>

        <div className="flex items-center space-x-2 text-white mb-4">
          <Layers className="w-5 h-5 text-blue-400" />
          <h3 className="text-sm font-bold">
            {initialLine ? 'Edit Production Line' : 'Add New Production Line'}
          </h3>
        </div>

        <form onSubmit={handleSubmit} className="space-y-3">
          <div>
            <label className="block text-xs font-medium text-slate-300 mb-1">
              Line Name <span className="text-rose-400">*</span>
            </label>
            <input
              type="text"
              required
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="e.g. Line 5"
              className="w-full bg-slate-800 border border-slate-700 rounded px-2.5 py-1.5 text-xs text-white placeholder-slate-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
            />
          </div>

          <div>
            <label className="block text-xs font-medium text-slate-300 mb-1">
              Sequence / Order
            </label>
            <input
              type="number"
              value={sequence ?? ''}
              onChange={(e) => setSequence(e.target.value ? parseInt(e.target.value, 10) : undefined)}
              placeholder="Auto sequence if blank"
              className="w-full bg-slate-800 border border-slate-700 rounded px-2.5 py-1.5 text-xs text-white placeholder-slate-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
            />
          </div>

          <div className="mt-5 flex justify-end space-x-2 pt-2">
            <button
              type="button"
              onClick={onClose}
              className="px-3 py-1.5 bg-slate-800 hover:bg-slate-700 text-slate-300 rounded text-xs font-medium transition-colors"
            >
              Cancel
            </button>
            <button
              type="submit"
              className="px-4 py-1.5 bg-blue-600 hover:bg-blue-500 text-white rounded text-xs font-semibold shadow transition-colors"
            >
              {initialLine ? 'Save Changes' : 'Create Line'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
