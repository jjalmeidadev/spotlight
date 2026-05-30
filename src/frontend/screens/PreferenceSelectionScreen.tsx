import React from 'react';
import {
  Image,
  Pressable,
  SafeAreaView,
  ScrollView,
  StatusBar,
  StyleSheet,
  Text,
  View,
} from 'react-native';

import { usePreferenceSelectionViewModel } from '@/frontend/viewmodels/usePreferenceSelectionViewModel';

const preferenceIcon = require('../../../documentos/img/preferential_icon.png');
const priceIcon = require('../../../documentos/img/preco_icon.png');

type PreferenceSelectionScreenProps = {
  onComplete?: () => void;
};

export default function PreferenceSelectionScreen({ onComplete }: PreferenceSelectionScreenProps) {
  const viewModel = usePreferenceSelectionViewModel();

  function handleContinue() {
    const selection = viewModel.goToNextStep();

    if (!selection) {
      return;
    }

    onComplete?.();
  }

  return (
    <SafeAreaView style={styles.safeArea}>
      <StatusBar barStyle="light-content" backgroundColor="#0A0A0A" />

      <View style={styles.container}>
        <ScrollView
          bounces={false}
          showsVerticalScrollIndicator={false}
          contentContainerStyle={styles.scrollContent}
        >
          <Text style={styles.title}>Preferências</Text>

          {viewModel.currentStep === 'genres' ? (
            <View style={styles.stepContent}>
              <Text style={styles.stepDescription}>
                Nos ajude a entender sua preferência de gênero de espetáculos
              </Text>

              <Image
                source={preferenceIcon}
                style={styles.stepIcon}
                resizeMode="contain"
                accessibilityLabel="Ícone de preferência"
              />

              <View style={styles.genreGroup}>
                {viewModel.genres.map((genre) => (
                  <PreferenceOption
                    key={genre.id}
                    label={genre.label}
                    active={viewModel.selectedGenreIds.includes(genre.id)}
                    onPress={() => viewModel.toggleGenre(genre.id)}
                  />
                ))}
              </View>
            </View>
          ) : (
            <View style={styles.stepContent}>
              <Text style={styles.stepDescription}>
                Nos ajude a entender o preço que gostaria de pagar nos ingressos dos espetáculos
              </Text>

              <Image
                source={priceIcon}
                style={styles.stepIcon}
                resizeMode="contain"
                accessibilityLabel="Ícone de preço"
              />

              <View style={styles.priceGroup}>
                {viewModel.priceRanges.map((priceRange) => (
                  <PreferenceOption
                    key={priceRange.id}
                    label={priceRange.label}
                    active={viewModel.selectedPriceRangeId === priceRange.id}
                    onPress={() => viewModel.selectPriceRange(priceRange.id)}
                  />
                ))}
              </View>
            </View>
          )}
        </ScrollView>

        <View style={styles.footer}>
          <View style={styles.onboardingIndicator} accessibilityRole="progressbar">
            {viewModel.stepIndicators.map((step) => (
              <View
                key={step.id}
                style={[
                  styles.onboardingBar,
                  step.active || step.completed
                    ? styles.onboardingBarActive
                    : styles.onboardingBarInactive,
                ]}
              />
            ))}
          </View>

          <Pressable
            accessibilityRole="button"
            disabled={!viewModel.canContinue}
            onPress={handleContinue}
            style={({ pressed }) => [
              styles.continueButton,
              !viewModel.canContinue && styles.continueButtonDisabled,
              pressed && viewModel.canContinue && styles.continueButtonPressed,
            ]}
          >
            <Text style={styles.continueLabel}>Continuar</Text>
          </Pressable>
        </View>
      </View>
    </SafeAreaView>
  );
}

type PreferenceOptionProps = {
  active: boolean;
  label: string;
  onPress: () => void;
};

function PreferenceOption({ active, label, onPress }: PreferenceOptionProps) {
  return (
    <Pressable
      accessibilityRole="button"
      onPress={onPress}
      style={({ pressed }) => [styles.option, active && styles.optionActive, pressed && styles.pressed]}
    >
      <Text style={[styles.optionLabel, active && styles.optionLabelActive]}>{label}</Text>
    </Pressable>
  );
}

const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
    backgroundColor: '#0A0A0A',
  },
  container: {
    flex: 1,
    backgroundColor: '#0A0A0A',
  },
  scrollContent: {
    flexGrow: 1,
    paddingHorizontal: 16,
    paddingTop: 42,
    paddingBottom: 104,
  },
  title: {
    color: '#FFFFFF',
    fontFamily: 'Gotham-Bold',
    fontSize: 30,
    fontWeight: '700',
    letterSpacing: 1.8,
    lineHeight: 36,
    marginBottom: 18,
    textTransform: 'uppercase',
  },
  stepContent: {
    alignItems: 'center',
    width: '100%',
  },
  stepDescription: {
    alignSelf: 'flex-start',
    color: 'rgba(255,255,255,0.68)',
    fontFamily: 'Gotham-Book',
    fontSize: 14,
    fontWeight: '300',
    lineHeight: 20,
    marginBottom: 18,
    maxWidth: 324,
  },
  stepIcon: {
    height: 84,
    marginBottom: 24,
    tintColor: '#E8420A',
    width: 84,
  },
  priceGroup: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: 10,
    justifyContent: 'flex-start',
    width: '100%',
  },
  genreGroup: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    gap: 10,
    justifyContent: 'flex-start',
    width: '100%',
  },
  option: {
    alignItems: 'center',
    borderColor: 'rgba(255,255,255,0.16)',
    borderRadius: 6,
    borderWidth: 1,
    flexBasis: '48%',
    flexGrow: 1,
    justifyContent: 'center',
    minHeight: 42,
    paddingHorizontal: 14,
    paddingVertical: 9,
  },
  optionActive: {
    backgroundColor: '#E8420A',
    borderColor: '#E8420A',
  },
  optionLabel: {
    color: 'rgba(255,255,255,0.72)',
    fontFamily: 'Gotham-Book',
    fontSize: 12,
    fontWeight: '500',
    lineHeight: 16,
  },
  optionLabelActive: {
    color: '#FFFFFF',
  },
  footer: {
    position: 'absolute',
    left: 0,
    right: 0,
    bottom: 20,
    alignItems: 'center',
    paddingHorizontal: 16,
  },
  onboardingIndicator: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    columnGap: 7,
    marginBottom: 14,
  },
  onboardingBar: {
    width: 56,
    height: 5,
    borderRadius: 2,
  },
  onboardingBarActive: {
    backgroundColor: '#E8420A',
  },
  onboardingBarInactive: {
    backgroundColor: 'transparent',
    borderWidth: 1,
    borderColor: 'rgba(255,255,255,0.3)',
  },
  continueButton: {
    alignItems: 'center',
    backgroundColor: '#E8420A',
    borderRadius: 6,
    height: 52,
    justifyContent: 'center',
    width: '100%',
  },
  continueButtonDisabled: {
    opacity: 0.45,
  },
  continueButtonPressed: {
    opacity: 0.82,
  },
  continueLabel: {
    color: '#FFFFFF',
    fontFamily: 'Gotham-Bold',
    fontSize: 13,
    fontWeight: '700',
    letterSpacing: 1.56,
    textTransform: 'uppercase',
  },
  pressed: {
    opacity: 0.84,
  },
});
