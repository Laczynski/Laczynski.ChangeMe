import { Injectable, inject } from '@angular/core';
import { GridQuery, GridResult } from '@laczynski/datagrid';
import { ApiService } from '@shared/api/services/api.service';
import { Observable } from 'rxjs';
import {
  AddIssueCommentRequest,
  CreateIssueRequest,
  IssueAssignableUserDto,
  IssueAttachmentDto,
  IssueCommentDto,
  IssueDetailsDto,
  IssueDto,
  IssueHistoryEntryDto,
  IssueWatchStateDto,
  UpdateIssueRequest
} from '../models/issue.model';

@Injectable({
  providedIn: 'root'
})
export class IssuesService {
  private readonly apiService = inject(ApiService);
  private readonly baseEndpoint = 'issues';

  getAllIssues(grid: GridQuery): Observable<GridResult<IssueDto>> {
    return this.apiService.get<GridResult<IssueDto>>(this.baseEndpoint, { grid });
  }

  getIssue(id: string): Observable<IssueDetailsDto> {
    return this.apiService.get<IssueDetailsDto>(`${this.baseEndpoint}/${id}`);
  }

  getIssueComments(
    issueId: string,
    grid: GridQuery
  ): Observable<GridResult<IssueCommentDto>> {
    return this.apiService.get<GridResult<IssueCommentDto>>(
      `${this.baseEndpoint}/${issueId}/comments`,
      { grid }
    );
  }

  getIssueHistory(
    issueId: string,
    grid: GridQuery
  ): Observable<GridResult<IssueHistoryEntryDto>> {
    return this.apiService.get<GridResult<IssueHistoryEntryDto>>(
      `${this.baseEndpoint}/${issueId}/history`,
      { grid }
    );
  }

  getIssueAttachments(
    issueId: string,
    grid: GridQuery
  ): Observable<GridResult<IssueAttachmentDto>> {
    return this.apiService.get<GridResult<IssueAttachmentDto>>(
      `${this.baseEndpoint}/${issueId}/attachments`,
      { grid }
    );
  }

  uploadIssueAttachment(issueId: string, file: File): Observable<IssueAttachmentDto> {
    const formData = new FormData();
    formData.append('File', file, file.name);

    return this.apiService.postFormData<IssueAttachmentDto>(
      `${this.baseEndpoint}/${issueId}/attachments`,
      formData
    );
  }

  downloadIssueAttachment(issueId: string, attachmentId: string): Observable<Blob> {
    return this.apiService.getBlob(
      `${this.baseEndpoint}/${issueId}/attachments/${attachmentId}/content`
    );
  }

  deleteIssueAttachment(issueId: string, attachmentId: string): Observable<string> {
    return this.apiService.delete<string>(
      `${this.baseEndpoint}/${issueId}/attachments/${attachmentId}`
    );
  }

  getAssignableUsers(): Observable<IssueAssignableUserDto[]> {
    return this.apiService.get<IssueAssignableUserDto[]>(
      `${this.baseEndpoint}/assignable-users`
    );
  }

  createIssue(request: CreateIssueRequest): Observable<IssueDetailsDto> {
    return this.apiService.post<IssueDetailsDto>(this.baseEndpoint, request);
  }

  updateIssue(request: UpdateIssueRequest): Observable<IssueDetailsDto> {
    return this.apiService.put<IssueDetailsDto>(
      `${this.baseEndpoint}/${request.id}`,
      request
    );
  }

  addComment(
    issueId: string,
    request: AddIssueCommentRequest
  ): Observable<IssueDetailsDto> {
    return this.apiService.post<IssueDetailsDto>(
      `${this.baseEndpoint}/${issueId}/comments`,
      request
    );
  }

  watchIssue(issueId: string): Observable<IssueWatchStateDto> {
    return this.apiService.post<IssueWatchStateDto>(
      `${this.baseEndpoint}/${issueId}/watch`,
      {}
    );
  }

  unwatchIssue(issueId: string): Observable<IssueWatchStateDto> {
    return this.apiService.delete<IssueWatchStateDto>(
      `${this.baseEndpoint}/${issueId}/watch`
    );
  }

  deleteIssue(id: string): Observable<string> {
    return this.apiService.delete<string>(`${this.baseEndpoint}/${id}`);
  }
}
