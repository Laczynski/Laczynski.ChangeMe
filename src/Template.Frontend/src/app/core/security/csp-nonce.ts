const CSP_NONCE_PLACEHOLDER = '__CSP_NONCE__';

/**
 * Reads the per-request CSP nonce from `ngCspNonce` on `<app-root>`.
 * nginx replaces `__CSP_NONCE__` in index.html via sub_filter.
 */
export function readCspNonce(): string | undefined {
  if (typeof document === 'undefined') {
    return undefined;
  }

  const nonce = document.querySelector('app-root')?.getAttribute('ngCspNonce');

  if (!nonce || nonce === CSP_NONCE_PLACEHOLDER) {
    return undefined;
  }

  return nonce;
}
