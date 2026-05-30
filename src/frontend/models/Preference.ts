export type PreferenceOption = {
  id: string;
  label: string;
};

export type PreferenceSelection = {
  priceRangeId: string | null;
  genreIds: string[];
};

export type PreferenceStep = 'genres' | 'price';

export type PreferenceStepIndicator = {
  id: PreferenceStep;
  active: boolean;
  completed: boolean;
};
