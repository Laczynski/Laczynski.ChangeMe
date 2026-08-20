export const ToastConfig = {
  KEY: 'app',
  LIFE_MS: 5000,
  ERROR_LIFE_MS: 8000
} as const;

export type ToastSeverity =
  'success' | 'info' | 'warn' | 'error' | 'secondary' | 'contrast';
