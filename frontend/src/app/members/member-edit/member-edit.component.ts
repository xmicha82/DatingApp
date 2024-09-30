import {
  Component,
  HostListener,
  inject,
  OnInit,
  ViewChild,
  viewChild,
} from '@angular/core';
import { AccountService } from '../../_services/account.service';
import { MembersService } from '../../_services/members.service';
import { Member } from '../../_models/member';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { FormsModule, NgForm } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-member-edit',
  standalone: true,
  imports: [TabsModule, FormsModule, CommonModule],
  templateUrl: './member-edit.component.html',
  styleUrl: './member-edit.component.css',
})
export class MemberEditComponent implements OnInit {
  private accountService = inject(AccountService);
  private membersService = inject(MembersService);
  private toastr = inject(ToastrService);
  @ViewChild('editForm') editForm?: NgForm;
  member?: Member;

  @HostListener('window:beforeunload', ['$event'])
  notify($event: any) {
    if (this.editForm?.dirty) {
      $event.returnValue = true;
    }
  }

  ngOnInit(): void {
    this.loadMember();
  }

  loadMember() {
    const user = this.accountService.currentUser();
    if (!user) return;
    this.membersService.getMember(user.username).subscribe({
      next: (member) => {
        this.member = member;
      },
    });
  }

  updateMember() {
    this.membersService.updateMember(this.editForm?.value).subscribe({
      next: (_) => {
        this.toastr.success('Changes saved successfully');
        this.editForm?.reset(this.member);
      },
    });
  }
}
