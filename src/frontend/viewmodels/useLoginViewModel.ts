import { useState } from 'react';
import { loginUsuario, Usuario } from '../models/Usuario';
import { obterLocalizacaoAtual, salvarLocalizacaoUsuario } from '../repositories/LocationRepository';

export function useLoginViewModel(onSuccess?: (usuario: Usuario) => void) {
  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function entrar() {
    try {
      setLoading(true);
      setError(null);
      const usuario = await loginUsuario({ email, senha });

      if (usuario.compartilharLocalizacao) {
        try {
          const localizacao = await obterLocalizacaoAtual();
          await salvarLocalizacaoUsuario(usuario.accessToken, localizacao);
          onSuccess?.({ ...usuario, localizacao });
          return;
        } catch (locationError) {
          console.warn(locationError);
        }
      }

      onSuccess?.(usuario);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Não foi possível entrar.');
    } finally {
      setLoading(false);
    }
  }

  return {
    email,
    senha,
    loading,
    error,
    setEmail,
    setSenha,
    entrar,
  };
}
