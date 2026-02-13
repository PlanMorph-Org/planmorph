'use client';

import { useEffect, useState } from 'react';
import ArchitectLayout from '@/src/components/ArchitectLayout';
import api from '@/src/lib/api';
import { Design } from '@/src/types';
import toast, { Toaster } from 'react-hot-toast';
import Link from 'next/link';
import { useRoleGuard } from '@/src/hooks/useRoleGuard';
import { useCurrencyStore } from '@/src/store/currencyStore';
import { formatCurrency } from '@/src/lib/currency';
import { motion } from 'framer-motion';

const fadeUp = {
  hidden: { opacity: 0, y: 20 },
  visible: (i: number = 0) => ({ opacity: 1, y: 0, transition: { delay: i * 0.08, duration: 0.5, ease: [0.16, 1, 0.3, 1] } }) as const,
};

export default function ArchitectDashboard() {
  const { storedUser, isAuthorized } = useRoleGuard({ requiredRole: 'Architect', redirectTo: '/architect/login', message: 'Access denied. Architect account required.' });
  const [designs, setDesigns] = useState<Design[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [stats, setStats] = useState({ totalDesigns: 0, approvedDesigns: 0, pendingDesigns: 0, totalSales: 0 });
  const { currency, rates } = useCurrencyStore();

  useEffect(() => { if (isAuthorized) loadDashboardData(); }, [isAuthorized]);

  const loadDashboardData = async () => {
    setIsLoading(true);
    try {
      const response = await api.get<Design[]>('/designs/my-designs');
      setDesigns(response.data);
      setStats({
        totalDesigns: response.data.length,
        approvedDesigns: response.data.filter((d: any) => d.status === 'Approved').length,
        pendingDesigns: response.data.filter((d: any) => d.status === 'PendingApproval').length,
        totalSales: 0,
      });
    } catch { toast.error('Failed to load dashboard data'); }
    finally { setIsLoading(false); }
  };

  const getStatusStyle = (status: string) => {
    switch (status) {
      case 'Approved': return 'bg-verified/15 text-verified border-verified/20';
      case 'PendingApproval': return 'bg-golden/15 text-golden border-golden/20';
      case 'Rejected': return 'bg-red-500/15 text-red-400 border-red-500/20';
      default: return 'bg-white/10 text-white/50 border-white/10';
    }
  };

  const statCards = [
    { label: 'Total Designs', value: stats.totalDesigns, gradient: 'from-brand-accent/20 to-blue-500/10', color: 'text-brand-accent', icon: <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" /></svg> },
    { label: 'Approved', value: stats.approvedDesigns, gradient: 'from-verified/20 to-emerald-500/10', color: 'text-verified', icon: <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" /></svg> },
    { label: 'Pending Review', value: stats.pendingDesigns, gradient: 'from-golden/20 to-amber-500/10', color: 'text-golden', icon: <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" /></svg> },
    { label: 'Total Sales', value: formatCurrency(stats.totalSales, currency, rates), gradient: 'from-purple-500/20 to-violet-500/10', color: 'text-purple-400', icon: <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" /></svg> },
  ];

  if (isLoading) {
    return (
      <ArchitectLayout>
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-10">
          <div className="h-8 w-40 shimmer-bg rounded mb-6" />
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-8">{[...Array(4)].map((_, i) => <div key={i} className="glass-card rounded-xl p-6 h-24 shimmer-bg" />)}</div>
          <div className="glass-card rounded-xl p-6 h-64 shimmer-bg" />
        </div>
      </ArchitectLayout>
    );
  }

  return (
    <ArchitectLayout>
      <Toaster position="top-right" toastOptions={{ style: { background: '#1F2937', color: '#E5E7EB', border: '1px solid rgba(255,255,255,0.08)' } }} />
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Header */}
        <motion.div initial={{ opacity: 0, y: 16 }} animate={{ opacity: 1, y: 0 }} className="mb-8">
          <h1 className="text-2xl md:text-3xl font-display font-bold text-white">Dashboard</h1>
          <p className="text-sm text-white/40 mt-1">Welcome back, {storedUser?.firstName}!</p>
          <p className="text-[10px] text-white/20 mt-0.5">Amounts shown in {currency}. Payouts are processed in KES.</p>
        </motion.div>

        {/* Stats */}
        <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-4 mb-8">
          {statCards.map((card, i) => (
            <motion.div key={card.label} variants={fadeUp} initial="hidden" animate="visible" custom={i}
              className="glass-card rounded-xl p-5 card-hover">
              <div className="flex items-center gap-3">
                <div className={`w-10 h-10 rounded-lg bg-gradient-to-br ${card.gradient} flex items-center justify-center ${card.color}`}>{card.icon}</div>
                <div>
                  <p className="text-[10px] text-white/30 uppercase tracking-widest">{card.label}</p>
                  <p className="text-xl font-bold text-white font-mono">{card.value}</p>
                </div>
              </div>
            </motion.div>
          ))}
        </div>

        {/* Actions */}
        <motion.div initial={{ opacity: 0, y: 12 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.3 }} className="flex flex-wrap gap-3 mb-8">
          <Link href="/architect/upload" className="inline-flex items-center gap-2 px-5 py-2.5 bg-golden text-brand font-semibold rounded-lg hover:bg-golden-light transition-all duration-300 text-sm">
            <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" /></svg>
            Upload Design
          </Link>
          <Link href="/architect/verifications" className="inline-flex items-center gap-2 px-5 py-2.5 border border-golden/30 text-golden/70 hover:text-golden hover:border-golden/50 font-medium rounded-lg transition-all duration-300 text-sm">
            Review Verifications
          </Link>
          <Link href="/architect/earnings" className="inline-flex items-center gap-2 px-5 py-2.5 border border-white/10 text-white/40 hover:text-white hover:border-white/20 font-medium rounded-lg transition-all duration-300 text-sm">
            View Earnings
          </Link>
        </motion.div>

        {/* Designs Table */}
        <motion.div initial={{ opacity: 0, y: 16 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.4 }}
          className="glass-card rounded-xl overflow-hidden">
          <div className="px-5 py-4 border-b border-white/6">
            <h2 className="text-sm font-semibold text-white">My Designs</h2>
          </div>
          {designs.length === 0 ? (
            <div className="text-center py-16">
              <div className="w-14 h-14 mx-auto mb-3 glass-card-light rounded-full flex items-center justify-center">
                <svg className="w-6 h-6 text-white/20" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" /></svg>
              </div>
              <p className="text-white/40 text-sm mb-1">No designs yet</p>
              <p className="text-white/20 text-xs mb-5">Upload your first design to get started.</p>
              <Link href="/architect/upload" className="inline-flex items-center px-5 py-2 bg-golden text-brand font-medium rounded-lg hover:bg-golden-light transition-all text-sm">
                Upload Design
              </Link>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="min-w-full text-sm">
                <thead>
                  <tr className="text-white/30 text-[10px] uppercase tracking-widest">
                    <th className="px-5 py-3 text-left font-medium">Design</th>
                    <th className="px-5 py-3 text-left font-medium">Category</th>
                    <th className="px-5 py-3 text-left font-medium">Price</th>
                    <th className="px-5 py-3 text-left font-medium">Status</th>
                    <th className="px-5 py-3 text-left font-medium">Created</th>
                    <th className="px-5 py-3 text-left font-medium">Actions</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-white/6">
                  {designs.map((design: any) => (
                    <tr key={design.id} className="hover:bg-white/[0.02] transition-colors">
                      <td className="px-5 py-4">
                        <div className="flex items-center gap-3">
                          <div className="flex-shrink-0 w-10 h-10 rounded-lg overflow-hidden bg-brand-light">
                            {design.previewImages?.length > 0 ? (
                              <img className="w-full h-full object-cover" src={design.previewImages[0]} alt="" />
                            ) : (
                              <div className="w-full h-full bg-white/5" />
                            )}
                          </div>
                          <span className="font-medium text-white text-sm">{design.title}</span>
                        </div>
                      </td>
                      <td className="px-5 py-4 text-white/40">{design.category}</td>
                      <td className="px-5 py-4 text-white/60 font-mono">{formatCurrency(design.price, currency, rates)}</td>
                      <td className="px-5 py-4">
                        <span className={`px-2.5 py-0.5 text-[10px] font-semibold rounded-full border ${getStatusStyle(design.status)}`}>
                          {design.status}
                        </span>
                      </td>
                      <td className="px-5 py-4 text-white/30 text-xs">{new Date(design.createdAt).toLocaleDateString()}</td>
                      <td className="px-5 py-4">
                        <Link href={`/architect/designs/${design.id}`} className="text-xs text-golden/60 hover:text-golden transition-colors">
                          Edit
                        </Link>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </motion.div>
      </div>
    </ArchitectLayout>
  );
}
