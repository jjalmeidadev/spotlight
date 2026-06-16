import { useCallback, useEffect, useState } from 'react';


import {
  EspetaculoDetalhe,
  EspetaculoSemelhante,
  obterDetalhesEspetaculo,
  obterEspetaculosSemelhantes,
} from '../models/DetailsModels';
import { avaliarEspetaculo } from '../repositories/RatingRepository';


type UseDetailsViewModelOptions = {
  accessToken?: string | null;
  onComprarIngresso?: (linkCompra: string) => void;
};


export function useDetailsViewModel(
  espetaculoId: string,
  options?: UseDetailsViewModelOptions,
) {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [detalhe, setDetalhe] = useState<EspetaculoDetalhe | null>(null);
  const [espetaculosSemelhantes, setEspetaculosSemelhantes] = useState<EspetaculoSemelhante[]>([]);
  const [avaliacaoUsuario, setAvaliacaoUsuario] = useState<number | null>(null);
  const [avaliando, setAvaliando] = useState(false);


  const recarregar = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);


      const [detalheCarregado, semelhantes] = await Promise.all([
        obterDetalhesEspetaculo(espetaculoId),
        obterEspetaculosSemelhantes(espetaculoId),
      ]);


      setDetalhe(detalheCarregado);
      setEspetaculosSemelhantes(semelhantes);
    } catch (err) {
      setDetalhe(null);
      setEspetaculosSemelhantes([]);
      setError(err instanceof Error ? err.message : 'Não foi possível carregar os detalhes.');
    } finally {
      setLoading(false);
    }
  }, [espetaculoId]);


  useEffect(() => {
    recarregar();
  }, [recarregar]);


  async function avaliar(nota: number) {
    try {
      setAvaliando(true);
      await avaliarEspetaculo(espetaculoId, nota, options?.accessToken);
      setAvaliacaoUsuario(nota);
      setDetalhe((atual) => atual ? { ...atual, notaMedia: nota } : atual);
    } finally {
      setAvaliando(false);
    }
  }


  function comprarIngresso() {
    if (!detalhe?.linkCompra) {
      return;
    }


    options?.onComprarIngresso?.(detalhe.linkCompra);
  }


  return {
    loading,
    error,
    detalhe,
    espetaculosSemelhantes,
    avaliacaoUsuario,
    avaliando,
    avaliar,
    recarregar,
    comprarIngresso,
  };
}

