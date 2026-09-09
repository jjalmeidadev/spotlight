import React from 'react';
import { StyleSheet, Text, View } from 'react-native';
import { colors, spacing, typography } from '@/frontend/theme';

type CardProps = {
  title: string;
  description?: string;
  badge?: string;
};

export function Card({ title, description, badge }: CardProps) {
  return (
    <View style={styles.card}>
      {badge ? <Text style={styles.badge}>{badge}</Text> : null}
      <Text style={styles.title}>{title}</Text>
      {description ? <Text style={styles.description}>{description}</Text> : null}
    </View>
  );
}

const styles = StyleSheet.create({
  card: {
    borderRadius: 22,
    backgroundColor: colors.surface,
    borderWidth: 1,
    borderColor: colors.borderStrong,
    padding: spacing.md,
    gap: spacing.xs,
    minHeight: 120,
  },
  badge: {
    color: colors.primary,
    ...typography.caption,
    textTransform: 'uppercase',
  },
  title: {
    color: colors.textStrong,
    ...typography.label,
  },
  description: {
    color: colors.textSubtle,
    ...typography.body,
  },
});