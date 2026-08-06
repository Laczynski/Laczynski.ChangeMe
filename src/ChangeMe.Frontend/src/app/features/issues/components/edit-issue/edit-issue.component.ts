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
  FormArray,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
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
  IssueFieldErrors,
  issuePriorities,
  issueStatuses
} from '@features/issues/utils/issue.utils';
import {
  AccordionComponent,
  ButtonComponent,
  CardComponent,
  MessageBarComponent,
  SelectComponent,
  SpinnerComponent,
  TextareaComponent,
  TextComponent
} from '@laczynski/ui';
import { BackButtonComponent } from '@shared/components/back-button/back-button.component';
import { DefaultExpandedAccordionDirective } from '@shared/directives/default-expanded-accordion.directive';
import { fieldError } from '@shared/forms/field-error';

type EditIssueForm = {
  title: FormControl<string>;
  description: FormControl<string>;
  status: FormControl<IssueStatus>;
  priority: FormControl<IssuePriority>;
  assignedToUserId: FormControl<string | null>;
  acceptanceCriteria: FormArray<FormGroup<EditAcceptanceCriterionForm>>;
};

type EditAcceptanceCriterionForm = {
  id: FormControl<string>;
  content: FormControl<string>;
};

@Component({
  selector: 'app-edit-issue',
  imports: [
    DatePipe,
    ReactiveFormsModule,
    BackButtonComponent,
    ButtonComponent,
    TextComponent,
    TextareaComponent,
    SelectComponent,
    MessageBarComponent,
    AccordionComponent,
    CardComponent,
    DefaultExpandedAccordionDirective,
    SpinnerComponent
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
  readonly submitted = signal(false);
  protected readonly fieldError = fieldError;
  protected readonly IssueFieldErrors = IssueFieldErrors;

  readonly statusItems = computed(() =>
    this.issueStatuses().map((item) => ({ value: item.value, label: item.label }))
  );
  readonly priorityItems = computed(() =>
    this.issuePriorities().map((item) => ({ value: item.value, label: item.label }))
  );
  readonly assignableUserItems = computed(() =>
    this.assignableUsers().map((user) => ({
      value: user.id,
      label: user.displayLabel
    }))
  );

  readonly form = new FormGroup<EditIssueForm>({
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
    acceptanceCriteria: new FormArray<FormGroup<EditAcceptanceCriterionForm>>([])
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
          this.form.setValue({
            title: issue.title,
            description: issue.description,
            status: issue.status,
            priority: issue.priority,
            assignedToUserId: issue.assignedToUserId ?? null,
            acceptanceCriteria: []
          });
          this.setAcceptanceCriteria(issue);
          this.form.markAsPristine();
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
    this.form.controls.acceptanceCriteria.push(this.createAcceptanceCriterionGroup());
  }

  removeAcceptanceCriterion(index: number): void {
    this.form.controls.acceptanceCriteria.removeAt(index);
  }

  cancel(): void {
    const issueId = this.id();
    if (!issueId) {
      void this.router.navigate(['/issues']);
      return;
    }

    void this.router.navigate(['/issues', issueId]);
  }

  onSubmit(): void {
    this.submitError.set(null);
    this.submitted.set(true);

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const id = this.id();
    const version = this.issue()?.version;
    if (!id || version === undefined) {
      return;
    }

    const request: UpdateIssueRequest = {
      id,
      version,
      title: this.form.controls.title.value.trim(),
      description: this.form.controls.description.value.trim(),
      status: this.form.controls.status.value,
      priority: this.form.controls.priority.value,
      assignedToUserId: this.form.controls.assignedToUserId.value,
      acceptanceCriteria: this.form.controls.acceptanceCriteria.controls.map(
        (criterion) => ({
          id: criterion.controls.id.value || undefined,
          content: criterion.controls.content.value.trim()
        })
      )
    };

    this.isSubmitting.set(true);

    this.issuesService
      .updateIssue(request)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (issue) => {
          this.isSubmitting.set(false);
          this.toastService.success('Issue saved', issue.title);
          void this.router.navigate(['/issues', issue.id]);
        },
        error: (error: Error) => {
          this.submitError.set(error.message);
          this.isSubmitting.set(false);
        }
      });
  }

  private setAcceptanceCriteria(issue: IssueDetailsDto): void {
    this.form.controls.acceptanceCriteria.clear();

    issue.acceptanceCriteria.forEach((criterion) => {
      this.form.controls.acceptanceCriteria.push(
        this.createAcceptanceCriterionGroup(criterion.id, criterion.content)
      );
    });
  }

  private createAcceptanceCriterionGroup(
    id = '',
    content = ''
  ): FormGroup<EditAcceptanceCriterionForm> {
    return new FormGroup({
      id: new FormControl(id, { nonNullable: true }),
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
