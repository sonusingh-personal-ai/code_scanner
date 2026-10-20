import axios from 'axios';
import type {
  DashboardInitDto,
  ShiftStatsDto,
  LineDto,
  TestResultDto,
  SaveResponseRequestDto
} from '../types';

export const API_BASE = 'http://localhost:5000/api';

const api = axios.create({
  baseURL: API_BASE,
  headers: {
    'Content-Type': 'application/json'
  }
});

export const dashboardApi = {
  getInitData: async (): Promise<DashboardInitDto> => {
    const res = await api.get<DashboardInitDto>('/dashboard/init');
    return res.data;
  },
  getShiftStats: async (lineId?: number): Promise<ShiftStatsDto> => {
    const res = await api.get<ShiftStatsDto>('/dashboard/shift-stats', {
      params: { lineId }
    });
    return res.data;
  }
};

export const testingApi = {
  processTelemetry: async (request: SaveResponseRequestDto): Promise<TestResultDto> => {
    const res = await api.post<TestResultDto>('/testing/process', request);
    return res.data;
  },
  simulateCycle: async (request: SaveResponseRequestDto): Promise<TestResultDto> => {
    const res = await api.post<TestResultDto>('/testing/simulate', request);
    return res.data;
  },
  getPorts: async (): Promise<string[]> => {
    const res = await api.get<string[]>('/testing/ports');
    return res.data;
  },
  connectPort: async (portName: string, baudRate: number): Promise<{ success: boolean; port: string; baudRate: number }> => {
    const res = await api.post('/testing/connect-port', { portName, baudRate });
    return res.data;
  },
  disconnectPort: async (): Promise<{ success: boolean }> => {
    const res = await api.post('/testing/disconnect-port');
    return res.data;
  }
};

export const linesApi = {
  getAll: async (): Promise<LineDto[]> => {
    const res = await api.get<LineDto[]>('/lines');
    return res.data;
  },
  create: async (name: string, sequence?: number): Promise<LineDto> => {
    const res = await api.post<LineDto>('/lines', { name, sequence });
    return res.data;
  },
  update: async (id: number, name: string, sequence?: number): Promise<boolean> => {
    const res = await api.put(`/lines/${id}`, { name, sequence });
    return res.data.success;
  },
  move: async (id: number, direction: 'up' | 'down'): Promise<boolean> => {
    const res = await api.post(`/lines/${id}/move`, { direction });
    return res.data.success;
  },
  delete: async (id: number): Promise<boolean> => {
    const res = await api.delete(`/lines/${id}`);
    return res.data.success;
  }
};

export default api;
