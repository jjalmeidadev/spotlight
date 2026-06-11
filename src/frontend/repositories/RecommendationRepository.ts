const API_BASE_URL = process.env.EXPO_PUBLIC_API_BASE_URL ?? 'http://localhost:5000';

export type RecommendedShow = {
  id: string;
  title: string;
  genre?: string | null;
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
    status?: string;
    notaMedia?: number | string;
  };
  score?: number | string;
  tipoAlgoritmo?: string;
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
    status: item.espetaculo.status ?? '',
    averageRating: Number(item.espetaculo.notaMedia ?? 0),
    score: Number(item.score ?? 0),
    algorithm: item.tipoAlgoritmo ?? 'similaridade_item',
  };
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
