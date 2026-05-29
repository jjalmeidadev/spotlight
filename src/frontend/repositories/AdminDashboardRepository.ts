import { AdminDashboardData } from '@/frontend/models/AdminDashboard';

const adminDashboardData: AdminDashboardData = {
  adminName: 'Celso',
  metrics: [
    {
      id: 'visits',
      label: 'Visitas no Portal',
      value: '2300',
      icon: 'portal',
    },
    {
      id: 'tickets-sold',
      label: 'Ingressos\nVendidos',
      value: '300/500',
      icon: 'ticket',
    },
    {
      id: 'conversions',
      label: 'Clicks em comprar',
      value: '234',
      icon: 'purchase-click',
    },
    {
      id: 'avg-stay',
      label: 'Média de\nPermanência',
      value: '23 segs',
      icon: 'stopwatch',
    },
  ],
  exhibitionTypes: [
    {
      id: 'external',
      label: 'Externa',
      value: 4100,
      color: '#526cf0',
    },
    {
      id: 'aventura',
      label: 'Aventura',
      value: 1900,
      color: '#ff4627',
    },
  ],
  salesByPeriod: {
    week: [
      { id: 'seg', label: 'Seg', value: 4200 },
      { id: 'ter', label: 'Ter', value: 8300 },
      { id: 'qua', label: 'Qua', value: 6200 },
      { id: 'qui', label: 'Qui', value: 10400 },
      { id: 'sex', label: 'Sex', value: 14700 },
      { id: 'sab', label: 'Sab', value: 18800 },
      { id: 'dom', label: 'Dom', value: 15600 },
    ],
    month: [
      { id: 'jan', label: 'Jan', value: 3800 },
      { id: 'fev', label: 'Fev', value: 12200 },
      { id: 'mar', label: 'Mar', value: 17800 },
      { id: 'abr', label: 'Abr', value: 6400 },
      { id: 'mai', label: 'Mai', value: 10100 },
      { id: 'jun', label: 'Jun', value: 3800 },
      { id: 'ago', label: 'Ago', value: 10700 },
      { id: 'set', label: 'Set', value: 1200 },
      { id: 'out', label: 'Out', value: 15000 },
      { id: 'nov', label: 'Nov', value: 5900 },
      { id: 'dez', label: 'Dez', value: 15000 },
    ],
    year: [
      { id: '2022', label: '22', value: 98000 },
      { id: '2023', label: '23', value: 126000 },
      { id: '2024', label: '24', value: 149000 },
      { id: '2025', label: '25', value: 174000 },
      { id: '2026', label: '26', value: 162000 },
    ],
  },
};

export function getAdminDashboardData(): AdminDashboardData {
  return adminDashboardData;
}
