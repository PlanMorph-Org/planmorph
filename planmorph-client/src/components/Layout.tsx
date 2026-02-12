import React from 'react';
import Link from 'next/link';
import { useAuthStore } from '../store/authStore';
import { useRouter } from 'next/navigation';
import CurrencySelector from '@/src/components/CurrencySelector';
import { useCurrencyStore } from '@/src/store/currencyStore';

interface LayoutProps {
  children: React.ReactNode;
}

export default function Layout({ children }: LayoutProps) {
  const { user, isAuthenticated, logout } = useAuthStore();
  const router = useRouter();
  const { currency } = useCurrencyStore();

  const handleLogout = () => {
    logout();
    router.push('/');
  };

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Navigation */}
      <nav className="bg-white shadow-sm">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between h-16">
            <div className="flex">
              <Link href="/" className="flex items-center gap-2">
                <img src="/planmorph.svg" alt="PlanMorph" className="h-8 w-auto" />
                <span className="text-2xl font-bold text-blue-600">PlanMorph</span>
              </Link>
              <div className="hidden sm:ml-6 sm:flex sm:space-x-8">
                <Link
                  href="/designs"
                  className="inline-flex items-center px-1 pt-1 text-sm font-medium text-gray-900 hover:text-blue-600"
                >
                  Browse Designs
                </Link>
                <Link
                  href="/about"
                  className="inline-flex items-center px-1 pt-1 text-sm font-medium text-gray-500 hover:text-gray-900"
                >
                  About
                </Link>
              </div>
            </div>

            <div className="flex items-center">
              {isAuthenticated ? (
                <div className="flex items-center space-x-4">
                  <CurrencySelector />
                  <span className="text-sm text-gray-700">
                    Hello, {user?.firstName}
                  </span>
                  <Link
                    href="/my-orders"
                    className="text-sm font-medium text-gray-700 hover:text-blue-600"
                  >
                    My Orders
                  </Link>
                  <button
                    onClick={handleLogout}
                    className="text-sm font-medium text-gray-700 hover:text-blue-600"
                  >
                    Logout
                  </button>
                </div>
              ) : (
                <div className="flex items-center space-x-4">
                  <CurrencySelector />
                  <Link
                    href="/login"
                    className="text-sm font-medium text-gray-700 hover:text-blue-600"
                  >
                    Client Login
                  </Link>
                  <Link
                    href="/architect/login"
                    className="text-sm font-medium text-gray-500 hover:text-blue-600"
                  >
                    Architect Login
                  </Link>
                  <Link
                    href="/engineer/login"
                    className="text-sm font-medium text-gray-500 hover:text-emerald-600"
                  >
                    Engineer Login
                  </Link>
                  <Link
                    href="/register"
                    className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700"
                  >
                    Sign Up
                  </Link>
                </div>
              )}
            </div>
          </div>
        </div>
      </nav>

      {/* Main Content */}
      <main>{children}</main>

      {/* Footer */}
      <footer className="bg-gray-800 text-white mt-12">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          <div className="grid grid-cols-1 md:grid-cols-4 gap-8">
            <div>
              <div className="flex items-center gap-2 mb-4">
                <img src="/planmorph.svg" alt="PlanMorph" className="h-7 w-auto" />
                <h3 className="text-lg font-semibold">PlanMorph</h3>
              </div>
              <p className="text-gray-400">
                Global marketplace for verified architectural designs.
              </p>
              <p className="text-gray-500 text-sm mt-2">
                Construction services currently available in Kenya. Prices shown in {currency}.
                Billing currency is KES.
              </p>
            </div>
            <div>
              <h3 className="text-lg font-semibold mb-4">Quick Links</h3>
              <ul className="space-y-2">
                <li>
                  <Link href="/designs" className="text-gray-400 hover:text-white">
                    Browse Designs
                  </Link>
                </li>
                <li>
                  <Link href="/about" className="text-gray-400 hover:text-white">
                    About Us
                  </Link>
                </li>
              </ul>
            </div>
            <div>
              <h3 className="text-lg font-semibold mb-4">Legal</h3>
              <ul className="space-y-2">
                <li>
                  <Link href="/privacy-policy" className="text-gray-400 hover:text-white">
                    Privacy Policy
                  </Link>
                </li>
                <li>
                  <Link href="/terms-of-service" className="text-gray-400 hover:text-white">
                    Terms of Service
                  </Link>
                </li>
              </ul>
            </div>
            <div>
              <h3 className="text-lg font-semibold mb-4">Contact</h3>
              <p className="text-gray-400">Email: info@planmorph.com</p>
              <p className="text-gray-400">Support: support@planmorph.com</p>
              <p className="text-gray-400">Phone: +[country code] [number]</p>
            </div>
          </div>
          <div className="mt-8 pt-8 border-t border-gray-700 text-center text-gray-400">
            <p>&copy; 2025 PlanMorph. All rights reserved.</p>
            <p className="mt-2 text-sm">
              By using this site, you agree to our{' '}
              <Link href="/terms-of-service" className="text-blue-400 hover:underline">
                Terms of Service
              </Link>
              {' '}and{' '}
              <Link href="/privacy-policy" className="text-blue-400 hover:underline">
                Privacy Policy
              </Link>.
            </p>
          </div>
        </div>
      </footer>
    </div>
  );
}
