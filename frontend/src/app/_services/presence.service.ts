import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
} from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { User } from '../_models/user';
import { take } from 'rxjs';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root',
})
export class PresenceService {
  hubsUrl = environment.hubsUrl;
  private hubConnection?: HubConnection;
  private toastr = inject(ToastrService);
  private router = inject(Router);
  onlineUsers = signal<string[]>([]);

  createHubConnection(user: User) {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(`${this.hubsUrl}/presence`, {
        accessTokenFactory: () => user.token,
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start().catch((error) => console.log(error));

    this.hubConnection.on('UserIsOnline', (username) => {
      this.onlineUsers.update((users) => [...users, username]);
    });

    this.hubConnection.on('UserIsOffline', (username) => {
      this.onlineUsers.update((users) => users.filter((u) => u !== username));
    });

    this.hubConnection.on('GetOnlineUsers', (usernames) =>
      this.onlineUsers.set(usernames)
    );

    this.hubConnection.on(
      'NewMessageReceived',
      (sender: { username: string; knownAs: string }) => {
        this.toastr
          .info(`${sender.knownAs} sent a message`)
          .onTap.pipe(take(1))
          .subscribe({
            next: () =>
              this.router.navigateByUrl(
                `/members/${sender.username}?tab=Messages`
              ),
          });
      }
    );
  }

  stopHubConnection() {
    console.log('stopping?');

    if (this.hubConnection?.state === HubConnectionState.Connected) {
      console.log('stopping');

      this.hubConnection.stop().catch((error) => console.log(error));
    }
  }
}
