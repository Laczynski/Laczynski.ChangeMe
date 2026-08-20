import { CommonModule } from '@angular/common';
import { Component, DestroyRef, effect, inject, input, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import {
  applyEach,
  form,
  FormField,
  FormRoot,
  maxLength,
  minLength,
  required
} from '@angular/forms/signals';
import { Router } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import {
  IssueAssignableUserDto,
  IssueDetailsDto,
  IssuePriority,
  IssueStatus,
  UpdateIssueRequest
} from '@features/issues/models/issue.model';
import { IssuesService } from '@features/issues/services/issues.service';
import {
  IssueAcceptanceCriteriaConstraints,
  IssueConstraints,
  issuePriorities,
  issueStatuses
} from '@features/issues/utils/issue.utils';
import { BackButtonComponent } from '@shared/components/back-button/back-button.component';
import { FormFieldComponent } from '@shared/components/form-field/form-field.component';
import { whenTouched } from '@shared/forms/signal-forms.utils';
import { ButtonDirective } from 'primeng/button';
import { Card } from 'primeng/card';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { Panel } from 'primeng/panel';
import { ProgressSpinner } from 'primeng/progressspinner';
import { Select } from 'primeng/select';
import { Textarea } from 'primeng/textarea';
import { firstValueFrom } from 'rxjs';

type EditIssueFormModel = {
  title: string;
  description: string;
  status: IssueStatus;
  priority: IssuePriority;
  assignedToUserId: string | null;
  acceptanceCriteria: { id: string; content: string }[];
};

@Component({
  selector: 'app-edit-issue',
  imports: [
    CommonModule,
    FormField,
    FormFieldComponent,
    FormRoot,
    FormsModule,
    Card,
    BackButtonComponent,
    ButtonDirective,
    InputText,
    Textarea,
    Select,
    Message,
    Panel,
    ProgressSpinner
  ],
  templateUrl: './edit-issue.component.html'
})
export class EditIssueComponent {
  readonly id = input<string>();

  private readonly issuesService = inject(IssuesService);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly issuePriorities = issuePriorities;
  readonly issueStatuses = issueStatuses;
  readonly issueConstraints = IssueConstraints;
  readonly issueAcceptanceCriteriaConstraints = IssueAcceptanceCriteriaConstraints;
  readonly assignableUsers = signal<IssueAssignableUserDto[]>([]);
  readonly issue = signal<IssueDetailsDto | null>(null);
  readonly isLoadingIssue = signal(true);
  readonly isLoadingAssignableUsers = signal(true);
  readonly isSubmitting = signal(false);
  readonly loadError = signal<string | null>(null);
  readonly submitError = signal<string | null>(null);

  readonly issueModel = signal<EditIssueFormModel>({
    title: '',
    description: '',
    status: IssueStatus.NEW,
    priority: IssuePriority.MEDIUM,
    assignedToUserId: null,
    acceptanceCriteria: []
  });

  readonly issueForm = form(
    this.issueModel,
    (path) => {
      required(path.title, { when: whenTouched, message: 'Title is required' });
      minLength(path.title, IssueConstraints.TITLE_MIN_LENGTH, {
        when: whenTouched,
        message: `Title must be at least ${IssueConstraints.TITLE_MIN_LENGTH} characters`
      });
      maxLength(path.title, IssueConstraints.TITLE_MAX_LENGTH, {
        message: `Title must be at most ${IssueConstraints.TITLE_MAX_LENGTH} characters`
      });

      required(path.description, {
        when: whenTouched,
        message: 'Description is required'
      });
      maxLength(path.description, IssueConstraints.DESCRIPTION_MAX_LENGTH, {
        message: `Description must be at most ${IssueConstraints.DESCRIPTION_MAX_LENGTH} characters`
      });

      required(path.status, { when: whenTouched, message: 'Status is required' });
      required(path.priority, { when: whenTouched, message: 'Priority is required' });

      applyEach(path.acceptanceCriteria, (item) => {
        required(item.content, {
          when: whenTouched,
          message: 'Acceptance criterion is required'
        });
        maxLength(item.content, IssueAcceptanceCriteriaConstraints.CONTENT_MAX_LENGTH, {
          message: `Acceptance criterion must be at most ${IssueAcceptanceCriteriaConstraints.CONTENT_MAX_LENGTH} characters`
        });
      });
    },
    {
      submission: {
        action: async () => {
          this.submitError.set(null);

          const id = this.id();
          const version = this.issue()?.version;
          if (!id || version === undefined) {
            return;
          }

          const model = this.issueModel();
          const request: UpdateIssueRequest = {
            id,
            version,
            title: model.title.trim(),
            description: model.description.trim(),
            status: model.status,
            priority: model.priority,
            assignedToUserId: model.assignedToUserId,
            acceptanceCriteria: model.acceptanceCriteria.map((criterion) => ({
              id: criterion.id || undefined,
              content: criterion.content.trim()
            }))
          };

          this.isSubmitting.set(true);

          try {
            const issue = await firstValueFrom(this.issuesService.updateIssue(request));
            this.toastService.success('Issue saved', issue.title);
            void this.router.navigate(['/issues', issue.id]);
          } catch (error) {
            this.submitError.set(
              error instanceof Error ? error.message : 'Save failed.'
            );
          } finally {
            this.isSubmitting.set(false);
          }
        }
      }
    }
  );

  constructor() {
    this.issuesService
      .getAssignableUsers()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (users) => {
          this.assignableUsers.set(users);
          this.isLoadingAssignableUsers.set(false);
        },
        error: () => {
          this.isLoadingAssignableUsers.set(false);
        }
      });

    effect(() => {
      const issueId = this.id();
      if (!issueId) {
        return;
      }
      this.loadIssue(issueId);
    });
  }

  private loadIssue(issueId: string): void {
    if (!issueId) {
      this.isLoadingIssue.set(false);
      return;
    }

    this.isLoadingIssue.set(true);
    this.loadError.set(null);
    this.submitError.set(null);

    this.issuesService
      .getIssue(issueId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (issue) => {
          this.issue.set(issue);
          this.issueModel.set({
            title: issue.title,
            description: issue.description,
            status: issue.status,
            priority: issue.priority,
            assignedToUserId: issue.assignedToUserId ?? null,
            acceptanceCriteria: issue.acceptanceCriteria.map((criterion) => ({
              id: criterion.id,
              content: criterion.content
            }))
          });
          this.isLoadingIssue.set(false);
        },
        error: (error: Error) => {
          this.loadError.set(error.message);
          this.isLoadingIssue.set(false);
        }
      });
  }

  refresh(): void {
    const issueId = this.id();
    if (!issueId) {
      return;
    }
    this.loadIssue(issueId);
  }

  addAcceptanceCriterion(): void {
    this.issueModel.update((model) => ({
      ...model,
      acceptanceCriteria: [...model.acceptanceCriteria, { id: '', content: '' }]
    }));
  }

  removeAcceptanceCriterion(index: number): void {
    this.issueModel.update((model) => ({
      ...model,
      acceptanceCriteria: model.acceptanceCriteria.filter((_, i) => i !== index)
    }));
  }

  setStatus(status: IssueStatus): void {
    this.issueForm.status().value.set(status);
  }

  setPriority(priority: IssuePriority): void {
    this.issueForm.priority().value.set(priority);
  }

  setAssignedToUserId(userId: string | null): void {
    this.issueForm.assignedToUserId().value.set(userId);
  }

  cancel(): void {
    const issueId = this.id();
    if (!issueId) {
      void this.router.navigate(['/issues']);
      return;
    }

    void this.router.navigate(['/issues', issueId]);
  }
}
