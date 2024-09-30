import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { Member } from '../_models/member';

@Injectable({
  providedIn: 'root',
})
export class MembersService {
  private httpClient = inject(HttpClient);
  baseUrl = environment.apiUrl;

  getMembers() {
    return this.httpClient.get<Member[]>(`${this.baseUrl}/users`);
  }

  getMember(username: string) {
    return this.httpClient.get<Member>(`${this.baseUrl}/users/${username}`);
  }
}
