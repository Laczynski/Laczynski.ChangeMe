export type ConfirmDialogAcceptVariant = 'default' | 'destructive';

export type ConfirmDialogRequest = {
  header: string;
  message: string;
  acceptLabel?: string;
  rejectLabel?: string;
  acceptVariant?: ConfirmDialogAcceptVariant;
  accept?: () => void;
  reject?: () => void;
};
