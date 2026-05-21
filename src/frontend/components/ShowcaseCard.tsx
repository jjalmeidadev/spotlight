import React from 'react';
import { ImageBackground, Pressable, StyleSheet, Text, View } from 'react-native';
import { colors, spacing, typography } from '@/frontend/theme';

type ShowcaseCardProps = {
  title: string;
  subtitle?: string;
  imageUri?: string;
  onPress?: () => void;
};

export function ShowcaseCard({ title, subtitle, imageUri, onPress }: ShowcaseCardProps) {
  return (
    <Pressable onPress={onPress} style={styles.pressable}>
      <ImageBackground
        source={imageUri ? { uri: imageUri } : undefined}
        resizeMode="cover"
        imageStyle={styles.image}
        style={styles.card}
      >
        <View style={styles.overlay} />
        <View style={styles.content}>
          <Text style={styles.title} numberOfLines={2}>
            {title}
          </Text>
          {subtitle ? (
            <Text style={styles.subtitle} numberOfLines={1}>
              {subtitle}
            </Text>
          ) : null}
          <View style={styles.ctaRow}>
            <Text style={styles.cta}>VER MAIS</Text>
            <View style={styles.ctaLine} />
          </View>
        </View>
      </ImageBackground>
    </Pressable>
  );
}

const styles = StyleSheet.create({
  pressable: {
    width: 160,
  },
  card: {
    width: '100%',
    minHeight: 220,
    borderRadius: 18,
    overflow: 'hidden',
    justifyContent: 'flex-end',
    backgroundColor: colors.surface,
  },
  image: {
    borderRadius: 18,
  },
  overlay: {
    ...StyleSheet.absoluteFillObject,
    backgroundColor: 'rgba(17, 20, 18, 0.28)',
  },
  content: {
    position: 'relative',
    padding: spacing.md,
    gap: 4,
  },
  title: {
    color: colors.background,
    ...typography.label,
    fontSize: 16,
    lineHeight: 18,
    textShadowColor: 'rgba(0, 0, 0, 0.35)',
    textShadowOffset: { width: 0, height: 1 },
    textShadowRadius: 4,
  },
  subtitle: {
    color: colors.background,
    ...typography.caption,
    textShadowColor: 'rgba(0, 0, 0, 0.35)',
    textShadowOffset: { width: 0, height: 1 },
    textShadowRadius: 4,
  },
  ctaRow: {
    marginTop: spacing.xs,
    gap: spacing.xxs,
  },
  cta: {
    color: colors.primarySoft,
    ...typography.caption,
    textTransform: 'uppercase',
  },
  ctaLine: {
    height: 2,
    width: '100%',
    borderRadius: 999,
    backgroundColor: colors.primary,
  },
});