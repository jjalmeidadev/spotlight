import { useEffect, useState } from 'react';
import { DURACAO_CARREGAMENTO_MS, EstadoCarregamento } from '../models/Carregamento';

export function useCarregamentoViewModel(onPronto?: () => void) {
  const [estado, setEstado] = useState<EstadoCarregamento>({ pronto: false });

  useEffect(() => {
    const timer = setTimeout(() => {
      setEstado({ pronto: true });
      onPronto?.();
    }, DURACAO_CARREGAMENTO_MS);

    return () => clearTimeout(timer);
  }, []);

  return { pronto: estado.pronto };
}
