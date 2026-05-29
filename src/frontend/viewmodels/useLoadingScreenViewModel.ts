import { useEffect, useState } from 'react';

/**
 * Estado da verificação de sessão no boot.
 * - `authenticated` → app vai pra Home; `unauthenticated` → Login;
 *   `error` → Login com aviso de falha.
 */
export type SessionStatus =
  | 'checking'
  | 'authenticated'
  | 'unauthenticated'
  | 'error';

export interface LoadingScreenViewModel {
  status: SessionStatus;
}

// Evita "flash" quando a checagem real é instantânea.
const MIN_SPLASH_DURATION_MS = 1500;

export function useLoadingScreenViewModel(): LoadingScreenViewModel {
  const [status, setStatus] = useState<SessionStatus>('checking');

  useEffect(() => {
    let cancelled = false;
    const startedAt = Date.now();

    async function verificarSessao(): Promise<SessionStatus> {
      try {
        // TODO: substituir esta simulação pela integração real:
        //   1. ler o token salvo (ex.: AsyncStorage / SecureStore);
        //   2. chamar GET /usuario/me com Authorization: Bearer <token>;
        //   3. se 200 → 'authenticated'; se 401 → 'unauthenticated'.
        await wait(400);
        return 'unauthenticated';
      } catch {
        return 'error';
      }
    }

    (async () => {
      const resultado = await verificarSessao();
      const decorrido = Date.now() - startedAt;
      const restante = Math.max(0, MIN_SPLASH_DURATION_MS - decorrido);
      if (restante > 0) await wait(restante);
      if (!cancelled) setStatus(resultado);
    })();

    return () => {
      cancelled = true;
    };
  }, []);

  return { status };
}

function wait(ms: number): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve, ms));
}
