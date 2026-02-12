import React from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { useRoleGuard } from '@/src/hooks/useRoleGuard';
import CurrencySelector from '@/src/components/CurrencySelector';

interface EngineerLayoutProps {
  children: React.ReactNode;
}

export default function EngineerLayout({ children }: EngineerLayoutProps) {
  const router = useRouter();
  const { storedUser, isChecking } = useRoleGuard({
    requiredRole: 'Engineer',
    redirectTo: '/engineer/login',
    message: 'Access denied. Engineer account required.',
  });

  if (isChecking) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-emerald-600"></div>
      </div>
    );
  }

  const handleLogout = () => {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    router.push('/engineer/login');
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <nav className="bg-white shadow-sm border-b border-gray-200">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between h-16">
            <div className="flex">
              <Link href="/engineer/dashboard" className="flex items-center gap-2">
                <img src="/planmorph.svg" alt="PlanMorph" className="h-8 w-auto" />
                <span className="text-2xl font-bold text-emerald-600">PlanMorph</span>
                <span className="ml-2 text-sm text-gray-500">Engineer Portal</span>
              </Link>
              <div className="hidden sm:ml-6 sm:flex sm:space-x-8">
                <Link
                  href="/engineer/dashboard"
                  className="inline-flex items-center px-1 pt-1 text-sm font-medium text-gray-900 hover:text-emerald-600"
                >
                  Dashboard
                </Link>
              </div>
            </div>

            <div className="flex items-center">
              <div className="flex items-center space-x-4">
                <CurrencySelector />
                {storedUser && (
                  <span className="text-sm text-gray-700">
                    {storedUser.firstName} {storedUser.lastName}
                  </span>
                )}
                <button
                  onClick={handleLogout}
                  className="text-sm font-medium text-gray-700 hover:text-emerald-600"
                >
                  Logout
                </button>
              </div>
            </div>
          </div>
        </div>
      </nav>

      <main>{children}</main>
    </div>
  );
}
