export const API_ENDPOINTS = {
  // Authentication
  AUTH: {
    LOGIN: '/auth/login',
    REFRESH: '/auth/refresh',
    LOGOUT: '/auth/logout'
  },
  
  // Balances
  BALANCES: {
    LATEST: '/balances/latest',
    BY_PERIOD: '/balances/by-period',
    UPLOAD: '/balances/upload',
    HISTORY: '/balances/history',
    SUMMARY: '/balances/summary',
    SUMMARY_BY_PERIOD: '/balances/summary/by-period'
  }
} as const;
