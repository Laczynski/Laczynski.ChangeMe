function escapeDialogMessageHtml(value: string): string {
  return value
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#39;');
}

export function highlightDialogValue(value: string): string {
  return `<strong class="font-semibold">${escapeDialogMessageHtml(value)}</strong>`;
}
