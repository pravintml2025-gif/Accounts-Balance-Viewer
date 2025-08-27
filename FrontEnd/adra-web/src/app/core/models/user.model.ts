export interface User {
  id: string;
  username: string;
  email: string;
  role: string;
  isActive: boolean;
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  expiresAt: string;
  roles: string[];
}
