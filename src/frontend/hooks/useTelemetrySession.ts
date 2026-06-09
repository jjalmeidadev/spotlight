import { useEffect, useRef } from 'react';
import { AppState, AppStateStatus } from 'react-native';
import {
  emitTelemetryEvent,
  endTelemetrySession,
  startTelemetrySession,
} from '@/frontend/repositories/TelemetryRepository';

type UseTelemetrySessionOptions = {
  accessToken?: string | null;
  screen: string;
};

export function useTelemetrySession({ accessToken, screen }: UseTelemetrySessionOptions) {
  const accessTokenRef = useRef(accessToken);
  const lastScreenRef = useRef<string | null>(null);

  useEffect(() => {
    accessTokenRef.current = accessToken;
  }, [accessToken]);

  useEffect(() => {
    startTelemetrySession(accessTokenRef.current);

    function handleAppStateChange(nextState: AppStateStatus) {
      if (nextState === 'active') {
        startTelemetrySession(accessTokenRef.current);
        return;
      }

      endTelemetrySession('background', accessTokenRef.current);
    }

    const subscription = AppState.addEventListener('change', handleAppStateChange);

    return () => {
      subscription.remove();
      endTelemetrySession('app_unmount', accessTokenRef.current);
    };
  }, []);

  useEffect(() => {
    if (lastScreenRef.current === screen) return;

    lastScreenRef.current = screen;
    emitTelemetryEvent(
      {
        eventType: 'screen_viewed',
        screen,
        properties: { routeName: screen },
      },
      accessTokenRef.current,
    );
  }, [screen]);
}
