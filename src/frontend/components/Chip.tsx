import React from 'react';
import { Pressable, PressableProps, StyleSheet, Text } from 'react-native';
import { colors, spacing, typography } from '@/frontend/theme';

type ChipProps = PressableProps & {
  label: string;
  active?: boolean;
  tone?: 'light' | 'dark';
};

export function Chip({ label, active = false, tone = 'light', ...props }: ChipProps) {
  const isDarkTone = tone === 'dark';

  return (
    <Pressable
      accessibilityRole="button"
      style={({ pressed }) => [
        styles.base,
        isDarkTone
          ? active
            ? styles.darkActive
            : styles.darkInactive
          : active
            ? styles.lightActive
            : styles.lightInactive,
        pressed && styles.pressed,
      ]}
      {...props}
    >
      <Text
        style={[
          styles.label,
          isDarkTone
            ? active
              ? styles.darkActiveLabel
              : styles.darkInactiveLabel
            : active
              ? styles.lightActiveLabel
              : styles.lightInactiveLabel,
        ]}
      >
        {label}
      </Text>
    </Pressable>
  );
}

const styles = StyleSheet.create({
  base: {
    minHeight: spacing.touchMin,
    borderRadius: 999,
    paddingHorizontal: spacing.md,
    paddingVertical: spacing.sm,
    alignItems: 'center',
    justifyContent: 'center',
  },
  lightActive: {
    backgroundColor: colors.text,
    borderWidth: 1,
    borderColor: colors.text,
  },
  lightInactive: {
    backgroundColor: colors.background,
    borderWidth: 1,
    borderColor: colors.borderSoft,
  },
  darkActive: {
    backgroundColor: colors.background,
    borderWidth: 1,
    borderColor: colors.background,
  },
  darkInactive: {
    backgroundColor: 'transparent',
    borderWidth: 1,
    borderColor: colors.textStrong,
  },
  pressed: {
    opacity: 0.85,
  },
  label: {
    ...typography.chip,
  },
  lightActiveLabel: {
    color: colors.background,
  },
  lightInactiveLabel: {
    color: colors.text,
  },
  darkActiveLabel: {
    color: colors.text,
  },
  darkInactiveLabel: {
    color: colors.textStrong,
  },
});
