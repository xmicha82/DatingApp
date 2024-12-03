import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { LikesService } from '../_services/likes.service';
import { LikePredicate } from '../_types/like-predicate';
import { FormsModule } from '@angular/forms';
import { ButtonsModule } from 'ngx-bootstrap/buttons';
import { MemberCardComponent } from '../members/member-card/member-card.component';
import { PaginationModule } from 'ngx-bootstrap/pagination';

@Component({
  selector: 'app-lists',
  standalone: true,
  imports: [FormsModule, ButtonsModule, MemberCardComponent, PaginationModule],
  templateUrl: './lists.component.html',
  styleUrl: './lists.component.css',
})
export class ListsComponent implements OnInit, OnDestroy {
  likesService = inject(LikesService);
  predicate: LikePredicate = 'liked';
  pageNumber = 0;
  pageSize = 5;

  ngOnInit(): void {
    this.loadLikes();
  }

  loadLikes() {
    this.likesService.getLikes(this.predicate, this.pageNumber, this.pageSize);
  }

  pageChanged(event: any) {
    if (this.pageNumber !== event.page) {
      this.pageNumber = event.page;
      this.loadLikes();
    }
  }

  getTitle() {
    switch (this.predicate) {
      case 'liked':
        return 'Members you liked';
      case 'likedBy':
        return 'Members who liked you';
      case 'mutual':
        return 'Mutual likes';
    }
  }

  ngOnDestroy(): void {
    this.likesService.paginatedResult.set(null);
  }
}
