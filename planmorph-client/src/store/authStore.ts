import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import api from '../lib/api';
import { AuthResponse, User } from '../types';

interface AuthState {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
  login: (email: string, password: string) => Promise<void>;
  loginWithGoogleClient: (googleIdToken: string) => Promise<void>;
  register: (data: RegisterData) => Promise<AuthResponse>;
  registerWithGoogleClient: (data: GoogleClientRegisterData) => Promise<AuthResponse>;
  logout: () => void;
  isAdmin: () => boolean;
}

interface RegisterData {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  phoneNumber: string;
  role?: 'Client' | 'Architect' | 'Engineer';
  professionalLicense?: string;
  yearsOfExperience?: number;
  portfolioUrl?: string;
  specialization?: string;
}

interface GoogleClientRegisterData {
  googleIdToken: string;
  firstName?: string;
  lastName?: string;
  phoneNumber?: string;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      user: null,
      token: null,
      isAuthenticated: false,

      login: async (email: string, password: string) => {
        const response = await api.post<AuthResponse>('/auth/login', {
          email,
          password,
        });

        const { token, ...userData } = response.data;

        // Allow Client, Admin, and Student roles to access the portal
        if (userData.role !== 'Client' && userData.role !== 'Admin' && userData.role !== 'Student') {
          localStorage.removeItem('token');
          localStorage.removeItem('user');
          set({
            user: null,
            token: null,
            isAuthenticated: false,
          });
          throw new Error('Access denied. Please contact your administrator if you believe this is an error.');
        }

        localStorage.setItem('token', token);
        localStorage.setItem('user', JSON.stringify({
          email: userData.email,
          firstName: userData.firstName,
          lastName: userData.lastName,
          role: userData.role,
        }));

        set({
          user: {
            id: '',
            email: userData.email,
            firstName: userData.firstName,
            lastName: userData.lastName,
            role: userData.role,
          },
          token,
          isAuthenticated: true,
        });
      },

      loginWithGoogleClient: async (googleIdToken: string) => {
        const response = await api.post<AuthResponse>('/auth/login-client-google', {
          googleIdToken,
        });

        const { token, ...userData } = response.data;

        if (userData.role !== 'Client') {
          localStorage.removeItem('token');
          localStorage.removeItem('user');
          set({
            user: null,
            token: null,
            isAuthenticated: false,
          });
          throw new Error('This Google account is not linked to a client profile.');
        }

        localStorage.setItem('token', token);
        localStorage.setItem('user', JSON.stringify({
          email: userData.email,
          firstName: userData.firstName,
          lastName: userData.lastName,
          role: userData.role,
        }));

        set({
          user: {
            id: '',
            email: userData.email,
            firstName: userData.firstName,
            lastName: userData.lastName,
            role: userData.role,
          },
          token,
          isAuthenticated: true,
        });
      },

      register: async (data: RegisterData) => {
        const response = await api.post<AuthResponse>('/auth/register', data);

        const { token, ...userData } = response.data;

        if (token && token.length > 0 && userData.role !== 'PendingApproval') {
          localStorage.setItem('token', token);
          localStorage.setItem('user', JSON.stringify({
            email: userData.email,
            firstName: userData.firstName,
            lastName: userData.lastName,
            role: userData.role,
          }));
          set({
            user: {
              id: '',
              email: userData.email,
              firstName: userData.firstName,
              lastName: userData.lastName,
              role: userData.role,
            },
            token,
            isAuthenticated: true,
          });
        } else {
          localStorage.removeItem('token');
          localStorage.removeItem('user');
          set({
            user: null,
            token: null,
            isAuthenticated: false,
          });
        }

        return response.data;
      },

      registerWithGoogleClient: async (data: GoogleClientRegisterData) => {
        const response = await api.post<AuthResponse>('/auth/register-client-google', data);

        const { token, ...userData } = response.data;

        if (token && token.length > 0 && userData.role === 'Client') {
          localStorage.setItem('token', token);
          localStorage.setItem('user', JSON.stringify({
            email: userData.email,
            firstName: userData.firstName,
            lastName: userData.lastName,
            role: userData.role,
          }));
          set({
            user: {
              id: '',
              email: userData.email,
              firstName: userData.firstName,
              lastName: userData.lastName,
              role: userData.role,
            },
            token,
            isAuthenticated: true,
          });
        }

        return response.data;
      },

      logout: () => {
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        set({
          user: null,
          token: null,
          isAuthenticated: false,
        });
      },

      isAdmin: () => {
        const state = get();
        return state.user?.role === 'Admin';
      },
    }),
    {
      name: 'auth-storage',
    }
  )
);
