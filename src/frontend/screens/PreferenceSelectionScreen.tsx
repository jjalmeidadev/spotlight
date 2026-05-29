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

import { Chip } from '@/frontend/components/Chip';
import { colors, spacing, typography } from '@/frontend/theme';
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
      <StatusBar barStyle="light-content" backgroundColor={colors.surface} />

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
                  <Chip
                    key={genre.id}
                    label={genre.label}
                    tone="dark"
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
                  <Chip
                    key={priceRange.id}
                    label={priceRange.label}
                    tone="dark"
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

const styles = StyleSheet.create({
  safeArea: {
    flex: 1,
    backgroundColor: colors.surface,
  },
  container: {
    flex: 1,
    backgroundColor: colors.surface,
  },
  scrollContent: {
    flexGrow: 1,
    paddingHorizontal: spacing.xl,
    paddingTop: spacing.xxl,
    paddingBottom: 104,
  },
  title: {
    color: colors.textStrong,
    ...typography.title,
    fontSize: 36,
    lineHeight: 42,
    textAlign: 'center',
    marginBottom: spacing.xl,
  },
  stepContent: {
    alignItems: 'center',
    width: '100%',
  },
  stepDescription: {
    color: colors.textStrong,
    ...typography.subtitle,
    fontSize: 15,
    lineHeight: 22,
    textAlign: 'center',
    marginBottom: spacing.lg,
    maxWidth: 304,
  },
  stepIcon: {
    width: 92,
    height: 92,
    marginBottom: spacing.xl,
  },
  priceGroup: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    justifyContent: 'center',
    columnGap: spacing.md,
    rowGap: spacing.lg,
  },
  genreGroup: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    justifyContent: 'center',
    columnGap: spacing.lg,
    rowGap: spacing.xxxl,
    paddingHorizontal: spacing.sm,
  },
  footer: {
    position: 'absolute',
    left: 0,
    right: 0,
    bottom: spacing.xl,
    alignItems: 'center',
    paddingHorizontal: spacing.xl,
  },
  onboardingIndicator: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    columnGap: spacing.xs,
    marginBottom: spacing.md,
  },
  onboardingBar: {
    width: 56,
    height: 5,
    borderRadius: 999,
  },
  onboardingBarActive: {
    backgroundColor: colors.textStrong,
  },
  onboardingBarInactive: {
    backgroundColor: 'transparent',
    borderWidth: 1,
    borderColor: colors.textStrong,
  },
  continueButton: {
    minHeight: 28,
    minWidth: 148,
    borderRadius: 999,
    alignItems: 'center',
    justifyContent: 'center',
    backgroundColor: colors.background,
    paddingHorizontal: spacing.lg,
  },
  continueButtonDisabled: {
    opacity: 0.45,
  },
  continueButtonPressed: {
    opacity: 0.82,
    transform: [{ scale: 0.99 }],
  },
  continueLabel: {
    color: colors.text,
    ...typography.caption,
    fontFamily: typography.fontFamily.bold,
    fontSize: 11,
  },
});
