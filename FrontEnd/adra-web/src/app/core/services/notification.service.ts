import { Injectable } from '@angular/core';
import { ToastrService } from 'ngx-toastr';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {

  constructor(private toastr: ToastrService) {}

  showSuccess(message: string, title?: string): void {
    this.toastr.success(message, title || 'Success');
  }

  showError(message: string, title?: string): void {
    this.toastr.error(message, title || 'Error');
  }

  showWarning(message: string, title?: string): void {
    this.toastr.warning(message, title || 'Warning');
  }

  showInfo(message: string, title?: string): void {
    this.toastr.info(message, title || 'Info');
  }
}
