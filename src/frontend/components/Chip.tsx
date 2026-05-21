import React from 'react';
import { Pressable, PressableProps, StyleSheet, Text } from 'react-native';
import { colors, spacing, typography } from '@/frontend/theme';

type ChipProps = PressableProps & {
  label: string;
  active?: boolean;
};

export function Chip({ label, active = false, ...props }: ChipProps) {
  return (
    <Pressable
      accessibilityRole="button"
      style={({ pressed }) => [
        styles.base,
        active ? styles.active : styles.inactive,
        pressed && styles.pressed,
      ]}
      {...props}
    >
      <Text style={[styles.label, active ? styles.activeLabel : styles.inactiveLabel]}>{label}</Text>
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
  active: {
    backgroundColor: colors.text,
    borderWidth: 1,
    borderColor: colors.text,
  },
  inactive: {
    backgroundColor: colors.background,
    borderWidth: 1,
    borderColor: colors.borderSoft,
  },
  pressed: {
    opacity: 0.85,
  },
  label: {
    ...typography.chip,
  },
  activeLabel: {
    color: colors.background,
  },
  inactiveLabel: {
    color: colors.text,
  },
});