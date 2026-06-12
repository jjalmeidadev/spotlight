import { PreferenceOption, PreferenceSelection } from '@/frontend/models/Preference';
import { getApiBaseUrl } from '@/frontend/config/api';

const API_BASE_URL = getApiBaseUrl();
const REQUEST_TIMEOUT_MS = 10000;

export type PreferenceOptions = {
  priceRanges: PreferenceOption[];
  genres: PreferenceOption[];
};

const preferenceOptions: PreferenceOptions = {
  priceRanges: [
    { id: 'price-up-to-50', label: 'Até R$50' },
    { id: 'price-50-100', label: 'R$50-100' },
    { id: 'price-100-200', label: 'R$100-200' },
  ],
  genres: [
    { id: 'drama', label: 'Drama' },
    { id: 'comedia', label: 'Comédia' },
    { id: 'musical', label: 'Musical' },
    { id: 'infantil', label: 'Infantil' },
    { id: 'acao', label: 'Ação' },
    { id: 'romance', label: 'Romance' },
    { id: 'suspense', label: 'Suspense' },
    { id: 'stand-up', label: 'Stand-up' },
    { id: 'palestra', label: 'Palestra' },
  ],
};

export function getPreferenceOptions(): PreferenceOptions {
  return preferenceOptions;
}

export async function salvarPreferenciasUsuario(
  accessToken: string | null | undefined,
  selection: PreferenceSelection,
) {
  if (!accessToken) {
    throw new Error('Token de acesso ausente para salvar preferencias.');
  }

  const url = `${API_BASE_URL}/usuario/me/preferencias`;
  const requestPayload = toApiPayload(selection);

  console.log('Salvando preferencias do usuario:', {
    url,
    generos: requestPayload.generosFavoritos.length,
    faixaPrecoMax: requestPayload.faixaPrecoMax,
  });

  const response = await fetchWithTimeout(url, {
    method: 'PUT',
    headers: {
      Authorization: `Bearer ${accessToken}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(requestPayload),
  }, 'salvar preferencias');

  const responsePayload = await parseJsonResponse(response);

  console.log('Resposta ao salvar preferencias:', response.status, responsePayload);

  if (!response.ok) {
    throw new Error(getApiErrorMessage(responsePayload, 'Nao foi possivel salvar suas preferencias.'));
  }

  return responsePayload;
}

export async function obterPreferenciasUsuario(accessToken: string | null | undefined) {
  if (!accessToken) {
    return null;
  }

  const url = `${API_BASE_URL}/usuario/me/preferencias`;

  const response = await fetchWithTimeout(url, {
    method: 'GET',
    headers: {
      Authorization: `Bearer ${accessToken}`,
    },
  }, 'verificar preferencias');

  const responsePayload = await parseJsonResponse(response);

  console.log('Resposta ao buscar preferencias:', response.status, responsePayload);

  if (!response.ok) {
    throw new Error(getApiErrorMessage(responsePayload, 'Nao foi possivel carregar suas preferencias.'));
  }

  return responsePayload;
}

export function preferenciasEstaoCompletas(preferencias: any) {
  return Array.isArray(preferencias?.generosFavoritos)
    && preferencias.generosFavoritos.length > 0
    && preferencias?.faixaPrecoMax !== null
    && preferencias?.faixaPrecoMax !== undefined;
}

function toApiPayload(selection: PreferenceSelection) {
  const selectedGenres = preferenceOptions.genres
    .filter((genre) => selection.genreIds.includes(genre.id))
    .map((genre) => genre.label);

  return {
    generosFavoritos: selectedGenres,
    categoriasFavoritas: [],
    faixaPrecoMax: getPriceRangeMax(selection.priceRangeId),
  };
}

function getPriceRangeMax(priceRangeId: string | null) {
  switch (priceRangeId) {
    case 'price-up-to-50':
      return 50;
    case 'price-50-100':
      return 100;
    case 'price-100-200':
      return 200;
    default:
      return null;
  }
}

function getApiErrorMessage(payload: any, fallback: string) {
  const details = typeof payload?.details === 'string'
    ? ` (${payload.details})`
    : '';

  return `${payload?.error ?? fallback}${details}`;
}

async function parseJsonResponse(response: Response) {
  const text = await response.text();

  if (!text.trim()) {
    return {};
  }

  return JSON.parse(text);
}

async function fetchWithTimeout(url: string, init: RequestInit, operation: string) {
  const controller = new AbortController();
  const timeoutId = setTimeout(() => controller.abort(), REQUEST_TIMEOUT_MS);

  try {
    return await fetch(url, {
      ...init,
      signal: controller.signal,
    });
  } catch (error) {
    if (error instanceof Error && error.name === 'AbortError') {
      throw new Error(`Tempo esgotado ao ${operation}. Verifique se o app esta na mesma rede do backend.`);
    }

    throw error;
  } finally {
    clearTimeout(timeoutId);
  }
}
