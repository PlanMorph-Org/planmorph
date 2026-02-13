'use client';

import { useEffect, useState } from 'react';
import ArchitectLayout from '@/src/components/ArchitectLayout';
import toast, { Toaster } from 'react-hot-toast';
import { useRoleGuard } from '@/src/hooks/useRoleGuard';
import { useCurrencyStore } from '@/src/store/currencyStore';
import { formatCurrency } from '@/src/lib/currency';
import { motion } from 'framer-motion';

interface Sale { id: string; orderNumber: string; designTitle: string; amount: number; date: string; commission: number; }

const fadeUp = {
  hidden: { opacity: 0, y: 20 },
  visible: (i: number = 0) => ({ opacity: 1, y: 0, transition: { delay: i * 0.08, duration: 0.5, ease: [0.16, 1, 0.3, 1] } }) as const,
};

export default function ArchitectEarningsPage() {
  const { isAuthorized } = useRoleGuard({ requiredRole: 'Architect', redirectTo: '/architect/login', message: 'Access denied. Architect account required.' });
  const [sales, setSales] = useState<Sale[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [stats, setStats] = useState({ totalEarnings: 0, thisMonth: 0, totalSales: 0, pending: 0 });
  const { currency, rates } = useCurrencyStore();

  useEffect(() => { if (isAuthorized) loadEarnings(); }, [isAuthorized]);

  const loadEarnings = async () => {
    setIsLoading(true);
    try {
      // TODO: Create endpoint to get architect sales — using mock data
      const mockSales: Sale[] = [
        { id: '1', orderNumber: 'PM-20250208-12345', designTitle: 'Modern 3-Bedroom Bungalow', amount: 50000, date: '2025-02-01', commission: 50000 * 0.7 },
        { id: '2', orderNumber: 'PM-20250207-67890', designTitle: 'Luxury 4-Bedroom Mansion', amount: 120000, date: '2025-01-28', commission: 120000 * 0.7 },
      ];
      setSales(mockSales);
      const total = mockSales.reduce((s, sale) => s + sale.commission, 0);
      const currentMonth = new Date().getMonth();
      const thisMonth = mockSales.filter(s => new Date(s.date).getMonth() === currentMonth).reduce((sum, s) => sum + s.commission, 0);
      setStats({ totalEarnings: total, thisMonth, totalSales: mockSales.length, pending: 0 });
    } catch { toast.error('Failed to load earnings data'); }
    finally { setIsLoading(false); }
  };

  const statCards = [
    { label: 'Total Earnings', value: formatCurrency(stats.totalEarnings, currency, rates), gradient: 'from-verified/20 to-emerald-500/10', color: 'text-verified' },
    { label: 'This Month', value: formatCurrency(stats.thisMonth, currency, rates), gradient: 'from-brand-accent/20 to-blue-500/10', color: 'text-brand-accent' },
    { label: 'Total Sales', value: stats.totalSales, gradient: 'from-purple-500/20 to-violet-500/10', color: 'text-purple-400' },
    { label: 'Pending', value: formatCurrency(stats.pending, currency, rates), gradient: 'from-golden/20 to-amber-500/10', color: 'text-golden' },
  ];

  if (isLoading) {
    return (
      <ArchitectLayout>
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-10">
          <div className="h-8 w-32 shimmer-bg rounded mb-6" />
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-8">{[...Array(4)].map((_, i) => <div key={i} className="glass-card rounded-xl p-6 h-24 shimmer-bg" />)}</div>
          <div className="glass-card rounded-xl p-6 h-48 shimmer-bg" />
        </div>
      </ArchitectLayout>
    );
  }

  return (
    <ArchitectLayout>
      <Toaster position="top-right" toastOptions={{ style: { background: '#1F2937', color: '#E5E7EB', border: '1px solid rgba(255,255,255,0.08)' } }} />
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <motion.div initial={{ opacity: 0, y: 16 }} animate={{ opacity: 1, y: 0 }} className="mb-8">
          <h1 className="text-2xl md:text-3xl font-display font-bold text-white">Earnings</h1>
          <p className="text-sm text-white/40 mt-1">Track your design sales and commissions</p>
          <p className="text-[10px] text-white/20 mt-0.5">Amounts shown in {currency}. Payouts are processed in KES.</p>
        </motion.div>

        {/* Stats */}
        <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-4 mb-8">
          {statCards.map((card, i) => (
            <motion.div key={card.label} variants={fadeUp} initial="hidden" animate="visible" custom={i} className="glass-card rounded-xl p-5 card-hover">
              <p className="text-[10px] text-white/30 uppercase tracking-widest mb-1">{card.label}</p>
              <p className={`text-xl font-bold font-mono ${card.color}`}>{card.value}</p>
            </motion.div>
          ))}
        </div>

        {/* Commission Info */}
        <motion.div initial={{ opacity: 0, y: 12 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.3 }}
          className="glass-card-light rounded-xl p-5 mb-8 border border-golden/10">
          <h3 className="text-sm font-semibold text-golden mb-1.5">Commission Structure</h3>
          <p className="text-sm text-white/50">
            You receive <span className="text-golden font-semibold">70%</span> of each design sale. PlanMorph retains 30% for platform maintenance, marketing, payment processing, and customer support.
          </p>
        </motion.div>

        {/* Sales Table */}
        <motion.div initial={{ opacity: 0, y: 16 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.4 }} className="glass-card rounded-xl overflow-hidden mb-8">
          <div className="px-5 py-4 border-b border-white/6">
            <h2 className="text-sm font-semibold text-white">Sales History</h2>
          </div>
          {sales.length === 0 ? (
            <div className="text-center py-16">
              <div className="w-14 h-14 mx-auto mb-3 glass-card-light rounded-full flex items-center justify-center">
                <svg className="w-6 h-6 text-white/20" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>
              </div>
              <p className="text-white/40 text-sm">No sales yet</p>
              <p className="text-white/20 text-xs mt-1">Earnings appear here once clients purchase your designs.</p>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="min-w-full text-sm">
                <thead>
                  <tr className="text-white/30 text-[10px] uppercase tracking-widest">
                    <th className="px-5 py-3 text-left font-medium">Order</th>
                    <th className="px-5 py-3 text-left font-medium">Design</th>
                    <th className="px-5 py-3 text-left font-medium">Sale Price</th>
                    <th className="px-5 py-3 text-left font-medium">Your Commission (70%)</th>
                    <th className="px-5 py-3 text-left font-medium">Date</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-white/6">
                  {sales.map((sale) => (
                    <tr key={sale.id} className="hover:bg-white/[0.02] transition-colors">
                      <td className="px-5 py-4 font-mono text-sm text-white/60">{sale.orderNumber}</td>
                      <td className="px-5 py-4 text-white/50">{sale.designTitle}</td>
                      <td className="px-5 py-4 text-white/40 font-mono">{formatCurrency(sale.amount, currency, rates)}</td>
                      <td className="px-5 py-4 font-semibold text-verified font-mono">{formatCurrency(sale.commission, currency, rates)}</td>
                      <td className="px-5 py-4 text-white/30 text-xs">{new Date(sale.date).toLocaleDateString()}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </motion.div>

        {/* Withdrawal Section */}
        <motion.div initial={{ opacity: 0, y: 16 }} animate={{ opacity: 1, y: 0 }} transition={{ delay: 0.5 }} className="glass-card rounded-xl p-6">
          <h3 className="text-sm font-semibold text-white mb-2">Withdraw Earnings</h3>
          <p className="text-xs text-white/30 mb-4">
            Request a withdrawal to receive your earnings. Minimum withdrawal: {formatCurrency(10000, currency, rates)} (paid in KES).
          </p>
          <button
            className="px-5 py-2.5 bg-golden text-brand font-semibold rounded-lg hover:bg-golden-light transition-all duration-300 text-sm disabled:opacity-30 disabled:cursor-not-allowed"
            disabled={stats.totalEarnings < 10000}
          >
            Request Withdrawal
          </button>
          {stats.totalEarnings < 10000 && (
            <p className="text-xs text-white/20 mt-2">
              Minimum not met. Balance: {formatCurrency(stats.totalEarnings, currency, rates)}
            </p>
          )}
        </motion.div>
      </div>
    </ArchitectLayout>
  );
}
