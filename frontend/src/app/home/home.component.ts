import { Component } from '@angular/core';
import { RegisterComponent } from '../register/register.component';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [RegisterComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css',
})
export class HomeComponent {
  registerMode = false;
  users: any;

  registerToggle() {
    this.registerMode = !this.registerMode;
  }

  onCancel() {
    this.registerMode = false;
  }
}
