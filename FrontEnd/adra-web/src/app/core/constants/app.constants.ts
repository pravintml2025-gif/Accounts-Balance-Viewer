export const APP_CONSTANTS = {
  // User Roles
  USER_ROLES: {
    ADMIN: 'Admin',
    USER: 'User'
  },
  
  // Local Storage Keys
  STORAGE_KEYS: {
    TOKEN: 'adra_token',
    USER: 'adra_user',
    PREFERENCES: 'adra_preferences'
  },
  
  // UI Constants
  UI: {
    ITEMS_PER_PAGE: 25,
    DEBOUNCE_TIME: 300,
    TOAST_DURATION: 5000
  },
  
  // File Upload
  FILE_UPLOAD: {
    ALLOWED_EXTENSIONS: ['.csv', '.xlsx', '.xls', '.tsv', '.txt'],
    MAX_SIZE_MB: 10
  }
} as const;
