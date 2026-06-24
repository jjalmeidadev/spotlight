import React from 'react';
import {
  ActivityIndicator,
  Image,
  Pressable,
  ScrollView,
  StatusBar,
  StyleSheet,
  Text,
  View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';

import {
  DashboardMetric,
  DashboardMetricIcon,
  SalesDataPoint,
  SalesPeriod,
  SalesPeriodOption,
} from '@/frontend/models/AdminDashboard';
import { spacing, typography } from '@/frontend/theme';
import { useAdminDashboardViewModel } from '@/frontend/viewmodels/useAdminDashboardViewModel';

const adminAvatar = require('../../../documentos/img/celso.png');

type AdminDashboardScreenProps = {
  profileName?: string;
};

export default function AdminDashboardScreen({ profileName }: AdminDashboardScreenProps) {
  const viewModel = useAdminDashboardViewModel(profileName ?? 'Administrador');

  if (viewModel.loading) {
    return (
      <SafeAreaView style={styles.safeArea}>
        <StatusBar barStyle="dark-content" backgroundColor={dashboardColors.background} />
        <View style={styles.centeredState}>
          <ActivityIndicator color={dashboardColors.ink} />
          <Text style={styles.stateText}>Carregando dashboard...</Text>
        </View>
      </SafeAreaView>
    );
  }

  if (viewModel.error) {
    return (
      <SafeAreaView style={styles.safeArea}>
        <StatusBar barStyle="dark-content" backgroundColor={dashboardColors.background} />
        <View style={styles.centeredState}>
          <Text style={styles.stateText}>{viewModel.error}</Text>
        </View>
      </SafeAreaView>
    );
  }

  return (
    <SafeAreaView style={styles.safeArea}>
      <StatusBar barStyle="dark-content" backgroundColor={dashboardColors.background} />

      <ScrollView
        bounces={false}
        showsVerticalScrollIndicator={false}
        contentContainerStyle={styles.scrollContent}
      >
        <View style={styles.welcomeRow}>
          <Text style={styles.welcomeText}>Bem vindo, {viewModel.adminName}!</Text>
          <Image source={adminAvatar} resizeMode="cover" style={styles.avatar} />
        </View>

        <Text style={styles.title}>Estatísticas</Text>

        <View style={styles.metricsGrid}>
          {viewModel.metrics.map((metric) => (
            <MetricCard key={metric.id} metric={metric} />
          ))}
        </View>

        <View style={styles.panel}>
          <Text style={styles.sectionTitle}>Gêneros no catálogo</Text>
          <DonutChart />

          <View style={styles.legendRow}>
            {viewModel.exhibitionTypes.map((type) => (
              <View key={type.id} style={styles.legendItem}>
                <View style={[styles.legendDot, { backgroundColor: type.color }]} />
                <Text style={styles.legendText}>{type.label}</Text>
              </View>
            ))}
          </View>
        </View>

        <View style={styles.salesPanel}>
          <Text style={styles.sectionTitle}>Sessões por período</Text>

          <PeriodTabs
            options={viewModel.salesPeriodOptions}
            selectedPeriod={viewModel.selectedSalesPeriod}
            onSelectPeriod={viewModel.selectSalesPeriod}
          />

          <BarChart data={viewModel.salesData} maxValue={viewModel.salesMaxValue} />
        </View>
      </ScrollView>
    </SafeAreaView>
  );
}

type MetricCardProps = {
  metric: DashboardMetric;
};

function MetricCard({ metric }: MetricCardProps) {
  return (
    <View style={styles.metricCard}>
      <View style={styles.metricHeader}>
        <MetricIcon icon={metric.icon} />
        <Text style={styles.metricLabel}>{metric.label}</Text>
      </View>
      <Text style={styles.metricValue}>{metric.value}</Text>
    </View>
  );
}

type MetricIconProps = {
  icon: DashboardMetricIcon;
};

function MetricIcon({ icon }: MetricIconProps) {
  if (icon === 'stopwatch') {
    return (
      <View style={styles.stopwatchIcon}>
        <View style={styles.stopwatchKnob} />
        <View style={styles.stopwatchTop} />
        <View style={styles.stopwatchHandLong} />
        <View style={styles.stopwatchHandShort} />
      </View>
    );
  }

  if (icon === 'ticket') {
    return (
      <View style={styles.ticketIcon}>
        <View style={styles.ticketTopLine} />
        <View style={styles.ticketBottomLine} />
      </View>
    );
  }

  if (icon === 'portal') {
    return (
      <View style={styles.portalIcon}>
        <View style={styles.portalFrame}>
          <View style={styles.portalHeader} />
          <View style={styles.portalEye}>
            <View style={styles.portalPupil} />
          </View>
        </View>
      </View>
    );
  }

  return (
    <View style={styles.purchaseClickIcon}>
      <View style={styles.purchaseBag}>
        <View style={styles.purchaseBagHandle} />
      </View>
      <View style={styles.clickDot} />
      <View style={styles.clickRayTop} />
      <View style={styles.clickRaySide} />
    </View>
  );
}

function DonutChart() {
  return (
    <View style={styles.donutOuter}>
      <View style={styles.donutRing} />
      <View style={styles.donutBlueArc} />
      <View style={styles.donutHole} />
    </View>
  );
}

type PeriodTabsProps = {
  options: SalesPeriodOption[];
  selectedPeriod: SalesPeriod;
  onSelectPeriod: (period: SalesPeriod) => void;
};

function PeriodTabs({ options, selectedPeriod, onSelectPeriod }: PeriodTabsProps) {
  return (
    <View style={styles.periodTabs}>
      {options.map((option) => {
        const active = option.id === selectedPeriod;

        return (
          <Pressable
            accessibilityRole="button"
            key={option.id}
            onPress={() => onSelectPeriod(option.id)}
            style={({ pressed }) => [
              styles.periodTab,
              active && styles.periodTabActive,
              pressed && styles.pressed,
            ]}
          >
            <Text style={[styles.periodTabLabel, active && styles.periodTabLabelActive]}>
              {option.label}
            </Text>
          </Pressable>
        );
      })}
    </View>
  );
}

type BarChartProps = {
  data: SalesDataPoint[];
  maxValue: number;
};

function BarChart({ data, maxValue }: BarChartProps) {
  const yAxisLabels = ['25k', '15k', '10k', '5k', '1k', '0'];
  const barTrackHeight = 210;

  return (
    <View style={styles.chartContainer}>
      <View style={styles.yAxis}>
        {yAxisLabels.map((label) => (
          <Text key={label} style={styles.axisLabel}>
            {label}
          </Text>
        ))}
      </View>

      <View style={styles.barsArea}>
        {data.map((item) => {
          const fillHeight = Math.max((item.value / maxValue) * barTrackHeight, 8);

          return (
            <View key={item.id} style={styles.barGroup}>
              <View style={styles.barTrack}>
                <View style={[styles.barFill, { height: fillHeight }]} />
              </View>
              <Text style={styles.barLabel} numberOfLines={1}>
                {item.label}
              </Text>
            </View>
          );
        })}
      </View>
    </View>
  );
}

const dashboardColors = {
  background: '#f0f2f1',
  panel: '#ffffff',
  ink: '#050505',
  muted: '#a9aebb',
  tab: '#8e8e8e',
  blue: '#2d6df6',
  blueDeep: '#526cf0',
  blueSoft: '#e8ecfb',
  red: '#ff4627',
};

const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
    backgroundColor: dashboardColors.background,
  },
  centeredState: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
    paddingHorizontal: spacing.xl,
    rowGap: spacing.md,
  },
  stateText: {
    color: dashboardColors.ink,
    ...typography.body,
    textAlign: 'center',
  },
  scrollContent: {
    flexGrow: 1,
    paddingBottom: spacing.xxxl,
  },
  welcomeRow: {
    minHeight: 48,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'flex-end',
    columnGap: spacing.xs,
    paddingHorizontal: spacing.xl,
    backgroundColor: '#eef4ef',
  },
  welcomeText: {
    color: dashboardColors.ink,
    ...typography.caption,
    fontFamily: typography.fontFamily.bold,
    fontSize: 11,
    letterSpacing: 0,
  },
  avatar: {
    width: 28,
    height: 28,
    borderRadius: 14,
  },
  title: {
    color: dashboardColors.ink,
    ...typography.title,
    fontSize: 42,
    lineHeight: 48,
    letterSpacing: 0,
    marginTop: 56,
    marginBottom: spacing.xxl,
    paddingHorizontal: spacing.xl,
  },
  metricsGrid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: spacing.md,
    paddingHorizontal: spacing.md,
    marginBottom: spacing.md,
  },
  metricCard: {
    width: '47.6%',
    minHeight: 112,
    justifyContent: 'space-between',
    borderRadius: 16,
    backgroundColor: dashboardColors.panel,
    paddingHorizontal: spacing.md,
    paddingVertical: spacing.md,
  },
  metricHeader: {
    minHeight: 38,
    flexDirection: 'row',
    alignItems: 'center',
    columnGap: spacing.xs,
  },
  metricLabel: {
    color: dashboardColors.ink,
    ...typography.label,
    flex: 1,
    fontSize: 18,
    lineHeight: 22,
    letterSpacing: 0,
  },
  metricValue: {
    color: dashboardColors.ink,
    fontFamily: typography.fontFamily.book,
    fontSize: 30,
    lineHeight: 34,
    letterSpacing: 0,
    textAlign: 'center',
  },
  panel: {
    backgroundColor: dashboardColors.panel,
    borderRadius: 18,
    paddingTop: 76,
    paddingHorizontal: spacing.xl,
    paddingBottom: spacing.xxxl,
    marginBottom: spacing.xs,
  },
  salesPanel: {
    backgroundColor: dashboardColors.panel,
    borderRadius: 18,
    paddingTop: spacing.xl,
    paddingHorizontal: spacing.xl,
    paddingBottom: spacing.xl,
  },
  sectionTitle: {
    color: dashboardColors.ink,
    ...typography.title,
    fontSize: 24,
    lineHeight: 30,
    letterSpacing: 0,
    marginBottom: spacing.lg,
  },
  donutOuter: {
    alignSelf: 'center',
    width: 176,
    height: 176,
    borderRadius: 88,
    alignItems: 'center',
    justifyContent: 'center',
    backgroundColor: dashboardColors.panel,
    marginTop: spacing.xs,
  },
  donutRing: {
    position: 'absolute',
    width: 136,
    height: 136,
    borderRadius: 68,
    borderWidth: 22,
    borderColor: dashboardColors.red,
  },
  donutBlueArc: {
    position: 'absolute',
    width: 136,
    height: 136,
    borderRadius: 68,
    borderWidth: 22,
    borderColor: 'transparent',
    borderTopColor: dashboardColors.blueDeep,
    borderRightColor: dashboardColors.blueDeep,
    transform: [{ rotate: '8deg' }],
  },
  donutHole: {
    width: 76,
    height: 76,
    borderRadius: 38,
    backgroundColor: dashboardColors.panel,
  },
  legendRow: {
    flexDirection: 'row',
    justifyContent: 'center',
    columnGap: spacing.xxxl,
    marginTop: spacing.lg,
  },
  legendItem: {
    flexDirection: 'row',
    alignItems: 'center',
    columnGap: spacing.sm,
  },
  legendDot: {
    width: 10,
    height: 10,
    borderRadius: 5,
  },
  legendText: {
    color: dashboardColors.ink,
    ...typography.label,
    fontSize: 16,
    lineHeight: 20,
    letterSpacing: 0,
  },
  periodTabs: {
    minHeight: 44,
    flexDirection: 'row',
    alignItems: 'center',
    borderRadius: 5,
    backgroundColor: dashboardColors.tab,
    padding: 3,
    marginBottom: spacing.xl,
  },
  periodTab: {
    flex: 1,
    minHeight: 38,
    alignItems: 'center',
    justifyContent: 'center',
    borderRadius: 5,
  },
  periodTabActive: {
    backgroundColor: dashboardColors.panel,
  },
  periodTabLabel: {
    color: dashboardColors.ink,
    fontFamily: typography.fontFamily.book,
    fontSize: 16,
    lineHeight: 20,
    letterSpacing: 0,
  },
  periodTabLabelActive: {
    fontFamily: typography.fontFamily.bold,
  },
  chartContainer: {
    minHeight: 260,
    flexDirection: 'row',
  },
  yAxis: {
    width: 38,
    height: 220,
    justifyContent: 'space-between',
    paddingTop: spacing.xs,
    paddingBottom: 20,
  },
  axisLabel: {
    color: dashboardColors.muted,
    ...typography.caption,
    fontSize: 13,
    lineHeight: 16,
    letterSpacing: 0,
  },
  barsArea: {
    flex: 1,
    height: 244,
    flexDirection: 'row',
    alignItems: 'flex-start',
    justifyContent: 'space-between',
    columnGap: 6,
  },
  barGroup: {
    flex: 1,
    minWidth: 16,
    height: 244,
    alignItems: 'center',
  },
  barTrack: {
    width: 16,
    height: 210,
    borderRadius: 999,
    justifyContent: 'flex-end',
    overflow: 'hidden',
    backgroundColor: dashboardColors.blueSoft,
  },
  barFill: {
    width: '100%',
    borderRadius: 999,
    backgroundColor: dashboardColors.blue,
  },
  barLabel: {
    color: '#c3c7d2',
    ...typography.caption,
    width: 30,
    fontSize: 12,
    lineHeight: 16,
    letterSpacing: 0,
    textAlign: 'center',
    marginTop: spacing.xs,
  },
  purchaseClickIcon: {
    width: 34,
    height: 34,
  },
  purchaseBag: {
    position: 'absolute',
    left: 2,
    top: 10,
    width: 23,
    height: 20,
    borderRadius: 4,
    borderWidth: 2,
    borderColor: dashboardColors.ink,
  },
  purchaseBagHandle: {
    position: 'absolute',
    left: 5,
    top: -8,
    width: 10,
    height: 9,
    borderTopLeftRadius: 8,
    borderTopRightRadius: 8,
    borderWidth: 2,
    borderBottomWidth: 0,
    borderColor: dashboardColors.ink,
  },
  clickDot: {
    position: 'absolute',
    right: 3,
    bottom: 4,
    width: 9,
    height: 9,
    borderRadius: 5,
    backgroundColor: dashboardColors.ink,
  },
  clickRayTop: {
    position: 'absolute',
    right: 7,
    top: 2,
    width: 2,
    height: 8,
    borderRadius: 999,
    backgroundColor: dashboardColors.ink,
  },
  clickRaySide: {
    position: 'absolute',
    right: 0,
    top: 9,
    width: 8,
    height: 2,
    borderRadius: 999,
    backgroundColor: dashboardColors.ink,
  },
  stopwatchIcon: {
    width: 34,
    height: 34,
    borderRadius: 17,
    borderWidth: 2,
    borderColor: dashboardColors.ink,
  },
  stopwatchKnob: {
    position: 'absolute',
    top: -7,
    left: 12,
    width: 10,
    height: 6,
    borderRadius: 2,
    borderWidth: 2,
    borderColor: dashboardColors.ink,
  },
  stopwatchTop: {
    position: 'absolute',
    right: -3,
    top: 1,
    width: 8,
    height: 2,
    backgroundColor: dashboardColors.ink,
    transform: [{ rotate: '36deg' }],
  },
  stopwatchHandLong: {
    position: 'absolute',
    left: 16,
    top: 9,
    width: 2,
    height: 10,
    backgroundColor: dashboardColors.ink,
    transform: [{ rotate: '42deg' }],
  },
  stopwatchHandShort: {
    position: 'absolute',
    left: 12,
    top: 16,
    width: 8,
    height: 2,
    backgroundColor: dashboardColors.ink,
  },
  ticketIcon: {
    width: 34,
    height: 24,
    borderRadius: 4,
    borderWidth: 2,
    borderColor: dashboardColors.ink,
    transform: [{ rotate: '3deg' }],
  },
  ticketTopLine: {
    position: 'absolute',
    top: 7,
    left: 4,
    right: 4,
    height: 2,
    backgroundColor: dashboardColors.ink,
  },
  ticketBottomLine: {
    position: 'absolute',
    bottom: 6,
    left: 4,
    right: 4,
    height: 2,
    backgroundColor: dashboardColors.ink,
  },
  portalIcon: {
    width: 36,
    height: 34,
    justifyContent: 'center',
  },
  portalFrame: {
    width: 32,
    height: 25,
    borderRadius: 5,
    borderWidth: 2,
    borderColor: dashboardColors.ink,
    overflow: 'hidden',
  },
  portalHeader: {
    height: 7,
    borderBottomWidth: 2,
    borderColor: dashboardColors.ink,
  },
  portalEye: {
    alignSelf: 'center',
    width: 16,
    height: 9,
    borderRadius: 9,
    borderWidth: 2,
    borderColor: dashboardColors.ink,
    alignItems: 'center',
    justifyContent: 'center',
    marginTop: 4,
  },
  portalPupil: {
    width: 4,
    height: 4,
    borderRadius: 2,
    backgroundColor: dashboardColors.ink,
  },
  pressed: {
    opacity: 0.82,
  },
});
