import { useEffect, useMemo, useState } from 'react';

import {
  AdminDashboardData,
  SalesDataPoint,
  SalesPeriod,
  SalesPeriodOption,
} from '@/frontend/models/AdminDashboard';
import { getAdminDashboardData } from '@/frontend/repositories/AdminDashboardRepository';

const salesPeriodOptions: SalesPeriodOption[] = [
  { id: 'week', label: 'Semana' },
  { id: 'month', label: 'Mês' },
  { id: 'year', label: 'Ano' },
];

const emptyDashboard: AdminDashboardData = {
  adminName: '',
  metrics: [],
  exhibitionTypes: [],
  salesByPeriod: {
    week: [],
    month: [],
    year: [],
  },
};

export function useAdminDashboardViewModel(adminName: string) {
  const [dashboard, setDashboard] = useState<AdminDashboardData>(emptyDashboard);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedSalesPeriod, setSelectedSalesPeriod] = useState<SalesPeriod>('month');

  useEffect(() => {
    let active = true;

    async function loadDashboard() {
      try {
        setLoading(true);
        setError(null);
        const data = await getAdminDashboardData(adminName);

        if (active) {
          setDashboard(data);
        }
      } catch (err) {
        if (active) {
          setError(err instanceof Error ? err.message : 'Não foi possível carregar o dashboard.');
        }
      } finally {
        if (active) {
          setLoading(false);
        }
      }
    }

    loadDashboard();

    return () => {
      active = false;
    };
  }, [adminName]);

  const salesData: SalesDataPoint[] = dashboard.salesByPeriod[selectedSalesPeriod];
  const salesMaxValue = Math.max(...salesData.map((item) => item.value), 1);
  const exhibitionTotal = dashboard.exhibitionTypes.reduce((total, item) => total + item.value, 0);

  function selectSalesPeriod(period: SalesPeriod) {
    setSelectedSalesPeriod(period);
  }

  return {
    adminName: dashboard.adminName,
    error,
    metrics: dashboard.metrics,
    loading,
    exhibitionTypes: dashboard.exhibitionTypes,
    exhibitionTotal,
    salesPeriodOptions,
    selectedSalesPeriod,
    salesData,
    salesMaxValue,
    selectSalesPeriod,
  };
}
