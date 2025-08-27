export const environment = {
  production: true,
  appName: 'Accounts Balance Viewer',
  api: {
    baseUrl: 'https://adra-api-app-a7cefxhyeqdvcycw.southeastasia-01.azurewebsites.net/api/v1',
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
