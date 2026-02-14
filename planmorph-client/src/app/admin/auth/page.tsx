'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { useAuthStore } from '../../../store/authStore';
import Layout from '../../../components/Layout';
import { ShieldExclamationIcon, ExclamationTriangleIcon } from '@heroicons/react/24/outline';
import { motion } from 'framer-motion';
import Link from 'next/link';

export default function AdminAuth() {
  const { user, isAuthenticated } = useAuthStore();
  const router = useRouter();
  const [checkComplete, setCheckComplete] = useState(false);

  useEffect(() => {
    // If user is authenticated and is admin, redirect to dashboard
    if (isAuthenticated && user) {
      const isAdmin = user.role === 'Admin' || user.role?.includes('Admin');
      if (isAdmin) {
        router.push('/admin');
        return;
      }
    }
    setCheckComplete(true);
  }, [isAuthenticated, user, router]);

  if (!checkComplete) {
    return (
      <Layout>
        <div className="min-h-screen flex items-center justify-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-brand-accent"></div>
        </div>
      </Layout>
    );
  }

  return (
    <Layout>
      <div className="min-h-screen pt-24 pb-12 flex items-center justify-center">
        <div className="max-w-md w-full mx-4">
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.6 }}
            className="glass-card p-8 rounded-2xl border border-red-500/30"
          >
            {/* Warning Icon */}
            <div className="flex justify-center mb-6">
              <div className="w-16 h-16 rounded-full bg-gradient-to-br from-red-500 to-orange-500 flex items-center justify-center">
                <ExclamationTriangleIcon className="w-8 h-8 text-white" />
              </div>
            </div>

            {/* Title */}
            <div className="text-center mb-6">
              <h1 className="text-2xl font-display font-bold text-white mb-2">
                Restricted Access
              </h1>
              <p className="text-red-400 text-sm font-medium">
                Management Personnel Only
              </p>
            </div>

            {/* Notice */}
            <div className="bg-red-500/10 border border-red-500/30 rounded-lg p-4 mb-6">
              <div className="flex items-start gap-3">
                <ShieldExclamationIcon className="w-5 h-5 text-red-400 mt-0.5 flex-shrink-0" />
                <div className="text-sm text-white/80">
                  <p className="font-medium mb-2">Access Denied</p>
                  <p className="leading-relaxed">
                    This administrative dashboard is restricted to authorized management personnel only. 
                    If you believe you should have access to this area, please contact your system administrator.
                  </p>
                </div>
              </div>
            </div>

            {/* Current User Info */}
            {isAuthenticated && user ? (
              <div className="bg-white/5 rounded-lg p-4 mb-6">
                <p className="text-xs text-white/60 mb-1">Currently signed in as:</p>
                <p className="text-white font-medium">{user.firstName} {user.lastName}</p>
                <p className="text-white/60 text-sm">{user.email}</p>
                <p className="text-white/60 text-sm">Role: {user.role}</p>
              </div>
            ) : (
              <div className="bg-white/5 rounded-lg p-4 mb-6">
                <p className="text-white/60 text-sm">
                  You are not currently signed in. Please authenticate with an authorized administrator account.
                </p>
              </div>
            )}

            {/* Action Buttons */}
            <div className="space-y-3">
              <Link
                href="/"
                className="w-full bg-gradient-to-r from-brand-accent to-blue-500 text-white px-4 py-3 rounded-lg font-medium text-center block hover:shadow-lg hover:shadow-brand-accent/25 transition-all duration-300"
              >
                Return to Home
              </Link>
              
              {!isAuthenticated && (
                <Link
                  href="/login"
                  className="w-full bg-white/10 border border-white/20 text-white px-4 py-3 rounded-lg font-medium text-center block hover:bg-white/20 transition-all duration-300"
                >
                  Sign In
                </Link>
              )}
            </div>

            {/* Footer Notice */}
            <div className="mt-6 pt-4 border-t border-white/10">
              <p className="text-xs text-white/40 text-center">
                All access attempts are logged and monitored for security purposes.
              </p>
            </div>
          </motion.div>
        </div>
      </div>
    </Layout>
  );
}