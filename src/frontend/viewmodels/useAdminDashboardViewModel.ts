import { useMemo, useState } from 'react';

import {
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

export function useAdminDashboardViewModel() {
  const dashboard = useMemo(() => getAdminDashboardData(), []);
  const [selectedSalesPeriod, setSelectedSalesPeriod] = useState<SalesPeriod>('month');

  const salesData: SalesDataPoint[] = dashboard.salesByPeriod[selectedSalesPeriod];
  const salesMaxValue = Math.max(...salesData.map((item) => item.value), 1);
  const exhibitionTotal = dashboard.exhibitionTypes.reduce((total, item) => total + item.value, 0);

  function selectSalesPeriod(period: SalesPeriod) {
    setSelectedSalesPeriod(period);
  }

  return {
    adminName: dashboard.adminName,
    metrics: dashboard.metrics,
    exhibitionTypes: dashboard.exhibitionTypes,
    exhibitionTotal,
    salesPeriodOptions,
    selectedSalesPeriod,
    salesData,
    salesMaxValue,
    selectSalesPeriod,
  };
}
