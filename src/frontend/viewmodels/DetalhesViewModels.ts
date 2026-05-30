import { useCallback, useEffect, useState } from 'react';


import {
  EspetaculoDetalhe,
  EspetaculoSemelhante,
  obterDetalhesEspetaculo,
  obterEspetaculosSemelhantes,
} from '../models/DetalhesModels';


type UseDetalhesViewModelOptions = {
  onComprarIngresso?: (linkCompra: string) => void;
};


export function useDetalhesViewModel(
  espetaculoId: string,
  options?: UseDetalhesViewModelOptions,
) {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [detalhe, setDetalhe] = useState<EspetaculoDetalhe | null>(null);
  const [espetaculosSemelhantes, setEspetaculosSemelhantes] = useState<EspetaculoSemelhante[]>([]);
  const [favorito, setFavorito] = useState(false);


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


  function alternarFavorito() {
    setFavorito((atual: boolean) => !atual);
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
    favorito,
    alternarFavorito,
    recarregar,
    comprarIngresso,
  };
}



