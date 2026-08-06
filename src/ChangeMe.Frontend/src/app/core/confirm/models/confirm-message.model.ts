export type ConfirmMessagePart =
  | { type: 'text'; value: string }
  | { type: 'strong'; value: string };

export type ConfirmMessageContent = string | ConfirmMessagePart[];

export function isConfirmMessageParts(
  message: ConfirmMessageContent | undefined
): message is ConfirmMessagePart[] {
  return Array.isArray(message);
}
