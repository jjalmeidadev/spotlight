export type DashboardMetricId = 'conversions' | 'avg-stay' | 'tickets-sold' | 'visits';

export type DashboardMetricIcon = 'purchase-click' | 'stopwatch' | 'ticket' | 'portal';

export type DashboardMetric = {
  id: DashboardMetricId;
  label: string;
  value: string;
  icon: DashboardMetricIcon;
};

export type ExhibitionTypeShare = {
  id: string;
  label: string;
  value: number;
  color: string;
};

export type SalesPeriod = 'week' | 'month' | 'year';

export type SalesPeriodOption = {
  id: SalesPeriod;
  label: string;
};

export type SalesDataPoint = {
  id: string;
  label: string;
  value: number;
};

export type AdminDashboardData = {
  adminName: string;
  metrics: DashboardMetric[];
  exhibitionTypes: ExhibitionTypeShare[];
  salesByPeriod: Record<SalesPeriod, SalesDataPoint[]>;
};
