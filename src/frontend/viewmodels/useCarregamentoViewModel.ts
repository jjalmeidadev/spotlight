import { useEffect, useRef } from 'react';
import { DURACAO_CARREGAMENTO_MS } from '../models/Carregamento';

export function useCarregamentoViewModel(onPronto?: () => void) {
  const onProntoRef = useRef(onPronto);
  useEffect(() => { onProntoRef.current = onPronto; });

  useEffect(() => {
    const timer = setTimeout(() => {
      onProntoRef.current?.();
    }, DURACAO_CARREGAMENTO_MS);
    return () => clearTimeout(timer);
  }, []);
}
