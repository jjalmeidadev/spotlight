import { getApiBaseUrl } from '@/frontend/config/api';

export const API_BASE_URL = getApiBaseUrl();

type FetchJsonOptions = RequestInit & {
  operation?: string;
  retries?: number;
  timeoutMs?: number;
};

const DEFAULT_TIMEOUT_MS = 18000;
const DEFAULT_RETRIES = 1;
const RETRY_DELAY_MS = 450;

export async function fetchWithTimeout(
  url: string,
  init: RequestInit = {},
  operation = 'carregar dados',
  timeoutMs = DEFAULT_TIMEOUT_MS,
) {
  const controller = new AbortController();
  const timeoutId = setTimeout(() => controller.abort(), timeoutMs);

  try {
    return await fetch(url, {
      ...init,
      signal: controller.signal,
    });
  } catch (error) {
    if (isAbortError(error)) {
      throw new Error(`Tempo esgotado ao ${operation}. Backend usado: ${API_BASE_URL}.`);
    }

    if (error instanceof TypeError) {
      throw new Error(`Falha de rede ao ${operation}. Confirme se o celular esta na mesma rede do backend (${API_BASE_URL}).`);
    }

    throw error;
  } finally {
    clearTimeout(timeoutId);
  }
}

export async function fetchJson<T>(
  path: string,
  options: FetchJsonOptions = {},
): Promise<{ response: Response; payload: T }> {
  const {
    operation = 'carregar dados',
    retries = DEFAULT_RETRIES,
    timeoutMs = DEFAULT_TIMEOUT_MS,
    ...init
  } = options;
  const url = path.startsWith('http') ? path : `${API_BASE_URL}${path}`;
  let lastError: unknown;

  for (let attempt = 0; attempt <= retries; attempt += 1) {
    try {
      const response = await fetchWithTimeout(url, init, operation, timeoutMs);
      const payload = await parseJsonResponse<T>(response);

      return { response, payload };
    } catch (error) {
      lastError = error;

      if (attempt >= retries || !isRetryable(error)) {
        break;
      }

      await delay(RETRY_DELAY_MS * (attempt + 1));
    }
  }

  throw lastError;
}

export function getApiErrorMessage(payload: any, fallback: string) {
  const details = typeof payload?.details === 'string'
    ? ` (${payload.details})`
    : '';

  return `${payload?.error ?? payload?.message ?? fallback}${details}`;
}

function isRetryable(error: unknown) {
  return error instanceof TypeError
    || (error instanceof Error && /tempo esgotado|falha de rede/i.test(error.message));
}

function isAbortError(error: unknown) {
  return error instanceof Error && error.name === 'AbortError';
}

function delay(ms: number) {
  return new Promise((resolve) => {
    setTimeout(resolve, ms);
  });
}

async function parseJsonResponse<T>(response: Response): Promise<T> {
  const text = await response.text();

  if (!text.trim()) {
    return {} as T;
  }

  return JSON.parse(text) as T;
}
