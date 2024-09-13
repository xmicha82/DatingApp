import { Component, EventEmitter, inject, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AccountService } from '../_services/account.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css',
})
export class RegisterComponent {
  private accountService = inject(AccountService);
  private toastrService = inject(ToastrService);
  model: any = {};
  @Output() cancel = new EventEmitter();

  register() {
    this.accountService.register(this.model).subscribe({
      next: (response) => {
        console.log(response);
        this.onCancel();
      },
      error: (err) => this.toastrService.error(err.error),
    });
  }

  onCancel() {
    this.cancel.emit();
  }
}
