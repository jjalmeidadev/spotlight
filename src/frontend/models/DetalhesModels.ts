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
  const tags = [
    detalhe.genre,
    detalhe.sessions[0]?.date ? formatDateTag(detalhe.sessions[0].date) : null,
    detalhe.sessions[0]?.time,
  ].filter((tag): tag is string => Boolean(tag));

  return {
    id: detalhe.id,
    titulo: detalhe.title,
    descricao: detalhe.description ?? 'Descricao indisponivel para este espetaculo.',
    imagemUrl: detalhe.imageUrl ?? undefined,
    percentualAprovacao: toApprovalPercent(detalhe.averageRating),
    notaMedia: detalhe.averageRating,
    tags,
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
