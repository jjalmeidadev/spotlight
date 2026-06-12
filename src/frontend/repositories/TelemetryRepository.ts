import { Platform } from 'react-native';
import { getApiBaseUrl } from '@/frontend/config/api';

const API_BASE_URL = getApiBaseUrl();
const TELEMETRY_PATH = '/usuario/telemetria';
const TELEMETRY_ENABLED = process.env.EXPO_PUBLIC_TELEMETRY_ENABLED !== 'false';
const TELEMETRY_TIMEOUT_MS = 5000;
const TELEMETRY_BACKOFF_MS = 60_000;
const MAX_FAILURES_BEFORE_BACKOFF = 2;

type TelemetryEntity = {
  type: 'espetaculo' | 'sessao' | 'teatro' | 'categoria' | 'busca';
  id: string;
};

export type TelemetryEventType =
  | 'session_started'
  | 'screen_viewed'
  | 'show_card_clicked'
  | 'show_viewed'
  | 'search_performed'
  | 'favorite_toggled'
  | 'purchase_clicked'
  | 'session_ended';

export type TelemetryEventInput = {
  eventType: TelemetryEventType;
  screen?: string;
  entity?: TelemetryEntity;
  properties?: Record<string, unknown>;
};

let anonymousId = createTelemetryId('anon');
let sessionId = createTelemetryId('sess');
let sessionStartedAt = Date.now();
let sessionStarted = false;
let sessionEnded = false;
let consecutiveFailures = 0;
let pausedUntil = 0;
let warningLogged = false;

export function startTelemetrySession(accessToken?: string | null) {
  if (sessionStarted && !sessionEnded) {
    return;
  }

  if (!sessionStarted) {
    sessionStarted = true;
    emitTelemetryEvent(
      {
        eventType: 'session_started',
        properties: { entryPoint: 'app_open' },
      },
      accessToken,
    );
    return;
  }

  sessionId = createTelemetryId('sess');
  sessionStartedAt = Date.now();
  sessionStarted = true;
  sessionEnded = false;

  emitTelemetryEvent(
    {
      eventType: 'session_started',
      properties: { entryPoint: 'app_resume' },
    },
    accessToken,
  );
}

export function endTelemetrySession(reason: string, accessToken?: string | null) {
  if (sessionEnded) return;

  const durationMs = Math.max(0, Date.now() - sessionStartedAt);
  sessionEnded = true;

  emitTelemetryEvent(
    {
      eventType: 'session_ended',
      properties: { durationMs, reason },
    },
    accessToken,
  );
}

export function emitTelemetryEvent(
  event: TelemetryEventInput,
  accessToken?: string | null,
) {
  if (!TELEMETRY_ENABLED || Date.now() < pausedUntil) {
    return;
  }

  void sendTelemetryEvent(event, accessToken).then(() => {
    consecutiveFailures = 0;
    warningLogged = false;
  }).catch((error) => {
    consecutiveFailures += 1;

    if (consecutiveFailures >= MAX_FAILURES_BEFORE_BACKOFF) {
      pausedUntil = Date.now() + TELEMETRY_BACKOFF_MS;
    }

    if (__DEV__) {
      if (!warningLogged) {
        console.warn('Telemetria indisponivel; eventos pausados temporariamente.', error);
        warningLogged = true;
      }
    }
  });
}

async function sendTelemetryEvent(
  event: TelemetryEventInput,
  accessToken?: string | null,
) {
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
  };

  if (accessToken) {
    headers.Authorization = `Bearer ${accessToken}`;
  }

  const controller = new AbortController();
  const timeout = setTimeout(() => controller.abort(), TELEMETRY_TIMEOUT_MS);

  let response: Response;

  try {
    response = await fetch(`${API_BASE_URL}${TELEMETRY_PATH}`, {
      method: 'POST',
      headers,
      signal: controller.signal,
      body: JSON.stringify({
        eventId: createTelemetryId('evt'),
        eventType: event.eventType,
        source: 'mobile',
        sessionId,
        occurredAt: new Date().toISOString(),
        screen: event.screen,
        entity: event.entity,
        properties: event.properties,
        device: {
          platform: Platform.OS,
        },
        anonymousId: accessToken ? undefined : anonymousId,
      }),
    });
  } finally {
    clearTimeout(timeout);
  }

  if (!response.ok) {
    throw new Error(`Telemetria retornou HTTP ${response.status}.`);
  }
}

function createTelemetryId(prefix: string) {
  const randomPart = Math.random().toString(36).slice(2, 10);
  return `${prefix}_${Date.now().toString(36)}_${randomPart}`;
}
