'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { useAuthStore } from '../../store/authStore';
import Layout from '../../components/Layout';
import { ShieldExclamationIcon, UserGroupIcon, TicketIcon, ChartBarIcon, Cog6ToothIcon } from '@heroicons/react/24/outline';
import { motion } from 'framer-motion';

export default function AdminDashboard() {
  const { user, isAuthenticated } = useAuthStore();
  const router = useRouter();
  const [isAuthorized, setIsAuthorized] = useState(false);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    // Check if user is authenticated and has admin role
    if (!isAuthenticated || !user) {
      router.push('/admin/auth');
      return;
    }

    // Check if user role includes Admin
    const isAdmin = user.role === 'Admin' || user.role?.includes('Admin');
    if (!isAdmin) {
      router.push('/admin/auth');
      return;
    }

    setIsAuthorized(true);
    setLoading(false);
  }, [isAuthenticated, user, router]);

  if (loading) {
    return (
      <Layout>
        <div className="min-h-screen flex items-center justify-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-brand-accent"></div>
        </div>
      </Layout>
    );
  }

  if (!isAuthorized) {
    return null; // Will redirect
  }

  const quickStats = [
    { label: 'Active Tickets', value: '24', icon: TicketIcon, color: 'from-red-500 to-orange-500' },
    { label: 'Total Users', value: '1,247', icon: UserGroupIcon, color: 'from-blue-500 to-cyan-500' },
    { label: 'Monthly Revenue', value: '$32,450', icon: ChartBarIcon, color: 'from-green-500 to-emerald-500' },
    { label: 'System Health', value: '99.9%', icon: Cog6ToothIcon, color: 'from-purple-500 to-pink-500' }
  ];

  return (
    <Layout>
      <div className="min-h-screen pt-24 pb-12">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          {/* Header */}
          <div className="mb-12">
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.6 }}
              className="flex items-center gap-4 mb-4"
            >
              <div className="w-12 h-12 rounded-xl bg-gradient-to-br from-brand-accent to-blue-500 flex items-center justify-center">
                <ShieldExclamationIcon className="w-6 h-6 text-white" />
              </div>
              <div>
                <h1 className="text-3xl font-display font-bold text-white">Admin Dashboard</h1>
                <p className="text-white/60">Welcome back, {user?.firstName}</p>
              </div>
            </motion.div>
            
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.6, delay: 0.1 }}
              className="glass-card p-4 rounded-xl border border-brand-accent/20"
            >
              <p className="text-white/70 text-sm">
                🔒 <strong>Restricted Access:</strong> This dashboard is for authorized personnel only. 
                All actions are logged and monitored for security purposes.
              </p>
            </motion.div>
          </div>

          {/* Quick Stats */}
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6 mb-12">
            {quickStats.map((stat, index) => {
              const Icon = stat.icon;
              return (
                <motion.div
                  key={stat.label}
                  initial={{ opacity: 0, y: 20 }}
                  animate={{ opacity: 1, y: 0 }}
                  transition={{ duration: 0.6, delay: index * 0.1 + 0.2 }}
                  className="glass-card p-6 rounded-xl hover:bg-white/5 transition-all duration-300"
                >
                  <div className="flex items-center justify-between mb-4">
                    <div className={`w-10 h-10 rounded-lg bg-gradient-to-br ${stat.color} flex items-center justify-center`}>
                      <Icon className="w-5 h-5 text-white" />
                    </div>
                  </div>
                  <div>
                    <div className="text-2xl font-bold text-white mb-1">{stat.value}</div>
                    <div className="text-white/60 text-sm">{stat.label}</div>
                  </div>
                </motion.div>
              );
            })}
          </div>

          {/* Quick Actions */}
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.6, delay: 0.6 }}
            className="grid grid-cols-1 md:grid-cols-3 gap-6"
          >
            <div className="glass-card p-6 rounded-xl">
              <h3 className="text-xl font-semibold text-white mb-4">Support Tickets</h3>
              <p className="text-white/60 mb-6">Manage customer support tickets and respond to inquiries.</p>
              <button className="w-full bg-gradient-to-r from-brand-accent to-blue-500 text-white px-4 py-2 rounded-lg font-medium hover:shadow-lg hover:shadow-brand-accent/25 transition-all duration-300">
                View Tickets
              </button>
            </div>

            <div className="glass-card p-6 rounded-xl">
              <h3 className="text-xl font-semibold text-white mb-4">User Management</h3>
              <p className="text-white/60 mb-6">Manage user accounts, roles, and permissions.</p>
              <button className="w-full bg-gradient-to-r from-green-500 to-emerald-500 text-white px-4 py-2 rounded-lg font-medium hover:shadow-lg hover:shadow-green-500/25 transition-all duration-300">
                Manage Users
              </button>
            </div>

            <div className="glass-card p-6 rounded-xl">
              <h3 className="text-xl font-semibold text-white mb-4">System Settings</h3>
              <p className="text-white/60 mb-6">Configure system settings and application preferences.</p>
              <button className="w-full bg-gradient-to-r from-purple-500 to-pink-500 text-white px-4 py-2 rounded-lg font-medium hover:shadow-lg hover:shadow-purple-500/25 transition-all duration-300">
                Settings
              </button>
            </div>
          </motion.div>
        </div>
      </div>
    </Layout>
  );
}