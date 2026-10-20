import React from 'react';
import { AlertCircle, CheckCircle2, Info, X } from 'lucide-react';

interface AlertModalProps {
  isOpen: boolean;
  title: string;
  message: string;
  type?: 'info' | 'success' | 'warning' | 'error';
  onClose: () => void;
}

export const AlertModal: React.FC<AlertModalProps> = ({
  isOpen,
  title,
  message,
  type = 'info',
  onClose
}) => {
  if (!isOpen) return null;

  const getIcon = () => {
    switch (type) {
      case 'success':
        return <CheckCircle2 className="w-6 h-6 text-emerald-400" />;
      case 'warning':
      case 'error':
        return <AlertCircle className="w-6 h-6 text-rose-400" />;
      default:
        return <Info className="w-6 h-6 text-blue-400" />;
    }
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

        <div className="flex items-start space-x-3">
          <div className="shrink-0">{getIcon()}</div>
          <div>
            <h3 className="text-sm font-bold text-white mb-1">{title}</h3>
            <p className="text-xs text-slate-300 leading-relaxed whitespace-pre-line">{message}</p>
          </div>
        </div>

        <div className="mt-5 flex justify-end">
          <button
            onClick={onClose}
            className="px-4 py-1.5 bg-blue-600 hover:bg-blue-500 text-white rounded text-xs font-semibold shadow transition-colors"
          >
            Acknowledge
          </button>
        </div>
      </div>
    </div>
  );
};
