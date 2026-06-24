import { AdminDashboardData } from '@/frontend/models/AdminDashboard';
import {
  CatalogShowDetail,
  listarCatalogoCompleto,
  obterDetalheEspetaculoCatalogo,
} from '@/frontend/repositories/CatalogRepository';

const COLORS = ['#526cf0', '#ff4627', '#17211D', '#D7DE65', '#8e8e8e', '#1f9d7a', '#a45ee5', '#f29f05'];
const WEEK_LABELS = ['Dom', 'Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sab'];
const MONTH_LABELS = ['Jan', 'Fev', 'Mar', 'Abr', 'Mai', 'Jun', 'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez'];

export async function getAdminDashboardData(adminName: string): Promise<AdminDashboardData> {
  const shows = await listarCatalogoCompleto(50);
  const details = await Promise.allSettled(
    shows.map((show) => obterDetalheEspetaculoCatalogo(show.id)),
  );
  const detailedShows = details.flatMap((result) =>
    result.status === 'fulfilled' ? [result.value] : [],
  );
  const sessions = detailedShows.flatMap((show) => show.sessions);
  const activeShows = shows.filter((show) => show.status === 'em_cartaz');
  const averageRating = shows.length === 0
    ? 0
    : shows.reduce((total, show) => total + show.averageRating, 0) / shows.length;

  return {
    adminName,
    metrics: [
      {
        id: 'shows',
        label: 'Espetáculos',
        value: String(shows.length),
        icon: 'portal',
      },
      {
        id: 'sessions',
        label: 'Sessões\nDisponíveis',
        value: String(sessions.length),
        icon: 'ticket',
      },
      {
        id: 'active-shows',
        label: 'Em cartaz',
        value: String(activeShows.length),
        icon: 'purchase-click',
      },
      {
        id: 'avg-rating',
        label: 'Nota\nMédia',
        value: averageRating.toFixed(1),
        icon: 'stopwatch',
      },
    ],
    exhibitionTypes: buildGenreShares(shows),
    salesByPeriod: {
      week: buildWeekSeries(sessions),
      month: buildMonthSeries(sessions),
      year: buildYearSeries(sessions),
    },
  };
}

function buildGenreShares(shows: Array<{ genre?: string | null }>) {
  const counts = new Map<string, number>();

  shows.forEach((show) => {
    const genre = show.genre?.trim() || 'Sem gênero';
    counts.set(genre, (counts.get(genre) ?? 0) + 1);
  });

  return Array.from(counts.entries())
    .sort((a, b) => b[1] - a[1])
    .map(([label, value], index) => ({
      id: label.toLowerCase().replace(/\s+/g, '-'),
      label,
      value,
      color: COLORS[index % COLORS.length],
    }));
}

function buildWeekSeries(sessions: CatalogShowDetail['sessions']) {
  const counts = Array.from({ length: 7 }, () => 0);

  sessions.forEach((session) => {
    const date = parseDate(session.date);
    if (date) {
      counts[date.getUTCDay()] += 1;
    }
  });

  return WEEK_LABELS.map((label, index) => ({
    id: label.toLowerCase(),
    label,
    value: counts[index],
  }));
}

function buildMonthSeries(sessions: CatalogShowDetail['sessions']) {
  const counts = Array.from({ length: 12 }, () => 0);

  sessions.forEach((session) => {
    const date = parseDate(session.date);
    if (date) {
      counts[date.getUTCMonth()] += 1;
    }
  });

  return MONTH_LABELS.map((label, index) => ({
    id: label.toLowerCase(),
    label,
    value: counts[index],
  }));
}

function buildYearSeries(sessions: CatalogShowDetail['sessions']) {
  const counts = new Map<number, number>();
  const currentYear = new Date().getUTCFullYear();

  sessions.forEach((session) => {
    const date = parseDate(session.date);
    if (date) {
      const year = date.getUTCFullYear();
      counts.set(year, (counts.get(year) ?? 0) + 1);
    }
  });

  const years = Array.from(counts.keys()).sort((a, b) => a - b);
  const displayYears = years.length > 0 ? years : [currentYear];

  return displayYears.map((year) => ({
    id: String(year),
    label: String(year).slice(-2),
    value: counts.get(year) ?? 0,
  }));
}

function parseDate(value?: string) {
  if (!value) {
    return null;
  }

  const date = new Date(`${value}T00:00:00Z`);
  return Number.isNaN(date.getTime()) ? null : date;
}
