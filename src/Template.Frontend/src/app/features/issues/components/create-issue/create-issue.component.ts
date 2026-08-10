import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  FormArray,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
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
import { NgIcon, provideIcons } from '@ng-icons/core';
import { lucideCheck, lucideLoader2, lucidePlus, lucideTrash } from '@ng-icons/lucide';
import { BackButtonComponent } from '@shared/components/back-button/back-button.component';
import { HlmAlertImports } from '@spartan/ui/alert';
import { HlmButtonImports } from '@spartan/ui/button';
import { HlmCardImports } from '@spartan/ui/card';
import { HlmCheckboxImports } from '@spartan/ui/checkbox';
import { HlmFieldImports } from '@spartan/ui/field';
import { HlmInputImports } from '@spartan/ui/input';
import { HlmSelectImports } from '@spartan/ui/select';
import { HlmTextareaImports } from '@spartan/ui/textarea';

type CreateIssueForm = {
  title: FormControl<string>;
  description: FormControl<string>;
  status: FormControl<IssueStatus>;
  priority: FormControl<IssuePriority>;
  assignedToUserId: FormControl<string | null>;
  watchAfterCreate: FormControl<boolean>;
  acceptanceCriteria: FormArray<FormGroup<AcceptanceCriterionForm>>;
};

type AcceptanceCriterionForm = {
  content: FormControl<string>;
};

@Component({
  selector: 'app-create-issue',
  imports: [
    ReactiveFormsModule,
    NgIcon,
    ...HlmCardImports,
    ...HlmButtonImports,
    ...HlmFieldImports,
    ...HlmInputImports,
    ...HlmTextareaImports,
    ...HlmSelectImports,
    ...HlmCheckboxImports,
    ...HlmAlertImports,
    BackButtonComponent
  ],
  providers: [provideIcons({ lucideCheck, lucideLoader2, lucidePlus, lucideTrash })],
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

  readonly form = new FormGroup<CreateIssueForm>({
    title: new FormControl('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.minLength(IssueConstraints.TITLE_MIN_LENGTH),
        Validators.maxLength(IssueConstraints.TITLE_MAX_LENGTH)
      ]
    }),
    description: new FormControl('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.maxLength(IssueConstraints.DESCRIPTION_MAX_LENGTH)
      ]
    }),
    status: new FormControl(IssueStatus.NEW, {
      nonNullable: true,
      validators: [Validators.required]
    }),
    priority: new FormControl(IssuePriority.MEDIUM, {
      nonNullable: true,
      validators: [Validators.required]
    }),
    assignedToUserId: new FormControl<string | null>(null),
    watchAfterCreate: new FormControl(true, { nonNullable: true }),
    acceptanceCriteria: new FormArray<FormGroup<AcceptanceCriterionForm>>([])
  });

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
    this.form.controls.acceptanceCriteria.push(this.createAcceptanceCriterionGroup());
  }

  removeAcceptanceCriterion(index: number): void {
    this.form.controls.acceptanceCriteria.removeAt(index);
  }

  cancel(): void {
    void this.router.navigate(['/issues']);
  }

  onSubmit(): void {
    this.submitError.set(null);

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const request: CreateIssueRequest = {
      title: this.form.controls.title.value.trim(),
      description: this.form.controls.description.value.trim(),
      status: this.form.controls.status.value,
      priority: this.form.controls.priority.value,
      assignedToUserId: this.form.controls.assignedToUserId.value,
      watchAfterCreate: this.form.controls.watchAfterCreate.value,
      acceptanceCriteria: this.form.controls.acceptanceCriteria.controls.map(
        (criterion) => ({
          content: criterion.controls.content.value.trim()
        })
      )
    };

    this.isSubmitting.set(true);

    this.issuesService
      .createIssue(request)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (issue) => {
          this.isSubmitting.set(false);
          this.toastService.success('Issue created', issue.title);
          void this.router.navigate(['/issues', issue.id]);
        },
        error: (error: Error) => {
          this.submitError.set(error.message);
          this.isSubmitting.set(false);
        }
      });
  }

  onStatusChange(value: IssueStatus | null | undefined): void {
    if (value !== null && value !== undefined) {
      this.form.controls.status.setValue(value);
    }
  }

  onPriorityChange(value: IssuePriority | null | undefined): void {
    if (value !== null && value !== undefined) {
      this.form.controls.priority.setValue(value);
    }
  }

  onAssignedToChange(value: string | null | undefined): void {
    this.form.controls.assignedToUserId.setValue(value ?? null);
  }

  private createAcceptanceCriterionGroup(
    content = ''
  ): FormGroup<AcceptanceCriterionForm> {
    return new FormGroup({
      content: new FormControl(content, {
        nonNullable: true,
        validators: [
          Validators.required,
          Validators.maxLength(IssueAcceptanceCriteriaConstraints.CONTENT_MAX_LENGTH)
        ]
      })
    });
  }
}
