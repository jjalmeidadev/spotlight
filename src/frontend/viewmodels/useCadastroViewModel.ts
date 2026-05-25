import { useState } from 'react';
import { cadastrarUsuario, Genero, Usuario } from '../models/Usuario';

const GENEROS: Genero[] = ['Feminino', 'Masculino', 'Outro', 'Prefiro não informar'];

export function useCadastroViewModel(onSuccess?: (usuario: Usuario) => void) {
  const [nome, setNome] = useState('');
  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');
  const [idade, setIdade] = useState('');
  const [genero, setGenero] = useState<Genero>('Prefiro não informar');
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
        idade,
        genero,
        compartilharLocalizacao,
      });
      onSuccess?.(usuario);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Não foi possível criar a conta.');
    } finally {
      setLoading(false);
    }
  }

  return {
    nome,
    email,
    senha,
    idade,
    genero,
    generos: GENEROS,
    compartilharLocalizacao,
    loading,
    error,
    setNome,
    setEmail,
    setSenha,
    setIdade,
    setGenero,
    setCompartilharLocalizacao,
    criarConta,
  };
}
