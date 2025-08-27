import { Injectable } from '@angular/core';
import { APP_CONSTANTS } from '../constants/app.constants';
import { User } from '../models';

@Injectable({
  providedIn: 'root'
})
export class StorageService {

  setToken(token: string): void {
    localStorage.setItem(APP_CONSTANTS.STORAGE_KEYS.TOKEN, token);
  }

  getToken(): string | null {
    return localStorage.getItem(APP_CONSTANTS.STORAGE_KEYS.TOKEN);
  }

  removeToken(): void {
    localStorage.removeItem(APP_CONSTANTS.STORAGE_KEYS.TOKEN);
  }

  setUser(user: User): void {
    localStorage.setItem(APP_CONSTANTS.STORAGE_KEYS.USER, JSON.stringify(user));
  }

  getUser(): User | null {
    try {
      const user = localStorage.getItem(APP_CONSTANTS.STORAGE_KEYS.USER);
      if (!user || user === 'undefined' || user === 'null') {
        return null;
      }
      return JSON.parse(user);
    } catch (error) {
      console.warn('Error parsing user from storage:', error);
      // Clear invalid data
      localStorage.removeItem(APP_CONSTANTS.STORAGE_KEYS.USER);
      return null;
    }
  }

  removeUser(): void {
    localStorage.removeItem(APP_CONSTANTS.STORAGE_KEYS.USER);
  }

  clear(): void {
    localStorage.removeItem(APP_CONSTANTS.STORAGE_KEYS.TOKEN);
    localStorage.removeItem(APP_CONSTANTS.STORAGE_KEYS.USER);
    localStorage.removeItem(APP_CONSTANTS.STORAGE_KEYS.PREFERENCES);
  }
}
