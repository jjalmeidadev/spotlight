import { useMemo, useState } from 'react';

import {
  PreferenceSelection,
  PreferenceStep,
  PreferenceStepIndicator,
} from '@/frontend/models/Preference';
import { getPreferenceOptions } from '@/frontend/repositories/PreferenceRepository';

const initialSelection: PreferenceSelection = {
  priceRangeId: null,
  genreIds: [],
};

const steps: PreferenceStep[] = ['genres', 'price'];

export function usePreferenceSelectionViewModel() {
  const options = useMemo(() => getPreferenceOptions(), []);
  const [selection, setSelection] = useState<PreferenceSelection>(initialSelection);
  const [currentStep, setCurrentStep] = useState<PreferenceStep>('genres');

  function selectPriceRange(priceRangeId: string) {
    setSelection((current) => ({
      ...current,
      priceRangeId,
    }));
  }

  function toggleGenre(genreId: string) {
    setSelection((current) => {
      const genreIsSelected = current.genreIds.includes(genreId);

      return {
        ...current,
        genreIds: genreIsSelected
          ? current.genreIds.filter((selectedGenreId) => selectedGenreId !== genreId)
          : [...current.genreIds, genreId],
      };
    });
  }

  const genreStepIsComplete = selection.genreIds.length > 0;
  const priceStepIsComplete = Boolean(selection.priceRangeId);
  const currentStepIndex = steps.indexOf(currentStep);
  const canContinue = currentStep === 'genres' ? genreStepIsComplete : priceStepIsComplete;
  const isFinalStep = currentStep === 'price';
  const stepIndicators: PreferenceStepIndicator[] = steps.map((step) => ({
    id: step,
    active: step === currentStep,
    completed: step === 'genres' ? genreStepIsComplete : priceStepIsComplete,
  }));

  function goToNextStep() {
    if (!canContinue) {
      return null;
    }

    if (!isFinalStep) {
      setCurrentStep(steps[currentStepIndex + 1]);
      return null;
    }

    return submitPreferences();
  }

  function submitPreferences() {
    if (!genreStepIsComplete || !priceStepIsComplete) {
      return null;
    }

    return selection;
  }

  return {
    priceRanges: options.priceRanges,
    genres: options.genres,
    currentStep,
    isFinalStep,
    stepIndicators,
    selectedPriceRangeId: selection.priceRangeId,
    selectedGenreIds: selection.genreIds,
    canContinue,
    selectPriceRange,
    toggleGenre,
    goToNextStep,
    submitPreferences,
  };
}
