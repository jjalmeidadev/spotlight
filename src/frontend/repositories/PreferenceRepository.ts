import { PreferenceOption, PreferenceSelection } from '@/frontend/models/Preference';
import { fetchJson, getApiErrorMessage } from '@/frontend/repositories/http';

const PREFERENCE_TIMEOUT_MS = 20000;

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

  const requestPayload = toApiPayload(selection);

  const { response, payload: responsePayload } = await fetchJson<any>('/usuario/me/preferencias', {
    method: 'PUT',
    headers: {
      Authorization: `Bearer ${accessToken}`,
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(requestPayload),
    operation: 'salvar preferencias',
    retries: 1,
    timeoutMs: PREFERENCE_TIMEOUT_MS,
  });

  if (!response.ok) {
    throw new Error(getApiErrorMessage(responsePayload, 'Nao foi possivel salvar suas preferencias.'));
  }

  return responsePayload;
}

export async function obterPreferenciasUsuario(accessToken: string | null | undefined) {
  if (!accessToken) {
    return null;
  }

  const { response, payload: responsePayload } = await fetchJson<any>('/usuario/me/preferencias', {
    method: 'GET',
    headers: {
      Authorization: `Bearer ${accessToken}`,
    },
    operation: 'verificar preferencias',
    retries: 1,
    timeoutMs: PREFERENCE_TIMEOUT_MS,
  });

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
