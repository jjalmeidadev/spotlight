import { useState } from 'react';
import { cadastrarUsuario, Genero, Usuario } from '../models/Usuario';
import { obterLocalizacaoAtual, salvarLocalizacaoUsuario } from '../repositories/LocationRepository';

const GENEROS: Genero[] = ['Feminino', 'Masculino', 'Outro', 'Prefiro nao informar'];

export function useCadastroViewModel(onSuccess?: (usuario: Usuario) => void) {
  const [nome, setNome] = useState('');
  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');
  const [dataNascimento, setDataNascimentoState] = useState('');
  const [genero, setGenero] = useState<Genero>('Prefiro nao informar');
  const [compartilharLocalizacao, setCompartilharLocalizacao] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function criarConta() {
    try {
      setLoading(true);
      setError(null);
      const usuario = await cadastrarUsuario({
        nome,
        email,
        senha,
        dataNascimento,
        genero,
        compartilharLocalizacao,
      });

      if (compartilharLocalizacao) {
        try {
          const localizacao = await obterLocalizacaoAtual();
          await salvarLocalizacaoUsuario(usuario.accessToken, localizacao);
          console.log('Localizacao resolvida:', localizacao);
          onSuccess?.({ ...usuario, localizacao });
          return;
        } catch (locationError) {
          console.warn(locationError);
        }
      }

      onSuccess?.(usuario);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Nao foi possivel criar a conta.');
    } finally {
      setLoading(false);
    }
  }

  function setDataNascimento(value: string) {
    const digits = value.replace(/\D/g, '').slice(0, 8);
    const day = digits.slice(0, 2);
    const month = digits.slice(2, 4);
    const year = digits.slice(4, 8);

    setDataNascimentoState([day, month, year].filter(Boolean).join('/'));
  }

  return {
    nome,
    email,
    senha,
    dataNascimento,
    genero,
    generos: GENEROS,
    compartilharLocalizacao,
    loading,
    error,
    setNome,
    setEmail,
    setSenha,
    setDataNascimento,
    setGenero,
    setCompartilharLocalizacao,
    criarConta,
  };
}
