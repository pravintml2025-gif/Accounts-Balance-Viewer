export interface Balance {
  id: string;
  account: string;
  amount: number;
  year: number;
  month: number;
  createdAt: Date;
  updatedAt: Date;
}

export interface UploadRequest {
  file: File;
  year: number;
  month: number;
}

export interface UploadResponse {
  success: boolean;
  message: string;
  processedRecords: number;
  skippedRecords: number;
  errors: string[];
}
