import { useState } from 'react';
import { loginUsuario, Usuario } from '../models/User';
import { obterLocalizacaoAtual, salvarLocalizacaoUsuario } from '../repositories/LocationRepository';
import { obterPerfilUsuario } from '../repositories/UserRepository';

export function useLoginViewModel(onSuccess?: (usuario: Usuario) => void) {
  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function entrar() {
    try {
      setLoading(true);
      setError(null);
      const usuarioAutenticado = await loginUsuario({ email, senha });
      let usuario = usuarioAutenticado;

      if (usuarioAutenticado.accessToken) {
        try {
          const perfil = await obterPerfilUsuario(usuarioAutenticado.accessToken);
          usuario = {
            ...usuarioAutenticado,
            nome: perfil.nome || usuarioAutenticado.nome,
            email: perfil.email || usuarioAutenticado.email,
            dataNascimento: perfil.dataNascimento ?? usuarioAutenticado.dataNascimento,
            tipoUsuario: perfil.tipoUsuario,
          };
        } catch (profileError) {
          console.warn(profileError);
        }
      }

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
