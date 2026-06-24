import { fetchJson, getApiErrorMessage } from '@/frontend/repositories/http';

const RATING_TIMEOUT_MS = 18000;

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

  const { response, payload } = await fetchJson<RatingResponse & { error?: string; message?: string; details?: string }>(
    `/catalogo/espetaculos/${encodeURIComponent(espetaculoId)}/avaliacoes`,
    {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${accessToken}`,
        'Content-Type': 'application/json',
        Accept: 'application/json',
      },
      body: JSON.stringify({ nota }),
      operation: 'registrar avaliacao',
      retries: 1,
      timeoutMs: RATING_TIMEOUT_MS,
    },
  );

  if (!response.ok) {
    throw new Error(getApiErrorMessage(payload, 'Nao foi possivel registrar sua avaliacao.'));
  }

  if (__DEV__) {
    console.log('Avaliacao registrada:', {
      espetaculoId,
      nota,
      avaliacaoId: payload.id,
    });
  }

  return payload;
}
