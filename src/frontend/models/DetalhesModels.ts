import { obterDetalheEspetaculoCatalogo } from '../repositories/CatalogRepository';
import { listarEspetaculosSimilares } from '../repositories/RecommendationRepository';

export type EspetaculoSemelhante = {
  id: string;
  titulo: string;
  imagemUrl?: string;
  score?: number;
  algoritmo?: string;
};

export type EspetaculoDetalhe = {
  id: string;
  titulo: string;
  descricao: string;
  imagemUrl?: string;
  percentualAprovacao: number;
  notaMedia: number;
  tags: string[];
  classificacaoIndicativa?: string | null;
  duracao?: number | null;
  faixaEtaria?: string | null;
  genero?: string | null;
  preco?: string | null;
  proximaSessao?: string | null;
  produtora?: string | null;
  status?: string;
  teatro?: string | null;
  linkCompra?: string;
};

const MOCK_SEMELHANTES: EspetaculoSemelhante[] = [
  {
    id: '30',
    titulo: 'Mamma Mia!',
  },
];

function wait(ms = 250) {
  return new Promise((resolve) => {
    setTimeout(resolve, ms);
  });
}

export async function obterDetalhesEspetaculo(espetaculoId: string): Promise<EspetaculoDetalhe> {
  const detalhe = await obterDetalheEspetaculoCatalogo(espetaculoId);
  const primeiraSessaoDisponivel = detalhe.sessions.find((session) => session.purchaseLink);
  const primeiraSessao = detalhe.sessions[0];
  const tags = [
    detalhe.genre,
    detalhe.duration ? `${detalhe.duration}min` : null,
  ].filter((tag): tag is string => Boolean(tag));

  return {
    id: detalhe.id,
    titulo: detalhe.title,
    descricao: detalhe.description ?? 'Descricao indisponivel para este espetaculo.',
    imagemUrl: detalhe.imageUrl ?? undefined,
    percentualAprovacao: toApprovalPercent(detalhe.averageRating),
    notaMedia: detalhe.averageRating,
    tags,
    classificacaoIndicativa: detalhe.contentRating,
    duracao: detalhe.duration,
    faixaEtaria: detalhe.ageRange,
    genero: detalhe.genre,
    preco: formatPriceRange(primeiraSessao?.minPrice, primeiraSessao?.maxPrice),
    proximaSessao: primeiraSessao?.date ? formatSession(primeiraSessao.date, primeiraSessao.time) : null,
    produtora: detalhe.producer,
    status: detalhe.status,
    teatro: primeiraSessao?.theater?.name ?? null,
    linkCompra: primeiraSessaoDisponivel?.purchaseLink ?? undefined,
  };
}

export async function obterEspetaculosSemelhantes(
  espetaculoId: string,
): Promise<EspetaculoSemelhante[]> {
  if (!espetaculoId.trim()) {
    return [];
  }

  try {
    const similares = await listarEspetaculosSimilares(espetaculoId, 8);

    if (similares.length > 0) {
      return similares.map((item) => ({
        id: item.id,
        titulo: item.title,
        imagemUrl: item.imageUrl ?? undefined,
        score: item.score,
        algoritmo: item.algorithm,
      }));
    }
  } catch {
    // Mantem a tela navegavel quando o backend local nao estiver ativo.
  }

  await wait();
  return MOCK_SEMELHANTES;
}

function toApprovalPercent(rating: number) {
  return Math.max(0, Math.min(100, Math.round((rating / 5) * 100)));
}

function formatDateTag(value: string) {
  const [year, month, day] = value.split('-');

  if (!year || !month || !day) {
    return value;
  }

  return `${day}/${month}`;
}

function formatSession(date: string, time?: string) {
  const dateLabel = formatDateTag(date);
  return time ? `${dateLabel} as ${time}` : dateLabel;
}

function formatPriceRange(minPrice?: number | null, maxPrice?: number | null) {
  if (minPrice == null && maxPrice == null) {
    return null;
  }

  const formatCurrency = (value: number) =>
    value.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });

  if (minPrice != null && maxPrice != null && minPrice !== maxPrice) {
    return `${formatCurrency(minPrice)} a ${formatCurrency(maxPrice)}`;
  }

  return formatCurrency(minPrice ?? maxPrice ?? 0);
}
