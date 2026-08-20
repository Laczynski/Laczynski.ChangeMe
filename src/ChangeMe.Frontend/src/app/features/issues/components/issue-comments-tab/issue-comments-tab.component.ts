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
import { form, FormField, FormRoot, maxLength, required } from '@angular/forms/signals';
import { ToastService } from '@core/toast/services/toast.service';
import { IssueCommentDto } from '@features/issues/models/issue.model';
import { IssuesService } from '@features/issues/services/issues.service';
import { IssueCommentConstraints } from '@features/issues/utils/issue.utils';
import { FormFieldComponent } from '@shared/components/form-field/form-field.component';
import {
  createIssueTabGridQuery,
  hasMoreGridItems
} from '@shared/data/utils/grid.utils';
import { whenTouched } from '@shared/forms/signal-forms.utils';
import { ButtonDirective } from 'primeng/button';
import { Message } from 'primeng/message';
import { ProgressSpinner } from 'primeng/progressspinner';
import { Textarea } from 'primeng/textarea';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-issue-comments-tab',
  imports: [
    DatePipe,
    FormField,
    FormFieldComponent,
    FormRoot,
    ButtonDirective,
    Textarea,
    Message,
    ProgressSpinner
  ],
  templateUrl: './issue-comments-tab.component.html',
  host: { class: 'block' }
})
export class IssueCommentsTabComponent {
  readonly issueId = input.required<string>();

  private readonly issuesService = inject(IssuesService);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly issueCommentConstraints = IssueCommentConstraints;

  readonly comments = signal<IssueCommentDto[]>([]);
  readonly commentsTotalCount = signal(0);
  readonly loadError = signal<string | null>(null);
  readonly commentError = signal<string | null>(null);
  readonly isSubmittingComment = signal(false);
  readonly isLoadingComments = signal(false);
  readonly isLoadingMoreComments = signal(false);
  readonly hasLoadedComments = signal(false);

  readonly canShowMoreComments = computed(() =>
    hasMoreGridItems(this.comments().length, this.commentsTotalCount())
  );

  readonly commentModel = signal({ content: '' });

  readonly commentForm = form(
    this.commentModel,
    (path) => {
      required(path.content, {
        when: whenTouched,
        message: 'Comment content is required'
      });
      maxLength(path.content, IssueCommentConstraints.CONTENT_MAX_LENGTH, {
        message: `Comment must be at most ${IssueCommentConstraints.CONTENT_MAX_LENGTH} characters`
      });
    },
    {
      submission: {
        action: async () => {
          this.commentError.set(null);
          this.isSubmittingComment.set(true);

          try {
            await firstValueFrom(
              this.issuesService.addComment(this.issueId(), {
                content: this.commentModel().content.trim()
              })
            );
            this.reloadCommentsFromStart(this.issueId());
            this.commentModel.set({ content: '' });
            this.commentForm().reset();
            this.toastService.success('Comment added');
          } catch (error) {
            this.commentError.set(
              error instanceof Error ? error.message : 'Add comment failed.'
            );
          } finally {
            this.isSubmittingComment.set(false);
          }
        }
      }
    }
  );

  private lastLoadedIssueId: string | null = null;
  private commentsRequestId = 0;

  constructor() {
    effect(() => {
      const issueId = this.issueId();
      if (issueId === this.lastLoadedIssueId) {
        return;
      }

      this.lastLoadedIssueId = issueId;
      this.commentError.set(null);
      this.reloadCommentsFromStart(issueId);
    });
  }

  showMoreComments(): void {
    if (!this.canShowMoreComments() || this.isLoadingMoreComments()) {
      return;
    }

    this.loadComments(this.issueId(), {
      append: true,
      skip: this.comments().length
    });
  }

  private reloadCommentsFromStart(issueId: string): void {
    this.loadComments(issueId);
  }

  private loadComments(
    issueId: string,
    options: { append?: boolean; skip?: number } = {}
  ): void {
    const append = options.append ?? false;
    const skip = options.skip ?? 0;
    const requestId = ++this.commentsRequestId;

    if (append) {
      this.isLoadingMoreComments.set(true);
    } else {
      this.isLoadingComments.set(true);
    }

    this.loadError.set(null);

    this.issuesService
      .getIssueComments(issueId, createIssueTabGridQuery(skip))
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          if (requestId !== this.commentsRequestId) {
            return;
          }

          if (append) {
            this.comments.update((items) => [...items, ...result.items]);
          } else {
            this.comments.set(result.items);
          }

          this.commentsTotalCount.set(result.totalCount);
          this.isLoadingComments.set(false);
          this.isLoadingMoreComments.set(false);
          this.hasLoadedComments.set(true);
        },
        error: (error: Error) => {
          if (requestId !== this.commentsRequestId) {
            return;
          }

          this.loadError.set(error.message);
          this.isLoadingComments.set(false);
          this.isLoadingMoreComments.set(false);
        }
      });
  }
}
