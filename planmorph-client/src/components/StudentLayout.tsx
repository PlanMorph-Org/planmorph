'use client';

import React from 'react';
import { useRoleGuard } from '@/src/hooks/useRoleGuard';
import FloatingActionBar from './FloatingActionBar';

interface StudentLayoutProps {
  children: React.ReactNode;
}

const navItems = [
  { href: '/student/dashboard', label: 'Dashboard' },
  { href: '/student/projects', label: 'Projects' },
  { href: '/student/earnings', label: 'Earnings' },
  { href: '/student/profile', label: 'Profile' },
];

export default function StudentLayout({ children }: StudentLayoutProps) {
  const { storedUser, isChecking } = useRoleGuard({
    requiredRole: 'Student',
    redirectTo: '/student/login',
    message: 'Access denied. Student account required.',
  });

  if (isChecking) {
    return (
      <div className="min-h-screen bg-brand flex items-center justify-center">
        <div className="relative w-12 h-12">
          <div className="absolute inset-0 rounded-full border-2 border-white/10" />
          <div className="absolute inset-0 rounded-full border-2 border-t-indigo animate-spin" />
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-brand">
      <FloatingActionBar
        navItems={navItems}
        accentColor="indigo"
        portalLabel="Student"
        logoHref="/student/dashboard"
        authMode="student"
        portalUser={storedUser}
        showCurrency={false}
      />
      <main>{children}</main>
    </div>
  );
}
