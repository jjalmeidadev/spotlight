import { getApiBaseUrl } from '@/frontend/config/api';
import { obterUrlMidiaConhecida } from './CatalogRepository';

const API_BASE_URL = getApiBaseUrl();
const SUPABASE_URL = process.env.EXPO_PUBLIC_SUPABASE_URL ?? 'https://nyaxqgujyyvkjnqzbklf.supabase.co';

export type RecommendedShow = {
  id: string;
  title: string;
  genre?: string | null;
  imageUrl?: string | null;
  status: string;
  averageRating: number;
  score: number;
  algorithm: string;
  reason?: string | null;
  generatedAt?: string;
};

export type SimilarShow = {
  id: string;
  title: string;
  genre?: string | null;
  imageUrl?: string | null;
  status: string;
  averageRating: number;
  score: number;
  algorithm: string;
};

type RecommendationEnvelope = {
  data?: RecommendationItem[];
};

type SimilarEnvelope = {
  data?: SimilarItem[];
};

type RecommendationItem = {
  espetaculo?: {
    id?: number | string;
    titulo?: string;
    genero?: string | null;
    imagemUrl?: string | null;
    midias?: RecommendationMedia[];
    status?: string;
    notaMedia?: number | string;
  };
  score?: number | string;
  tipoAlgoritmo?: string;
  motivo?: string | null;
  dataGeracao?: string;
};

type SimilarItem = {
  espetaculo?: {
    id?: number | string;
    titulo?: string;
    genero?: string | null;
    imagemUrl?: string | null;
    midias?: RecommendationMedia[];
    status?: string;
    notaMedia?: number | string;
  };
  score?: number | string;
  tipoAlgoritmo?: string;
};

type RecommendationMedia = {
  tipoMidia: 'catalog_card' | 'catalog_hero_background' | 'catalog_hero_artwork' | 'detail_image';
  storageBucket: string;
  storagePath: string;
  altText?: string | null;
  ordemExibicao: number;
};

export async function listarRecomendacoesUsuario(
  accessToken: string | null | undefined,
  limite = 8,
): Promise<RecommendedShow[]> {
  if (!accessToken) {
    return [];
  }

  const response = await fetch(`${API_BASE_URL}/recomendacao/me?limite=${limite}`, {
    method: 'GET',
    headers: {
      Authorization: `Bearer ${accessToken}`,
      Accept: 'application/json',
    },
  });

  const payload = await parseJsonResponse<RecommendationEnvelope>(response);

  if (!response.ok) {
    throw new Error(getApiErrorMessage(payload, 'Nao foi possivel carregar recomendacoes.'));
  }

  return (payload.data ?? [])
    .map(toRecommendedShow)
    .filter((item): item is RecommendedShow => item !== null);
}

export async function listarEspetaculosSimilares(
  espetaculoId: string,
  limite = 8,
): Promise<SimilarShow[]> {
  if (!espetaculoId.trim()) {
    return [];
  }

  const response = await fetch(
    `${API_BASE_URL}/recomendacao/espetaculos/${encodeURIComponent(espetaculoId)}/similares?limite=${limite}`,
    {
      method: 'GET',
      headers: {
        Accept: 'application/json',
      },
    },
  );

  const payload = await parseJsonResponse<SimilarEnvelope>(response);

  if (!response.ok) {
    throw new Error(getApiErrorMessage(payload, 'Nao foi possivel carregar espetaculos similares.'));
  }

  return (payload.data ?? [])
    .map(toSimilarShow)
    .filter((item): item is SimilarShow => item !== null);
}

function toRecommendedShow(item: RecommendationItem): RecommendedShow | null {
  if (!item.espetaculo?.id || !item.espetaculo.titulo) {
    return null;
  }

  return {
    id: String(item.espetaculo.id),
    title: item.espetaculo.titulo,
    genre: item.espetaculo.genero,
    imageUrl: item.espetaculo.imagemUrl
      ?? obterUrlMidia(item.espetaculo, 'catalog_card')
      ?? obterUrlMidiaConhecida(item.espetaculo.titulo, 'catalog_card'),
    status: item.espetaculo.status ?? '',
    averageRating: Number(item.espetaculo.notaMedia ?? 0),
    score: Number(item.score ?? 0),
    algorithm: item.tipoAlgoritmo ?? 'modelo_recomendacao',
    reason: item.motivo,
    generatedAt: item.dataGeracao,
  };
}

function toSimilarShow(item: SimilarItem): SimilarShow | null {
  if (!item.espetaculo?.id || !item.espetaculo.titulo) {
    return null;
  }

  return {
    id: String(item.espetaculo.id),
    title: item.espetaculo.titulo,
    genre: item.espetaculo.genero,
    imageUrl: item.espetaculo.imagemUrl
      ?? obterUrlMidia(item.espetaculo, 'catalog_card')
      ?? obterUrlMidiaConhecida(item.espetaculo.titulo, 'catalog_card'),
    status: item.espetaculo.status ?? '',
    averageRating: Number(item.espetaculo.notaMedia ?? 0),
    score: Number(item.score ?? 0),
    algorithm: item.tipoAlgoritmo ?? 'similaridade_item',
  };
}

function obterUrlMidia(
  item: { midias?: RecommendationMedia[] },
  tipoMidia: RecommendationMedia['tipoMidia'],
) {
  const media = item.midias
    ?.filter((entry) => entry.tipoMidia === tipoMidia)
    .sort((a, b) => a.ordemExibicao - b.ordemExibicao)[0];

  if (!media) {
    return null;
  }

  const encodedPath = media.storagePath
    .trim()
    .split('/')
    .map((segment) => encodeURIComponent(segment))
    .join('/');

  return `${SUPABASE_URL.replace(/\/$/, '')}/storage/v1/object/public/${media.storageBucket}/${encodedPath}`;
}

function getApiErrorMessage(payload: any, fallback: string) {
  const details = typeof payload?.details === 'string'
    ? ` (${payload.details})`
    : '';

  return `${payload?.error ?? fallback}${details}`;
}

async function parseJsonResponse<T>(response: Response): Promise<T> {
  const text = await response.text();

  if (!text.trim()) {
    return {} as T;
  }

  return JSON.parse(text) as T;
}
