import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { Member } from '../_models/member';
import { of, tap } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class MembersService {
  private httpClient = inject(HttpClient);
  baseUrl = environment.apiUrl;
  members = signal<Member[]>([]);

  getMembers() {
    return this.httpClient.get<Member[]>(`${this.baseUrl}/users`).subscribe({
      next: (members) => this.members.set(members),
    });
  }

  getMember(username: string) {
    const member = this.members().find((x) => x.username === username);
    if (member) return of(member);

    return this.httpClient.get<Member>(`${this.baseUrl}/users/${username}`);
  }

  updateMember(member: Member) {
    return this.httpClient.put(`${this.baseUrl}/users`, member).pipe(
      tap(() => {
        this.members.update((members) =>
          members.map((m) => (m.username === member.username ? member : m))
        );
      })
    );
  }
}
