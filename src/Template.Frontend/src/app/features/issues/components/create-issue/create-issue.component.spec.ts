import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import {
  IssueDetailsDto,
  IssuePriority,
  IssueStatus
} from '@features/issues/models/issue.model';
import { IssuesService } from '@features/issues/services/issues.service';
import { MessageService } from 'primeng/api';
import { of } from 'rxjs';
import { CreateIssueComponent } from './create-issue.component';

describe('CreateIssueComponent', () => {
  let fixture: ComponentFixture<CreateIssueComponent>;
  let component: CreateIssueComponent;
  let issuesService: {
    getAssignableUsers: ReturnType<typeof vi.fn>;
    createIssue: ReturnType<typeof vi.fn>;
  };

  beforeEach(async () => {
    issuesService = {
      getAssignableUsers: vi.fn(() => of([])),
      createIssue: vi.fn(() =>
        of({
          id: 'issue-1',
          title: 'Regression in filters'
        } as IssueDetailsDto)
      )
    };

    await TestBed.configureTestingModule({
      imports: [CreateIssueComponent],
      providers: [
        provideRouter([
          { path: 'issues/:id', component: CreateIssueComponent },
          { path: 'issues', component: CreateIssueComponent }
        ]),
        MessageService,
        ToastService,
        { provide: IssuesService, useValue: issuesService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(CreateIssueComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('does not submit when required fields are invalid', () => {
    component.form.patchValue({
      title: 'ab',
      description: ''
    });

    component.onSubmit();

    expect(issuesService.createIssue).not.toHaveBeenCalled();
    expect(component.form.controls.title.touched).toBe(true);
    expect(component.form.controls.description.touched).toBe(true);
  });

  it('submits trimmed values and navigates to issue details', () => {
    const router = TestBed.inject(Router);
    const navigateSpy = vi.spyOn(router, 'navigate').mockResolvedValue(true);

    component.form.patchValue({
      title: '  Regression in filters  ',
      description: '  Steps to reproduce  ',
      status: IssueStatus.NEW,
      priority: IssuePriority.HIGH,
      assignedToUserId: null,
      watchAfterCreate: false
    });

    component.onSubmit();

    expect(issuesService.createIssue).toHaveBeenCalledWith({
      title: 'Regression in filters',
      description: 'Steps to reproduce',
      status: IssueStatus.NEW,
      priority: IssuePriority.HIGH,
      assignedToUserId: null,
      watchAfterCreate: false,
      acceptanceCriteria: []
    });
    expect(navigateSpy).toHaveBeenCalledWith(['/issues', 'issue-1']);
  });

  it('includes trimmed acceptance criteria in the create request', () => {
    component.addAcceptanceCriterion();
    component.form.controls.acceptanceCriteria.at(0)?.patchValue({
      content: '  User can save filters  '
    });
    component.form.patchValue({
      title: 'Filter persistence',
      description: 'Filters should survive reload.'
    });

    component.onSubmit();

    expect(issuesService.createIssue).toHaveBeenCalledWith(
      expect.objectContaining({
        acceptanceCriteria: [{ content: 'User can save filters' }]
      })
    );
  });
});
