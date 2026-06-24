import React from 'react';
import {
  ActivityIndicator,
  Pressable,
  ScrollView,
  StatusBar,
  StyleSheet,
  Text,
  TextInput,
  View,
} from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';

import {
  DashboardMetric,
  DashboardMetricIcon,
  ExhibitionTypeShare,
  SalesDataPoint,
  SalesPeriod,
  SalesPeriodOption,
} from '@/frontend/models/AdminDashboard';
import { promoverUsuarioAdministrador } from '@/frontend/repositories/UserRepository';
import { spacing, typography } from '@/frontend/theme';
import { useAdminDashboardViewModel } from '@/frontend/viewmodels/useAdminDashboardViewModel';
import { ProfileSheet } from '@/frontend/components/ProfileSheet';

type AdminDashboardScreenProps = {
  accessToken?: string | null;
  profileEmail?: string;
  profileName?: string;
  onOpenCatalog?: () => void;
  onLogout?: () => void;
  onProfileUpdated?: (name: string) => void;
};

export default function AdminDashboardScreen({
  accessToken,
  profileEmail,
  profileName,
  onOpenCatalog,
  onLogout,
  onProfileUpdated,
}: AdminDashboardScreenProps) {
  const [profileOpen, setProfileOpen] = React.useState(false);
  const [adminEmail, setAdminEmail] = React.useState('');
  const [promoteLoading, setPromoteLoading] = React.useState(false);
  const [promoteMessage, setPromoteMessage] = React.useState<string | null>(null);
  const viewModel = useAdminDashboardViewModel(profileName ?? 'Administrador');

  async function handlePromoteAdmin() {
    try {
      setPromoteLoading(true);
      setPromoteMessage(null);
      const usuario = await promoverUsuarioAdministrador(accessToken, adminEmail);
      setAdminEmail('');
      setPromoteMessage(`${usuario.nome} agora é administrador. Oriente a pessoa a sair e entrar novamente.`);
    } catch (err) {
      setPromoteMessage(err instanceof Error ? err.message : 'Não foi possível promover o usuário.');
    } finally {
      setPromoteLoading(false);
    }
  }

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
          <Pressable
            accessibilityRole="button"
            onPress={onOpenCatalog}
            style={({ pressed }) => [styles.catalogButton, pressed && styles.pressed]}
          >
            <Text style={styles.catalogButtonText}>Ver app</Text>
          </Pressable>
          <Text style={styles.welcomeText}>Bem vindo, {viewModel.adminName}!</Text>
          <Pressable accessibilityRole="button" hitSlop={8} onPress={() => setProfileOpen(true)} style={styles.avatarFallback}>
            <Text style={styles.avatarInitial}>{getInitial(viewModel.adminName)}</Text>
          </Pressable>
        </View>

        <Text style={styles.title}>Estatísticas</Text>

        <View style={styles.metricsGrid}>
          {viewModel.metrics.map((metric) => (
            <MetricCard key={metric.id} metric={metric} />
          ))}
        </View>

        <View style={styles.panel}>
          <Text style={styles.sectionTitle}>Gêneros no catálogo</Text>
          <GenreChart data={viewModel.exhibitionTypes} total={viewModel.exhibitionTotal} />
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

        <View style={styles.adminPanel}>
          <Text style={styles.sectionTitle}>Administradores</Text>
          <Text style={styles.adminDescription}>
            Promova usuários cadastrados para liberar acesso ao dashboard e às rotas administrativas.
          </Text>
          <TextInput
            autoCapitalize="none"
            keyboardType="email-address"
            onChangeText={setAdminEmail}
            placeholder="email@dominio.com"
            placeholderTextColor="#9a9a9a"
            style={styles.adminInput}
            value={adminEmail}
          />
          {promoteMessage ? <Text style={styles.adminMessage}>{promoteMessage}</Text> : null}
          <Pressable
            accessibilityRole="button"
            disabled={promoteLoading || !adminEmail.trim()}
            onPress={handlePromoteAdmin}
            style={({ pressed }) => [
              styles.promoteButton,
              pressed && styles.pressed,
              (promoteLoading || !adminEmail.trim()) && styles.disabled,
            ]}
          >
            {promoteLoading ? <ActivityIndicator color="#ffffff" /> : <Text style={styles.promoteButtonText}>Promover admin</Text>}
          </Pressable>
        </View>
      </ScrollView>

      <ProfileSheet
        accessToken={accessToken}
        email={profileEmail}
        name={viewModel.adminName}
        onClose={() => setProfileOpen(false)}
        onLogout={onLogout ?? (() => undefined)}
        onProfileUpdated={onProfileUpdated}
        visible={profileOpen}
      />
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

type GenreChartProps = {
  data: ExhibitionTypeShare[];
  total: number;
};

function GenreChart({ data, total }: GenreChartProps) {
  if (data.length === 0 || total === 0) {
    return <Text style={styles.emptyChartText}>Nenhum gênero encontrado no catálogo.</Text>;
  }

  return (
    <View style={styles.genreChart}>
      {data.map((type) => {
        const percentage = Math.round((type.value / total) * 100);

        return (
          <View key={type.id} style={styles.genreRow}>
            <View style={styles.genreHeader}>
              <View style={[styles.legendDot, { backgroundColor: type.color }]} />
              <Text style={styles.legendText} numberOfLines={1}>
                {type.label}
              </Text>
              <Text style={styles.genreValue}>{percentage}%</Text>
            </View>
            <View style={styles.genreTrack}>
              <View
                style={[
                  styles.genreFill,
                  { width: `${Math.max(percentage, 4)}%`, backgroundColor: type.color },
                ]}
              />
            </View>
          </View>
        );
      })}
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

function getInitial(value: string) {
  return value.trim().charAt(0).toUpperCase() || 'A';
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
  catalogButton: {
    alignItems: 'center',
    borderColor: dashboardColors.ink,
    borderRadius: 999,
    borderWidth: 1,
    justifyContent: 'center',
    marginRight: 'auto',
    minHeight: 28,
    paddingHorizontal: spacing.md,
  },
  catalogButtonText: {
    color: dashboardColors.ink,
    fontFamily: typography.fontFamily.bold,
    fontSize: 11,
    lineHeight: 14,
    letterSpacing: 0,
  },
  welcomeText: {
    color: dashboardColors.ink,
    ...typography.caption,
    fontFamily: typography.fontFamily.bold,
    fontSize: 11,
    letterSpacing: 0,
  },
  avatarFallback: {
    width: 28,
    height: 28,
    borderRadius: 14,
    alignItems: 'center',
    justifyContent: 'center',
    backgroundColor: dashboardColors.ink,
  },
  avatarInitial: {
    color: dashboardColors.panel,
    fontFamily: typography.fontFamily.bold,
    fontSize: 12,
    lineHeight: 14,
    letterSpacing: 0,
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
    paddingTop: spacing.xl,
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
  adminPanel: {
    backgroundColor: dashboardColors.panel,
    borderRadius: 18,
    marginTop: spacing.xs,
    paddingHorizontal: spacing.xl,
    paddingVertical: spacing.xl,
  },
  adminDescription: {
    color: dashboardColors.tab,
    ...typography.body,
    fontSize: 13,
    lineHeight: 18,
    letterSpacing: 0,
    marginBottom: spacing.md,
  },
  adminInput: {
    borderColor: dashboardColors.blueSoft,
    borderRadius: 8,
    borderWidth: 1,
    color: dashboardColors.ink,
    fontFamily: typography.fontFamily.book,
    fontSize: 14,
    minHeight: 44,
    paddingHorizontal: spacing.md,
  },
  adminMessage: {
    color: dashboardColors.ink,
    ...typography.caption,
    fontSize: 12,
    lineHeight: 16,
    letterSpacing: 0,
    marginTop: spacing.sm,
  },
  promoteButton: {
    alignItems: 'center',
    backgroundColor: dashboardColors.ink,
    borderRadius: 8,
    justifyContent: 'center',
    marginTop: spacing.md,
    minHeight: 44,
  },
  promoteButtonText: {
    color: dashboardColors.panel,
    fontFamily: typography.fontFamily.bold,
    fontSize: 13,
    letterSpacing: 0,
  },
  sectionTitle: {
    color: dashboardColors.ink,
    ...typography.title,
    fontSize: 24,
    lineHeight: 30,
    letterSpacing: 0,
    marginBottom: spacing.lg,
  },
  genreChart: {
    rowGap: spacing.md,
  },
  genreRow: {
    rowGap: spacing.xs,
  },
  genreHeader: {
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
    flex: 1,
    fontSize: 16,
    lineHeight: 20,
    letterSpacing: 0,
  },
  genreValue: {
    color: dashboardColors.muted,
    ...typography.caption,
    fontSize: 13,
    lineHeight: 16,
    letterSpacing: 0,
    minWidth: 38,
    textAlign: 'right',
  },
  genreTrack: {
    height: 8,
    borderRadius: 999,
    overflow: 'hidden',
    backgroundColor: dashboardColors.blueSoft,
  },
  genreFill: {
    height: '100%',
    borderRadius: 999,
  },
  emptyChartText: {
    color: dashboardColors.muted,
    ...typography.body,
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
  disabled: {
    opacity: 0.58,
  },
});
