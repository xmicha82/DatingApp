import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { PaginatedResult, Pagination } from '../_models/pagination';
import { Message } from '../_models/message';
import {
  setPaginatedResponse,
  setPaginationHeaders,
} from './pagination-helper';
import { Container as MessageContainer } from '../_types/container-type';

@Injectable({
  providedIn: 'root',
})
export class MessageService {
  baseUrl = environment.apiUrl;
  private httpClient = inject(HttpClient);
  paginatedResult = signal<PaginatedResult<Message[]> | null>(null);

  getMessages(
    pageNumber: number,
    pageSize: number,
    container: MessageContainer
  ) {
    let params = setPaginationHeaders(pageNumber, pageSize);

    params = params.append('container', container);

    return this.httpClient
      .get<Message[]>(`${this.baseUrl}/messages`, {
        observe: 'response',
        params,
      })
      .subscribe({
        next: (response) =>
          setPaginatedResponse(response, this.paginatedResult),
      });
  }

  getMessageThread(username: string) {
    return this.httpClient.get<Message[]>(
      `${this.baseUrl}/messages/threads/${username}`
    );
  }

  sendMessage(username: string, content: string) {
    return this.httpClient.post<Message>(`${this.baseUrl}/messages`, {
      recipientUsername: username,
      content,
    });
  }

  deleteMessage(id: number) {
    return this.httpClient.delete(`${this.baseUrl}/messages/${id}`);
  }
}
