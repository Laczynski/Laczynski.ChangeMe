import { DatePipe } from '@angular/common';
import {
  Component,
  computed,
  DestroyRef,
  effect,
  inject,
  input,
  signal
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  ButtonComponent,
  FileComponent,
  MessageBarComponent,
  SpinnerComponent
} from '@laczynski/ui';
import { ConfirmService } from '@core/confirm/services/confirm.service';
import { ToastService } from '@core/toast/services/toast.service';
import { IssueAttachmentDto } from '@features/issues/models/issue.model';
import { IssuesService } from '@features/issues/services/issues.service';
import {
  getDeleteAttachmentConfirmMessage,
  issueAttachmentAccept,
  IssueConstraints
} from '@features/issues/utils/issue.utils';
import {
  createIssueTabGridQuery,
  hasMoreGridItems
} from '@shared/data/utils/grid.utils';
import { formatFileSize } from '@shared/data/utils/file-size.utils';

@Component({
  selector: 'app-issue-attachments-tab',
  imports: [DatePipe, ButtonComponent, FileComponent, MessageBarComponent, SpinnerComponent],
  templateUrl: './issue-attachments-tab.component.html',
  host: { class: 'block' }
})
export class IssueAttachmentsTabComponent {
  readonly issueId = input.required<string>();

  private readonly issuesService = inject(IssuesService);
  private readonly toastService = inject(ToastService);
  private readonly confirmService = inject(ConfirmService);
  private readonly destroyRef = inject(DestroyRef);

  readonly IssueConstraints = IssueConstraints;
  readonly issueAttachmentAccept = issueAttachmentAccept;
  readonly formatFileSize = formatFileSize;

  readonly attachments = signal<IssueAttachmentDto[]>([]);
  readonly attachmentsTotalCount = signal(0);
  readonly loadError = signal<string | null>(null);
  readonly uploadError = signal<string | null>(null);
  readonly selectedFile = signal<File | null>(null);
  readonly isUploading = signal(false);
  readonly isLoadingAttachments = signal(false);
  readonly isLoadingMoreAttachments = signal(false);
  readonly hasLoadedAttachments = signal(false);
  readonly downloadingAttachmentId = signal<string | null>(null);
  readonly deletingAttachmentId = signal<string | null>(null);

  readonly canShowMoreAttachments = computed(() =>
    hasMoreGridItems(this.attachments().length, this.attachmentsTotalCount())
  );

  private lastLoadedIssueId: string | null = null;
  private attachmentsRequestId = 0;

  constructor() {
    effect(() => {
      const issueId = this.issueId();
      if (issueId === this.lastLoadedIssueId) {
        return;
      }

      this.lastLoadedIssueId = issueId;
      this.uploadError.set(null);
      this.selectedFile.set(null);
      this.reloadAttachmentsFromStart(issueId);
    });
  }

  onFileSelected(files: File[]): void {
    this.uploadError.set(null);
    this.selectedFile.set(files[0] ?? null);
  }

  uploadAttachment(): void {
    const file = this.selectedFile();
    if (!file || this.isUploading()) {
      return;
    }

    if (file.size > IssueConstraints.ATTACHMENT_MAX_FILE_SIZE_BYTES) {
      this.uploadError.set(
        `File cannot exceed ${formatFileSize(IssueConstraints.ATTACHMENT_MAX_FILE_SIZE_BYTES)}.`
      );
      return;
    }

    this.isUploading.set(true);
    this.uploadError.set(null);

    this.issuesService
      .uploadIssueAttachment(this.issueId(), file)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.reloadAttachmentsFromStart(this.issueId());
          this.selectedFile.set(null);
          this.isUploading.set(false);
          this.toastService.success('Attachment uploaded');
        },
        error: (error: Error) => {
          this.uploadError.set(error.message);
          this.isUploading.set(false);
        }
      });
  }

  downloadAttachment(attachment: IssueAttachmentDto): void {
    if (this.downloadingAttachmentId()) {
      return;
    }

    this.downloadingAttachmentId.set(attachment.id);

    this.issuesService
      .downloadIssueAttachment(this.issueId(), attachment.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (blob) => {
          const url = URL.createObjectURL(blob);
          const anchor = document.createElement('a');
          anchor.href = url;
          anchor.download = attachment.originalFileName;
          document.body.appendChild(anchor);
          anchor.click();
          anchor.remove();
          globalThis.setTimeout(() => URL.revokeObjectURL(url), 0);
          this.downloadingAttachmentId.set(null);
        },
        error: (error: Error) => {
          this.toastService.showApiError(error, 'Could not download attachment');
          this.downloadingAttachmentId.set(null);
        }
      });
  }

  confirmDeleteAttachment(attachment: IssueAttachmentDto): void {
    if (!attachment.canDelete || this.deletingAttachmentId()) {
      return;
    }

    this.confirmService.confirm({
      header: 'Delete attachment',
      message: getDeleteAttachmentConfirmMessage(attachment.originalFileName),
      acceptLabel: 'Delete',
      rejectLabel: 'Cancel',
      acceptVariant: 'danger',
      accept: () => this.deleteAttachment(attachment.id)
    });
  }

  showMoreAttachments(): void {
    if (!this.canShowMoreAttachments() || this.isLoadingMoreAttachments()) {
      return;
    }

    this.loadAttachments(this.issueId(), {
      append: true,
      skip: this.attachments().length
    });
  }

  private deleteAttachment(attachmentId: string): void {
    this.deletingAttachmentId.set(attachmentId);

    this.issuesService
      .deleteIssueAttachment(this.issueId(), attachmentId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.reloadAttachmentsFromStart(this.issueId());
          this.deletingAttachmentId.set(null);
          this.toastService.success('Attachment deleted');
        },
        error: (error: Error) => {
          this.toastService.showApiError(error, 'Could not delete attachment');
          this.deletingAttachmentId.set(null);
        }
      });
  }

  private reloadAttachmentsFromStart(issueId: string): void {
    this.loadAttachments(issueId);
  }

  private loadAttachments(
    issueId: string,
    options: { append?: boolean; skip?: number } = {}
  ): void {
    const append = options.append ?? false;
    const skip = options.skip ?? 0;
    const requestId = ++this.attachmentsRequestId;

    if (append) {
      this.isLoadingMoreAttachments.set(true);
    } else {
      this.isLoadingAttachments.set(true);
    }

    this.loadError.set(null);

    this.issuesService
      .getIssueAttachments(issueId, createIssueTabGridQuery(skip))
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          if (requestId !== this.attachmentsRequestId) {
            return;
          }

          if (append) {
            this.attachments.update((items) => [...items, ...result.items]);
          } else {
            this.attachments.set(result.items);
          }

          this.attachmentsTotalCount.set(result.totalCount);
          this.isLoadingAttachments.set(false);
          this.isLoadingMoreAttachments.set(false);
          this.hasLoadedAttachments.set(true);
        },
        error: (error: Error) => {
          if (requestId !== this.attachmentsRequestId) {
            return;
          }

          this.loadError.set(error.message);
          this.isLoadingAttachments.set(false);
          this.isLoadingMoreAttachments.set(false);
        }
      });
  }
}
