import { getApiBaseUrl } from '@/frontend/config/api';

const API_BASE_URL = getApiBaseUrl();

type RatingResponse = {
  id?: number;
  nota?: number;
  comentario?: string | null;
  createdAt?: string;
};

export async function avaliarEspetaculo(
  espetaculoId: string,
  nota: number,
  accessToken?: string | null,
): Promise<RatingResponse> {
  if (!accessToken) {
    throw new Error('Entre na sua conta para avaliar o espetaculo.');
  }

  const response = await fetch(
    `${API_BASE_URL}/catalogo/espetaculos/${encodeURIComponent(espetaculoId)}/avaliacoes`,
    {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${accessToken}`,
        'Content-Type': 'application/json',
        Accept: 'application/json',
      },
      body: JSON.stringify({ nota }),
    },
  );

  const payload = await parseJsonResponse<RatingResponse & { error?: string; message?: string; details?: string }>(response);

  if (!response.ok) {
    const details = payload.details ? ` (${payload.details})` : '';
    throw new Error(`${payload.error ?? payload.message ?? 'Nao foi possivel registrar sua avaliacao.'}${details}`);
  }

  return payload;
}

async function parseJsonResponse<T>(response: Response): Promise<T> {
  const text = await response.text();

  if (!text.trim()) {
    return {} as T;
  }

  return JSON.parse(text) as T;
}
