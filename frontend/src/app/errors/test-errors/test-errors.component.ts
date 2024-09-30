import { HttpClient } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-test-errors',
  standalone: true,
  imports: [],
  templateUrl: './test-errors.component.html',
  styleUrl: './test-errors.component.css',
})
export class TestErrorsComponent {
  baseUrl = environment.apiUrl;
  private httpClient = inject(HttpClient);
  validationErrors: string[] = [];

  get400() {
    this.httpClient.get(`${this.baseUrl}/buggy/bad-request`).subscribe({
      next: (response) => console.log(response),
      error: (err) => console.error(err),
    });
  }

  get401() {
    this.httpClient.get(`${this.baseUrl}/buggy/auth`).subscribe({
      next: (response) => console.log(response),
      error: (err) => console.error(err),
    });
  }

  get404() {
    this.httpClient.get(`${this.baseUrl}/buggy/not-found`).subscribe({
      next: (response) => console.log(response),
      error: (err) => console.error(err),
    });
  }

  get500() {
    this.httpClient.get(`${this.baseUrl}/buggy/server-error`).subscribe({
      next: (response) => console.log(response),
      error: (err) => console.error(err),
    });
  }
  get400Validation() {
    this.httpClient.post(`${this.baseUrl}/account/register`, {}).subscribe({
      next: (response) => console.log(response),
      error: (err) => {
        console.error('400Validation', err);
        this.validationErrors = err;
      },
    });
  }
}
