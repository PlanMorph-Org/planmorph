'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import Layout from '@/src/components/Layout';
import api from '@/src/lib/api';
import { Ticket } from '@/src/types';
import toast, { Toaster } from 'react-hot-toast';
import { useRoleGuard } from '@/src/hooks/useRoleGuard';
import { motion } from 'framer-motion';

const fadeUp = {
  hidden: { opacity: 0, y: 20 },
  visible: (i: number = 0) => ({
    opacity: 1,
    y: 0,
    transition: { delay: i * 0.06, duration: 0.5, ease: [0.16, 1, 0.3, 1] },
  }),
};

const getStatusStyle = (status: string) => {
  switch (status) {
    case 'Open': return 'bg-blue-500/15 text-blue-400 border-blue-500/20';
    case 'Assigned': return 'bg-golden/15 text-golden border-golden/20';
    case 'InProgress': return 'bg-brand-accent/15 text-brand-accent border-brand-accent/20';
    case 'Resolved': return 'bg-verified/15 text-verified border-verified/20';
    case 'Closed': return 'bg-white/10 text-white/50 border-white/10';
    default: return 'bg-white/10 text-white/50 border-white/10';
  }
};

const getPriorityStyle = (priority: string) => {
  switch (priority) {
    case 'Low': return 'bg-white/10 text-white/50';
    case 'Medium': return 'bg-golden/15 text-golden';
    case 'High': return 'bg-orange-500/15 text-orange-400';
    case 'Urgent': return 'bg-red-500/15 text-red-400';
    default: return 'bg-white/10 text-white/50';
  }
};

export default function SupportPage() {
  const router = useRouter();
  const { isAuthorized, isChecking } = useRoleGuard({
    requiredRole: 'Client',
    redirectTo: '/login',
  });

  const [tickets, setTickets] = useState<Ticket[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    if (isAuthorized) loadTickets();
  }, [isAuthorized]);

  const loadTickets = async () => {
    setIsLoading(true);
    try {
      const response = await api.get<Ticket[]>('/ticket');
      setTickets(response.data);
    } catch {
      toast.error('Failed to load tickets');
    } finally {
      setIsLoading(false);
    }
  };

  if (isChecking || isLoading) {
    return (
      <Layout>
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-10">
          <div className="h-8 w-40 shimmer-bg rounded mb-6" />
          <div className="space-y-4">
            {[...Array(3)].map((_, i) => (
              <div key={i} className="glass-card rounded-xl p-6">
                <div className="h-4 w-1/3 shimmer-bg rounded mb-3" />
                <div className="h-3 w-1/2 shimmer-bg rounded" />
              </div>
            ))}
          </div>
        </div>
      </Layout>
    );
  }

  return (
    <Layout>
      <Toaster
        position="top-right"
        toastOptions={{
          style: {
            background: '#1F2937',
            color: '#E5E7EB',
            border: '1px solid rgba(255,255,255,0.08)',
          },
        }}
      />

      {/* Header */}
      <section className="relative py-14 border-b border-white/6">
        <div className="absolute inset-0 bg-hero-gradient" />
        <div className="relative max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between items-center">
            <div>
              <motion.h1
                initial={{ opacity: 0, y: 16 }}
                animate={{ opacity: 1, y: 0 }}
                className="text-2xl md:text-3xl font-display font-bold text-white tracking-tight mb-1"
              >
                Support Tickets
              </motion.h1>
              <motion.p
                initial={{ opacity: 0, y: 12 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 0.1 }}
                className="text-xs text-white/30"
              >
                Get help from our support team
              </motion.p>
            </div>
            <motion.button
              initial={{ opacity: 0, scale: 0.95 }}
              animate={{ opacity: 1, scale: 1 }}
              transition={{ delay: 0.15 }}
              onClick={() => router.push('/support/create')}
              className="flex items-center gap-2 px-5 py-2.5 bg-brand-accent text-white font-semibold rounded-lg hover:bg-blue-500 transition-all duration-300 shadow-blue btn-glow text-sm"
            >
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 4v16m8-8H4" />
              </svg>
              New Ticket
            </motion.button>
          </div>
        </div>
      </section>

      {/* Tickets List */}
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {tickets.length === 0 ? (
          <div className="text-center py-20">
            <div className="w-16 h-16 mx-auto mb-4 glass-card rounded-full flex items-center justify-center">
              <svg className="w-7 h-7 text-white/20" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
              </svg>
            </div>
            <p className="text-white/50 mb-1">No support tickets yet</p>
            <p className="text-xs text-white/25 mb-6">
              Create a ticket to get help from our support team.
            </p>
            <button
              onClick={() => router.push('/support/create')}
              className="px-6 py-2.5 bg-brand-accent text-white font-medium rounded-lg hover:bg-blue-500 transition-all duration-300 shadow-blue btn-glow text-sm"
            >
              Create Ticket
            </button>
          </div>
        ) : (
          <div className="space-y-4">
            {tickets.map((ticket, i) => (
              <motion.div
                key={ticket.id}
                variants={fadeUp}
                initial="hidden"
                animate="visible"
                custom={i}
                className="glass-card rounded-xl p-5 card-hover cursor-pointer"
                onClick={() => router.push(`/support/${ticket.id}`)}
              >
                <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
                  <div className="flex-1">
                    <div className="flex items-center gap-3 mb-2 flex-wrap">
                      {ticket.ticketNumber && (
                        <span className="text-sm font-mono font-semibold text-brand-accent">
                          {ticket.ticketNumber}
                        </span>
                      )}
                      <span className={`px-2.5 py-0.5 text-[10px] font-semibold rounded-full border ${getStatusStyle(ticket.status)}`}>
                        {ticket.status}
                      </span>
                      <span className={`px-2 py-0.5 text-[10px] font-medium rounded-full ${getPriorityStyle(ticket.priority)}`}>
                        {ticket.priority}
                      </span>
                      <span className="text-[10px] text-white/30">
                        {ticket.category}
                      </span>
                    </div>
                    <p className="text-sm font-medium text-white/80 mb-1">
                      {ticket.subject}
                    </p>
                    <p className="text-xs text-white/25">
                      Created {new Date(ticket.createdAt).toLocaleDateString()} &bull; {ticket.messages.length} message(s)
                      {ticket.unreadMessageCount > 0 && (
                        <span className="ml-2 px-1.5 py-0.5 bg-brand-accent/20 text-brand-accent text-[10px] font-semibold rounded-full">
                          {ticket.unreadMessageCount} new
                        </span>
                      )}
                    </p>
                  </div>
                  <span className="text-xs text-brand-accent hover:text-blue-400 transition-colors">
                    View Details &rarr;
                  </span>
                </div>
              </motion.div>
            ))}
          </div>
        )}
      </div>
    </Layout>
  );
}
