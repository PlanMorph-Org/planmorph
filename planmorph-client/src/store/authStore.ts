import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import api from '../lib/api';
import { AuthResponse, User } from '../types';

interface AuthState {
  user: User | null;
  token: string | null;
  isAuthenticated: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (data: RegisterData) => Promise<AuthResponse>;
  logout: () => void;
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

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      user: null,
      token: null,
      isAuthenticated: false,

      login: async (email: string, password: string) => {
        const response = await api.post<AuthResponse>('/auth/login', {
          email,
          password,
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
          throw new Error('Please use the appropriate professional login portal.');
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

      logout: () => {
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        set({
          user: null,
          token: null,
          isAuthenticated: false,
        });
      },
    }),
    {
      name: 'auth-storage',
    }
  )
);
