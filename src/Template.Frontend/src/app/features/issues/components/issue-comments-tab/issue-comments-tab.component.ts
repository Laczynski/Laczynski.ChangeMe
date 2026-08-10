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
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { ToastService } from '@core/toast/services/toast.service';
import { IssueCommentDto } from '@features/issues/models/issue.model';
import { IssuesService } from '@features/issues/services/issues.service';
import { IssueCommentConstraints } from '@features/issues/utils/issue.utils';
import {
  createIssueTabGridQuery,
  hasMoreGridItems
} from '@shared/data/utils/grid.utils';
import { ButtonDirective } from 'primeng/button';
import { Message } from 'primeng/message';
import { ProgressSpinner } from 'primeng/progressspinner';
import { Textarea } from 'primeng/textarea';

type CommentForm = {
  content: FormControl<string>;
};

@Component({
  selector: 'app-issue-comments-tab',
  imports: [
    DatePipe,
    ReactiveFormsModule,
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
  readonly isSubmitted = signal(false);
  readonly isSubmittingComment = signal(false);
  readonly isLoadingComments = signal(false);
  readonly isLoadingMoreComments = signal(false);
  readonly hasLoadedComments = signal(false);

  readonly canShowMoreComments = computed(() =>
    hasMoreGridItems(this.comments().length, this.commentsTotalCount())
  );

  readonly commentForm = new FormGroup<CommentForm>({
    content: new FormControl('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.maxLength(IssueCommentConstraints.CONTENT_MAX_LENGTH)
      ]
    })
  });

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

  shouldShowCommentError(): boolean {
    return (
      !!this.commentForm.controls.content.errors &&
      (this.commentForm.controls.content.touched || this.isSubmitted())
    );
  }

  addComment(): void {
    this.isSubmitted.set(true);
    this.commentError.set(null);

    if (this.commentForm.invalid) {
      this.commentForm.markAllAsTouched();
      return;
    }

    this.isSubmittingComment.set(true);

    this.issuesService
      .addComment(this.issueId(), {
        content: this.commentForm.controls.content.value.trim()
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.reloadCommentsFromStart(this.issueId());
          this.commentForm.reset({ content: '' });
          this.commentForm.markAsPristine();
          this.commentForm.markAsUntouched();
          this.isSubmitted.set(false);
          this.isSubmittingComment.set(false);
          this.toastService.success('Comment added');
        },
        error: (error: Error) => {
          this.commentError.set(error.message);
          this.isSubmittingComment.set(false);
        }
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
