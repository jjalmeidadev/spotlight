import { useEffect, useState } from 'react';
import type { Usuario } from '../models/User';

/**
 * Estado da verificacao de sessao no boot.
 * - `authenticated` -> app valida preferencias antes da Home;
 * - `unauthenticated` -> Login;
 * - `error` -> Login com aviso de falha.
 */
export type SessionStatus =
  | 'checking'
  | 'authenticated'
  | 'unauthenticated'
  | 'error';

export type SessionUser = Usuario;

export interface LoadingScreenViewModel {
  status: SessionStatus;
  usuario: SessionUser | null;
}

// Evita "flash" quando a checagem real e instantanea.
const MIN_SPLASH_DURATION_MS = 1500;

export function useLoadingScreenViewModel(): LoadingScreenViewModel {
  const [status, setStatus] = useState<SessionStatus>('checking');
  const [usuario, setUsuario] = useState<SessionUser | null>(null);

  useEffect(() => {
    let cancelled = false;
    const startedAt = Date.now();

    async function verificarSessao(): Promise<{ status: SessionStatus; usuario: SessionUser | null }> {
      try {
        // TODO: substituir esta simulacao pela integracao real:
        //   1. ler o token salvo (ex.: AsyncStorage / SecureStore);
        //   2. chamar GET /usuario/me com Authorization: Bearer <token>;
        //   3. se 200 -> retornar { status: 'authenticated', usuario };
        //   4. se 401 -> retornar { status: 'unauthenticated', usuario: null }.
        await wait(400);
        return { status: 'unauthenticated', usuario: null };
      } catch {
        return { status: 'error', usuario: null };
      }
    }

    (async () => {
      const resultado = await verificarSessao();
      const decorrido = Date.now() - startedAt;
      const restante = Math.max(0, MIN_SPLASH_DURATION_MS - decorrido);
      if (restante > 0) await wait(restante);
      if (!cancelled) {
        setUsuario(resultado.usuario);
        setStatus(resultado.status);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, []);

  return { status, usuario };
}

function wait(ms: number): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve, ms));
}
