import { useEffect, useState } from 'react';

/**
 * Estado da verificação de sessão no boot do app.
 * - `checking`: ainda decidindo;
 * - `authenticated`: token salvo e válido → app vai pra Home;
 * - `unauthenticated`: sem token (ou expirado) → app vai pra Login;
 * - `error`: falha inesperada (rede, decode) → app vai pra Login com aviso.
 */
export type SessionStatus =
  | 'checking'
  | 'authenticated'
  | 'unauthenticated'
  | 'error';

export interface LoadingScreenViewModel {
  status: SessionStatus;
}

/** Mínimo de exibição do splash, em ms — evita "flash" quando a checagem é instantânea. */
const MIN_SPLASH_DURATION_MS = 1500;

/**
 * ViewModel do splash inicial. Hoje só simula a verificação da sessão
 * com um delay; o ponto exato pra plugar a integração real está marcado
 * com `TODO`. Quando integrar o storage do JWT, troque o trecho indicado.
 */
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
