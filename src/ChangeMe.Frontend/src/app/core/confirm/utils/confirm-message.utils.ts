import type { ConfirmMessagePart } from '@core/confirm/models/confirm-message.model';

export function confirmText(value: string): ConfirmMessagePart {
  return { type: 'text', value };
}

export function confirmStrong(value: string): ConfirmMessagePart {
  return { type: 'strong', value };
}

export function confirmMessage(
  ...parts: Array<string | ConfirmMessagePart>
): ConfirmMessagePart[] {
  return parts.map((part) => (typeof part === 'string' ? confirmText(part) : part));
}
