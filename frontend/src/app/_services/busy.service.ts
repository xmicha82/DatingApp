import { inject, Injectable } from '@angular/core';
import { NgxSpinnerService } from 'ngx-spinner';

@Injectable({
  providedIn: 'root',
})
export class BusyService {
  private spinnerService = inject(NgxSpinnerService);
  busyRequestsCount = 0;

  busy() {
    this.busyRequestsCount++;
    this.spinnerService.show(undefined, {
      type: 'ball-fussion',
      bdColor: 'rgba(255,255,255,0)',
      color: '#333333',
    });
  }

  idle() {
    this.busyRequestsCount--;
    if (this.busyRequestsCount <= 0) {
      this.spinnerService.hide();
    }
  }
}
