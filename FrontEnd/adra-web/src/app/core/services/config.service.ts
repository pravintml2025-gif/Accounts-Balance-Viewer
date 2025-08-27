import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ConfigService {
  
  // API Configuration
  get apiBaseUrl(): string {
    return environment.api.baseUrl;
  }
  
  get apiTimeout(): number {
    return environment.api.timeout;
  }
  
  // Authentication Configuration
  get tokenKey(): string {
    return environment.auth.tokenKey;
  }
  
  get refreshTokenKey(): string {
    return environment.auth.refreshTokenKey;
  }
  
  // Feature Flags
  get isFileUploadEnabled(): boolean {
    return environment.features.enableFileUpload;
  }
  
  get maxFileSize(): number {
    return environment.features.maxFileSize;
  }
  
  // Utility Methods
  buildApiUrl(endpoint: string): string {
    return `${this.apiBaseUrl}${endpoint.startsWith('/') ? endpoint : '/' + endpoint}`;
  }
  
  isProduction(): boolean {
    return environment.production;
  }
}
