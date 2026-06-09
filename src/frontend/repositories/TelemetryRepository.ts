import { Platform } from 'react-native';

const API_BASE_URL = process.env.EXPO_PUBLIC_API_BASE_URL ?? 'http://localhost:5000';
const TELEMETRY_PATH = '/usuario/telemetria';

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
  void sendTelemetryEvent(event, accessToken).catch((error) => {
    if (__DEV__) {
      console.warn('Falha ao enviar telemetria:', error);
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

  const response = await fetch(`${API_BASE_URL}${TELEMETRY_PATH}`, {
    method: 'POST',
    headers,
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

  if (!response.ok) {
    throw new Error(`Telemetria retornou HTTP ${response.status}.`);
  }
}

function createTelemetryId(prefix: string) {
  const randomPart = Math.random().toString(36).slice(2, 10);
  return `${prefix}_${Date.now().toString(36)}_${randomPart}`;
}
