export type { ConfirmMessageContent, ConfirmMessagePart } from './models/confirm-message.model';
export { isConfirmMessageParts } from './models/confirm-message.model';
export { ConfirmService, type ConfirmRequest } from './services/confirm.service';
export {
  confirmMessage,
  confirmStrong,
  confirmText
} from './utils/confirm-message.utils';
