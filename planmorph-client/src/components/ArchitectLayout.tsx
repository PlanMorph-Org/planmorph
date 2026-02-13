'use client';

import React from 'react';
import { useRoleGuard } from '@/src/hooks/useRoleGuard';
import FloatingActionBar from './FloatingActionBar';

interface ArchitectLayoutProps {
  children: React.ReactNode;
}

const navItems = [
  { href: '/architect/dashboard', label: 'Dashboard' },
  { href: '/architect/upload', label: 'Upload' },
  { href: '/architect/verifications', label: 'Verifications' },
  { href: '/architect/earnings', label: 'Earnings' },
];

export default function ArchitectLayout({ children }: ArchitectLayoutProps) {
  const { storedUser, isChecking } = useRoleGuard({
    requiredRole: 'Architect',
    redirectTo: '/architect/login',
    message: 'Access denied. Architect account required.',
  });

  if (isChecking) {
    return (
      <div className="min-h-screen bg-brand flex items-center justify-center">
        <div className="relative w-12 h-12">
          <div className="absolute inset-0 rounded-full border-2 border-white/10" />
          <div className="absolute inset-0 rounded-full border-2 border-t-brand-accent animate-spin" />
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-brand">
      <FloatingActionBar
        navItems={navItems}
        accentColor="golden"
        portalLabel="Architect"
        logoHref="/architect/dashboard"
        authMode="architect"
        portalUser={storedUser}
        showCurrency
      />
      <main>{children}</main>
    </div>
  );
}
