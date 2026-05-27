import { Platform } from 'react-native';

const fontFamilies = {
  bold: 'Gotham-Bold',
  medium: 'Gotham-Medium',
  book: 'Gotham-Book',
} as const;

export const typography = {
  fontFamily: fontFamilies,
  title: {
    fontFamily: fontFamilies.bold,
    fontSize: 28,
    lineHeight: 34,
    letterSpacing: 0.8,
    includeFontPadding: false,
    ...(Platform.OS === 'android' ? { textAlignVertical: 'center' as const } : null),
  },
  subtitle: {
    fontFamily: fontFamilies.medium,
    fontSize: 14,
    lineHeight: 20,
    letterSpacing: 0.2,
    includeFontPadding: false,
  },
  body: {
    fontFamily: fontFamilies.book,
    fontSize: 14,
    lineHeight: 20,
    letterSpacing: 0.1,
    includeFontPadding: false,
  },
  label: {
    fontFamily: fontFamilies.medium,
    fontSize: 14,
    lineHeight: 18,
    letterSpacing: 0.2,
    includeFontPadding: false,
  },
  button: {
    fontFamily: fontFamilies.bold,
    fontSize: 16,
    lineHeight: 20,
    letterSpacing: 0.4,
    includeFontPadding: false,
  },
  chip: {
    fontFamily: fontFamilies.bold,
    fontSize: 13,
    lineHeight: 16,
    letterSpacing: 0.3,
    includeFontPadding: false,
  },
  caption: {
    fontFamily: fontFamilies.book,
    fontSize: 12,
    lineHeight: 16,
    letterSpacing: 0.2,
    includeFontPadding: false,
  },
} as const;

export type AppTypography = typeof typography;