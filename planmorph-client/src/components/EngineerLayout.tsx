'use client';

import React from 'react';
import { useRoleGuard } from '@/src/hooks/useRoleGuard';
import FloatingActionBar from './FloatingActionBar';

interface EngineerLayoutProps {
  children: React.ReactNode;
}

const navItems = [
  { href: '/engineer/dashboard', label: 'Dashboard' },
];

export default function EngineerLayout({ children }: EngineerLayoutProps) {
  const { storedUser, isChecking } = useRoleGuard({
    requiredRole: 'Engineer',
    redirectTo: '/engineer/login',
    message: 'Access denied. Engineer account required.',
  });

  if (isChecking) {
    return (
      <div className="min-h-screen bg-brand flex items-center justify-center">
        <div className="relative w-12 h-12">
          <div className="absolute inset-0 rounded-full border-2 border-white/10" />
          <div className="absolute inset-0 rounded-full border-2 border-t-slate-teal animate-spin" />
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-brand">
      <FloatingActionBar
        navItems={navItems}
        accentColor="teal"
        portalLabel="Engineer"
        logoHref="/engineer/dashboard"
        authMode="engineer"
        portalUser={storedUser}
        showCurrency
      />
      <main>{children}</main>
    </div>
  );
}
