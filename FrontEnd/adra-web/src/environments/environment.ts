export const environment = {
  production: false,
  appName: 'Accounts Balance Viewer',
  api: {
    baseUrl: 'http://localhost:5241/api/v1',
    timeout: 30000
  },
  auth: {
    tokenKey: 'adra_token',
    refreshTokenKey: 'adra_refresh_token',
    tokenExpirationKey: 'adra_token_exp'
  },
  features: {
    enableFileUpload: true,
    enableReports: true,
    maxFileSize: 10485760 // 10MB
  }
};
