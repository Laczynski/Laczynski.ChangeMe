import { Component, DestroyRef, inject, signal } from '@angular/core';
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
  CreateIssueRequest,
  IssueAssignableUserDto,
  IssuePriority,
  IssueStatus
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
import { Checkbox } from 'primeng/checkbox';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { Panel } from 'primeng/panel';
import { Select } from 'primeng/select';
import { Textarea } from 'primeng/textarea';
import { firstValueFrom } from 'rxjs';

type CreateIssueFormModel = {
  title: string;
  description: string;
  status: IssueStatus;
  priority: IssuePriority;
  assignedToUserId: string | null;
  watchAfterCreate: boolean;
  acceptanceCriteria: { content: string }[];
};

@Component({
  selector: 'app-create-issue',
  imports: [
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
    Checkbox,
    Message,
    Panel
  ],
  templateUrl: './create-issue.component.html'
})
export class CreateIssueComponent {
  private readonly issuesService = inject(IssuesService);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly issuePriorities = issuePriorities;
  readonly issueStatuses = issueStatuses;
  readonly issueConstraints = IssueConstraints;
  readonly issueAcceptanceCriteriaConstraints = IssueAcceptanceCriteriaConstraints;
  readonly assignableUsers = signal<IssueAssignableUserDto[]>([]);
  readonly isLoadingAssignableUsers = signal(true);
  readonly isSubmitting = signal(false);
  readonly submitError = signal<string | null>(null);

  readonly issueModel = signal<CreateIssueFormModel>({
    title: '',
    description: '',
    status: IssueStatus.NEW,
    priority: IssuePriority.MEDIUM,
    assignedToUserId: null,
    watchAfterCreate: true,
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
          this.isSubmitting.set(true);

          const model = this.issueModel();
          const request: CreateIssueRequest = {
            title: model.title.trim(),
            description: model.description.trim(),
            status: model.status,
            priority: model.priority,
            assignedToUserId: model.assignedToUserId,
            watchAfterCreate: model.watchAfterCreate,
            acceptanceCriteria: model.acceptanceCriteria.map((criterion) => ({
              content: criterion.content.trim()
            }))
          };

          try {
            const issue = await firstValueFrom(this.issuesService.createIssue(request));
            this.toastService.success('Issue created', issue.title);
            void this.router.navigate(['/issues', issue.id]);
          } catch (error) {
            this.submitError.set(
              error instanceof Error ? error.message : 'Create failed.'
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
  }

  addAcceptanceCriterion(): void {
    this.issueModel.update((model) => ({
      ...model,
      acceptanceCriteria: [...model.acceptanceCriteria, { content: '' }]
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
    void this.router.navigate(['/issues']);
  }
}
