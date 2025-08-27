import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatListModule } from '@angular/material/list';
import { MatDividerModule } from '@angular/material/divider';
import { RouterModule } from '@angular/router';

import { BalanceService } from '../../../../core/services/balance.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { UploadResponse } from '../../../../core/models';

@Component({
  selector: 'app-balance-upload',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressBarModule,
    MatSnackBarModule,
    MatExpansionModule,
    MatListModule,
    MatDividerModule
  ],
  templateUrl: './balance-upload.component.html',
  styleUrls: ['./balance-upload.component.scss']
})
export class BalanceUploadComponent {
  uploadForm: FormGroup;
  selectedFile: File | null = null;
  isDragOver = false;
  isUploading = false;
  uploadResult: UploadResponse | null = null;
  invalidAccounts: string[] = [];

  constructor(
    private fb: FormBuilder,
    private balanceService: BalanceService,
    private notificationService: NotificationService
  ) {
    this.uploadForm = this.fb.group({
      file: ['', Validators.required]
    });
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;

    const files = event.dataTransfer?.files;
    if (files && files.length > 0) {
      this.handleFileSelection(files[0]);
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.handleFileSelection(input.files[0]);
    }
  }

  private handleFileSelection(file: File): void {
    // Validate file type
    const allowedTypes = [
      'text/csv',
      'application/vnd.ms-excel',
      'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
      'text/tab-separated-values'
    ];

    if (!allowedTypes.includes(file.type) && !file.name.match(/\.(csv|xlsx?|tsv)$/i)) {
      this.notificationService.showError('Please select a valid file (CSV, Excel, or TSV)');
      return;
    }

    // Validate file size (10MB limit)
    const maxSize = 10 * 1024 * 1024;
    if (file.size > maxSize) {
      this.notificationService.showError('File size must be less than 10MB');
      return;
    }

    this.selectedFile = file;
    this.uploadForm.patchValue({ file: file.name });
  }

  onUpload(): void {
    if (!this.selectedFile) {
      return;
    }

    // For now, use current year and month
    // In a real application, you might want to add form fields for these
    const currentDate = new Date();
    const year = currentDate.getFullYear();
    const month = currentDate.getMonth() + 1;

    this.isUploading = true;
    this.uploadResult = null;
    this.invalidAccounts = [];

    this.balanceService.uploadBalanceFile(this.selectedFile, year, month).subscribe({
      next: (response: UploadResponse) => {
        this.uploadResult = response;
        this.extractInvalidAccounts(response.errors);
        this.isUploading = false;

        if (response.success) {
          // Clear caches to ensure fresh data on next load
          this.balanceService.clearCache();
          
          if (response.skippedRecords > 0) {
            // Partial success with warnings
            this.notificationService.showWarning(
              `Upload completed with warnings: ${response.processedRecords} processed, ${response.skippedRecords} skipped`
            );
          } else {
            // Complete success
            this.notificationService.showSuccess(response.message);
          }
        } else {
          // Failed upload
          this.notificationService.showError('Upload failed: ' + response.message);
        }
      },
      error: (error: any) => {
        console.error('Upload failed:', error);
        this.isUploading = false;
        
        // Provide specific error messages based on error status/message
        if (error.status === 400) {
          const message = error.error?.message || 'Invalid file or data format.';
          this.notificationService.showError(message);
        } else if (error.status === 413) {
          this.notificationService.showError('File is too large. Please select a smaller file.');
        } else if (error.status === 401 || error.status === 403) {
          this.notificationService.showError('You do not have permission to upload files.');
        } else {
          this.notificationService.showError('Upload failed. Please check your file and try again.');
        }
      }
    });
  }

  private extractInvalidAccounts(errors: string[]): void {
    this.invalidAccounts = errors
      .filter(error => error.includes('Invalid Account:'))
      .map(error => {
        const match = error.match(/Invalid Account: '([^']+)'/);
        return match ? match[1] : '';
      })
      .filter(account => account !== '');
  }

  getResultIconClass(): string {
    if (!this.uploadResult) return '';
    
    if (this.uploadResult.success && this.uploadResult.skippedRecords === 0) {
      return 'success-icon';
    } else if (this.uploadResult.success && this.uploadResult.skippedRecords > 0) {
      return 'warning-icon';
    } else {
      return 'error-icon';
    }
  }

  getResultIcon(): string {
    if (!this.uploadResult) return 'info';
    
    if (this.uploadResult.success && this.uploadResult.skippedRecords === 0) {
      return 'check_circle';
    } else if (this.uploadResult.success && this.uploadResult.skippedRecords > 0) {
      return 'warning';
    } else {
      return 'error';
    }
  }

  getResultCardClass(): string {
    if (!this.uploadResult) return '';
    
    if (this.uploadResult.success && this.uploadResult.skippedRecords === 0) {
      return 'success-card';
    } else if (this.uploadResult.success && this.uploadResult.skippedRecords > 0) {
      return 'warning-card';
    } else {
      return 'error-card';
    }
  }

  onClear(): void {
    this.selectedFile = null;
    this.uploadForm.reset();
    
    // Clear file input
    const fileInput = document.querySelector('input[type="file"]') as HTMLInputElement;
    if (fileInput) {
      fileInput.value = '';
    }
  }

  getFileIcon(): string {
    if (!this.selectedFile) return 'insert_drive_file';
    
    const extension = this.selectedFile.name.split('.').pop()?.toLowerCase();
    switch (extension) {
      case 'csv':
      case 'tsv':
        return 'table_chart';
      case 'xlsx':
      case 'xls':
        return 'description';
      default:
        return 'insert_drive_file';
    }
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }
}
