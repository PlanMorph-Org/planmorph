'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import toast from 'react-hot-toast';
import { useAuthStore } from '@/src/store/authStore';

export type AuthRole = 'Client' | 'Architect' | 'Engineer' | 'Student';

interface StoredUser {
  email?: string;
  firstName?: string;
  lastName?: string;
  role?: string;
}

interface GuardOptions {
  requiredRole?: AuthRole;
  redirectTo: string;
  message?: string;
}

export function useRoleGuard({ requiredRole, redirectTo, message }: GuardOptions) {
  const router = useRouter();
  const { logout } = useAuthStore();
  const [storedUser, setStoredUser] = useState<StoredUser | null>(null);
  const [isAuthorized, setIsAuthorized] = useState(false);
  const [isChecking, setIsChecking] = useState(true);

  useEffect(() => {
    const token = localStorage.getItem('token');
    const userStr = localStorage.getItem('user');

    if (!token || !userStr) {
      logout();
      setStoredUser(null);
      setIsAuthorized(false);
      setIsChecking(false);
      router.push(redirectTo);
      return;
    }

    let parsedUser: StoredUser;
    try {
      parsedUser = JSON.parse(userStr);
    } catch {
      logout();
      setStoredUser(null);
      setIsAuthorized(false);
      setIsChecking(false);
      router.push(redirectTo);
      return;
    }

    if (requiredRole && parsedUser?.role !== requiredRole) {
      logout();
      setStoredUser(null);
      setIsAuthorized(false);
      setIsChecking(false);
      if (message) {
        toast.error(message);
      }
      router.push(redirectTo);
      return;
    }

    setStoredUser(parsedUser);
    setIsAuthorized(true);
    setIsChecking(false);
  }, [requiredRole, redirectTo, message, logout, router]);

  return { isAuthorized, isChecking, storedUser };
}
