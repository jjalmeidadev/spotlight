import { PreferenceOption } from '@/frontend/models/Preference';

export type PreferenceOptions = {
  priceRanges: PreferenceOption[];
  genres: PreferenceOption[];
};

const preferenceOptions: PreferenceOptions = {
  priceRanges: [
    { id: 'price-up-to-50', label: 'Até R$50' },
    { id: 'price-50-100', label: 'R$50-100' },
    { id: 'price-100-200', label: 'R$100-200' },
  ],
  genres: [
    { id: 'drama', label: 'Drama' },
    { id: 'comedia', label: 'Comédia' },
    { id: 'musical', label: 'Musical' },
    { id: 'infantil', label: 'Infantil' },
    { id: 'acao', label: 'Ação' },
    { id: 'romance', label: 'Romance' },
    { id: 'suspense', label: 'Suspense' },
    { id: 'stand-up', label: 'Stand-up' },
    { id: 'palestra', label: 'Palestra' },
  ],
};

export function getPreferenceOptions(): PreferenceOptions {
  return preferenceOptions;
}

