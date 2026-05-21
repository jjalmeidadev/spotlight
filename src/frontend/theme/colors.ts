export const colors = {
  background: '#ffffff',
  backgroundAlt: '#f8f6f2',
  surface: '#1b201d',
  surfaceAlt: '#ffffff',
  primary: '#ff5a2f',
  primarySoft: '#dbe8e3',
  text: '#111412',
  textStrong: '#eff4ef',
  textMuted: '#6f7671',
  textSubtle: '#bac3be',
  border: '#d8ddd8',
  borderStrong: '#2a302d',
  borderSoft: '#cfd5cf',
  error: '#c94f3a',
  placeholder: '#8a8a8a',
} as const;

export type AppColors = typeof colors;